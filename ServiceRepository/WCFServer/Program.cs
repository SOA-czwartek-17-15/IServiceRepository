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
                ServiceRepository Repository = new ServiceRepository();
                //Repository.Register("yoyo", "yyyy");

                var Server = new ServiceRepositoryHost(Repository, "net.tcp://localhost:41234/IServiceRepository");
                Server.AddDefaultEndpoint("net.tcp://localhost:41234/IServiceRepository");
                Server.Open();

                Console.WriteLine("Chyba działa...");
            }
            catch (ServiceRepositoryException Ex)
            {
                Console.WriteLine(Ex.Message);
            }

            Console.ReadLine();
        }
    }
}
