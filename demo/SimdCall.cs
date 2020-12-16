using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 原生simd指令调用，性能超高，每秒可以调用30亿次，支持的粒度非常细，如果跨语言调用，除法是
    /// 批量调用，否则性能会大打折扣
    /// |      Method |     Value |          N |       Mean | Error | Ratio | Rank |
    /// |------------ |---------- |----------- |-----------:|------:|------:|-----:|
    /// |     UseSimd | 123456789 | 1000000000 |   288.0 ms |    NA |  1.00 |    1 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class SimdCall
    {
        [Params(123456789)]
        public ulong Value;

        [Params(1_000_000_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {

        }


        /// <summary>
        /// 使用intel cpu内部的向量指令
        /// </summary>
        [Benchmark(Baseline = true)]
        public unsafe void UseSimd()
        {
            ulong count = 0;
            for (int i = 0; i < N; i++)
                count += Popcnt.X64.PopCount(Value);
        }

    }


}
