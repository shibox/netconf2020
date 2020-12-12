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
    /// 使用位移计算
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

        public unsafe static int ToStringSimple(byte* buffer, byte value)
        {
            if (value < 10)
            {
                *buffer = (byte)(value + 48);
                return 1;
            }
            else if (value < 100)
            {
                *buffer = (byte)((value / 10) + 48);
                *(buffer + 1) = (byte)((value % 10) + 48);
                return 2;
            }
            else
            {
                *buffer = (byte)((value / 100) + 48);
                *(buffer + 1) = (byte)(((value % 100) / 10) + 48);
                *(buffer + 2) = (byte)((value % 10) + 48);
                return 3;
            }
        }

        public unsafe static int ToStringFast(byte* buffer, byte value)
        {
            const byte AsciiDigitStart = (byte)'0';
            if (value < 10)
            {
                *buffer = (byte)(value + 48);
                return 1;
            }
            else if (value < 100)
            {
                var tens = (byte)((value * 205u) >> 11); // div10, valid to 1028
                *buffer = (byte)(tens + AsciiDigitStart);
                *(buffer + 1) = (byte)(value - (tens * 10) + AsciiDigitStart);
                return 2;
            }
            else
            {
                var digit0 = (byte)((value * 41u) >> 12); // div100, valid to 1098
                var digits01 = (byte)((value * 205u) >> 11); // div10, valid to 1028
                *buffer = (byte)(digit0 + AsciiDigitStart);
                *(buffer + 1) = (byte)(digits01 - (digit0 * 10) + AsciiDigitStart);
                *(buffer + 2) = (byte)(value - (digits01 * 10) + AsciiDigitStart);
                return 3;
            }
        }

    }
}
