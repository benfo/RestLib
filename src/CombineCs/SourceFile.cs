using System.Collections.Generic;

namespace CombineCs
{
    internal class SourceFile
    {
        public SourceFile(string fileName, List<string> namespaces, List<string> codeLines)
        {
            FileName = fileName;
            ProcessedLines = codeLines;
            Namespaces = namespaces;

            TrimEmptyStartingLines();
        }

        private void TrimEmptyStartingLines()
        {
            if (ProcessedLines.Count == 0)
                return;

            while (string.IsNullOrWhiteSpace(ProcessedLines[0]))
            {
                ProcessedLines.RemoveAt(0);
            }
        }

        public List<string> ProcessedLines { get; private set; }

        public List<string> Namespaces { get; private set; }

        public string FileName { get; private set; }
    }
}