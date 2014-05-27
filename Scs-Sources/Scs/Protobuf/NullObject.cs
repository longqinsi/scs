using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Hik.Protobuf
{
    [ProtoContract(SkipConstructor = true)]
    internal class NullObject : ArbitraryObject
    {
        protected override object GetValue()
        {
            return null;
        }

        private NullObject()
        {

        }

        public static ArbitraryObject Create(object value)
        {
            return new NullObject();
        }


        protected override void MergeObject(ref object value)
        {
            value = null;
        }
    }
}
