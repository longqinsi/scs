using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using Hik.Communication.ScsServices.Service;

namespace PhoneBookCommonLib
{
    [ScsService(Version = "1.0.0.0", WellKnownObjectMode = WellKnownObjectMode.Singleton)]
    public interface ISingletonService
    {
        int Increment();
        int Count { get; }
    }
}
