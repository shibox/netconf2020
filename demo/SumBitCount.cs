using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 14亿用户统计姓氏数量，每次耗时仅：8.8毫秒
    /// select count(0) from population where surname=‘赵’
    /// |        Method |   N |     Mean | Error | Ratio | Rank |
    /// |-------------- |---- |---------:|------:|------:|-----:|
    /// | BitTotalCount | 100 | 879.6 ms |    NA |  1.00 |    1 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class SumBitCount
    {
        /// <summary>
        /// 假如申请14亿个bit位
        /// </summary>
        private readonly static byte[] values = new byte[1_400_000_000 / 8];

        [Params(100)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            var rd = new Random(Guid.NewGuid().GetHashCode());
            rd.NextBytes(values);
        }


        /// <summary>
        /// 使用intel cpu内部的向量指令
        /// </summary>
        [Benchmark(Baseline = true)]
        public unsafe void BitTotalCount()
        {
            for (int i = 0; i < N; i++)
            {
                ulong count = 0;
                fixed (byte* pd = &values[0])
                {
                    //byte* start = pd;
                    //byte* end = start + values.Length - 32;
                    //while (start < end)
                    //{
                    //    count += Popcnt.X64.PopCount(*(ulong*)(start + 0));
                    //    count += Popcnt.X64.PopCount(*(ulong*)(start + 8));
                    //    count += Popcnt.X64.PopCount(*(ulong*)(start + 16));
                    //    count += Popcnt.X64.PopCount(*(ulong*)(start + 24));
                    //    start += 32;
                    //}

                    //这样实现更快，超过上面30%
                    ulong* start = (ulong*)pd;
                    ulong* end = start + values.Length / 8 - 32;
                    while (start < end)
                    {
                        count += Popcnt.X64.PopCount(*(start + 0));
                        count += Popcnt.X64.PopCount(*(start + 1));
                        count += Popcnt.X64.PopCount(*(start + 2));
                        count += Popcnt.X64.PopCount(*(start + 3));
                        start += 32;
                    }
                }
            }
                
        }

    }

}
