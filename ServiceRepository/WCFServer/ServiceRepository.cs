using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;

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

        public ServiceRepository()
        {
            Services = new List<Service>();
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

            Services.Add(NewService);
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

            Services.Remove(Service);
        }

        /**
         * Tu będzie próba połączenia
         * */
        public bool TryConnect(String Name)
        {
            return true;
        }

        /**
         * Odszukiwanie serwisu w liście Services
         * */
        private Service FindService(String Name)
        {
            return Services.Find(Element => Element.Name == Name);
        }

    }
}
