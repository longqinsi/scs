using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to simplify TCP socket operations.
    /// </summary>
    internal static class TcpHelper
    {
        /// <summary>
        /// This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <param name="localPort">The local port that the TCP socket binds to.</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not connect within specified timeoutMs</exception>
        public static Socket ConnectToServer(EndPoint endPoint, int timeoutMs, ref int localPort)
        {
            if (localPort > 65535 || localPort < 0)
            {
                throw new ArgumentOutOfRangeException("localPort", "Port number must be a positive interger less than or equal to 65535.");
            }
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (localPort > 0)
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
            }
            socket.Blocking = false;

            ConnectToServerInternal(endPoint, timeoutMs, socket);
            socket.Blocking = true;
            localPort = (socket.LocalEndPoint as IPEndPoint).Port;
            return socket;

        }

        [DebuggerNonUserCode]
        private static void ConnectToServerInternal(EndPoint endPoint, int timeoutMs, Socket socket)
        {
            try
            {
                socket.Connect(endPoint);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode != 10035)
                {
                    socket.Close();
                    throw;
                }

                if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectWrite))
                {
                    socket.Close();
                    throw new TimeoutException("The host failed to connect. Timeout occured.");
                }
            }
        }



    }
}
