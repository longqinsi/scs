using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Service;
using PhoneBookCommonLib;

/* This is a simple phone book server application that runs on SCS framework.
 */

namespace PhoneBookServer
{
    class Program
    {
        static void Main()
        {
            //Create a Scs Service application that runs on 10048 TCP port.
            var server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(Consts.ServerPort));
            server.ClientConnected += server_ClientConnected;
            server.ClientDisconnected += server_ClientDisconnected;
            server.ServiceRegisteredByClient += server_ServiceRegisteredByClient;
            server.ServiceUnregisteredByClient += server_ServiceUnregisteredByClient;

            //Add Phone Book Service to service application
            server.AddService<IPhoneBookService, PhoneBookService>(new PhoneBookService());
            server.AddService<ICalculatorService, CalculatorService>(new CalculatorService());
            server.AddService<ISingleCallService, SingleCallService>();
            server.AddService<ISingletonService, SingletonService>();
            server.AddService<ItemPropertyServiceBase, ItemPropertyService>();
            //Start server
            server.Start();

            //Wait user to stop server by pressing Enter
            Console.WriteLine("Phone Book Server started successfully. Press enter to stop...");
            Console.ReadLine();
            
            //Stop server
            server.Stop();
        }

        static void server_ServiceUnregisteredByClient(object sender, ServiceClientEventArgs e)
        {
            Console.WriteLine("Client {0} unregistered service {1}.", e.Client.ClientId, e.Client.ServiceName);
        }

        static void server_ServiceRegisteredByClient(object sender, ServiceClientEventArgs e)
        {
            Console.WriteLine("Client {0} registered service {1}.", e.Client.ClientId, e.Client.ServiceName);
        }

        static void server_ClientDisconnected(object sender, Hik.Communication.Scs.Server.ServerClientEventArgs e)
        {
            Console.WriteLine("Client {0} disconnected.", e.Client.ClientId);
            e.Client.MessageReceived -= Client_MessageReceived;
        }

        static void server_ClientConnected(object sender, Hik.Communication.Scs.Server.ServerClientEventArgs e)
        {
            Console.WriteLine("New client connected, ClientID:{0}.", e.Client.ClientId);
            e.Client.MessageReceived += Client_MessageReceived;
        }

        static void Client_MessageReceived(object sender, Hik.Communication.Scs.Communication.Messages.MessageEventArgs e)
        {
            var textMessage = e.Message as ScsTextMessage;
            if (textMessage != null)
            {
                var client = sender as IScsServerClient;
                Console.WriteLine("Text from client {0}:{1}", client.ClientId, textMessage.Text);
            }
        }
    }
}
