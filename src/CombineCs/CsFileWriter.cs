using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CombineCs;

static internal class CsFileWriter
{
    public static void CombineIntoSingleFile(List<SourceFile> sourceFiles)
    {
        var builder = new StringBuilder();

        var namespaces = sourceFiles
            .SelectMany(f => f.Namespaces)
            .Distinct()
            .OrderBy(f => f)
            .ToArray();

        foreach (var ns in namespaces)
        {
            builder.AppendLine(ns);
        }
        builder.AppendLine();

        const string outputNamespace = "CombineCs";
        builder.AppendFormat("namespace {0}\r\n", outputNamespace);
        builder.Append("{");

        foreach (var sourceFile in sourceFiles.OrderBy(f => f, new FirstLineComparer()))
        {
            builder.AppendLine();

            foreach (var processedLine in sourceFile.ProcessedLines)
            {
                builder.AppendLine(processedLine);
            }
        }
        builder.AppendLine("}");

        var outputFile = @"C:\.development\personal\RestLib\src\CombineCs\combined.cs";
        File.Delete(outputFile);
        File.WriteAllText(outputFile, builder.ToString());
    }
}