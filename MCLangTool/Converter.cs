using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stealthware.MCLangTool
{
    class Converter
    {
        public const string ASSET_DIR = @"\src\main\resources\assets\";
        public const string LANG_DIR = @"\lang\";
        public const string REF_LANG = "en_US";

        public string DevDir { get; set; }
        public string TranslationDir { get; set; }
        public IList<Tuple<string, string>> ModList { get; set; }

        public Converter()
        {
            ModList = new List<Tuple<string, string>>();
        }

        public void Convert(IOFormat input, IOFormat output)
        {
            Console.WriteLine("Starting conversion ({0} to {1})", input, output);

            foreach (var mod in ModList)
            {
                Console.WriteLine("Mod: " + mod.Item1);

                Dictionary<string, Dictionary<string, string>> data = null;

                switch (input)
                {
                    case IOFormat.Lang:
                        Console.WriteLine("Reading and processing Lang files...");
                        data = Lang.ReadFiles(DevDir, mod);
                        break;
                    case IOFormat.CSV:
                        Console.WriteLine("Reading and processing CSV file...");
                        data = CSV.ReadFile(TranslationDir, mod);
                        break;
                    case IOFormat.GSheet:
                        Console.WriteLine("NOT SUPPORTED");
                        break;
                }

                switch (output)
                {
                    case IOFormat.Lang:
                        Console.WriteLine("Writing Lang files...");
                        Lang.WriteFiles(DevDir, mod, data);
                        break;
                    case IOFormat.CSV:
                        Console.WriteLine("Writing combined CSV file...");
                        CSV.WriteFile(TranslationDir, mod, data);
                        break;
                    case IOFormat.GSheet:
                        Console.WriteLine("NOT SUPPORTED");
                        break;
                }

                Console.WriteLine();
            }

            Console.WriteLine("Conversion Complete");
        }
    }
}
