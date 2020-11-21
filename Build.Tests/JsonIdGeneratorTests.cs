using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace autoplaysharp.Build.Tests
{
    internal class JsonIdGeneratorTests
    {
        [Test]
        public void Test1()
        {
            const string source = @"
namespace Foo
{
    class C
    {
        void M()
        {
        }
    }
}";
            string output = GetGeneratedOutput(source);

            Assert.NotNull(output);

            Assert.AreEqual("class Foo { }", output);
        }

        private static string GetGeneratedOutput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic)
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create("foo", new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            ISourceGenerator generator = new JsonIdGenerator();

            string[] files = new[]{ "C:\\Code\\csharp\\autoplaysharp\\Core\\ui\\alliance_battle.json" };
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, files.Select(f => new AddtionalTestFile(f)));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            Assert.False(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

            string output = outputCompilation.SyntaxTrees.Last().ToString();
            Console.WriteLine(output);

            return output;
        }

        private class AddtionalTestFile : AdditionalText
        {
            internal AddtionalTestFile(string path)
            {
                Path = path;
            }

            public override SourceText? GetText(CancellationToken cancellationToken = new())
            {
                return SourceText.From(Path);
            }

            public override string Path { get; }
        }
    }
}
