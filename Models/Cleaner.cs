using System.Text.RegularExpressions;

namespace Models
{
    public class Cleaner
    {
        public static void Setup(string path)
        {
            var reg = new Regex(@"(\s?)(\d+)\s*(шт)");
            var lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Regex.Replace(lines[i], @"\\[A-Za-z][^;]*;", "").Trim();
            }

            File.WriteAllLines(path, lines);
        }
    }
}
