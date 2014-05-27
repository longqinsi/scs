using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.ScsServices.Communication;

namespace Hik.Communication.ScsServices.Client
{
    /// <summary>
    /// This class is used to build service clients to remotely invoke methods of a SCS service.
    /// </summary>
    public class ScsServiceClientBuilder
    {
        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="client">Underlying IScsClient object to communicate with server</param>
        /// <param name="clientObject">Client-side object that handles remote method calls from server to client.
        /// May be null if client has no methods to be invoked by server</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(IScsClient client, object clientObject = null, bool isSessionEnabled = true) where T : class
        {
            return new ScsServiceClient<T>(client, clientObject, isSessionEnabled);
        }
        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpoint">EndPoint of the server</param>
        /// <param name="clientObject">Client-side object that handles remote method calls from server to client.
        /// May be null if client has no methods to be invoked by server</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(ScsEndPoint endpoint, object clientObject = null, bool isSessionEnabled = true) where T : class
        {
            return CreateClient<T>(endpoint.CreateClient(), clientObject, isSessionEnabled);
        }

        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpointAddress">EndPoint address of the server</param>
        /// <param name="clientObject">Client-side object that handles remote method calls from server to client.
        /// May be null if client has no methods to be invoked by server</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(string endpointAddress, object clientObject = null, bool isSessionEnabled = true) where T : class
        {
            return CreateClient<T>(ScsEndPoint.CreateEndPoint(endpointAddress), clientObject, isSessionEnabled);
        }

        #region added by longqinsi 20140508
        /// <summary>
        /// Creates a client to connect to a SCS service and bound to a specific local port.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpoint">EndPoint of the server</param>
        /// <param name="loaclPort">The local port that the client is bound to.</param>
        /// 
        /// <param name="clientObject">Client-side object that handles remote method calls from server to client.
        /// May be null if client has no methods to be invoked by server</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(ScsEndPoint endpoint, int loaclPort, object clientObject = null, bool isSessionEnabled = true) where T : class
        {
            return CreateClient<T>(endpoint.CreateClient(loaclPort), clientObject, isSessionEnabled);
        }

        /// <summary>
        /// Creates a client to connect to a SCS service and bound to a specific local port.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpointAddress">EndPoint address of the server</param>
        /// <param name="loaclPort">The local port that the client is bound to.</param>
        /// <param name="clientObject">Client-side object that handles remote method calls from server to client.
        /// May be null if client has no methods to be invoked by server</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(string endpointAddress, int loaclPort, object clientObject = null, bool isSessionEnabled = true) where T : class
        {
            return CreateClient<T>(ScsEndPoint.CreateEndPoint(endpointAddress), loaclPort, clientObject, isSessionEnabled);
        }
        #endregion
    }
}
