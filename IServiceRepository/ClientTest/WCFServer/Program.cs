using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Configuration;
using System.ServiceModel.Channels;

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
        static void Main(string[] args)
        {
            try
            {
                string serviceRepoAddress = ConfigurationSettings.AppSettings["serviceRepoAddress"];
                var cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://87.207.93.4:41234/IServiceRepository"));
                IServiceRepository serwis = cf.CreateChannel();
                serwis.RegisterService("lolek2", "adres_lolek2");
                System.Threading.Thread.Sleep(3000);
                serwis.Alive("lolek2");
                System.Threading.Thread.Sleep(3000);
                serwis.Alive("lolek2");
                Console.WriteLine(serwis.GetServiceLocation("lolek2"));
                Console.ReadLine();
            }
            catch (FaultException ex)
            {
                string msg = "FaultException: " + ex.Message;           
                Console.WriteLine(msg);
                Console.ReadLine();
            }
        }
    }
    [ServiceContract]
    public interface IServiceRepository
    {
        [OperationContract]
        void RegisterService(String Name, String Address);

        [OperationContract]
        string GetServiceLocation(String Name);

        [OperationContract]
        void Unregister(String Name);

        [OperationContract]
        void Alive(String Name);
    }
    [ServiceContract]
    public class ServiceRepositoryException
    { 
    }

}
