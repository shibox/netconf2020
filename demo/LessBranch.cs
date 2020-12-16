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
    /// 更好的算法，减少分支，分支对性能的影响
    /// |    Method |          N |     Mean | Error | Ratio | Rank |
    /// |---------- |----------- |---------:|------:|------:|-----:|
    /// |  NoBranch | 1000000000 | 489.2 ms |    NA |  1.00 |    1 |
    /// | HasBranch | 1000000000 | 752.4 ms |    NA |  1.54 |    2 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class LessBranch
    {
        [Params(1_000_000_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {

        }

        [Benchmark(Baseline = true)]
        public void NoBranch()
        {
            int count = 0;
            for (int i = 0; i < N; i++)
                count += (i & 1);
        }

        [Benchmark]
        public void HasBranch()
        {
            int count = 0;
            for (int i = 0; i < N; i++)
            {
                if ((i & 1) == 1)
                    count++;
            }
        }
    }
}
