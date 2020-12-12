using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace demofx
{
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, targetCount: 2)]
    [RPlotExporter, RankColumn]
    public class SumTest
    {
        private readonly static int[] array = new int[100_000_000];

        [Params(10)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = 1;
        }

        //[Benchmark(Baseline = true)]
        //public unsafe void DirectSimd()
        //{
        //    var sum = 0L;
        //    for (int i = 0; i < N; i++)
        //    {
        //        //普通的加法实现
        //        var result = Sum(new ArraySegment<int>(array));
        //        sum += result;
        //    }
        //    Console.WriteLine($"direct simd result is:{sum}");
        //}

        //[Benchmark]
        //public unsafe void SumUseSimd()
        //{
        //    var sum = 0L;
        //    for (int i = 0; i < N; i++)
        //    {
        //        var type = assembly.GetType("RoslynCompile.Calculator");
        //        var instance = assembly.CreateInstance("RoslynCompile.Calculator");
        //        var meth = type.GetMember("Calculate").First() as MethodInfo;
        //        // 获取通过编译器生成的方法执行的结果
        //        int result = (int)meth.Invoke(instance, new object[] { new ArraySegment<int>(array) });
        //        sum += result;
        //    }
        //    Console.WriteLine($"compile simd result is:{sum}");
        //}

        [Benchmark]
        public void SumCommon()
        {
            var sum = 0L;
            for (int i = 0; i < N; i++)
            {
                //普通的加法实现
                var result = 0;
                for (int j = 0; j < array.Length; j++)
                    result += array[j];
                sum += result;
            }
            Console.WriteLine($"common result is:{sum}");
        }

//        public static void Compile()
//        {
//            var code = @"               
//using System;
//using System.Numerics;
//using System.Runtime.Intrinsics;
//using System.Runtime.Intrinsics.X86;

//namespace RoslynCompile
//{
//    public class Calculator
//    {
//        public unsafe static Int32 Calculate(ArraySegment<Int32> array)
//        {
//            if (array.Count == 0)
//                return 0;
//            int result = 0;
//            int size = Vector<int>.Count;
//            int[] ar = array.Array;
//            Vector256<int> swap = Vector256<int>.Zero;
//            int blockCount = array.Count / size;
//            int i = array.Offset;
//            fixed (int* ptr_a = ar)
//            {
//                for (; i < array.Offset + blockCount * size; i += Vector256<int>.Count)
//                {
//                    Vector256<int> v1 = Avx2.LoadVector256(ptr_a + i);
//                    swap = Avx2.Add(v1, swap);
//                }
//            }

//            for (int n = 0; n < size; ++n)
//                result += swap.GetElement(n);
//            for (; i < array.Offset + array.Count; i++)
//                result += ar[i];
//            return result;
//        }
//    }
//}";

//            var w = Stopwatch.StartNew();
//            var syntaxTree = CSharpSyntaxTree.ParseText(code);
//            string assemblyName = Path.GetRandomFileName();
//            var references = new MetadataReference[]
//            {
//                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
//            };
//            CSharpCompilation compilation = CSharpCompilation.Create(
//                assemblyName,
//                syntaxTrees: new[] { syntaxTree },
//                references: references,
//                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
//                allowUnsafe: true,
//                platform: Platform.X64,
//                optimizationLevel: OptimizationLevel.Release));
//            using (var ms = new MemoryStream())
//            {
//                EmitResult emitResult = compilation.Emit(ms);
//                if (!emitResult.Success)
//                {
//                    IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
//                        diagnostic.IsWarningAsError ||
//                        diagnostic.Severity == DiagnosticSeverity.Error);
//                }
//                else
//                {
//                    w = Stopwatch.StartNew();
//                    ms.Seek(0, SeekOrigin.Begin);
//                    assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
//                    Console.WriteLine("compile succeed");
//                }
//            }
//        }

//        public unsafe static Int32 Sum(ArraySegment<Int32> array)
//        {
//            if (array.Count == 0)
//                return 0;
//            int result = 0;
//            int size = Vector<int>.Count;
//            int[] ar = array.Array;
//            Vector256<int> swap = Vector256<int>.Zero;
//            int blockCount = array.Count / size;
//            int i = array.Offset;
//            fixed (int* ptr_a = ar)
//            {
//                for (; i < array.Offset + blockCount * size; i += Vector256<int>.Count)
//                {
//                    Vector256<int> v1 = Avx2.LoadVector256(ptr_a + i);
//                    swap = Avx2.Add(v1, swap);
//                }
//            }

//            for (int n = 0; n < size; ++n)
//                result += swap.GetElement(n);
//            for (; i < array.Offset + array.Count; i++)
//                result += ar[i];
//            return result;
//        }

    }

}
