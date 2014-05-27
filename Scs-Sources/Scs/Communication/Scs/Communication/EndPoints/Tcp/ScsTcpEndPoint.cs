using System;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Client.Tcp;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Server.Tcp;

namespace Hik.Communication.Scs.Communication.EndPoints.Tcp
{
    /// <summary>
    /// Represens a TCP end point in SCS.
    /// </summary>
    public sealed class ScsTcpEndPoint : ScsEndPoint
    {
        ///<summary>
        /// IP address of the server.
        ///</summary>
        public string IpAddress { get; set; }

        ///<summary>
        /// Listening TCP Port for incoming connection requests on server.
        ///</summary>
        public int TcpPort { get; private set; }

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified port number.
        /// </summary>
        /// <param name="tcpPort">Listening TCP Port for incoming connection requests on server</param>
        public ScsTcpEndPoint(int tcpPort)
        {
            TcpPort = tcpPort;
        }

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified IP address and port number.
        /// </summary>
        /// <param name="ipAddress">IP address of the server</param>
        /// <param name="port">Listening TCP Port for incoming connection requests on server</param>
        public ScsTcpEndPoint(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            TcpPort = port;
        }
        
        /// <summary>
        /// Creates a new ScsTcpEndPoint from a string address.
        /// Address format must be like IPAddress:Port (For example: 127.0.0.1:10085).
        /// </summary>
        /// <param name="address">TCP end point Address</param>
        /// <returns>Created ScsTcpEndpoint object</returns>
        public ScsTcpEndPoint(string address)
        {
            var splittedAddress = address.Trim().Split(':');
            IpAddress = splittedAddress[0].Trim();
            TcpPort = Convert.ToInt32(splittedAddress[1].Trim());
        }

        /// <summary>
        /// Creates a Scs Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Scs Server</returns>
        internal override IScsServer CreateServer()
        {
            return new ScsTcpServer(this);
        }

        /// <summary>
        /// Creates a Scs Client that uses this end point to connect to server.
        /// </summary>
        /// <returns>Scs Client</returns>
        internal override IScsClient CreateClient()
        {
            return new ScsTcpClient(this);
        }

        #region added by longqinsi 20140508
        /// <summary>
        /// Creates a Scs Client that uses this end point to connect to server and is bound to a specific local port.
        /// </summary>
        /// <param name="localPort">The loacl port that the client is bound to.</param>
        /// <returns>Scs Client</returns>
        internal override IScsClient CreateClient(int localPort)
        {
            return new ScsTcpClient(this, localPort);
        }
        #endregion

        /// <summary>
        /// Generates a string representation of this end point object.
        /// </summary>
        /// <returns>String representation of this end point object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(IpAddress) ? ("tcp://" + TcpPort) : ("tcp://" + IpAddress + ":" + TcpPort);
        }

        /// <summary>Create a clone of current object.</summary>
        /// <returns>The created clone.</returns>
        internal ScsTcpEndPoint Clone()
        {
            return new ScsTcpEndPoint(this.IpAddress, this.TcpPort);
        }

        /// <summary>Verifies whether equal to another SceEndPoint.</summary>
        /// <param name="other">The other ScsEndPoint to compare to.</param>
        /// <returns>Where equal to.</returns>
        public override bool Equals(ScsEndPoint other)
        {
            var otherScsTcpEndPoint = other as ScsTcpEndPoint;
            if(object.ReferenceEquals(otherScsTcpEndPoint, null))
            {
                return false;
            }
            return this.IpAddress == otherScsTcpEndPoint.IpAddress && this.TcpPort == otherScsTcpEndPoint.TcpPort;
        }

        /// <summary>
        /// Overrides base.GetHaseCode
        /// </summary>
        /// <returns>The hash code.</returns>

        public override int GetHashCode()
        {
            return this.IpAddress.GetHashCode() ^ this.TcpPort.GetHashCode();
        }

    }
}