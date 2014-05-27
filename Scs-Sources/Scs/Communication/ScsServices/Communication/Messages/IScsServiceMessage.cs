using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// Represents a message that is sent and received by rmi service and client.
    /// </summary>
    public interface IScsServiceMessage : IScsMessage
    {
        /// <summary>
        /// RMI Service Name.
        /// </summary>
        string ServiceName { get; }
    }
}
