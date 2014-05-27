using System.Runtime.Remoting.Messaging;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.ScsServices.Communication.Messengers;

namespace Hik.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class extends RemoteInvokeProxy to provide auto connect/disconnect mechanism
    /// if client is not connected to the server when a service method is called.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    internal class AutoConnectRemoteInvokeProxy<TProxy, TMessenger> : RemoteInvokeProxy<TProxy, TMessenger> where TMessenger : IMessenger
    {
        #region Private fields
        /// <summary>
        /// Reference to the client object that is used to connect/disconnect.
        /// </summary>
        private readonly IConnectableClient _client;

        /// <summary>
        /// Indicates whether session is enabled.
        /// </summary>
        private readonly bool _isSessionEnabled;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new AutoConnectRemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        /// <param name="client">Reference to the client object that is used to connect/disconnect</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        public AutoConnectRemoteInvokeProxy(RMIRequestReplyMessenger<TMessenger> clientMessenger, IConnectableClient client, bool isSessionEnabled)
            : base(clientMessenger)
        {
            _client = client;
            _isSessionEnabled = isSessionEnabled;
        }
        #endregion


        #region Overridden methods
        /// <summary>
        /// Overrides message calls and translates them to messages to remote application.
        /// </summary>
        /// <param name="msg">Method invoke message (from RealProxy base class)</param>
        /// <returns>Method invoke return message (to RealProxy base class)</returns>
        public override IMessage Invoke(IMessage msg)
        {
            //If not connected, connect
            if (_client.CommunicationState == CommunicationStates.Disconnected)
            {
                _client.Connect();
            }

            try
            {
                //Call mothod
                return base.Invoke(msg);
            }
            finally
            {
                //If session is not enabled, disconnect
                if (!_isSessionEnabled)
                {
                    _client.Disconnect();
                }
            }
        }
        #endregion
    }
}
