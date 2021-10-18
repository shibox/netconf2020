using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 原生simd指令调用，性能超高，每秒可以调用30亿次，支持的粒度非常细，如果跨语言调用，除非是
    /// 批量调用，否则性能会大打折扣，另外x86和x64性能基本没区别
    /// 
    /// 同样的功能的指令使用System.Runtime.Intrinsics命名空间下的性能更好，看起来要快30%以上
    ///     BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
    /// 11th Gen Intel Core i7-11370H 3.30GHz, 1 CPU, 8 logical and 4 physical cores
    /// .NET Core SDK=6.0.100-rc.1.21463.6
    ///   [Host]     : .NET Core 6.0.0 (CoreCLR 6.0.21.45113, CoreFX 6.0.21.45113), X64 RyuJIT
    ///   Job-ZSJBOS : .NET Core 6.0.0 (CoreCLR 6.0.21.45113, CoreFX 6.0.21.45113), X64 RyuJIT
    /// 
    /// IterationCount=1  LaunchCount=1  RunStrategy=ColdStart
    /// WarmupCount=1
    /// 
    /// |                         Method |          N |     Mean | Error | Ratio | Rank | Code Size | Gen 0 | Gen 1 | Gen 2 | Allocated |
    /// |------------------------------- |----------- |---------:|------:|------:|-----:|----------:|------:|------:|------:|----------:|
    /// |                 NativePopCount | 1000000000 | 277.9 ms |    NA |  1.00 |    4 |    2266 B |     - |     - |     - |     560 B |
    /// |                   WrapPopCount | 1000000000 | 404.9 ms |    NA |  1.46 |    5 |    1970 B |     - |     - |     - |     552 B |
    /// |        NativeTrailingZeroCount | 1000000000 | 279.1 ms |    NA |  1.00 |    4 |    2255 B |     - |     - |     - |     552 B |
    /// |     NativeTrailingZeroCountX86 | 1000000000 | 274.1 ms |    NA |  0.99 |    3 |    2256 B |     - |     - |     - |     560 B |
    /// |    NativeTrailingZeroCountBulk | 1000000000 | 223.3 ms |    NA |  0.80 |    2 |    2280 B |     - |     - |     - |     560 B |
    /// | NativeTrailingZeroCountBulkX86 | 1000000000 | 219.7 ms |    NA |  0.79 |    1 |    2280 B |     - |     - |     - |     568 B |
    /// |          WrapTrailingZeroCount | 1000000000 | 426.8 ms |    NA |  1.54 |    6 |    1970 B |     - |     - |     - |     552 B |
    /// |         NativeLeadingZeroCount | 1000000000 | 274.7 ms |    NA |  0.99 |    3 |    2255 B |     - |     - |     - |     576 B |
    /// |      NativeLeadingZeroCountX86 | 1000000000 | 274.2 ms |    NA |  0.99 |    3 |    2256 B |     - |     - |     - |     576 B |
    /// |           WrapLeadingZeroCount | 1000000000 | 405.0 ms |    NA |  1.46 |    5 |    1970 B |     - |     - |     - |     568 B |
    /// |                        ForLoop | 1000000000 | 281.4 ms |    NA |  1.01 |    4 |    1951 B |     - |     - |     - |     544 B |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RankColumn,MemoryDiagnoser, DisassemblyDiagnoser]

    //[RyuJitX64Job]
    public class SimdCall
    {
        public ulong ulongValue = 123456789;

        public uint uintValue = 123456789;

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
        public unsafe void NativePopCount()
        {
            ulong count = 0;
            for (int i = 0; i < N; i++)
                count += Popcnt.X64.PopCount(ulongValue);
            Console.WriteLine($"NativePopCount:{count}");
        }

        /// <summary>
        /// BitOperations下的该方法性能低一些
        /// </summary>
        [Benchmark]
        public unsafe void WrapPopCount()
        {
            long count = 0;
            for (int i = 0; i < N; i++)
                count += BitOperations.PopCount(ulongValue);
            Console.WriteLine($"WrapPopCount:{count}");
        }

        [Benchmark]
        public unsafe void NativeTrailingZeroCount()
        {
            ulong count = 0;
            for (int i = 0; i < N; i++)
                count += Bmi1.X64.TrailingZeroCount(ulongValue);
            Console.WriteLine($"NativeTrailingZeroCount:{count}");
        }

        [Benchmark]
        public unsafe void NativeTrailingZeroCountX86()
        {
            uint count = 0;
            for (int i = 0; i < N; i++)
                count += Bmi1.TrailingZeroCount(uintValue);
            Console.WriteLine($"NativeTrailingZeroCountX86:{count}");
        }

        [Benchmark]
        public unsafe void NativeTrailingZeroCountBulk()
        {
            ulong count = 0;
            for (int i = 0; i < N; i+=4)
            {
                count += Bmi1.X64.TrailingZeroCount(ulongValue);
                count += Bmi1.X64.TrailingZeroCount(ulongValue);
                count += Bmi1.X64.TrailingZeroCount(ulongValue);
                count += Bmi1.X64.TrailingZeroCount(ulongValue);
            }
            Console.WriteLine($"NativeTrailingZeroCountBulk:{count}");
        }

        [Benchmark]
        public unsafe void NativeTrailingZeroCountBulkX86()
        {
            uint count = 0;
            for (int i = 0; i < N; i += 4)
            {
                count += Bmi1.TrailingZeroCount(uintValue);
                count += Bmi1.TrailingZeroCount(uintValue);
                count += Bmi1.TrailingZeroCount(uintValue);
                count += Bmi1.TrailingZeroCount(uintValue);
            }
            Console.WriteLine($"NativeTrailingZeroCountBulkX86:{count}");
        }

        [Benchmark]
        public unsafe void WrapTrailingZeroCount()
        {
            long count = 0;
            for (int i = 0; i < N; i++)
                count += BitOperations.TrailingZeroCount(ulongValue);
            Console.WriteLine($"WrapTrailingZeroCount:{count}");
        }

        [Benchmark]
        public unsafe void NativeLeadingZeroCount()
        {
            ulong count = 0;
            for (int i = 0; i < N; i++)
                count += Lzcnt.X64.LeadingZeroCount(ulongValue);
            Console.WriteLine($"NativeLeadingZeroCount:{count}");
        }

        [Benchmark]
        public unsafe void NativeLeadingZeroCountX86()
        {
            uint count = 0;
            for (int i = 0; i < N; i++)
                count += Lzcnt.LeadingZeroCount(uintValue);
            Console.WriteLine($"NativeLeadingZeroCountX86:{count}");
        }

        [Benchmark]
        public unsafe void WrapLeadingZeroCount()
        {
            long count = 0;
            for (int i = 0; i < N; i++)
                count += BitOperations.LeadingZeroCount(ulongValue);
            Console.WriteLine($"WrapLeadingZeroCount:{count}");
        }

        [Benchmark]
        public unsafe void ForLoop()
        {
            long count = 0;
            for (int i = 0; i < N; i++)
                count++;
            Console.WriteLine($"ForLoop:{count}");
        }

    }


}
