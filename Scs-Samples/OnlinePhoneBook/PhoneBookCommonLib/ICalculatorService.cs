using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.ScsServices.Service;

namespace PhoneBookCommonLib
{
    [ScsService(Version = "1.0.0.0")]
    public interface ICalculatorService
    {
        //double Add(double a, double b);
        double Subtract(double a, double b);
        double Multiply(double a, double b);
        double Devide(double a, double b);
        int Add(int a, int b);
        int Subtract(int a, int b);
        int Multiply(int a, int b);
        int Devide(int a, int b);
        void Add(out double result, out string exp, params double[] array);

        void Add(ref double a, double b);

        double Log(double a, double newBase = Math.E);
    }
}
