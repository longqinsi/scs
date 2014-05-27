using System;
namespace Hik.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// Represents a message that is sent and received by server and client.
    /// </summary>
    public interface IScsMessage
    {
        /// <summary>
        /// Unique identified for this message. 
        /// </summary>
        Guid MessageId { get; }

        /// <summary>
        /// Unique identified for this message. 
        /// </summary>
        Guid RepliedMessageId { get; set; }
    }
}
