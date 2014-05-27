using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Communication.Messengers;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    /// This class is used to create service client objects that is used in server-side.
    /// </summary>
    internal static class ScsServiceClientStubFactory
    {
        /// <summary>
        /// Creates a new service client object that is used in server-side.
        /// </summary>
        /// <param name="serviceName">The service name binding to.</param>
        /// <param name="serverClient">Underlying server client object</param>
        /// <param name="requestReplyMessenger">RequestReplyMessenger object to send/receive messages over serverClient</param>
        /// <returns></returns>
        public static IScsServiceClientStub CreateServiceClient(string serviceName, IScsServerClient serverClient, RMIRequestReplyMessenger<IScsServerClient> requestReplyMessenger)
        {
            return new ScsServiceClientStub(serviceName, serverClient, requestReplyMessenger);
        }
    }
}
