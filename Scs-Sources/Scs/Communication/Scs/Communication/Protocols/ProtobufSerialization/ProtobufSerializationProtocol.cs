using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Hik.Collections;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols.BinarySerialization;
using Hik.Communication.ScsServices.Communication.Messages;
using ProtoBuf;
using ProtoBuf.Meta;


namespace Hik.Communication.Scs.Communication.Protocols.ProtobufSerialization
{
    /// <summary>
    /// One of the communication protocols between server and clients to send and receive a message.
    /// It uses json.net to serialize and deserialize messages, bzip2 to compress and decompress serialized messages.
    /// The tranformed contents on network are compressed.
    /// 
    /// Since BinarySerializationProtocol automatically writes message length to the beggining
    /// of the message, a message format of this class is:
    /// 
    /// [Message length (4 bytes)][ProtobufNet content (N bytes)]
    /// So, total length of the message = (N + 4) bytes;
    /// 
    /// </summary>
    public class ProtobufSerializationProtocol : BinarySerializationProtocol
    {
        /// <summary>运行初始化程序</summary>
        static ProtobufSerializationProtocol()
        {
            //为无法直接使用Protobuf-Net序列化的类注册序列化代理类
            RuntimeTypeModel.Default.Add(typeof(ScsRemoteException), false).SetSurrogate(typeof(ScsRemoteExceptionSurrogate));
            RuntimeTypeModel.Default.Add(typeof(CommunicationException), false).SetSurrogate(typeof(CommunicationExceptionSurrogate));

            //在Protobuf-Net中注册ScsMessage的内部子类
            Hik.Communication.Scs.Communication.Messages.ScsMessage.RegisterInternalSubtypesInProtobufNet();
            //初始化针对Protobuf-Net的类型缓存
            Hik.Collections.TypeCache.Initialize();
        }

        private static readonly ConcurrentDictionary<string, Type> typeCache = new ConcurrentDictionary<string, Type>();
        private static RuntimeTypeModel model = RuntimeTypeModel.Default;
        /// <summary>
        /// This method is used to serialize a IScsMessage to a byte array.
        /// This method can be overrided by derived classes to change serialization strategy.
        /// It is a couple with DeserializeMessage method and must be overrided together.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        /// <returns>
        /// Serialized message bytes.
        /// Does not include length of the message.
        /// </returns>
        protected override byte[] SerializeMessage(IScsMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                var messageType = message.GetType();
                model.SerializeWithLengthPrefix(ms, TypeCache.Singleton.GetIDByType(messageType), typeof(int), PrefixStyle.Fixed32, 0);
                model.SerializeWithLengthPrefix(ms, message, messageType, PrefixStyle.Fixed32, 0);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// This method is used to deserialize a IScsMessage from it's bytes.
        /// This method can be overrided by derived classes to change deserialization strategy.
        /// It is a couple with SerializeMessage method and must be overrided together.
        /// </summary>
        /// <param name="bytes">
        /// Bytes of message to be deserialized (does not include message length. It consist
        /// of a single whole message)
        /// </param>
        /// <returns>Deserialized message</returns>
        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                var id = (int)model.DeserializeWithLengthPrefix(ms, null, typeof(int), PrefixStyle.Fixed32, 0);
                var messageType = TypeCache.Singleton.GetTypeByID(id);
                return model.DeserializeWithLengthPrefix(ms, null, messageType, PrefixStyle.Fixed32, 0) as IScsMessage;
            }
        }
    }
}
