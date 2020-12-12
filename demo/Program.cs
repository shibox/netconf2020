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
            var summary = BenchmarkRunner.Run<CompilerAsAService>();
            
            //FastBenchmark.Run();
            //Console.WriteLine("finish!");
            //Console.ReadLine();
        }
    }
}
