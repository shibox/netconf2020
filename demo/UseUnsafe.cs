using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demo
{

    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class UseUnsafe
    {
        [Params(12345678)]
        public int Value;

        [Params(1_000_000_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {

        }

        

        [Benchmark(Baseline = true)]
        public unsafe void UseUnsafePoint()
        {
            int v = Value;
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                *((int*)buffer) = *((int*)&v);
        }

        [Benchmark]
        public unsafe void Common()
        {
            int v = Value;
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
            {
                //由高位到低位
                *(buffer + 0) = (byte)((i >> 24) & 0xFF);
                *(buffer + 1) = (byte)((i >> 16) & 0xFF);
                *(buffer + 2) = (byte)((i >> 8) & 0xFF);
                *(buffer + 3) = (byte)(i & 0xFF);
            }
        }

        
    }


}
