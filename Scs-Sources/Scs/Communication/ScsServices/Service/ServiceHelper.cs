using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hik.Communication.ScsServices.Service
{
    public static class ServiceHelper
    {
        /// <summary>
        /// The current client for a thread that called service method.
        /// </summary>
        [ThreadStatic]
        private static IScsServiceClientStub _currentClient;

        /// <summary>
        /// Gets the current client which called this service method. 
        /// </summary>
        /// <remarks>
        /// This property is thread-safe, if returns correct client when 
        /// called in a service method if the method is called by SCS system,
        /// else throws exception.
        /// </remarks>
        public static IScsServiceClientStub CurrentClient
        {
            get
            {
                if (_currentClient == null)
                {
                    throw new Exception("Client channel can not be obtained. CurrentClient property must be called by the thread which runs the service method.");
                }

                return _currentClient;
            }

            internal set
            {
                _currentClient = value;
            }
        }
    }
}
