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
         * Repozytorium albo z baza danych albo mock
         * */
        private Repository Repo;
        private NonRepository NonRepo;
        private MyTimer oTimer;
        bool Datab;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceRepository(bool Database=true)
        {
            Datab = Database;
            if (Datab)
            {
                Repo = new Repository();
                oTimer = new MyTimer(Repo);
            }
            else
            {
                NonRepo = new NonRepository();
                oTimer = new MyTimer(NonRepo);
            }
            //odpalenie oddzielnego watku na usuwanie serwisow ktore przestaly sie komunikowac
            Thread timerThread = new Thread(oTimer.StartTimer);
            timerThread.Start();
        }

        /**
         *  Rejestrowanie serwisu
         * */
        public void RegisterService(String Name, String Address, String Binding)
        {
           // if (Name == String.Empty || Address == String.Empty)
             //   throw new EmptyAddressOrNameException();

            if (FindService(Name, Binding) != null)
                Unregister(Name, Binding);

            var NewService = new Service();
            NewService.Adress = Address;
            NewService.Name = Name;
            NewService.LastSeen = DateTime.Now;
            NewService.Binding = Binding;
            Console.WriteLine("Zarejestrowano serwis: " + Name + " pod adresem: " + Address);
            log.Info("Zarejestrowano serwis: " + Name + " pod adresem: " + Address);
            if (Datab)
                Repo.AddService(NewService);
            else
                NonRepo.AddService(NewService);
        }

        /**
         *  Rejestrowanie serwisu
         * */
        public void RegisterService(String Name, String Address)
        {
            RegisterService(Name, Address, "NetTcpBinding");
        }


        /**
         *  Pobieranie adresu serwisu
         * */
        public String GetServiceLocation(String Name)
        {
            return GetServiceLocation(Name, "NetTcpBinding");
        }
        /**
         * Pobieranie adresu serwisu
         * */
        public String GetServiceLocation(String Name, String Binding)
        {
            var Service = FindService(Name, Binding);
            if (Service == null) return null;
           // if (Service == null) throw new ServiceNotFoundException();
            return Service.Adress;
        }

        /**
         *  Wyrejestrowywanie serwisu
         * */
        public void Unregister(String Name)
        {
            //this.Unregister(Name, "NetTcpBinding");
        }

        /**
         * Wyrejestrowywanie serwisu
         * */
        public void Unregister(String Name, String Binding)
        {
            var Service = FindService(Name, Binding);
            if (Service != null)
            { 
                //throw new ServiceNotFoundException();
                if (Datab)
                    Repo.RemoveService(Service);
                else
                    NonRepo.RemoveService(Service);
                Console.WriteLine("Wyrejestrowano serwis (user): " + Name);
                log.Info("Wyrejestrowano serwis (user): " + Name);
            }
        }

        /**
         * Odnowa połączenia aby wiadomo bylo czy serwis dalej istnieje
         * */
        public void Alive(String Name)
        {
            Alive(Name, "NetTcpBinding");
        }

        /**
         * Odnowa połączenia aby wiadomo bylo czy serwis dalej istnieje
         * */
        public void Alive(String Name, String Binding)
        {
            var Service = FindService(Name, Binding);
            if (Service != null)
            {
                //throw new ServiceNotFoundException();
                Service.LastSeen = DateTime.Now;
                if (Datab)
                    Repo.UpdateService(Service);
                else
                    NonRepo.UpdateService(Service);
                Console.WriteLine("Zglosil sie serwis: " + Name);
                log.Info("Zglosil sie serwis: " + Name);
            }
        }

        /**
         * Odszukiwanie serwisu w liście Services
         * */
        private Service FindService(String Name, String Binding)
        {
            if (Datab)
                return Repo.FindService(Name, Binding);
            else
                return NonRepo.FindService(Name, Binding);
        }

    }
}
