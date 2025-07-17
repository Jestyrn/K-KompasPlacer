using netDxf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace TesterModule
{
    public class TextProcessor
    {
        private static List<TextInfo> CountText;
        private static List<TextInfo> Text;

        public static List<TextInfo> GetCounterText(string path)
        {
            var dxfLines = File.ReadAllLines(path);
            CountText = new List<TextInfo>();

            for (int i = 0; i < dxfLines.Length; i++)
            {
                if (dxfLines[i].Trim().ToLower().Contains("mtext") && i + 30 < dxfLines.Length)
                {
                    if (dxfLines[i + 30].Trim().ToLower().Contains("шт"))
                    {
                        string name = dxfLines[i + 30].Trim();
                        double.TryParse(dxfLines[i + 14].Trim().Replace(".", ","), out double first);
                        double.TryParse(dxfLines[i + 16].Trim().Replace(".", ","), out double second);

                        CountText.Add(new TextInfo
                        {
                            Name = name,
                            Vect = new Vector2(first, second)
                        });
                    }
                }
            }

            CountText = CountText.Where(x => x.Name.Contains("-")).ToList();
            // 643

            return CountText;
        }

        public static List<TextInfo> GetText(string path)
        {
            var dxfLines = File.ReadAllLines(path);
            Text = new List<TextInfo>();

            for (int i = 0; i < dxfLines.Length; i++)
            {
                if (dxfLines[i].Trim().Contains("KTEXTSTYLE") && i - 2 > 0)
                {
                    if (!dxfLines[i - 2].Trim().ToLower().Contains("шт"))
                    {
                        string name = dxfLines[i - 2].Trim();
                        double.TryParse(dxfLines[i - 18].Trim().Replace(".", ","), out double first);
                        double.TryParse(dxfLines[i - 16].Trim().Replace(".", ","), out double second);

                        Text.Add(new TextInfo
                        {
                            Name = name,
                            Vect = new Vector2(first, second)
                        });
                    }
                }
            }

            Text = Text.Where(x => x.Name.Contains("-") & !x.Name.Contains("{") & !x.Name.Contains("}")).ToList();
            // 1480

            return Text;
        }
    }

    public class TextInfo()
    {
        // сдлеать поиск "шт", начать делать перерасчет дублей
        // если встретилось "Р-2 5 шт" и "Р-2 1 шт" - сделать единую "Р-2 6 шт"

        public string Name;
        public Vector2 Vect;
    }
}
