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
    /// 类似groupby功能的实现，测试计算速度
    /// values即一列数据，sql类似：select values,count(0) from table group by values
    /// 原生group by转换成数组操作，最快可以实现单核每秒聚合20亿条记录
    /// |       Method |  N |     Mean | Error | Ratio | Rank |
    /// |------------- |--- |---------:|------:|------:|-----:|
    /// | CountFastest | 10 | 481.0 ms |    NA |  1.00 |    1 |
    /// |  CountFaster | 10 | 545.6 ms |    NA |  1.13 |    2 |
    /// |    CountFast | 10 | 597.3 ms |    NA |  1.24 |    3 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class UseStackalloc
    {
        private readonly static byte[] values = new byte[100_000_000];

        [Params(10)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            var rd = new Random(Guid.NewGuid().GetHashCode());
            rd.NextBytes(values);
        }

        /// <summary>
        /// 使用最原始的数组，性能会比使用栈内存性能差一些
        /// </summary>
        /// <param name="rs"></param>
        private unsafe static void CountFast(uint[] rs)
        {
            var rss = new uint[rs.Length];
            fixed (byte* pd = &values[0])
            {
                byte* start = pd;
                byte* end = start + values.Length - 8;
                while (start < end)
                {
                    rss[*(start + 0)]++;
                    rss[*(start + 1)]++;
                    rss[*(start + 2)]++;
                    rss[*(start + 3)]++;
                    rss[*(start + 4)]++;
                    rss[*(start + 5)]++;
                    rss[*(start + 6)]++;
                    rss[*(start + 7)]++;
                    start += 8;
                }
            }
            for (int i = 0; i < rs.Length; i++)
                rs[i] += rss[i];
        }

        /// <summary>
        /// 使用栈内存的情况下，使用span会导致性能有点降低
        /// </summary>
        /// <param name="rs"></param>
        private unsafe static void CountFaster(uint[] rs)
        {
            Span<uint> rss = stackalloc uint[rs.Length];
            fixed (byte* pd = &values[0])
            {
                byte* start = pd;
                byte* end = start + values.Length - 8;
                while (start < end)
                {
                    rss[*(start + 0)]++;
                    rss[*(start + 1)]++;
                    rss[*(start + 2)]++;
                    rss[*(start + 3)]++;
                    rss[*(start + 4)]++;
                    rss[*(start + 5)]++;
                    rss[*(start + 6)]++;
                    rss[*(start + 7)]++;
                    start += 8;
                }
            }
            for (int i = 0; i < rs.Length; i++)
                rs[i] += rss[i];
        }

        /// <summary>
        /// 使用原生栈内存，性能最好
        /// 缺点：不能申请太大，一般是1MB以内，否则会报错
        /// </summary>
        /// <param name="rs"></param>
        private unsafe static void CountFastest(uint[] rs)
        {
            var rss = stackalloc uint[rs.Length];
            fixed (byte* pd = &values[0])
            {
                byte* start = pd;
                byte* end = start + values.Length - 8;
                while (start < end)
                {
                    rss[*(start + 0)]++;
                    rss[*(start + 1)]++;
                    rss[*(start + 2)]++;
                    rss[*(start + 3)]++;
                    rss[*(start + 4)]++;
                    rss[*(start + 5)]++;
                    rss[*(start + 6)]++;
                    rss[*(start + 7)]++;
                    start += 8;
                }
            }
            for (int i = 0; i < rs.Length; i++)
                rs[i] += rss[i];
        }
        

        [Benchmark(Baseline = true)]
        public unsafe void CountFastest()
        {
            var rs = new uint[256];
            for (int i = 0; i < N; i++)
                CountFastest(rs);
        }

        [Benchmark]
        public unsafe void CountFaster()
        {
            var rs = new uint[256];
            for (int i = 0; i < N; i++)
                CountFaster(rs);
        }

        [Benchmark]
        public unsafe void CountFast()
        {
            var rs = new uint[256];
            for (int i = 0; i < N; i++)
                CountFast(rs);
        }

    }

}
