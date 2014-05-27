namespace Hik.Communication.Scs.Communication.Protocols.ProtobufSerialization
{
    /// <summary>
    /// This class is used to create Binary Serialization Protocol objects.
    /// </summary>
    public class ProtobufSerializationProtocolFactory : IScsWireProtocolFactory
    {
        /// <summary>
        /// Creates a new Wire Protocol object.
        /// </summary>
        /// <returns>Newly created wire protocol object</returns>
        public IScsWireProtocol CreateWireProtocol()
        {
            return new ProtobufSerializationProtocol();
        }
    }
}
