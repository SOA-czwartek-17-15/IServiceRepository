using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using NServiceRepository;
using System.Configuration;
using log4net;
using System.Data.Entity;
using WCFServer.Models;
using System.Threading;
using ZeroMQ;
using zguide;
using Newtonsoft.Json;
using WCFServer;
using Contracts;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

/**
 * ######## ServiceRepository #########
 * 
 * Authors: Mateusz Ścirka, Konrad Seweryn
 * 
 * */

namespace NServiceRepository
{

    /**
     * Klasa główna programu
     * */
    public class Program
    {
        private static ServiceRepository Repository;

        //  ---------------------------------------------------------------------
        //  This is our client task
        //  It connects to the server, and then sends a request once per second
        //  It collects responses as they arrive, and it prints them out. We will
        //  run several client tasks in parallel, each with a different random ID.
        public static void ClientTask()
        {
            using (var context = ZmqContext.Create())
            {
                using (ZmqSocket client = context.CreateSocket(SocketType.DEALER))
                {
                    //  Generate printable identity for the client
                    string identity = ZHelpers.SetID(client, Encoding.Unicode);
                    //client.Connect("tcp://localhost:5570");
                    string serviceRepoAddress = ConfigurationSettings.AppSettings["serviceRepoAddressZMQ"];
                    client.Connect(serviceRepoAddress);

                    client.ReceiveReady += (s, e) =>
                    {
                        var zmsg = new ZMessage(e.Socket);
                        var lol = zmsg.BodyToString();
                        JSONMessage m = JsonConvert.DeserializeObject<JSONMessage>(lol);

                        switch (m.Service)
                        {
                            case "IServiceRepository":
                                if (m.Function == "GetServiceLocations")
                                {
                                    if (m.ReponseString != "null")
                                    {
                                        Console.WriteLine(m.ReponseString);
                                        List<ServiceAB> foo = JsonConvert.DeserializeObject<List<ServiceAB>>(m.ReponseString);
                                        Console.WriteLine("{0} : {1} : {2}, {3}", m.Service, m.Function, foo.ElementAt(0).Adress, foo.ElementAt(0).Binding);
                                    }
                                    else
                                    {
                                        Console.WriteLine("{0} : {1} : {2}", m.Service, m.Function, "Nie istnieje");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("{0} : {1} : {2}", m.Service, m.Function, m.ReponseString);
                                }
                                break;
                            default:
                                Console.WriteLine("Unknown: " + m.Service);
                                break;
                        }
                    };
                    var poller = new Poller(new List<ZmqSocket> { client });
                    var zmsg2 = new ZMessage("");
                    JSONMessage jsonMess = new JSONMessage();
                    jsonMess.Service = "NazwaSerwisu";
                    jsonMess.Function = "RegisterService";
                    jsonMess.Parameters = new string[] { "NazwaSerwisu", "AdresSerwisu", "Binding" };
                    string json = JsonConvert.SerializeObject(jsonMess);
                    zmsg2.StringToBody(json);
                    zmsg2.Send(client);

                    while (true)
                    {
                        //  Tick once per second, pulling in arriving messages
                        for (int centitick = 0; centitick < 100; centitick++)
                        {
                            poller.Poll(TimeSpan.FromMilliseconds(10));
                        }
                        var zmsg = new ZMessage("");
                        jsonMess = new JSONMessage();
                        jsonMess.Service = "NazwaSerwisu";
                        jsonMess.Function = "GetServiceLocations";
                        jsonMess.Parameters = new string[] { "NazwaSerwisu" };
                        json = JsonConvert.SerializeObject(jsonMess);
                        zmsg.StringToBody(json);
                        zmsg.Send(client);
                    }
                }
            }
        }

        //  ---------------------------------------------------------------------
        //  This is our server task
        //  It uses the multithreaded server model to deal requests out to a pool
        //  of workers and route replies back to clients. One worker can handle
        //  one request at a time but one client can talk to multiple workers at
        //  once.
        private static void ServerTask()
        {
            var workers = new List<Thread>(5);
            using (var context = ZmqContext.Create())
            {
                using (ZmqSocket frontend = context.CreateSocket(SocketType.ROUTER), backend = context.CreateSocket(SocketType.DEALER))
                {
                    frontend.Bind("tcp://*:5570");
                    backend.Bind("inproc://backend");

                    for (int workerNumber = 0; workerNumber < 5; workerNumber++)
                    {
                        workers.Add(new Thread(ServerWorker));
                        workers[workerNumber].Start(context);
                    }

                    //  Switch messages between frontend and backend
                    frontend.ReceiveReady += (s, e) =>
                    {
                        var zmsg = new ZMessage(e.Socket);
                        zmsg.Send(backend);
                    };

                    backend.ReceiveReady += (s, e) =>
                    {

                        var zmsg = new ZMessage(e.Socket);
                        string lol = zmsg.BodyToString();
                        JSONMessage m = JsonConvert.DeserializeObject<JSONMessage>(lol);
                        if (m.Function != null)
                        {
                            switch (m.Function)
                            {
                                case "RegisterService":
                                    Console.WriteLine("ZEROMQ RegisterService: " + m.Parameters[0] + ";" + m.Parameters[1] + ";" + m.Parameters[2]);
                                    m.ReponseString = "OK";
                                    Repository.RegisterService(m.Parameters[0], m.Parameters[1], m.Parameters[2]);
                                    break;
                                case "GetServiceLocation":
                                    Console.WriteLine("ZEROMQ GetServiceLocation: "  + m.Parameters[0] + ";" + m.Parameters[1]);
                                    if (m.Parameters[1]==null)
                                        m.ReponseString = Repository.GetServiceLocation(m.Parameters[0]);
                                    else
                                        m.ReponseString = Repository.GetServiceLocation(m.Parameters[0], m.Parameters[1]);
                                    break;
                                case "GetServiceLocations":
                                    Console.WriteLine("ZEROMQ GetServiceLocations: " + m.Parameters[0]);                                    
                                    List<ServiceAB> ser = Repository.GetServiceLocations(m.Parameters[0]);
                                    string locs = JsonConvert.SerializeObject(ser);
                                    m.ReponseString = locs;
                                    break;
                                case "Alive":
                                    Console.WriteLine("ZEROMQ Alive: " + ";" + m.Parameters[0]);
                                    Repository.Alive(m.Parameters[0]);
                                    m.ReponseString = "OK";
                                    break;
                                case "Unregister":
                                    Console.WriteLine("ZEROMQ Unregister: " + ";" + m.Parameters[0]);
                                    Repository.Unregister(m.Parameters[0]);
                                    m.ReponseString = "OK";
                                    break;
                                default:
                                    Console.WriteLine("Unknown: " + m.Function);
                                    break;
                            }

                        }
                        m.Service = "IServiceRepository";
                        string json = JsonConvert.SerializeObject(m);
                        zmsg.StringToBody(json);
                        zmsg.Send(frontend);
                    };

                    var poller = new Poller(new List<ZmqSocket> { frontend, backend });

                    while (true)
                    {
                        poller.Poll();
                    }
                }
            }
        }

        //  Accept a request and reply with the same text a random number of
        //  times, with random delays between replies.
        private static void ServerWorker(object context)
        {
            var randomizer = new Random(DateTime.Now.Millisecond);
            using (ZmqSocket worker = ((ZmqContext)context).CreateSocket(SocketType.DEALER))
            {
                worker.Connect("inproc://backend");

                while (true)
                {
                    //  The DEALER socket gives us the address envelope and message
                    var zmsg = new ZMessage(worker);
                    //  Send 0..4 replies back
                    int replies = randomizer.Next(5);
                    for (int reply = 0; reply < replies; reply++)
                    {
                        Thread.Sleep(randomizer.Next(1, 1000));
                        zmsg.Send(worker);
                    }
                }
            }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            //aby baza sie mogla zaktualizowac do obecnego modelu klasy
            Database.SetInitializer<EFDbContext>(new DropCreateDatabaseIfModelChanges<EFDbContext>());
            //korzystanie z log4neta
            log4net.Config.XmlConfigurator.Configure();
            //ServiceRepository Repository;
            try
            {
                //wybranie czy korzystamy z bazy danych czy mock
                Console.WriteLine("Service with Database ? (y/n)");
                if (Console.ReadLine().ToLower() == "y")
                    Repository = new ServiceRepository();
                else
                    Repository = new ServiceRepository(false);
                //pobranie adresu servRep z app.config
                string serviceRepoAddress = ConfigurationSettings.AppSettings["serviceRepoAddress"];
                //odpalenie serwisu
                var Server = new ServiceRepositoryHost(Repository, serviceRepoAddress);
                Server.AddDefaultEndpoint(serviceRepoAddress);
                ServiceDebugBehavior debug = Server.Description.Behaviors.Find<ServiceDebugBehavior>();
                // if not found - add behavior with setting turned on 
                if (debug == null)
                {
                    Server.Description.Behaviors.Add(
                            new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                }
                else
                {
                    // make sure setting is turned ON
                    if (!debug.IncludeExceptionDetailInFaults)
                    {
                        debug.IncludeExceptionDetailInFaults = true;
                    }
                }
                //komunikacja z innymi serwisami
                Server.Open();
                log.Info("Uruchomienie Serwera");
                Console.WriteLine("Uruchomienie Serwera");
                
                //A takze zeromq
                var serverThread = new Thread(ServerTask);
                serverThread.Start();

                //przykladowyKlient
                //var clients = new List<Thread>(1);
                //for (int clientNumber = 0; clientNumber < 1; clientNumber++)
                //{
                //    clients.Add(new Thread(ClientTask));
                //    clients[clientNumber].Start();
                //}

                Console.ReadLine();
            }
            catch (ServiceRepositoryException Ex)
            {
                log.Info("Złapano wyjatek: " + Ex.Message);
                Console.WriteLine(Ex.Message);
            }
            Console.ReadLine();
            log.Info("Zatrzymanie Serwera");
        }
    }
}
