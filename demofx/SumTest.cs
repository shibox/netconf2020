using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace demofx
{
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 2)]
    [RPlotExporter, RankColumn]
    public class SumTest
    {
        private readonly static int[] array = new int[100_000_000];

        [Params(10)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = 1;
        }

        [Benchmark]
        public void SumCommon()
        {
            var sum = 0L;
            for (int i = 0; i < N; i++)
            {
                //普通的加法实现
                var result = 0;
                for (int j = 0; j < array.Length; j++)
                    result += array[j];
                sum += result;
            }
            Console.WriteLine($"common result is:{sum}");
        }

    }

}
