using System;
using System.Runtime.Remoting;
namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    /// Any SCS Service interface class must has this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class ScsServiceAttribute : Attribute
    {
        /// <summary>
        /// Service Version. This property can be used to indicate the code version.
        /// This value is sent to client application on an exception, so, client application can know that service version is changed.
        /// Default value: NO_VERSION.
        /// </summary>
        public string Version { get; set; }

        private WellKnownObjectMode wellKnownObjectMode = WellKnownObjectMode.Singleton;

        /// <summary>
        /// Defines how well-known objects are activated.
        /// SingleCall: Every incoming message is serviced by a new object instance.
        /// Singleton:  Every incoming message is serviced by the same object instance.
        /// </summary>
        public WellKnownObjectMode WellKnownObjectMode
        {
            get { return wellKnownObjectMode; }
            set { wellKnownObjectMode = value; }
        }
        

        /// <summary>
        /// Creates a new ScsServiceAttribute object.
        /// </summary>
        public ScsServiceAttribute()
        {
            Version = "NO_VERSION";
        }
    }
}
