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
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 2)]
    [RPlotExporter, RankColumn]
    public class CompilerMode
    {
        [Params(1, 11, 123)]
        public byte Value;

        [Params(100_000_000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {

        }

        [Benchmark(Baseline = true)]
        public unsafe void ToStringSimple()
        {
            
        }

        [Benchmark]
        public unsafe void ToStringFast()
        {
            
        }

        public static void Run3(int version)
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
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);
            Console.WriteLine($"cost:{w.ElapsedMilliseconds}");
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    // some errors
                    IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                }
                else
                {
                    Console.WriteLine("compile succeed");

                    //w = Stopwatch.StartNew();
                    //ms.Seek(0, SeekOrigin.Begin);
                    //Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    //var type = assembly.GetType("RoslynCompile.Calculator");
                    //var instance = assembly.CreateInstance("RoslynCompile.Calculator");
                    //var meth = type.GetMember("Calculate").First() as MethodInfo;
                    //// get result
                    //int? result = meth.Invoke(instance, null) as int?;
                    //Console.WriteLine($"exec cost:{w.ElapsedMilliseconds}");
                    //Console.WriteLine(result);
                }
            }
        }

    }

}
