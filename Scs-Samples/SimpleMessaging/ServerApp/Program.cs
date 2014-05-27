using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
using System.Collections.Concurrent;
using System.Threading.Tasks;

/* This program is build to demonstrate a server application that listens incoming
 * client connections and reply messages.
 */

namespace ServerApp
{
    class Program
    {
        /// <summary>
        /// All clients connected to service.
        /// Key: Client's unique Id.
        /// Value: Reference to the client.
        /// </summary>
        static ConcurrentDictionary<long, IScsServerClient> _serverClients = new ConcurrentDictionary<long, IScsServerClient>();

        static Task _MonitorClientTask;

        static volatile bool _isRunning;
        static void Main()
        {
            _isRunning = true;
            _MonitorClientTask = Task.Factory.StartNew(MonitorClient);

            //Create a server that listens 10085 TCP port for incoming connections
            var server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(10085));

            //Register events of the server to be informed about clients
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            server.Start(); //Start the server

            Console.WriteLine("Server is started successfully. Press enter to stop...");
            Console.ReadLine(); //Wait user to press enter

            server.Stop(); //Stop the server

            _isRunning = false;

            _MonitorClientTask.Wait();
        }

        static void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            #region modified by longqinsi 20140508
            //Console.WriteLine("A new client is connected. Client Id = " + e.Client.ClientId);
            Console.WriteLine("A new client is connected. Client Id = {0}, IP = {1}, Port = {2}.",  e.Client.ClientId, (e.Client.RemoteEndPoint as ScsTcpEndPoint).IpAddress, (e.Client.RemoteEndPoint as ScsTcpEndPoint).TcpPort);
            #endregion

            //Register to MessageReceived event to receive messages from new client
            e.Client.MessageReceived += Client_MessageReceived;
            e.Client.MessageSent += Client_MessageSent;
            e.Client.Disconnected += Client_Disconnected;

            _serverClients[e.Client.ClientId] = e.Client;
        }

        static void Client_MessageSent(object sender, MessageEventArgs e)
        {
        }

        static void MonitorClient()
        {
            while(_isRunning)
            {
                foreach (var client in _serverClients.Values)
                {
                    //把超过1分钟没有通讯的客户端踢掉
                    var lastMinute = DateTime.Now.AddMinutes(-1);
                    if (client.LastReceivedMessageTime > lastMinute || client.LastSentMessageTime > lastMinute)
                    {
                        continue;
                    }
                    client.Disconnect();
                }
            }
        }

        static void Client_Disconnected(object sender, EventArgs e)
        {
            var client = sender as IScsServerClient;
            if (client != null)
            {
                Console.WriteLine("Client disconnected from server, ClientID = " + client.ClientId);
                IScsServerClient dummy;
                _serverClients.TryRemove(client.ClientId, out dummy);
            }
        }

        static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            #region modified by longqinsi 20140508
            //Console.WriteLine("A client is disconnected! Client Id = " + e.Client.ClientId);
            Console.WriteLine("A client is disconnected! Client Id = {0}, IP = {1}, Port = {2}.", e.Client.ClientId, (e.Client.RemoteEndPoint as ScsTcpEndPoint).IpAddress, (e.Client.RemoteEndPoint as ScsTcpEndPoint).TcpPort);
            #endregion
        }

        static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsTextMessage; //Server only accepts text messages
            if (message == null)
            {
                return;
            }

            //Get a reference to the client
            var client = (IScsServerClient)sender; 

            Console.WriteLine("Client sent a message: " + message.Text +
                              " (Cliend Id = " + client.ClientId + ")");

            //Send reply message to the client
            client.SendMessage(
                new ScsTextMessage(
                    "Hello client. I got your message (" + message.Text + ")",
                    message.MessageId //Set first message's id as replied message id
                    ));
        }
    }
}
