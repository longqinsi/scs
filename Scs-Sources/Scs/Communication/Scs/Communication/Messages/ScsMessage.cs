using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Hik.Collections;
using Hik.Communication.ScsServices.Communication.Messages;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Hik.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// Represents a message that is sent and received by server and client.
    /// This is the base class for all messages.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class ScsMessage : IScsMessage
    {
        /// <summary>
        /// 为了防止子类重复注册而设置的一对一映射集合
        /// 键：消息子类类型
        /// 值：消息子类索引号
        /// </summary>
        private static readonly BiDictionary<Type, int> registeredSubtypes = new BiDictionary<Type, int>();
        private static MetaType thisMetaType;
        /// <summary>供内部使用的子类号范围从100-199</summary>
        private static int internalSubTypeField = 100;
        private static object _SyncRoot = new object();
        /// <summary>
        /// Unique identified for this message.
        /// Default value: New GUID.
        /// Do not change if you do not want to do low level changes
        /// such as custom wire protocols.
        /// </summary>
        [ProtoMember(1)]
        public Guid MessageId { get; set; }

        /// <summary>
        /// This property is used to indicate that this is
        /// a Reply message to a message.
        /// It may be null if this is not a reply message.
        /// </summary>
        [ProtoMember(2)]
        public Guid RepliedMessageId { get; set; }


        /// <summary>在Protobuf-Net中注册内部子类</summary>
        internal static void RegisterInternalSubtypesInProtobufNet()
        {
            thisMetaType = RuntimeTypeModel.Default.Add(typeof(ScsMessage), true);
            #region 注意：下面这段代码的顺序千万不能改，否则会出错
            AddInternalSubMessageType<ScsPingMessage>();
            AddInternalSubMessageType<ScsTextMessage>();
            AddInternalSubMessageType<ScsRawDataMessage>();
            AddInternalSubMessageType<ScsRemoteInvokeMessage>();
            AddInternalSubMessageType<ScsRemoteInvokeReturnMessage>();
            AddInternalSubMessageType<ScsRemoteRegisterMessage>();
            AddInternalSubMessageType<ScsRemoteUnregisterMessage>();
            #endregion
        }

        /// <summary>将ScsMessage与本程序集内某个子类的继承关系注册到Protobuf-Net中</summary>
        /// <typeparam name="T">需注册子类</typeparam>
        private static void AddInternalSubMessageType<T>() where T : ScsMessage
        {
            lock (_SyncRoot)
            {
                var type = typeof(T);
                registeredSubtypes.Add(type, internalSubTypeField);
                thisMetaType.AddSubType(internalSubTypeField, type);
                internalSubTypeField++;
            }
        }

        /// <summary>
        /// Creates a new ScsMessage.
        /// </summary>
        public ScsMessage()
        {
            MessageId = Guid.NewGuid();
            RepliedMessageId = Guid.Empty;
        }

        /// <summary>
        /// Creates a new reply ScsMessage.
        /// </summary>
        /// <param name="repliedMessageId">
        /// Replied message id if this is a reply for
        /// a message.
        /// </param>
        public ScsMessage(Guid repliedMessageId)
        //: this()
        {
            MessageId = Guid.NewGuid();
            RepliedMessageId = repliedMessageId;
        }

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return RepliedMessageId == Guid.Empty
                       ? string.Format("ScsMessage [{0}]", MessageId)
                       : string.Format("ScsMessage [{0}] Replied To [{1}]", MessageId, RepliedMessageId);
        }

        /// <summary>将ScsMessage与某个子类的继承关系注册到Protobuf-Net中</summary>
        /// <typeparam name="T">需注册子类</typeparam>
        /// <param name="index">消息类型索引号，必须大于或等于200，并且不能与已有索引号重复</param>
        public static void AddSubMessageType<T>(int index) where T : ScsMessage
        {
            AddSubMessageType(typeof(T), index);
        }

        /// <summary>将ScsMessage与某个子类的继承关系注册到Protobuf-Net中</summary>
        /// <param name="subMessageType">需注册子类</param>
        /// <param name="index">消息类型索引号，必须大于或等于200，并且不能与已有索引号重复</param>
        public static void AddSubMessageType(Type subMessageType, int index)
        {
            if (index < 200)
            {
                throw new ArgumentOutOfRangeException("index", "索引号不能小于200.");
            }
            lock (_SyncRoot)
            {
                if (registeredSubtypes.ContainsKey(subMessageType))
                {
                    throw new ArgumentException(string.Format("消息类型{0}已注册，不能重复注册。", subMessageType.AssemblyQualifiedName), "subMessageType");
                }
                if (registeredSubtypes.ContainsValue(index))
                {
                    throw new ArgumentException(string.Format("消息类型索引号{0}已注册给消息类型{1}，不能重复注册。", index, registeredSubtypes.GetKey(index).AssemblyQualifiedName), "index");
                }
                thisMetaType.AddSubType(index, subMessageType);
                registeredSubtypes.Add(subMessageType, index);
            }
        }
    }
}
