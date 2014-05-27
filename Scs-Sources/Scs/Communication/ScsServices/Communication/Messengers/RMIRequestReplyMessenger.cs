using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.ScsServices.Communication.Messages;

namespace Hik.Communication.ScsServices.Communication.Messengers
{
    /// <summary>
    /// This class extends Hik.Communication.Scs.Communication.Messengers.RequestReplyMessenger
    /// to support RMI clients of different services on the same server use the same underlying socket.
    /// </summary>
    /// <typeparam name="TMessenger">Type of IMessenger object to use as underlying communication</typeparam>
    public class RMIRequestReplyMessenger<TMessenger> : RequestReplyMessenger<TMessenger> where TMessenger : IMessenger
    {
        /// <summary>
        /// Name of SCS RMI Service
        /// </summary>
        public string ServiceName { get; private set; }
        /// <summary>
        /// Creates a new RMIRequestReplyMessenger.
        /// </summary>
        /// <param name="messenger">IMessenger object to use as underlying communication</param>
        /// <param name="serviceName">Servcie name</param>
        public RMIRequestReplyMessenger(TMessenger messenger, string serviceName)
            : base(messenger)
        {
            this.ServiceName = serviceName;
        }

        /// <summary>
        /// Overrides base method to filter messages not belongs to the registered SCS RMI Service.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        protected override void Messenger_MessageReceived(object sender, MessageEventArgs e)
        {
            var serviceMessage = e.Message as IScsServiceMessage;
            if (serviceMessage != null && serviceMessage.ServiceName == this.ServiceName)
            {
                base.Messenger_MessageReceived(sender, e);
            }
        }
    }
}
