using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Utility;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Hik.Protobuf
{
    /// <summary>Used to support serializing object to protobuf</summary>
    [ProtoContract]
    [ProtoInclude(101, typeof(NullObject))]
    [ProtoInclude(102, typeof(ProtobufNetObject))]
    [ProtoInclude(103, typeof(MultiDimensionalArrayObject))]
    internal abstract class ArbitraryObject
    {
        private static RuntimeTypeModel model = RuntimeTypeModel.Default;

        protected abstract object GetValue();

        protected abstract void MergeObject(ref object value);

        public static object GetValue(ArbitraryObject obj)
        {
            if (obj == null)
            {
                return null;
            }
            return obj.GetValue();
        }

        public static void MergeObject(ArbitraryObject obj, ref object value)
        {
            if (obj != null)
            {
                obj.MergeObject(ref value);
            }
        }

        public static ArbitraryObject CreateArbitraryObject(object value)
        {
            if (value == null)
            {
                return NullObject.Create(value);
            }
            Type type = value.GetType();
            if (model.CanSerialize(type))
            {
                return ProtobufNetObject.Create(value);
            }
            else
            {
                if (type.IsArray)//数组
                {
                    var elementType = type.GetElementType();
                    if (model.CanSerialize(elementType))
                    {
                        return MultiDimensionalArrayObject.Create(value);
                    }
                    else
                    {
                        throw new NotSupportedException("The value is an array, but its element type cannot be serialized by protobuf-net. You may add ProtoContractAttribute to the element type or set a surrogate type for the element type. Element type name: " + TypeNameConverter.Default.ConvertToTypeName(elementType));
                    }
                }
                else
                {
                    throw new NotSupportedException("The type of value cannot be serialized by protobuf-net. You may add ProtoContractAttribute to the ype or set a surrogate type for the type. Type name: " + TypeNameConverter.Default.ConvertToTypeName(type));

                }
            }
        }
    }
}
