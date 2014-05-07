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
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            //aby baza sie mogla zaktualizowac do obecnego modelu klasy
            Database.SetInitializer<EFDbContext>(new DropCreateDatabaseIfModelChanges<EFDbContext>());
            //korzystanie z log4neta
            log4net.Config.XmlConfigurator.Configure();
            ServiceRepository Repository;
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
                Server.Open();
                log.Info("Uruchomienie Serwera");
                Console.WriteLine("Uruchomienie Serwera");
                //komunikacja z innymi serwisami
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
