using System.Threading;

namespace Hik.Communication.ScsServices.Client
{
    /// <summary>
    /// Provides some functionality that are used by clients.
    /// </summary>
    internal static class ScsServiceClientManager
    {
        /// <summary>
        /// Used to set an auto incremential unique identifier to servers.
        /// </summary>
        private static long _lastServerId;

        /// <summary>
        /// Gets an unique number to be used as idenfitier of a server.
        /// </summary>
        /// <returns></returns>
        public static long GetServerId()
        {
            return Interlocked.Increment(ref _lastServerId);
        }
    }
}
