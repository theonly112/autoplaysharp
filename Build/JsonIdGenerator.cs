using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace autoplaysharp.Build
{
    [Generator]
    // ReSharper disable once UnusedMember.Global
    // This is a source generator used by the compiler...
    public class JsonIdGenerator : ISourceGenerator
    {
        private class Element
        {
            public Element(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var f in context.AdditionalFiles)
            { 
                var json = JsonConvert.DeserializeObject<List<Element>>(File.ReadAllText(f.Path));

                var sb = new StringBuilder();
                sb.AppendLine("public partial class UIds");
                sb.AppendLine("{");
                foreach (var j in json)
                {
                    sb.AppendLine("\t/// <summary>");
                    sb.AppendLine($"\t/// {j.Id}");
                    sb.AppendLine("\t/// </summary>");
                    sb.AppendLine($"\tpublic const string {j.Id}=\"{j.Id}\";");
                }
                sb.AppendLine("}");

                context.AddSource(Path.GetFileName(f.Path), sb.ToString());
            }
        }
    }
}
