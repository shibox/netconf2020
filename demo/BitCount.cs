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
    /// bitcount最好的算法与cpu指令性能对比
    /// 功能：统计一个数字转换成二进制后，里面1的个数
    /// 如：0x0f0f0f0f转换成2进制是：0000 1111 0000 1111 0000 1111 0000 1111
    /// 通过对比，比用软件算法性能高6倍左右，并且通过这个数据可以看到10亿次函数调用开销
    /// 加上功能不到300毫秒，而java想使用这些硬件加速如果使用jni跨语言调用很耗时，
    /// 平均每秒只能调用百万量级，对于一些细粒度的函数性能完全不可接受，只能批量处理
    /// 
    /// 这个能用于做什么，一个典型的应用，过滤计算
    /// sql举例：select count(0) from table where age=18
    /// 0000 1111 0000 1111 0000 1111 0000 1111 这个序列就是通过bitmap交集计算出来的数据，
    /// 是1的位置就是满足条件的
    /// |      Method |     Value |          N |       Mean | Error | Ratio | Rank |
    /// |------------ |---------- |----------- |-----------:|------:|------:|-----:|
    /// |     UseSimd | 123456789 | 1000000000 |   288.0 ms |    NA |  1.00 |    1 |
    /// | UseSoftware | 123456789 | 1000000000 | 1,954.4 ms |    NA |  6.79 |    2 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class BitCount
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

        [Benchmark]
        public unsafe void UseSoftware()
        {
            ulong count = 0;
            for (int i = 0; i < N; i++)
                count += Popcount64(Value);
        }

        /// <summary>
        /// 一个比较高性能的软件算法实现
        /// </summary>
        /// <param name="x">64位bit数</param>
        /// <returns>64位bit中1的个数</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Popcount64(ulong x)
        {
            const ulong m1 = 0x5555555555555555L;
            const ulong m2 = 0x3333333333333333L;
            const ulong m4 = 0x0F0F0F0F0F0F0F0FL;
            const ulong h1 = 0x0101010101010101L;
            x -= (x >> 1) & m1;
            x = (x & m2) + ((x >> 2) & m2);
            x = (x + (x >> 4)) & m4;
            return (x * h1) >> 56;
        }

    }

}
