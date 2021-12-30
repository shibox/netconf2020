using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace demo
{
    /// <summary>
    /// 动态编译的一个示例，通过动态编译执行代码，生成结果
    /// </summary>
    public class CompileExample
    {
        public static void Run()
        {
            var source = @"   
            using System;
            namespace RoslynCompile
            {
                public class Calculator
                {
                    public int exec(int input)
                    {
                        return input + 1;
                    }
                }
            }";

            var assemblyName = Path.GetRandomFileName();
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: options);
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
            var type = assembly.GetType("RoslynCompile.Calculator");
            var instance = assembly.CreateInstance("RoslynCompile.Calculator");
            var method = type.GetMember("exec").First() as MethodInfo;
            var input = 123;
            int result = (int)method.Invoke(instance, new object[] { input });
            Console.WriteLine(result);
            //result is:124
        }
        
        public static void RunNet6()
        {
            //动态编译暂时不支持最新的C#10语法
            //var source = @"   
            //using System;
            //namespace RoslynCompile;
            
            //public class Calculator
            //{
            //    public int exec(int input)
            //    {
            //        return input + 1;
            //    }
            //}";

            var source = @"   
            using System;
            namespace RoslynCompile
            {
                public class Calculator
                {
                    public int exec(int input)
                    {
                        return input + 1;
                    }
                }
            }";

            var assemblyName = Path.GetRandomFileName();
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            //支持语言版本
            var pop = new CSharpParseOptions(LanguageVersion.Latest);
            var syntaxTree = CSharpSyntaxTree.ParseText(source,pop);
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: options);
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
            var type = assembly.GetType("RoslynCompile.Calculator");
            var instance = assembly.CreateInstance("RoslynCompile.Calculator");
            var method = type.GetMember("exec").First() as MethodInfo;
            var input = 123;
            int result = (int)method.Invoke(instance, new object[] { input });
            Console.WriteLine(result);
            //result is:124
        }

    }
}




