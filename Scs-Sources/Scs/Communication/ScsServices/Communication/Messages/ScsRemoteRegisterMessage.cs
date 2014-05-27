using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using ProtoBuf;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent to register a scs service.
    /// </summary>
    [Serializable]
    [ProtoContract(SkipConstructor=true)]
    public class ScsRemoteRegisterMessage : ScsMessage, IScsServiceMessage
    {
        /// <summary>
        /// Name of the remote service.
        /// </summary>
        [ProtoMember(1)]
        private string serviceName;

        /// <summary>
        /// RMI Service Name.
        /// </summary>
        public string ServiceName { get { return serviceName; } }
        internal ScsRemoteRegisterMessage(string serviceName)
        {
            this.serviceName = serviceName;
        }

        internal ScsRemoteRegisterMessage(string serviceName, Guid repliedMessageID)
        {
            this.serviceName = serviceName;
            this.RepliedMessageId = repliedMessageID;
        }

    }
}
