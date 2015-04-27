using System.Collections.Generic;
using System.IO;
using System.Linq;
using CombineCs;

static internal class CsFileReader
{
    public static List<SourceFile> ReadFiles(string path)
    {
        var files = Directory.GetFiles(path, "*.cs");
        var sourceFiles = new List<SourceFile>();

        foreach (var file in files)
        {
            var codeLines = new List<string>();
            var namespaces = new List<string>();

            var lines = File.ReadAllLines(file);
            
            foreach (var line in lines)
            {
                if (line.StartsWith("using "))
                {
                    namespaces.Add(line);
                    continue;
                }

                if (line.StartsWith("namespace ") || line.StartsWith("{") || line.StartsWith("}"))
                {
                    continue;
                }

                codeLines.Add(line);
            }

            var sourceFile = new SourceFile(Path.GetFileName(file), namespaces, codeLines);
            sourceFiles.Add(sourceFile);
        }


        return sourceFiles;
    }
}