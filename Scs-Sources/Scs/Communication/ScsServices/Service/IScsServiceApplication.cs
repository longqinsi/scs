using System;
using Hik.Communication.Scs.Server;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    /// Represents a SCS Service Application that is used to construct and manage a SCS service.
    /// </summary>
    public interface IScsServiceApplication
    {
        /// <summary>
        /// This event is raised when a new client connected to the server.
        /// </summary>
        event EventHandler<ServerClientEventArgs> ClientConnected;

        /// <summary>
        /// This event is raised when a client disconnected from the server.
        /// </summary>
        event EventHandler<ServerClientEventArgs> ClientDisconnected;

        /// <summary>
        /// This event is raised when a new client registeres the service.
        /// </summary>
        event EventHandler<ServiceClientEventArgs> ServiceRegisteredByClient;

        /// <summary>
        /// This event is raised when a client unregisters the service.
        /// </summary>
        event EventHandler<ServiceClientEventArgs> ServiceUnregisteredByClient;

        /// <summary>
        /// Get the underlying IScsServer object whic is used by the ScsServiceApplication instance to accept and manage client connections.
        /// </summary>
        IScsServer ScsServer { get; }


        /// <summary>
        /// Starts service application.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops service application.
        /// </summary>
        void Stop();

        /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <param name="service">An instance of TServiceClass.</param>
        void AddService<TServiceInterface, TServiceClass>(TServiceClass service)
            where TServiceClass : TServiceInterface
            where TServiceInterface : class;

                /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        void AddService<TServiceInterface, TServiceClass>()
            where TServiceClass : TServiceInterface, new()
            where TServiceInterface : class;

        /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <param name="activator">The delegate used to activate new service objects</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        void AddService<TServiceInterface, TServiceClass>(Func<TServiceClass> activator)
            where TServiceClass : TServiceInterface
            where TServiceInterface : class;

        /// <summary>
        /// Removes a previously added service object from this service application.
        /// It removes object according to interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <returns>True: removed. False: no service object with this interface</returns>
        bool RemoveService<TServiceInterface>() where TServiceInterface : class;
    }
}
