using System;
using System.Runtime.Remoting.Proxies;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Communication;
using Hik.Communication.ScsServices.Communication.Messengers;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    /// Implements IScsServiceClient.
    /// It is used to manage and monitor a service client.
    /// </summary>
    internal class ScsServiceClientStub : IScsServiceClientStub
    {
        #region Public events

        /// <summary>
        /// This event is raised when this client is disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        #endregion

        #region Public properties

        /// <summary>
        /// Unique identifier of the underlying message layer client.
        /// </summary>
        public long ClientId
        {
            get { return _serverClient.ClientId; }
        }

        /// <summary>
        /// Gets the underlying client used to send and receive messages.
        /// </summary>
        public IScsServerClient ServerClient { get { return _serverClient; } }

        /// <summary>
        /// Get the service name binding to.
        /// </summary>
        public string ServiceName
        {
            get { return _serviceName; }
        }

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        public ScsEndPoint RemoteEndPoint
        {
            get { return _serverClient.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the communication state of the Client.
        /// </summary>
        public CommunicationStates CommunicationState
        {
            get
            {
                return _serverClient.CommunicationState;
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to underlying IScsServerClient object.
        /// </summary>
        private readonly IScsServerClient _serverClient;

        /// <summary>
        /// The service name binding to.
        /// </summary>
        private readonly string _serviceName;

        /// <summary>
        /// This object is used to send messages to client.
        /// </summary>
        private readonly RMIRequestReplyMessenger<IScsServerClient> _requestReplyMessenger;

        /// <summary>
        /// Last created proxy object to invoke remote medhods.
        /// </summary>
        private RealProxy _realProxy;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new ScsServiceClient object.
        /// </summary>
        /// <param name="serviceName">The service name binding to.</param>
        /// <param name="serverClient">Reference to underlying IScsServerClient object</param>
        /// <param name="requestReplyMessenger">RequestReplyMessenger to send messages</param>
        public ScsServiceClientStub(string serviceName, IScsServerClient serverClient, RMIRequestReplyMessenger<IScsServerClient> requestReplyMessenger)
        {
            _serviceName = serviceName;
            _serverClient = serverClient;
            _serverClient.Disconnected += Client_Disconnected;
            _requestReplyMessenger = requestReplyMessenger;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Closes client connection.
        /// </summary>
        public void Disconnect()
        {
            _serverClient.Disconnected -= Client_Disconnected;
            _requestReplyMessenger.Stop();
            OnDisconnected();
        }

        /// <summary>
        /// Gets the client proxy interface that provides calling client methods remotely.
        /// </summary>
        /// <typeparam name="T">Type of client interface</typeparam>
        /// <returns>Client interface</returns>
        public T GetClientProxy<T>() where T : class
        {
            _realProxy = new RemoteInvokeProxy<T, IScsServerClient>(_requestReplyMessenger);
            return (T)_realProxy.GetTransparentProxy();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles disconnect event of _serverClient object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            _requestReplyMessenger.Stop();
            OnDisconnected();
        }

        #endregion

        #region Event raising methods

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        private void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
