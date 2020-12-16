using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 比较两个byte数组的性能,向量实现是直接普通比较方法的几十倍
    /// |     Method | Count |      N |        Mean | Error | Ratio | Rank |
    /// |----------- |------ |------- |------------:|------:|------:|-----:|
    /// | Intrinsics |  1000 | 100000 |    450.4 ms |    NA |  1.00 |    1 |
    /// |    Vectors |  1000 | 100000 |    581.0 ms |    NA |  1.29 |    2 |
    /// |      Naive |  1000 | 100000 | 12,156.3 ms |    NA | 26.99 |    3 |
    /// |            |       |        |             |       |       |      |
    /// | Intrinsics | 10000 | 100000 |    425.5 ms |    NA |  1.00 |    1 |
    /// |    Vectors | 10000 | 100000 |    585.9 ms |    NA |  1.38 |    2 |
    /// |      Naive | 10000 | 100000 | 12,849.9 ms |    NA | 30.20 |    3 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class CompareArray
    {
        private readonly static byte[] arrayA = new byte[100_000];
        private readonly static byte[] arrayB = new byte[100_000];

        [Params(1_000,10_000)]
        public int Count;

        [Params(100_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        [Benchmark(Baseline = true)]
        public unsafe void Intrinsics()
        {
            for (int i = 0; i < N; i++)
                Fastest();
        }

        /// <summary>
        /// 
        /// </summary>
        [Benchmark]
        public void Vectors()
        {
            for (int i = 0; i < N; i++)
                Faster();
        }

        /// <summary>
        /// 一般通用的实现
        /// </summary>
        [Benchmark]
        public void Naive()
        {
            for (int i = 0; i < N; i++)
                Common();
        }

        public unsafe static bool Fastest()
        {
            int vectorSize = 256 / 8, i = 0;
            //-1=unchecked((int)(0b1111_1111_1111_1111_1111_1111_1111_1111));
            const int equalsMask = -1;
            fixed (byte* ptrA = arrayA,ptrB = arrayB)
            {
                for (; i <= arrayA.Length - vectorSize; i += vectorSize)
                {
                    var va = Avx.LoadVector256(ptrA + i);
                    var vb = Avx.LoadVector256(ptrB + i);
                    var areEqual = Avx2.CompareEqual(va, vb);
                    if (Avx2.MoveMask(areEqual) != equalsMask)
                        return false;
                }
                for (; i < arrayA.Length; i++)
                    if (arrayA[i] != arrayB[i])
                        return false;
                return true;
            }
        }

        public unsafe static bool Faster()
        {
            int vectorSize = Vector<byte>.Count;
            int i = 0;
            for (; i <= arrayA.Length - vectorSize; i += vectorSize)
            {
                var va = new Vector<byte>(arrayA, i);
                var vb = new Vector<byte>(arrayB, i);
                if (!Vector.EqualsAll(va, vb))
                    return false;
            }
            for (; i < arrayA.Length; i++)
                if (arrayA[i] != arrayB[i])
                    return false;
            return true;
        }

        public static bool Common()
        {
            for (int i = 0; i < arrayA.Length; i++)
                if (arrayA[i] != arrayB[i])
                    return false;
            return true;
        }

    }


}
