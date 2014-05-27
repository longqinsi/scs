using System;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Protobuf;
using ProtoBuf;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent as response message to a ScsRemoteInvokeMessage.
    /// It is used to send return value of method invocation.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class ScsRemoteInvokeReturnMessage : ScsMessage, IScsServiceMessage
    {
        /// <summary>
        /// Return value of remote method invocation.
        /// </summary>
        [ProtoMember(1)]
        internal ArbitraryObject ReturnValue { get; set; }

        /// <summary>
        /// Values of output arguments
        /// </summary>
        [ProtoMember(2)]
        internal ArbitraryObject[] OutArguments { get; set; }

        /// <summary>
        /// Indices of output arguments
        /// </summary>
        [ProtoMember(3)]
        internal int[] OutArgumentIndices { get; set; }

        /// <summary>
        /// Indicates whether an OutArgument is a Ref Argument
        /// </summary>
        [ProtoMember(4)]
        internal bool[] OutArgumentRefFlags { get; set; }

        /// <summary>
        /// If any exception occured during method invocation, this field contains Exception object.
        /// If no exception occured, this field is null.
        /// </summary>
        [ProtoMember(5)]
        internal ScsRemoteException RemoteException { get; set; }

        /// <summary>
        /// RMI Service Name.
        /// </summary>
        [ProtoMember(6)]
        public string ServiceName { get; internal set; }

        /// <summary>
        /// Represents this object as string.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return string.Format("ScsRemoteInvokeReturnMessage: Returns {0}, Exception = {1}", ArbitraryObject.GetValue(ReturnValue), RemoteException);
        }
    }
}
