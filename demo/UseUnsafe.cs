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
    /// 类型转换直接使用非安全代码的形式实现
    /// |         Method |    Value |          N |       Mean | Error | Ratio | Rank |
    /// |--------------- |--------- |----------- |-----------:|------:|------:|-----:|
    /// | UseUnsafePoint | 12345678 | 1000000000 |   536.1 ms |    NA |  1.00 |    1 |
    /// |    UnsafeWrite | 12345678 | 1000000000 |   542.1 ms |    NA |  1.01 |    2 |
    /// |   SimpleCommon | 12345678 | 1000000000 | 1,114.4 ms |    NA |  2.08 |    3 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 1)]
    [RPlotExporter, RankColumn]
    public class UseUnsafe
    {
        [Params(12345678)]
        public int Value;

        [Params(1_000_000_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            
        }

        
        /// <summary>
        /// 使用指针高性能的实现，直接拷贝内存，同样byte[]转int也一样
        /// </summary>
        [Benchmark(Baseline = true)]
        public unsafe void UseUnsafePoint()
        {
            int v = Value;
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                *((int*)buffer) = *((int*)&v);
        }

        /// <summary>
        /// 使用系统的函数，效果一样
        /// </summary>
        [Benchmark]
        public unsafe void UnsafeWrite()
        {
            int v = Value;
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
                Unsafe.Write(buffer, v);
        }

        /// <summary>
        /// 一般通用的实现
        /// </summary>
        [Benchmark]
        public unsafe void SimpleCommon()
        {
            int v = Value;
            byte* buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
            {
                //由高位到低位
                *(buffer + 0) = (byte)((i >> 24) & 0xFF);
                *(buffer + 1) = (byte)((i >> 16) & 0xFF);
                *(buffer + 2) = (byte)((i >> 8) & 0xFF);
                *(buffer + 3) = (byte)(i & 0xFF);
            }
        }

        
    }


}
