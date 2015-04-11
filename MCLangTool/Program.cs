using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stealthware.MCLangTool
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Stealthware MCLangTool v0.2");

            if (args.Length == 1 && args[0].ToLowerInvariant().Contains("help"))
            {
                Console.WriteLine("Tool to allow easy conversion of minecraft mod .lang files to CSV, and back.");
                Console.WriteLine();

                Console.WriteLine("Available Arguments (general):");
                Console.WriteLine("/DevDir=\"PATH\"");
                Console.WriteLine("\tSpecify the top level of the development directory. This should contain sub-folders for mods.");
                Console.WriteLine("\tYou must use \" if the path contains spaces!");
                Console.WriteLine("/TransDir=\"PATH\"");
                Console.WriteLine("\tSpecify the translation directory. This is where CSV files will be written.");
                Console.WriteLine("\tYou must use \" if the path contains spaces!");
                Console.WriteLine("/Mod=Name OR /Mod=Name,AssetDir");
                Console.WriteLine("\tAdd a mod to be processed. Can use multiple arguments to add more mods.");
                Console.WriteLine("\tName is used as the mod name in files and also must match the folder in DevDir.");
                Console.WriteLine("\tAssetDir can be specified if the folder in the assets path needs to be different from Name.");
                Console.WriteLine();

                Console.WriteLine("Available Arguments (task):");
                Console.WriteLine("/Input=FORMAT");
                Console.WriteLine("\tSelects the input format for the current conversion task.");
                Console.WriteLine("\tSupported Formats: Lang, CSV, GSheet");
                Console.WriteLine("/Output=FORMAT");
                Console.WriteLine("\tSelects the output format for the current conversion task.");
                Console.WriteLine("\tSupported Formats: Lang, CSV, GSheet");
                Console.WriteLine();
                return;
            }

            Converter converter = new Converter();
            IOFormat input = IOFormat.Lang;
            IOFormat output = IOFormat.CSV;

            foreach (var item in args)
            {
                if (item.StartsWith("/Input="))
                {
                    input = (IOFormat)Enum.Parse(typeof(IOFormat), item.Substring(item.IndexOf('=') + 1));
                }
                else if (item.StartsWith("/Output="))
                {
                    output = (IOFormat)Enum.Parse(typeof(IOFormat), item.Substring(item.IndexOf('=') + 1));
                }
                else if (item.StartsWith("/DevDir="))
                {
                    converter.DevDir = item.Substring(item.IndexOf('=') + 1) + Path.DirectorySeparatorChar;
                }
                else if (item.StartsWith("/TransDir="))
                {
                    converter.TranslationDir = item.Substring(item.IndexOf('=') + 1) + Path.DirectorySeparatorChar;
                }
                else if (item.StartsWith("/SheetId="))
                {
                    converter.SheetId = item.Substring(item.IndexOf('=') + 1);
                }
                else if (item.StartsWith("/Mod="))
                {
                    string modArg = item.Substring(item.IndexOf('=') + 1);
                    string[] split = modArg.Split(',');
                    if (split.Length == 1)
                    {
                        converter.ModList.Add(Tuple.Create(split[0], split[0]));
                    }
                    else if (split.Length == 2)
                    {
                        converter.ModList.Add(Tuple.Create(split[0], split[1]));
                    }
                }
            }

            converter.Convert(input, output);
        }

    }
}
