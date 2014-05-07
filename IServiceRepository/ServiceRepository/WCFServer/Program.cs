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
            Database.SetInitializer<EFDbContext>(new DropCreateDatabaseIfModelChanges<EFDbContext>());
            log4net.Config.XmlConfigurator.Configure();
            try
            {
                ServiceRepository Repository = new ServiceRepository();
                //Repository.Register("yoyo", "yyyy");
                string serviceRepoAddress = ConfigurationSettings.AppSettings["serviceRepoAddress"];
                var Server = new ServiceRepositoryHost(Repository, serviceRepoAddress);
                Server.AddDefaultEndpoint("net.tcp://localhost:41234/IServiceRepository");
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
                Console.WriteLine("Chyba działa...");
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
