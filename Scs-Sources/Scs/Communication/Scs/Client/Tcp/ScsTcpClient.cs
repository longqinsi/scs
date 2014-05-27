using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;
using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class ScsTcpClient : ScsClientBase
    {
        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        /// <summary>
        /// The loacl port which the client is binded to.
        /// </summary>
        private int localPort = 0;

        /// <summary>Get local port of the Tcp Client</summary>
        public int LocalPort { get { return localPort; } }

        /// <summary>
        /// Creates a new ScsTcpClient object.
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        public ScsTcpClient(ScsTcpEndPoint serverEndPoint)
        {
            _serverEndPoint = serverEndPoint;
        }

        public ScsTcpClient(ScsTcpEndPoint serverEndPoint, int localPort)
            : this(serverEndPoint)
        {
            if (localPort > 65535 || localPort < 0)
            {
                throw new System.ArgumentOutOfRangeException("localPort", "Port number must be a positive integer not greater than 65535.");
            }
            this.localPort = localPort;
        }

        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            return new TcpCommunicationChannel(
                TcpHelper.ConnectToServer(
                    new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort),
                    ConnectTimeout,
                    ref localPort
                    ));
        }
    }
}
