using System;
using System.ServiceModel;
using SCME.InterfaceImplementations;

namespace SCME.DatabaseServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var serviceHost = new ServiceHost(typeof(CentralDatabaseService)))
            {
                serviceHost.Open();
                Console.WriteLine("Service started");
                Console.WriteLine("Press any key to stop");
                Console.ReadKey();
                serviceHost.Close();
            }


        }
    }
}
