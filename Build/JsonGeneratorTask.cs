using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace autoplaysharp.Build
{
    public class JsonGeneratorTask : Task
    {
        public string InputFiles { get; set; }

        class Element
        { 
            public string Id { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            //throw new NotImplementedException();
            Log.LogMessage(MessageImportance.High, InputFiles);
            var files = InputFiles.Split(';');

            var projectDir = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            foreach (var f in files)
            {
                var json = JsonConvert.DeserializeObject<List<Element>>(File.ReadAllText(Path.Combine(projectDir, f)));

                var sb = new StringBuilder();
                sb.AppendLine("partial class UIds");
                sb.AppendLine("{");
                foreach (var j in json)
                {
                    sb.AppendLine($"\t/// <summary>");
                    sb.AppendLine($"\t/// {j.Id}");
                    sb.AppendLine($"\t/// </summary>");
                    sb.AppendLine($"\tpublic const string {j.Id}=\"{j.Id}\";");
                }
                sb.AppendLine("}");
                File.WriteAllText(Path.Combine(projectDir, $"{f}.cs"), sb.ToString());
            }
            return true;
        }
    }
}
