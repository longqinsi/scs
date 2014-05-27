using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.ScsServices.Service;
using PhoneBookCommonLib;

namespace PhoneBookServer
{
    class CalculatorService : ICalculatorService
    {
        public void Add(ref double a, double b)
        {
            Console.WriteLine("double:{0}+{1}={2}", a, b, a += b);
        }

        public double Subtract(double a, double b)
        {
            double result = a - b;
            Console.WriteLine("double:{0}-{1}={2}", a, b, result);
            return result;
        }

        public double Multiply(double a, double b)
        {
            double result = a * b;
            Console.WriteLine("double:{0}*{1}={2}", a, b, result);
            return result;
        }

        public double Devide(double a, double b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }
            double result = a / b;
            var message = string.Format("double:{0}/{1}={2}", a, b, result);
            Console.WriteLine(message);
            if (ServiceHelper.CurrentClient != null)
            {
                ServiceHelper.CurrentClient.ServerClient.SendMessage(new ScsTextMessage(message));
            }
            return result;
        }

        public int Add(int a, int b)
        {
            int result = a + b;
            Console.WriteLine("int:{0}+{1}={2}", a, b, result);
            return result;
        }

        public int Subtract(int a, int b)
        {
            int result = a - b;
            Console.WriteLine("int:{0}-{1}={2}", a, b, result);
            return result;
        }

        public int Multiply(int a, int b)
        {
            int result = a * b;
            Console.WriteLine("int:{0}*{1}={2}", a, b, result);
            return result;
        }

        public int Devide(int a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }
            int result = a / b;
            Console.WriteLine("int:{0}/{1}={2}", a, b, result);
            return result;
        }


        public void Add(out double result, out string exp, params double[] array)
        {
            result = 0;
            if (array == null || array.Length == 0)
            {
                exp = "double:0";
                return;
            }
            else
            {
                exp = "double:" + array[0].ToString();
                result = array[0];
            }
            for (int i = 1; i < array.Length; i++)
            {
                exp += " + " + array[i];
                result += array[i];
            }
            exp += " = " + result;
            Console.WriteLine(exp);
        }

        public double Log(double a, double newBase = Math.E)
        {
            return Math.Log(a, newBase);
        }
    }
}
