using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using ProtoBuf;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent to unregister a scs service.
    /// </summary>
    [Serializable]
    [ProtoContract(SkipConstructor=true)]
    public class ScsRemoteUnregisterMessage : ScsMessage, IScsServiceMessage
    {
        /// <summary>
        /// RMI Service Name.
        /// </summary>
        [ProtoMember(1)]
        private string serviceName;

        /// <summary>
        /// Get RMI Service Name.
        /// </summary>
        public string ServiceName { get { return serviceName; } }
        internal ScsRemoteUnregisterMessage(string serviceName)
        {
            this.serviceName = serviceName;
        }

        internal ScsRemoteUnregisterMessage(string serviceName, Guid repliedMessageID)
        {
            this.serviceName = serviceName;
            this.RepliedMessageId = repliedMessageID;
        }

    }
}
