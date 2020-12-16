using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 使用位移计算，因为除法运算的底层实现是比较耗性能的
    /// |         Method | Value |         N |     Mean | Error |   StdDev | Ratio | RatioSD | Rank |
    /// |--------------- |------ |---------- |---------:|------:|---------:|------:|--------:|-----:|
    /// | ToStringSimple |     1 | 100000000 | 219.4 ms |    NA | 25.69 ms |  1.00 |    0.00 |    1 |
    /// |   ToStringFast |     1 | 100000000 | 228.7 ms |    NA |  9.74 ms |  1.05 |    0.08 |    2 |
    /// |                |       |           |          |       |          |       |         |      |
    /// | ToStringSimple |    11 | 100000000 | 317.4 ms |    NA | 97.85 ms |  1.00 |    0.00 |    2 |
    /// |   ToStringFast |    11 | 100000000 | 271.1 ms |    NA | 68.42 ms |  0.86 |    0.05 |    1 |
    /// |                |       |           |          |       |          |       |         |      |
    /// | ToStringSimple |   123 | 100000000 | 480.2 ms |    NA | 80.37 ms |  1.00 |    0.00 |    2 |
    /// |   ToStringFast |   123 | 100000000 | 329.0 ms |    NA | 69.60 ms |  0.68 |    0.03 |    1 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 2)]
    [RPlotExporter, RankColumn]
    public class UseShift
    {
        [Params(1,11,123)]
        public byte Value;

        [Params(100_000_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            
        }

        [Benchmark(Baseline = true)]
        public unsafe void ToStringSimple()
        {
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                ToStringSimple(buffer, Value);
        }

        [Benchmark]
        public unsafe void ToStringFast()
        {
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                ToStringFast(buffer, Value);
        }

        [Benchmark]
        public unsafe void ToStringSimple3()
        {
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                ToString3Length(buffer, Value);
        }

        [Benchmark]
        public unsafe void ToStringFast3()
        {
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                ToStringFast3Length(buffer, Value);
        }

        /// <summary>
        /// 简单的除法实现，除法底层实现是比较耗性能的
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public unsafe static int ToStringSimple(byte* buffer, byte value)
        {
            if (value < 10)
            {
                *buffer = (byte)(value + (byte)'0');
                return 1;
            }
            else if (value < 100)
            {
                *buffer = (byte)((value / 10) + (byte)'0');
                *(buffer + 1) = (byte)((value % 10) + (byte)'0');
                return 2;
            }
            else
            {
                *buffer = (byte)((value / 100) + (byte)'0');
                *(buffer + 1) = (byte)(((value % 100) / 10) + (byte)'0');
                *(buffer + 2) = (byte)((value % 10) + (byte)'0');
                return 3;
            }
        }

        public unsafe static int ToString3Length(byte* buffer, byte value)
        {
            *buffer = (byte)((value / 100) + (byte)'0');
            *(buffer + 1) = (byte)(((value % 100) / 10) + (byte)'0');
            *(buffer + 2) = (byte)((value % 10) + (byte)'0');
            return 3;
        }

        /// <summary>
        /// 使用乘法加位移实现
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public unsafe static int ToStringFast(byte* buffer, byte value)
        {
            if (value < 10)
            {
                *buffer = (byte)(value + (byte)'0');
                return 1;
            }
            else if (value < 100)
            {
                var tens = (byte)((value * 205u) >> 11); // div10, valid to 1028
                *buffer = (byte)(tens + (byte)'0');
                *(buffer + 1) = (byte)(value - (tens * 10) + (byte)'0');
                return 2;
            }
            else
            {
                var digit0 = (byte)((value * 41u) >> 12); // div100, valid to 1098
                var digits01 = (byte)((value * 205u) >> 11); // div10, valid to 1028
                *buffer = (byte)(digit0 + (byte)'0');
                *(buffer + 1) = (byte)(digits01 - (digit0 * 10) + (byte)'0');
                *(buffer + 2) = (byte)(value - (digits01 * 10) + (byte)'0');
                return 3;
            }
        }

        public unsafe static int ToStringFast3Length(byte* buffer, byte value)
        {
            var digit0 = (byte)((value * 41u) >> 12);
            var digits01 = (byte)((value * 205u) >> 11);
            *buffer = (byte)(digit0 + (byte)'0');
            *(buffer + 1) = (byte)(digits01 - (digit0 * 10) + (byte)'0');
            *(buffer + 2) = (byte)(value - (digits01 * 10) + (byte)'0');
            return 3;
        }

    }
}
