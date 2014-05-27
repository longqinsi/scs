using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.ScsServices.Communication;
using Hik.Communication.ScsServices.Communication.Messages;
using Hik.Communication.ScsServices.Communication.Messengers;
using Hik.Protobuf;
using Hik.Utility;

namespace Hik.Communication.ScsServices.Client
{
    /// <summary>
    /// Represents a service client that consumes a SCS service.
    /// </summary>
    /// <typeparam name="T">Type of service interface</typeparam>
    internal class ScsServiceClient<T> : IScsServiceClient<T> where T : class
    {
        #region Public events

        /// <summary>
        /// This event is raised when client connected to server.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        #endregion

        #region Public properties

        /// <summary>
        /// Timeout for connecting to a server (as milliseconds).
        /// Default value: 15 seconds (15000 ms).
        /// </summary>
        public int ConnectTimeout
        {
            get { return _client.ConnectTimeout; }
            set { _client.ConnectTimeout = value; }
        }

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        public CommunicationStates CommunicationState
        {
            get
            {
                _isResgitered_SyncRoot.EnterReadLock();
                try
                {
                    if (_client.CommunicationState == CommunicationStates.Connected && _isRegistered)
                    {
                        return CommunicationStates.Connected;
                    }
                    else
                    {
                        return CommunicationStates.Disconnected;
                    }
                }
                finally
                {
                    _isResgitered_SyncRoot.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Reference to the service proxy to invoke remote service methods.
        /// </summary>
        public T ServiceProxy { get; private set; }

        /// <summary>
        /// Timeout value when invoking a service method.
        /// If timeout occurs before end of remote method call, an exception is thrown.
        /// Use -1 for no timeout (wait indefinite).
        /// Default value: 60000 (1 minute).
        /// </summary>
        public int Timeout
        {
            get { return _requestReplyMessenger.Timeout; }
            set { _requestReplyMessenger.Timeout = value; }
        }

        /// <summary>
        /// Gets the underlying IScsClient object to communicate with server
        /// </summary>
        public IScsClient Client { get { return _client; } }
        #endregion

        #region Private fields

        /// <summary>
        /// Underlying IScsClient object to communicate with server.
        /// </summary>
        private readonly IScsClient _client;

        /// <summary>
        /// Messenger object to send/receive messages over _client.
        /// </summary>
        private readonly RMIRequestReplyMessenger<IScsClient> _requestReplyMessenger;

        /// <summary>
        /// This object is used to create a transparent proxy to invoke remote methods on server.
        /// </summary>
        private readonly AutoConnectRemoteInvokeProxy<T, IScsClient> _realServiceProxy;

        /// <summary>
        /// The client object that is used to call method invokes in client side.
        /// May be null if client has no methods to be invoked by server.
        /// </summary>
        private readonly object _clientObject;

        /// <summary>
        /// Whether registered to server side rmi server.
        /// </summary>
        private bool _isRegistered = false;

        /// <summary>
        /// Syncronization object for _isRegistered.
        /// </summary>
        private ReaderWriterLockSlim _isResgitered_SyncRoot = new ReaderWriterLockSlim();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new ScsServiceClient object.
        /// </summary>
        /// <param name="client">Underlying IScsClient object to communicate with server</param>
        /// <param name="clientObject">The client object that is used to call method invokes in client side.
        /// May be null if client has no methods to be invoked by server.</param>
        /// <param name="isSessionEnabled">Indicates whether session is enabled.</param>
        public ScsServiceClient(IScsClient client, object clientObject, bool isSessionEnabled)
        {
            _client = client;
            _clientObject = clientObject;

            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;

            _requestReplyMessenger = new RMIRequestReplyMessenger<IScsClient>(client, TypeNameConverter.Default.ConvertToTypeName(typeof(T)));
            _requestReplyMessenger.MessageReceived += RequestReplyMessenger_MessageReceived;

            _realServiceProxy = new AutoConnectRemoteInvokeProxy<T, IScsClient>(_requestReplyMessenger, this, isSessionEnabled);
            ServiceProxy = (T)_realServiceProxy.GetTransparentProxy();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects to server.
        /// </summary>
        public void Connect()
        {
            _isResgitered_SyncRoot.EnterWriteLock();
            if (!_isRegistered)
            {
                try
                {
                    lock (_client)
                    {
                        if (_client.CommunicationState == CommunicationStates.Disconnected)
                        {
                            _client.Connect();
                        }
                    }
                    _client.SendMessage(new ScsRemoteRegisterMessage(TypeNameConverter.Default.ConvertToTypeName(typeof(T))));
                    this._isRegistered = true;
                }
                catch// (Exception ex)
                {
                }
                finally
                {
                    _isResgitered_SyncRoot.ExitWriteLock();
                }
            }
            else
            {
                _isResgitered_SyncRoot.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disconnects from server.
        /// Does nothing if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            _isResgitered_SyncRoot.EnterWriteLock();
            if (_isRegistered)
            {
                try
                {
                    if (_client.CommunicationState == CommunicationStates.Connected)
                    {
                        _client.SendMessage(new ScsRemoteUnregisterMessage(TypeNameConverter.Default.ConvertToTypeName(typeof(T))));
                    }
                }
                catch// (Exception ex)
                {
                }
                finally
                {
                    this._isRegistered = false;
                    _client.Connected -= Client_Connected;
                    _client.Disconnected -= Client_Disconnected;
                    _requestReplyMessenger.MessageReceived -= RequestReplyMessenger_MessageReceived;
                    _requestReplyMessenger.Stop();
                    _isResgitered_SyncRoot.ExitWriteLock();
                }
            }
            else
            {
                _isResgitered_SyncRoot.ExitWriteLock();
            }
        }

        /// <summary>
        /// Calls Disconnect method.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        #endregion

        #region Private methods
        private static ConcurrentDictionary<Type, MethodInfo[]> _clientObjectMethodCache = new ConcurrentDictionary<Type, MethodInfo[]>();
        /// <summary>
        /// Handles MessageReceived event of messenger.
        /// It gets messages from server and invokes appropriate method.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void RequestReplyMessenger_MessageReceived(object sender, MessageEventArgs e)
        {
            //Cast message to ScsRemoteInvokeMessage and check it
            var invokeMessage = e.Message as ScsRemoteInvokeMessage;
            if (invokeMessage == null)
            {
                return;
            }

            //Check client object.
            if (_clientObject == null)
            {
                SendInvokeResponse(invokeMessage, null, new ScsRemoteException("Client does not wait for method invocations by server."));
                return;
            }

            //Invoke method
            object returnValue;
            object[] outputParameters;
            List<int> outArgumentIndices = new List<int>();
            List<bool> outArgumentRefFlags = new List<bool>();
            try
            {
                var type = _clientObject.GetType();
                var methods = _clientObjectMethodCache.GetOrAdd(type, _ => _.GetMethods(BindingFlags.Public | BindingFlags.Instance));
                var matchedMethod = methods.FirstOrDefault(a => a.Name == invokeMessage.MethodName && a.GetParameters().Select(b => TypeNameConverter.Default.ConvertToTypeName(b.ParameterType)).SequenceEqual(invokeMessage.ParameterTypeNames));
                if (matchedMethod == null)
                {
                    SendInvokeResponse(invokeMessage, null, new ScsRemoteException("Client does not have a matched method."));
                    return;
                }
                var parameters = invokeMessage.Parameters != null ? invokeMessage.Parameters.Select(ArbitraryObject.GetValue).ToArray() : null;
                returnValue = matchedMethod.Invoke(_clientObject, parameters);
                if (parameters != null && parameters.Length > 0)
                {
                    ArrayList list = new ArrayList();
                    var matchedMethodParameters = matchedMethod.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (matchedMethodParameters[i].ParameterType.IsByRef)
                        {
                            if (outArgumentIndices == null)
                            {
                                outArgumentIndices = new List<int>();
                                outArgumentRefFlags = new List<bool>();
                            }
                            list.Add(parameters[i]);
                            outArgumentIndices.Add(i);
                            outArgumentRefFlags.Add(!matchedMethodParameters[i].IsOut);
                        }
                    }
                    outputParameters = list.ToArray();
                }
                else
                {
                    outputParameters = null;
                }
            }
            catch (TargetInvocationException ex)
            {
                var innerEx = ex.InnerException;
                SendInvokeResponse(invokeMessage, null, new ScsRemoteException(innerEx.Message, innerEx));
                return;
            }
            catch (Exception ex)
            {
                SendInvokeResponse(invokeMessage, null, new ScsRemoteException(ex.Message, ex));
                return;
            }

            //Send return value
            SendInvokeResponse(invokeMessage, returnValue, null, outputParameters, (outArgumentIndices != null) ? outArgumentIndices.ToArray() : null, (outArgumentRefFlags != null) ? outArgumentRefFlags.ToArray() : null);
        }

        /// <summary>
        /// Sends response to the remote application that invoked a service method.
        /// </summary>
        /// <param name="requestMessage">Request message</param>
        /// <param name="returnValue">Return value to send</param>
        /// <param name="exception">Exception to send</param>
        /// <param name="outputParameters">Output paramter values to send</param>
        /// <param name="outArgumentIndices">Indices of output arguments</param>
        /// <param name="outArgumentRefFlags">Indicates whether an OutArgument is a Ref Argument</param>
        private void SendInvokeResponse(IScsMessage requestMessage, object returnValue, ScsRemoteException exception, object[] outputParameters = null, int[] outArgumentIndices = null, bool[] outArgumentRefFlags = null)
        {
            try
            {
                _requestReplyMessenger.SendMessage(
                    new ScsRemoteInvokeReturnMessage
                    {
                        RepliedMessageId = requestMessage.MessageId,
                        ReturnValue = ArbitraryObject.CreateArbitraryObject(returnValue),
                        RemoteException = exception,
                        OutArguments = Array.ConvertAll((outputParameters ?? new object[0]), ArbitraryObject.CreateArbitraryObject),
                        OutArgumentIndices = outArgumentIndices,
                        OutArgumentRefFlags = outArgumentRefFlags,
                    });
            }
            catch
            {

            }
        }

        /// <summary>
        /// Handles Connected event of _client object.
        /// </summary>
        /// <param name="sender">Source of object</param>
        /// <param name="e">Event arguments</param>
        private void Client_Connected(object sender, EventArgs e)
        {
            _requestReplyMessenger.Start();
            OnConnected();
        }

        /// <summary>
        /// Handles Disconnected event of _client object.
        /// </summary>
        /// <param name="sender">Source of object</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            _requestReplyMessenger.Stop();
            OnDisconnected();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Raises Connected event.
        /// </summary>
        private void OnConnected()
        {
            var handler = Connected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

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