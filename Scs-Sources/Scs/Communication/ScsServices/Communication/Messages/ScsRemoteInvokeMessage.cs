using System;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Protobuf;
using ProtoBuf;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent to invoke a method of a remote application.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class ScsRemoteInvokeMessage : ScsMessage, IScsServiceMessage
    {
        /// <summary>
        /// Name of the remove service.
        /// </summary>
        [ProtoMember(1)]
        public string ServiceName { get; internal set; }

        /// <summary>
        /// Method of remote application to invoke.
        /// </summary>
        [ProtoMember(2)]
        internal string MethodName { get; set; }

        /// <summary>
        /// The assembly qualified name of parameter types. 
        /// </summary>
        [ProtoMember(3)]
        internal string[] ParameterTypeNames { get; set; }

        /// <summary>
        /// Parameters of method.
        /// </summary>
        [ProtoMember(4)]
        //public object[] Parameters { get; set; }
        internal ArbitraryObject[] Parameters { get; set; }


        /// <summary>
        /// Represents this object as string.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return string.Format("ScsRemoteInvokeMessage: {0}.{1}(...)", ServiceName, MethodName);
        }
    }
}
