using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hik.Utility;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Hik.Protobuf
{
    [ProtoContract(SkipConstructor = true)]
    internal class ProtobufNetObject : ArbitraryObject
    {
        [ProtoMember(1)]
        private byte[] value;

        [ProtoMember(2)]
        private string typeName;
        private ProtobufNetObject()
        {

        }

        public static ArbitraryObject Create(object value)
        {
            Type type = value.GetType();
            var returnValue = new ProtobufNetObject();
            returnValue.typeName = TypeNameConverter.Default.ConvertToTypeName(type);
            using(MemoryStream ms = new MemoryStream())
            {
                RuntimeTypeModel.Default.Serialize(ms, value);
                returnValue.value = ms.ToArray();
            }
            return returnValue;
        }

        protected override object GetValue()
        {
            Type type = TypeNameConverter.Default.ConvertToType(typeName);
            using(MemoryStream ms = new MemoryStream(value))
            {
                return RuntimeTypeModel.Default.Deserialize(ms, null, type);
            }
        }

        protected override void MergeObject(ref object value)
        {
            Type type = TypeNameConverter.Default.ConvertToType(typeName);
            using (MemoryStream ms = new MemoryStream(this.value))
            {
                value = RuntimeTypeModel.Default.Deserialize(ms, value, type);
            }
        }

    }
}
