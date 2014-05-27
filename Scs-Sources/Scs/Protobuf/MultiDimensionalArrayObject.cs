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
    internal class MultiDimensionalArrayObject : ArbitraryObject
    {
        [ProtoMember(1)]
        private byte[] value;//Protobuf-Net serialized flattened array.

        [ProtoMember(2)]
        private int[] lengths;//the lengths of all dimensions

        [ProtoMember(3)]
        private string flattenedArrayTypeName;//the assembly qualified name of the type of the flattened array;

        private MultiDimensionalArrayObject()
        {

        }
        public static ArbitraryObject Create(object value)
        {
            var array = (Array)value;
            MultiDimensionalArrayObject returnValue = new MultiDimensionalArrayObject();
            var flattendArray = CommonMethods.Flatten(array, out returnValue.lengths);
            returnValue.flattenedArrayTypeName = TypeNameConverter.Default.ConvertToTypeName(flattendArray.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                RuntimeTypeModel.Default.Serialize(ms, flattendArray);
                returnValue.value = ms.ToArray();
            }
            return returnValue;
        }

        protected override object GetValue()
        {
            Type flattenedArrayType = TypeNameConverter.Default.ConvertToType(flattenedArrayTypeName);
            Array flattendArray;
            using(MemoryStream ms = new MemoryStream(value))
            {
                flattendArray = RuntimeTypeModel.Default.Deserialize(ms, null, flattenedArrayType) as Array;
            }
            return CommonMethods.Recover(flattendArray, lengths);
        }

        protected override void MergeObject(ref object value)
        {
            value = GetValue();
        }
    }
}
