using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using Hik;
using Hik.Collections;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Communication.Messages;
using Hik.Communication.ScsServices.Communication.Messengers;
using Hik.Protobuf;
using Hik.Utility;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    /// Implements IScsServiceApplication and provides all functionallity.
    /// </summary>
    internal class ScsServiceApplication : IScsServiceApplication
    {
        #region Public events
        /// <summary>
        /// This event is raised when a new client connected to the server.
        /// </summary>
        public event EventHandler<ServerClientEventArgs> ClientConnected
        {
            add
            {
                _scsServer.ClientConnected -= value;
                _scsServer.ClientConnected += value;
            }
            remove
            {
                _scsServer.ClientConnected -= value;
            }
        }

        /// <summary>
        /// This event is raised when a client disconnected from the server.
        /// </summary>
        public event EventHandler<ServerClientEventArgs> ClientDisconnected
        {
            add
            {
                _scsServer.ClientDisconnected -= value;
                _scsServer.ClientDisconnected += value;
            }
            remove
            {
                _scsServer.ClientDisconnected -= value;
            }
        }

        /// <summary>
        /// This event is raised when a new client connected to the service.
        /// </summary>
        public event EventHandler<ServiceClientEventArgs> ServiceRegisteredByClient;

        /// <summary>
        /// This event is raised when a client disconnected from the service.
        /// </summary>
        public event EventHandler<ServiceClientEventArgs> ServiceUnregisteredByClient;

        #endregion

        #region Public properties
        /// <summary>
        /// Get the underlying IScsServer object whic is used by the ScsServiceApplication instance to accept and manage client connections.
        /// </summary>
        public IScsServer ScsServer { get { return _scsServer; } }
        #endregion

        #region Private fields

        /// <summary>
        /// Underlying IScsServer object to accept and manage client connections.
        /// </summary>
        private readonly IScsServer _scsServer;

        /// <summary>
        /// User service objects that is used to invoke incoming method invocation requests.
        /// Key: Service interface type's name.
        /// Value: Service object.
        /// </summary>
        private readonly ConcurrentDictionary<string, ServiceObject> _serviceObjects;

        /// <summary>
        /// All connected clients to service.
        /// Key: Client's unique Id.
        /// Value: 
        ///        Item1: Reference to message layer client.
        ///        Item2:
        ///             All RMI connection from the client.
        ///             Key: Service name.
        ///             Value: Reference to the RMI layer client.
        /// </summary>
        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, IScsServiceClientStub>> _serviceClients;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new ScsServiceApplication object.
        /// </summary>
        /// <param name="scsServer">Underlying IScsServer object to accept and manage client connections</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if scsServer argument is null</exception>
        public ScsServiceApplication(IScsServer scsServer)
        {
            if (scsServer == null)
            {
                throw new ArgumentNullException("scsServer");
            }

            _scsServer = scsServer;
            _scsServer.ClientConnected += ScsServer_ClientConnected;
            _scsServer.ClientDisconnected += ScsServer_ClientDisconnected;
            _serviceObjects = new ConcurrentDictionary<string, ServiceObject>();
            _serviceClients = new ConcurrentDictionary<long, ConcurrentDictionary<string, IScsServiceClientStub>>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts service application.
        /// </summary>
        public void Start()
        {
            _scsServer.Start();
        }

        /// <summary>
        /// Stops service application.
        /// </summary>
        public void Stop()
        {
            _scsServer.Stop();
        }

        /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <param name="service">An instance of TServiceClass.</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        public void AddService<TServiceInterface, TServiceClass>(TServiceClass service)
            where TServiceClass : TServiceInterface
            where TServiceInterface : class
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var type = typeof(TServiceInterface);
            var typeName = TypeNameConverter.Default.ConvertToTypeName(type);
            if (!_serviceObjects.TryAdd(typeName, ServiceObject<TServiceInterface, TServiceClass>.CreateInstance(service)))
            {
                throw new Exception("Service '" + typeName + "' is already added before.");
            }
        }

        /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        public void AddService<TServiceInterface, TServiceClass>()
            where TServiceClass : TServiceInterface, new()
            where TServiceInterface : class
        {
            AddService<TServiceInterface, TServiceClass>(CreateDefaultService<TServiceInterface, TServiceClass>);
        }

        /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <param name="activator">The delegate used to activate new service objects</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        public void AddService<TServiceInterface, TServiceClass>(Func<TServiceClass> activator)
            where TServiceClass : TServiceInterface
            where TServiceInterface : class
        {
            var type = typeof(TServiceInterface);
            var typeName = TypeNameConverter.Default.ConvertToTypeName(type);
            if (!_serviceObjects.TryAdd(typeName, ServiceObject<TServiceInterface, TServiceClass>.CreateInstance(activator)))
            {
                throw new Exception("Service '" + typeName + "' is already added before.");
            }
        }

        /// <summary>
        /// Removes a previously added service object from this service application.
        /// It removes object according to interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <returns>True: removed. False: no service object with this interface</returns>
        public bool RemoveService<TServiceInterface>()
            where TServiceInterface : class
        {
            ServiceObject serviceObject;
            if (_serviceObjects.TryRemove(TypeNameConverter.Default.ConvertToTypeName(typeof(TServiceInterface)), out serviceObject))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles ClientConnected event of _scsServer object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ScsServer_ClientConnected(object sender, ServerClientEventArgs e)
        {
            e.Client.MessageReceived += MessageLayerClient_MessageReceived;
            _serviceClients[e.Client.ClientId] = new ConcurrentDictionary<string, IScsServiceClientStub>();
        }

        private void MessageLayerClient_MessageReceived(object sender, MessageEventArgs e)
        {
            var messageLayerClient = (IScsServerClient)sender;
            //Cast message to ScsRemoteUnregisterMessage and check it
            var registerMessage = e.Message as ScsRemoteRegisterMessage;
            if (registerMessage != null)
            {
                ConcurrentDictionary<string, IScsServiceClientStub> client;
                if (_serviceClients.TryGetValue(messageLayerClient.ClientId, out client))
                {
                    var requestReplyMessenger = new RMIRequestReplyMessenger<IScsServerClient>(messageLayerClient, registerMessage.ServiceName);
                    requestReplyMessenger.MessageReceived += RMILayerClient_MessageReceived;
                    requestReplyMessenger.Start();

                    var serviceClient = ScsServiceClientStubFactory.CreateServiceClient(registerMessage.ServiceName, messageLayerClient, requestReplyMessenger);
                    client[registerMessage.ServiceName] = serviceClient;
                    OnClientConnected(serviceClient);
                }
                return;
            }

            //Cast message to ScsRemoteUnregisterMessage and check it
            var unregisterMessage = e.Message as ScsRemoteUnregisterMessage;
            if (unregisterMessage != null)
            {
                ConcurrentDictionary<string, IScsServiceClientStub> client;
                if (_serviceClients.TryGetValue(messageLayerClient.ClientId, out client))
                {
                    IScsServiceClientStub serviceClient;
                    if (client.TryRemove(unregisterMessage.ServiceName, out serviceClient))
                    {
                        serviceClient.Disconnect();
                        OnClientDisconnected(serviceClient);
                    }
                }
                return;
            }
        }

        /// <summary>
        /// Handles ClientDisconnected event of _scsServer object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ScsServer_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            ConcurrentDictionary<string, IScsServiceClientStub> disconnectedClient;
            if (_serviceClients.TryRemove(e.Client.ClientId, out disconnectedClient))
            {
                foreach (var item in disconnectedClient.Values)
                {
                    OnClientDisconnected(item);
                }
            }
        }

        /// <summary>
        /// Handles MessageReceived events of all clients, evaluates each message,
        /// finds appropriate service object and invokes appropriate method.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void RMILayerClient_MessageReceived(object sender, MessageEventArgs e)
        {
            //Get RequestReplyMessenger object (sender of event) to get client
            var requestReplyMessenger = (RMIRequestReplyMessenger<IScsServerClient>)sender;

            //Cast message to ScsRemoteInvokeMessage and check it
            var invokeMessage = e.Message as ScsRemoteInvokeMessage;
            if (invokeMessage == null)
            {
                return;
            }

            try
            {
                //Get client object
                ConcurrentDictionary<string, IScsServiceClientStub> clients;
                IScsServiceClientStub client;
                if (!_serviceClients.TryGetValue(requestReplyMessenger.Messenger.ClientId, out clients)
                    || !clients.TryGetValue(invokeMessage.ServiceName, out client))
                {
                    requestReplyMessenger.Messenger.Disconnect();
                    return;
                }

                //Get service object
                ServiceObject serviceObject;
                if (!_serviceObjects.TryGetValue(invokeMessage.ServiceName, out serviceObject))
                {
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException("There is no service with name '" + invokeMessage.ServiceName + "'"));
                    return;
                }

                //Invoke method
                try
                {
                    object returnValue;
                    object[] outputParameters;
                    int[] outArgumentIndices;
                    bool[] outArgumentRefFlags;
                    //Set client to service, so user service can get client
                    //in service method using CurrentClient property.
                    ServiceHelper.CurrentClient = client;
                    try
                    {
                        returnValue = serviceObject.InvokeMethod(invokeMessage.MethodName, invokeMessage.ParameterTypeNames ?? new string[0], out outputParameters, out outArgumentIndices, out outArgumentRefFlags, invokeMessage.Parameters != null ? invokeMessage.Parameters.Select(ArbitraryObject.GetValue).ToArray() : null);
                    }
                    finally
                    {
                        //Set CurrentClient as null since method call completed
                        ServiceHelper.CurrentClient = null;
                    }

                    //Send method invocation return value to the client
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, returnValue, null, outputParameters, outArgumentIndices, outArgumentRefFlags);
                }
                catch (TargetInvocationException ex)
                {
                    var innerEx = ex.InnerException;
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException(innerEx.Message + Environment.NewLine + "Service Version: " + serviceObject.ServiceAttribute.Version, innerEx));
                    return;
                }
                catch (Exception ex)
                {
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException(ex.Message + Environment.NewLine + "Service Version: " + serviceObject.ServiceAttribute.Version, ex));
                    return;
                }
            }
            catch (Exception ex)
            {
                SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException("An error occured during remote service method call.", ex));
                return;
            }
        }

        /// <summary>
        /// Sends response to the remote application that invoked a service method.
        /// </summary>
        /// <param name="client">Client that sent invoke message</param>
        /// <param name="requestMessage">Request message</param>
        /// <param name="returnValue">Return value to send</param>
        /// <param name="exception">Exception to send</param>
        /// <param name="outputParameters">Output paramter values to sentd</param>
        /// <param name="outArgumentIndices">Indices of output arguments</param>
        /// <param name="outArgumentRefFlags">Indicates whether an OutArgument is a Ref Argument</param>
        private static void SendInvokeResponse(IMessenger client, ScsRemoteInvokeMessage requestMessage, object returnValue, ScsRemoteException exception, object[] outputParameters = null, int[] outArgumentIndices = null, bool[] outArgumentRefFlags = null)
        {
            try
            {
                client.SendMessage(
                    new ScsRemoteInvokeReturnMessage
                        {
                            RepliedMessageId = requestMessage.MessageId,
                            ReturnValue = ArbitraryObject.CreateArbitraryObject(returnValue),
                            RemoteException = exception,
                            OutArguments = Array.ConvertAll((outputParameters ?? new object[0]), ArbitraryObject.CreateArbitraryObject),
                            ServiceName = requestMessage.ServiceName,
                            OutArgumentIndices = outArgumentIndices,
                            OutArgumentRefFlags = outArgumentRefFlags,
                        });
            }
            catch
            {

            }
        }

        /// <summary>
        /// Raises ClientConnected event.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientConnected(IScsServiceClientStub client)
        {
            var handler = ServiceRegisteredByClient;
            if (handler != null)
            {
                handler(this, new ServiceClientEventArgs(client));
            }
        }

        /// <summary>
        /// Raises ClientDisconnected event.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientDisconnected(IScsServiceClientStub client)
        {
            var handler = ServiceUnregisteredByClient;
            if (handler != null)
            {
                handler(this, new ServiceClientEventArgs(client));
            }
        }

        private static TServiceClass CreateDefaultService<TServiceInterface, TServiceClass>()
            where TServiceClass : TServiceInterface, new()
            where TServiceInterface : class
        {
            return new TServiceClass();
        }
        #endregion

        #region ServiceObject class
        internal sealed class MethodInfoWrapper
        {
            public MethodInfo MethodInfo { get; set; }
            public ParameterInfo[] Parameters { get; set; }
            /// <summary>
            /// Indices of output or byref parameter
            /// </summary>
            public int[] OutputParamterIndices { get; set; }

            /// <summary>
            /// Indicates whether an OutArgument is a Ref Argument
            /// </summary>
            public bool[] OutArgumentRefFlags{ get; set; }
        }

        private abstract class ServiceObject
        {
            public abstract object Service { get; }

            /// <summary>
            /// ScsService attribute of Service object's class.
            /// </summary>
            public ScsServiceAttribute ServiceAttribute { get; protected set; }

            /// <summary>
            /// To be implemented by inherited classes to invoke a method of Service object.
            /// </summary>
            /// <param name="methodName">Name of the method to invoke</param>
            /// <param name="parameterTypeNames">The assembly qualified name of parameter types</param>
            /// <param name="outputParameters">Output parameters of method</param>
            /// <param name="outArgumentIndices">Output parameter indices</param>
            /// <param name="parameters">Parameters of method</param>
            /// <param name="outArgumentRefFlags">Indicates whether an OutArgument is a Ref Argument</param>
            /// <returns>Return value of method</returns>
            public abstract object InvokeMethod(string methodName, string[] parameterTypeNames, out object[] outputParameters, out int[] outArgumentIndices, out bool[] outArgumentRefFlags, params object[] parameters);
        }

        /// <summary>
        /// Represents a user service object.
        /// It is used to invoke methods on a ScsService object.
        /// </summary>
        private sealed class ServiceObject<TServiceInterface, TServiceClass> : ServiceObject
            where TServiceClass : TServiceInterface
            where TServiceInterface : class
        {
            /// <summary>
            /// The service object that is used to invoke methods on.
            /// </summary>
            public override object Service
            {
                get
                {
                    if (ServiceAttribute.WellKnownObjectMode == WellKnownObjectMode.Singleton)
                    {
                        if (_serviceObject == null)
                        {
                            _serviceObject = _serviceActivator();
                        }
                        return _serviceObject;
                    }
                    else
                    {
                        return _serviceActivator();
                    }
                }
            }

            /// <summary>
            /// This collection stores a list of all public instance methods of service object.
            /// Key: Method name
            /// Value: Informations about method. 
            /// </summary>
            private ILookup<string, MethodInfoWrapper> _methods;

            /// <summary>
            /// This collcection stores a list of all public instance properties of service object;
            /// Key: Property name
            /// Value: Reference to the PropertyInfo object.
            /// </summary>
            private Dictionary<string, PropertyInfo> _properties;

            /// <summary>
            /// The service object that is used to invoke methods on.
            /// </summary>
            private TServiceClass _serviceObject;

            /// <summary>
            /// The delegate used to activate new service objects.
            /// </summary>
            private Func<TServiceClass> _serviceActivator;


            private ServiceObject() { }


            private static ServiceObject<TServiceInterface, TServiceClass> CreateInstanceInternal()
            {
                ServiceObject<TServiceInterface, TServiceClass> instance = new ServiceObject<TServiceInterface, TServiceClass>();
                Type serviceInterfaceType = typeof(TServiceInterface);
                var classAttribute = serviceInterfaceType.GetCustomAttributes(typeof(ScsServiceAttribute), true).FirstOrDefault();
                if (classAttribute == null)
                {
                    throw new Exception("Service interface (" + serviceInterfaceType.Name + ") must has ScsService attribute.");
                }

                instance.ServiceAttribute = (ScsServiceAttribute)classAttribute;
                instance._methods = serviceInterfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(a =>
                {
                    var methodName = a.Name;
                    var parameters = a.GetParameters();
                    var methodInfoWrapper = new MethodInfoWrapper { MethodInfo = a, Parameters = parameters };

                    var outputParamterIndices = new List<int>();
                    var outArgumentRefFlags = new List<bool>();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.IsByRef)
                        {
                            bool isRef = !parameters[i].IsOut;
                            outputParamterIndices.Add(i);
                            outArgumentRefFlags.Add(isRef);
                        }
                    }
                    if (outputParamterIndices.Count > 0)
                    {
                        methodInfoWrapper.OutputParamterIndices = outputParamterIndices.ToArray();
                        methodInfoWrapper.OutArgumentRefFlags = outArgumentRefFlags.ToArray();
                    }
                    return methodInfoWrapper;
                }).ToLookup(a => a.MethodInfo.Name);


                instance._properties = serviceInterfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(a => a.Name);
                return instance;
            }

            /// <summary>
            /// Creates a new ServiceObject.
            /// </summary>
            /// <param name="service">An instance of TServiceClass.</param>
            /// <returns>The new ServiceObject instance.</returns>
            /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
            public static ServiceObject<TServiceInterface, TServiceClass> CreateInstance(TServiceClass service)
            {
                var instance = CreateInstanceInternal();
                if (instance.ServiceAttribute.WellKnownObjectMode == WellKnownObjectMode.SingleCall)
                {
                    throw new ArgumentException(string.Format("Service type {0} is marked as WellKnownObjectMode.SingleCall, cannot be initialized via an existing object.", typeof(TServiceInterface)), "TServiceInterface");
                }
                instance._serviceObject = service;
                return instance;
            }

            /// <summary>
            /// Creates a new ServiceObject.
            /// </summary>
            /// <param name="activator">The delegate used to activate new service objects.</param>
            /// <returns>The new ServiceObject instance.</returns>
            /// <exception cref="ArgumentNullException">Throws ArgumentNullException if activator argument is null</exception>
            public static ServiceObject<TServiceInterface, TServiceClass> CreateInstance(Func<TServiceClass> activator)
            {
                if (activator == null)
                {
                    throw new ArgumentNullException("activator");
                }
                var instance = CreateInstanceInternal();
                instance._serviceActivator = activator;
                return instance;
            }



            /// <summary>
            /// To be implemented by inherited classes to invoke a method of Service object.
            /// </summary>
            /// <param name="methodName">Name of the method to invoke</param>
            /// <param name="parameterTypeNames">The assembly qualified name of parameter types</param>
            /// <param name="outputParameters">Output parameters of method</param>
            /// <param name="outArgumentIndices">Output parameter indices</param>
            /// <param name="parameters">Parameters of method</param>
            /// <param name="outArgumentRefFlags">Indicates whether an OutArgument is a Ref Argument</param>
            /// <returns>Return value of method</returns>
            public override object InvokeMethod(string methodName, string[] parameterTypeNames, out object[] outputParameters, out int[] outArgumentIndices, out bool[] outArgumentRefFlags, params object[] parameters)
            {
                //Check if there is a method with name methodName
                //if (!_methods.ContainsKey(methodName))
                if (!_methods.Contains(methodName))
                {
                    throw new Exception("There is not a method with name '" + methodName + "' in service class.");
                }

                //Check if it is a property call


                var overloadedMethods = _methods[methodName].ToList();//根据方法名找到的重载方法列表
                MethodInfoWrapper methodInfoWrapper = _methods[methodName].FirstOrDefault(a => a.Parameters.Select(b => TypeNameConverter.Default.ConvertToTypeName(b.ParameterType)).SequenceEqual(parameterTypeNames));
                if (methodInfoWrapper == null)
                {
                    if (!_methods.Contains(methodName))
                    {
                        throw new Exception("There is not a method with name '" + methodName + "' complying with the supplied parameter type names in service class.");
                    }
                }

                //Invoke method and return invoke result
                var returnValue = methodInfoWrapper.MethodInfo.Invoke(Service, parameters);
                if (methodInfoWrapper.OutputParamterIndices != null && methodInfoWrapper.OutputParamterIndices.Length > 0)
                {
                    outArgumentIndices = methodInfoWrapper.OutputParamterIndices;
                    outArgumentRefFlags = methodInfoWrapper.OutArgumentRefFlags;
                    outputParameters = new object[methodInfoWrapper.OutputParamterIndices.Length];
                    for (int i = 0; i < outputParameters.Length; i++)
                    {
                        outputParameters[i] = parameters[methodInfoWrapper.OutputParamterIndices[i]];
                    }
                }
                else
                {
                    outputParameters = null;
                    outArgumentIndices = null;
                    outArgumentRefFlags = null;
                }
                return returnValue;
            }


            /// <summary>
            /// Check whether paramter value list matches ParameterInfo list.
            /// </summary>
            /// <param name="parameterInfos">ParameterInfo list to match.</param>
            /// <param name="parametersValues">paramter value list to check.</param>
            /// <returns>check result</returns>
            private bool WhetherParametersMatch(ParameterInfo[] parameterInfos, object[] parametersValues)
            {
                bool parameterInfosIsNullOrEmpty = (object.ReferenceEquals(parameterInfos, null) || parameterInfos.Length == 0);
                bool parametersValuesIsNullOrEmpty = (object.ReferenceEquals(parametersValues, null) || parametersValues.Length == 0);
                if (parameterInfosIsNullOrEmpty != parametersValuesIsNullOrEmpty)
                {
                    return false;
                }
                else
                {
                    if (!parameterInfosIsNullOrEmpty)//parameterInfosIsNullOrEmpty && parametersValuesIsNullOrEmpty
                    {
                        return true;
                    }
                    else
                    {
                        if (parameterInfos.Length != parametersValues.Length)
                        {
                            return false;
                        }
                        else
                        {
                            for (int i = 0; i < parameterInfos.Length; i++)
                            {
                                if (parametersValues[i] != null)
                                {
                                    if (!parameterInfos[i].ParameterType.IsInstanceOfType(parametersValues[i]))
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (!parameterInfos[i].ParameterType.IsNullable())
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
