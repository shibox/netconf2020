using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 使用内联提升性能
    /// 这里使用xxhash做一个测试，使用内联和非内联性能相差比较大，不过现在编译器比较智能
    /// 会自动对一些比较小循环体内调用的函数体进行内联
    /// |         Method |  Size |        N |         Mean | Error | Ratio | Rank |
    /// |--------------- |------ |--------- |-------------:|------:|------:|-----:|
    /// |   XXHashInline |    10 | 10000000 |     67.67 ms |    NA |  1.00 |    1 |
    /// | XXHashNoInline |    10 | 10000000 |    115.32 ms |    NA |  1.70 |    2 |
    /// |                |       |          |              |       |       |      |
    /// |   XXHashInline |    64 | 10000000 |    132.62 ms |    NA |  1.00 |    1 |
    /// | XXHashNoInline |    64 | 10000000 |    346.17 ms |    NA |  2.61 |    2 |
    /// |                |       |          |              |       |       |      |
    /// |   XXHashInline |   128 | 10000000 |    179.17 ms |    NA |  1.00 |    1 |
    /// | XXHashNoInline |   128 | 10000000 |    506.63 ms |    NA |  2.83 |    2 |
    /// |                |       |          |              |       |       |      |
    /// |   XXHashInline |   512 | 10000000 |    449.43 ms |    NA |  1.00 |    1 |
    /// | XXHashNoInline |   512 | 10000000 |  1,508.20 ms |    NA |  3.36 |    2 |
    /// |                |       |          |              |       |       |      |
    /// |   XXHashInline |  1000 | 10000000 |    816.48 ms |    NA |  1.00 |    1 |
    /// | XXHashNoInline |  1000 | 10000000 |  2,698.58 ms |    NA |  3.31 |    2 |
    /// |                |       |          |              |       |       |      |
    /// |   XXHashInline | 10000 | 10000000 |  8,469.71 ms |    NA |  1.00 |    1 |
    /// | XXHashNoInline | 10000 | 10000000 | 25,310.76 ms |    NA |  2.99 |    2 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class UseInline
    {

        private readonly static byte[] values = new byte[1024 * 128];

        [Params(10,64,128,512,1000,10000)]
        public int Size;

        [Params(10000000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            var rd = new Random(Guid.NewGuid().GetHashCode());
            rd.NextBytes(values);
        }

        [Benchmark(Baseline = true)]
        public unsafe void XXHashInline()
        {
            fixed (byte* pd = &values[0])
            {
                for (int i = 0; i < N; i++)
                    Hash(pd, Size);
            }
        }

        [Benchmark]
        public unsafe void XXHashNoInline()
        {
            fixed (byte* pd = &values[0])
            {
                for (int i = 0; i < N; i++)
                    HashNoInline(pd, Size);
            }
        }

        #region xxhash

        private const ulong PRIME64_1 = 11400714785074694791UL;
        private const ulong PRIME64_2 = 14029467366897019727UL;
        private const ulong PRIME64_3 = 1609587929392839161UL;
        private const ulong PRIME64_4 = 9650029242287828579UL;
        private const ulong PRIME64_5 = 2870177450012600261UL;

        public static unsafe ulong Hash(byte* input, int count, ulong seed = 0)
        {
            ulong h64;

            byte* bEnd = input + count;

            if (count >= 32)
            {
                byte* limit = bEnd - 32;

                ulong v1 = seed + PRIME64_1 + PRIME64_2;
                ulong v2 = seed + PRIME64_2;
                ulong v3 = seed + 0;
                ulong v4 = seed - PRIME64_1;

                do
                {
                    v1 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);
                    v2 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);
                    v3 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);
                    v4 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);

                    v1 = rol31(v1);
                    v2 = rol31(v2);
                    v3 = rol31(v3);
                    v4 = rol31(v4);

                    v1 *= PRIME64_1;
                    v2 *= PRIME64_1;
                    v3 *= PRIME64_1;
                    v4 *= PRIME64_1;
                }
                while (input <= limit);

                h64 = rol1(v1) + rol7(v2) + rol12(v3) + rol18(v4);

                v1 *= PRIME64_2;
                v1 = rol31(v1);
                v1 *= PRIME64_1;
                h64 ^= v1;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v2 *= PRIME64_2;
                v2 = rol31(v2);
                v2 *= PRIME64_1;
                h64 ^= v2;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v3 *= PRIME64_2;
                v3 = rol31(v3);
                v3 *= PRIME64_1;
                h64 ^= v3;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v4 *= PRIME64_2;
                v4 = rol31(v4);
                v4 *= PRIME64_1;
                h64 ^= v4;
                h64 = h64 * PRIME64_1 + PRIME64_4;
            }
            else
            {
                h64 = seed + PRIME64_5;
            }

            h64 += (ulong)count;


            while (input + 8 <= bEnd)
            {
                ulong k1 = *((ulong*)input);
                k1 *= PRIME64_2;
                k1 = rol31(k1);
                k1 *= PRIME64_1;
                h64 ^= k1;
                h64 = rol27(h64) * PRIME64_1 + PRIME64_4;
                input += 8;
            }

            if (input + 4 <= bEnd)
            {
                h64 ^= *(uint*)input * PRIME64_1;
                h64 = rol23(h64) * PRIME64_2 + PRIME64_3;
                input += 4;
            }

            while (input < bEnd)
            {
                h64 ^= ((ulong)*input) * PRIME64_5;
                h64 = rol11(h64) * PRIME64_1;
                input++;
            }

            h64 ^= h64 >> 33;
            h64 *= PRIME64_2;
            h64 ^= h64 >> 29;
            h64 *= PRIME64_3;
            h64 ^= h64 >> 32;

            return h64;
        }

        public static unsafe ulong HashNoInline(byte* input, int count, ulong seed = 0)
        {
            ulong h64;

            byte* bEnd = input + count;

            if (count >= 32)
            {
                byte* limit = bEnd - 32;

                ulong v1 = seed + PRIME64_1 + PRIME64_2;
                ulong v2 = seed + PRIME64_2;
                ulong v3 = seed + 0;
                ulong v4 = seed - PRIME64_1;

                do
                {
                    v1 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);
                    v2 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);
                    v3 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);
                    v4 += *((ulong*)input) * PRIME64_2;
                    input += sizeof(ulong);

                    v1 = rol31No(v1);
                    v2 = rol31No(v2);
                    v3 = rol31No(v3);
                    v4 = rol31No(v4);

                    v1 *= PRIME64_1;
                    v2 *= PRIME64_1;
                    v3 *= PRIME64_1;
                    v4 *= PRIME64_1;
                }
                while (input <= limit);

                h64 = rol1No(v1) + rol7No(v2) + rol12No(v3) + rol18No(v4);

                v1 *= PRIME64_2;
                v1 = rol31No(v1);
                v1 *= PRIME64_1;
                h64 ^= v1;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v2 *= PRIME64_2;
                v2 = rol31No(v2);
                v2 *= PRIME64_1;
                h64 ^= v2;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v3 *= PRIME64_2;
                v3 = rol31No(v3);
                v3 *= PRIME64_1;
                h64 ^= v3;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v4 *= PRIME64_2;
                v4 = rol31No(v4);
                v4 *= PRIME64_1;
                h64 ^= v4;
                h64 = h64 * PRIME64_1 + PRIME64_4;
            }
            else
            {
                h64 = seed + PRIME64_5;
            }

            h64 += (ulong)count;


            while (input + 8 <= bEnd)
            {
                ulong k1 = *((ulong*)input);
                k1 *= PRIME64_2;
                k1 = rol31No(k1);
                k1 *= PRIME64_1;
                h64 ^= k1;
                h64 = rol27No(h64) * PRIME64_1 + PRIME64_4;
                input += 8;
            }

            if (input + 4 <= bEnd)
            {
                h64 ^= *(uint*)input * PRIME64_1;
                h64 = rol23No(h64) * PRIME64_2 + PRIME64_3;
                input += 4;
            }

            while (input < bEnd)
            {
                h64 ^= ((ulong)*input) * PRIME64_5;
                h64 = rol11No(h64) * PRIME64_1;
                input++;
            }

            h64 ^= h64 >> 33;
            h64 *= PRIME64_2;
            h64 ^= h64 >> 29;
            h64 *= PRIME64_3;
            h64 ^= h64 >> 32;

            return h64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol1(ulong x) { return (x << 1) | (x >> (64 - 1)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol7(ulong x) { return (x << 7) | (x >> (64 - 7)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol11(ulong x) { return (x << 11) | (x >> (64 - 11)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol12(ulong x) { return (x << 12) | (x >> (64 - 12)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol18(ulong x) { return (x << 18) | (x >> (64 - 18)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol23(ulong x) { return (x << 23) | (x >> (64 - 23)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol27(ulong x) { return (x << 27) | (x >> (64 - 27)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rol31(ulong x) { return (x << 31) | (x >> (64 - 31)); }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol1No(ulong x) { return (x << 1) | (x >> (64 - 1)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol7No(ulong x) { return (x << 7) | (x >> (64 - 7)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol11No(ulong x) { return (x << 11) | (x >> (64 - 11)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol12No(ulong x) { return (x << 12) | (x >> (64 - 12)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol18No(ulong x) { return (x << 18) | (x >> (64 - 18)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol23No(ulong x) { return (x << 23) | (x >> (64 - 23)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol27No(ulong x) { return (x << 27) | (x >> (64 - 27)); }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong rol31No(ulong x) { return (x << 31) | (x >> (64 - 31)); }

        #endregion

    }
}
