using NServiceRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServer.Models
{

    /**
    * Mock do bazy danych
    * */
    class NonRepository
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<Service> Services;
        public NonRepository()
        {
            Services = new List<Service>();
        }
        /**
        * Dodanie serwisu do bazy
        **/
        public void AddService(Service serv){
            Services.Add(serv);
        }
        /**
        * Usuniecie serwisu z bazy
        **/
        public void RemoveService(Service serv)
        {
            Services.Remove(serv);
        }
        /**
        * Zaktualizowanie czasu ostatniej komunikacji z serwisem
        **/
        public void UpdateService(Service serv)
        {
        }
        /**
        * Wyszukwanie serwisu w bazie
        **/
        public Service FindService(String Name)
        {
            return Services.Find(serv => serv.Name == Name);
        }
        /**
        * Usuniecie nieaktywnych serwisow
        **/
        public void KillZombieServices()
        {
            TimeSpan duration;
            foreach (var serv in Services)
            {
                duration = DateTime.Now - serv.LastSeen;
                if (duration.Seconds > 5)
                {          
                    Console.WriteLine("Serwis {0} wygasł", serv.Name);
                    log.Info("Serwis "+ serv.Name+" wygasł.");
                    Services.Remove(serv);
                }     
            }
        }
    }
}
