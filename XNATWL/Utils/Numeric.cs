using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public interface INumeric<T>
    {
        T Zero { get; }
        T One { get; }
        T MaxValue { get; }
        T MinValue { get; }
        T Add(T a, T b);
        // T Substract(....
        // T Mult...
    }

    public struct Numeric :
        INumeric<int>,
        INumeric<float>,
        INumeric<double>,
        INumeric<long>
        // INumeric<other types>
    {
        int INumeric<int>.Zero => 0;
        int INumeric<int>.One => 1;
        int INumeric<int>.MinValue => int.MinValue;
        int INumeric<int>.MaxValue => int.MaxValue;
        int INumeric<int>.Add(int x, int y) => x + y;
        float INumeric<float>.Zero => 0;
        float INumeric<float>.One => 1;
        float INumeric<float>.MinValue => float.MinValue;
        float INumeric<float>.MaxValue => float.MaxValue;
        float INumeric<float>.Add(float x, float y) => x + y;
        double INumeric<double>.Zero => 0;
        double INumeric<double>.One => 1;
        double INumeric<double>.MinValue => double.MinValue;
        double INumeric<double>.MaxValue => double.MaxValue;
        double INumeric<double>.Add(double x, double y) => x + y;
        long INumeric<long>.Zero => 0;
        long INumeric<long>.One => 1;
        long INumeric<long>.MinValue => long.MinValue;
        long INumeric<long>.MaxValue => long.MaxValue;
        long INumeric<long>.Add(long x, long y) => x + y;

        // other implementations...
    }
}
