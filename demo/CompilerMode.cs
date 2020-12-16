using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 不同模式下的编译性能测试
    /// 基本的任务平均每次编译时间大概15毫秒，即使是带上多种编译条件也一样,速度还是不错的
    /// |                  Method |   N |    Mean | Error |   StdDev | Ratio | RatioSD | Rank |
    /// |------------------------ |---- |--------:|------:|---------:|------:|--------:|-----:|
    /// |          CompileDefault | 100 | 1.417 s |    NA | 0.8729 s |  1.00 |    0.00 |    1 |
    /// | CompileUnsafeReleaseX64 | 100 | 1.415 s |    NA | 0.8486 s |  1.01 |    0.02 |    1 |
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 2)]
    [RPlotExporter, RankColumn]
    public class CompilerMode
    {

        [Params(100)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {

        }

        [Benchmark(Baseline = true)]
        public unsafe void CompileDefault()
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            for (int i = 0; i < N; i++)
                Compile(i, options);
        }

        [Benchmark]
        public unsafe void CompileUnsafeReleaseX64()
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                   allowUnsafe: true,
                   optimizationLevel: OptimizationLevel.Release,
                   platform:Platform.X64);
            for (int i = 0; i < N; i++)
                Compile(i, options);
        }

        public static void Compile(int version, CSharpCompilationOptions options)
        {
            var code = $"int v ={version};";
            var codeToCompile = @"   
using System;
namespace RoslynCompile
{
    public class Calculator
    {
        public int exec()
        {
            @@@
            return v;
        }
    }
}";

            codeToCompile = codeToCompile.Replace("@@@", code);
            var w = Stopwatch.StartNew();
            var syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);
            var assemblyName = Path.GetRandomFileName();
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: options);
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);
            if (!emitResult.Success)
            {
                var failures = emitResult.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                Console.WriteLine($"failures:{failures}");
            }
            else
            {
                //Console.WriteLine("compile succeed");
            }
        }

    }

}
