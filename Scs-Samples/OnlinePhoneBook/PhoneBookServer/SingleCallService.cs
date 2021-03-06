﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hik.Communication.ScsServices.Service;
using PhoneBookCommonLib;

namespace PhoneBookServer
{
    class SingleCallService : ISingleCallService
    {
        private int counter = 0;
        public int Increment()
        {
            int newValue = Interlocked.Increment(ref counter);
            Console.WriteLine("SingleCallService.Increment:" + newValue);
            return newValue;
        }

        private int counter2 = 0;
        public int Count
        {
            get
            {
                int newValue = Interlocked.Increment(ref counter2);
                Console.WriteLine("SingleCallService.Count:" + newValue);
                return newValue;
            }
        }
    }
}
