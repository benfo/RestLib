using System.Collections.Generic;

namespace CombineCs
{
    internal class FirstLineComparer : IComparer<SourceFile>
    {
        public int Compare(SourceFile x, SourceFile y)
        {
            if (x.ProcessedLines.Count == 0 || y.ProcessedLines.Count == 0)
                return 0;

            var firstLineX = x.ProcessedLines[0];
            var firstLineY = y.ProcessedLines[0];

            if (firstLineX.Contains("enum") && firstLineY.Contains("enum")) return 0;
            if (firstLineX.Contains("enum")) return -1;
            if (firstLineY.Contains("enum")) return 1;

            if (firstLineX.Contains("interface") && firstLineY.Contains("interface")) return 0;
            if (firstLineX.Contains("interface")) return -1;
            if (firstLineY.Contains("interface")) return 1;

            return 0;
        }
    }
}