using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    /// This class is used to create SCS Clients to connect to a SCS server.
    /// </summary>
    public static class ScsClientFactory
    {
        /// <summary>
        /// Creates a new client to connect to a server using an end point.
        /// </summary>
        /// <param name="endpoint">End point of the server to connect it</param>
        /// <returns>Created TCP client</returns>
        public static IScsClient CreateClient(ScsEndPoint endpoint)
        {
            return endpoint.CreateClient();
        }

        /// <summary>
        /// Creates a new client to connect to a server using an end point.
        /// </summary>
        /// <param name="endpointAddress">End point address of the server to connect it</param>
        /// <returns>Created TCP client</returns>
        public static IScsClient CreateClient(string endpointAddress)
        {
            return CreateClient(ScsEndPoint.CreateEndPoint(endpointAddress));
        }

        #region added by longqinsi 20140508
        /// <summary>
        /// Creates a new client, which is bound to a specific local port, to connect to a server using an end point.
        /// </summary>
        /// <param name="endpoint">End point of the server to connect it</param>
        /// <param name="localPort">The local port which the client is bound to.</param>
        /// <returns>Created TCP client</returns>
        public static IScsClient CreateClient(ScsEndPoint endpoint, int localPort)
        {
            return endpoint.CreateClient(localPort);
        }

        /// <summary>
        /// Creates a new client, which is bound to a specific local port, to connect to a server using an end point.
        /// </summary>
        /// <param name="endpointAddress">End point address of the server to connect it</param>
        /// <param name="localPort">The local port which the client is bound to.</param>
        /// <returns>Created TCP client</returns>
        public static IScsClient CreateClient(string endpointAddress, int localPort)
        {
            return CreateClient(ScsEndPoint.CreateEndPoint(endpointAddress), localPort);
        }
        #endregion
    }
}
