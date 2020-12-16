using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// simd在过滤过程中的使用-交集计算,每秒可以计算：1亿*8bit*10*1000毫秒/125毫秒=640亿bit/秒
    /// |        Method |  N |     Mean | Error |  StdDev | Ratio | Rank |
    /// |-------------- |--- |---------:|------:|--------:|------:|-----:|
    /// | SimdIntersect | 10 | 125.1 ms |    NA | 0.05 ms |  1.00 |    1 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 2)]
    [RPlotExporter, RankColumn]
    public class IntersectSimd
    {
        private readonly static byte[] arrayA = new byte[100_000_000];
        private readonly static byte[] arrayB = new byte[100_000_000];

        [Params(10)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            var rd = new Random(Guid.NewGuid().GetHashCode());
            rd.NextBytes(arrayA);
            rd.NextBytes(arrayB);
        }

        [Benchmark(Baseline = true)]
        public unsafe void SimdIntersect()
        {
            for (int i = 0; i < N; i++)
            {
                fixed (byte* pa = &arrayA[0], pb = &arrayB[0])
                {
                    byte* aStart = pa, bStart = pb;
                    byte* aEnd = pa + arrayA.Length - 32;
                    byte* bEnd = pb + arrayB.Length - 32;
                    while (aStart < aEnd)
                    {
                        Vector256<byte> va = Avx.LoadVector256(aStart);
                        Vector256<byte> vb = Avx.LoadVector256(bStart);
                        //进行交集计算，获得结果，每次处理32个字节
                        Vector256<byte> intersect = Avx2.And(va, vb);
                        aStart += 32;
                        bStart += 32;
                    }
                }
            }
        }

        [Benchmark]
        public unsafe void IntersectCommon()
        {
            for (int i = 0; i < N; i++)
            {
                //fixed (byte* pa = &arrayA[0], pb = &arrayB[0])
                //{
                //    ulong* aStart = (ulong*)pa, bStart = (ulong*)pb;
                //    ulong* aEnd = (ulong*)pa + arrayA.Length / 8 - 4;
                //    ulong* bEnd = (ulong*)pb + arrayB.Length / 8 - 4;
                //    while (aStart < aEnd)
                //    {
                //        ulong intersect = *aStart & *bStart;
                //        aStart ++;
                //        bStart ++;
                //        //aStart += 4;
                //        //bStart += 4;
                //    }
                //}

                fixed (byte* pa = &arrayA[0], pb = &arrayB[0])
                {
                    byte* aStart = pa, bStart = pb;
                    byte* aEnd = pa + arrayA.Length -1;
                    byte* bEnd = pb + arrayB.Length -1;
                    while (aStart < aEnd)
                    {
                        int intersect = *aStart & *bStart;
                        aStart++;
                        bStart++;
                    }
                }
            }
        }

    }
}
