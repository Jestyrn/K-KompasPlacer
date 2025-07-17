using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TesterModule
{
    public class SetReady
    {
        // открыть файл как тхт
        // внести изменения Регекс
        // сохранить и закрыть

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
