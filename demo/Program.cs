using BenchmarkDotNet.Running;
using System;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<UseShift>();
            //var summary = BenchmarkRunner.Run<UseUnsafe>();
            //var summary = BenchmarkRunner.Run<UseStackalloc>();
            //var summary = BenchmarkRunner.Run<CompilerAsAService>();
            //var summary = BenchmarkRunner.Run<SimdCall>();
            //var summary = BenchmarkRunner.Run<BitCount>();
            //var summary = BenchmarkRunner.Run<CompilerMode>();
            //var summary = BenchmarkRunner.Run<UseInline>();
            //var summary = BenchmarkRunner.Run<LessBranch>();
            //var summary = BenchmarkRunner.Run<CompareArray>();
            //var summary = BenchmarkRunner.Run<IntersectSimd>();
            var summary = BenchmarkRunner.Run<SumBitCount>();
            
            //CompareArray.Fastest();

            //FastBenchmark.Run();
            //Console.WriteLine("finish!");
            //Console.ReadLine();
        }
    }
}
