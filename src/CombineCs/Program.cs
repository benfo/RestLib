namespace CombineCs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var sourceFiles = CsFileReader.ReadFiles(@"C:\.development\personal\RestLib\src\RestLib\");

            CsFileWriter.CombineIntoSingleFile(sourceFiles);
        }
    }
}