using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using log4net;
using WCFServer.Models;
using WCFServer;
using System.Threading;

namespace NServiceRepository
{
    /**
     * Klasa modułu ServiceRepository
     * */
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceRepository : IServiceRepository
    {
        /**
         * Lista z serwisami
         * */
        private List<Service> Services;
        private Repository Repo;
        private MyTimer oTimer;
            

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceRepository()
        {
            Services = new List<Service>();
            Repo = new Repository();
            oTimer = new MyTimer(Repo);
            Repo.CleanServices();
            Thread timerThread = new Thread(oTimer.StartTimer);
            timerThread.Start();
        }

        /**
         *  Rejestrowanie serwisu
         * */
        public void RegisterService(String Name, String Address)
        {
            if (Name == String.Empty || Address == String.Empty)
                throw new EmptyAddressOrNameException();

            if (FindService(Name) != null)
                throw new ServiceAlreadyExistsException();

            var NewService = new Service();
            NewService.Adress = Address;
            NewService.Name = Name;
            NewService.LastSeen = DateTime.Now;
            Console.WriteLine("Zarejestrowano serwis: " + Name + " pod adresem: " + Address);
            log.Info("Zarejestrowano serwis: " + Name + " pod adresem: " + Address);
            Services.Add(NewService);
            Repo.AddService(NewService);
        }

        /**
         * Pobieranie adresu serwisu
         * */
        public String GetServiceLocation(String Name)
        {
            var Service = FindService(Name);

            if (Service == null) throw new ServiceNotFoundException();

            return Service.Adress;
        }

        /**
         * Wyrejestrowywanie serwisu
         * */
        public void Unregister(String Name)
        {
            var Service = FindService(Name);
            if (Service == null) throw new ServiceNotFoundException();
            Console.WriteLine("Wyrejestrowano serwis: " + Name);
            log.Info("Wyrejestrowano serwis: " + Name);
            Services.Remove(Service);
            Repo.RemoveService(Service);
        }

        /**
         * Tu będzie odnowa połączenia
         * */
        public void Alive(String Name)
        {
            var Service = FindService(Name);
            if (Service == null) throw new ServiceNotFoundException();
            Service.LastSeen = DateTime.Now; 
            Repo.UpdateService(Service);
            Console.WriteLine("Zglosil sie serwis: "+Name);
            log.Info("Zglosil sie serwis: " + Name );
        }

        /**
         * Odszukiwanie serwisu w liście Services
         * */
        private Service FindService(String Name)
        {
            return Repo.FindService(Name);
            //return Services.Find(Element => Element.Name == Name);
        }

    }
}
