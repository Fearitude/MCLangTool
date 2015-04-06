using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCLangTool
{
    class Program
    {
        const string DEV_DIR = @"C:\Users\Martin\Desktop\minecraft-development\new dev env\";
        const string TRANSLATION_DIR = @"C:\Users\Martin\Desktop\minecraft-development\new dev env\translations\";
        const string ASSET_DIR = @"\src\main\resources\assets\";
        const string LANG_DIR = @"\lang\";
        const string REF_LANG = "en_US";
        static readonly List<Tuple<string, string>> MOD_LIST = new List<Tuple<string, string>>();

        static Program()
        {
            MOD_LIST.Add(Tuple.Create("Explodables", "explodables"));
            MOD_LIST.Add(Tuple.Create("FearTweakPack", "feartweakpack"));
            MOD_LIST.Add(Tuple.Create("MoreMeat", "moremeat2"));
            MOD_LIST.Add(Tuple.Create("SCG", "scg"));
            MOD_LIST.Add(Tuple.Create("StealthwareCore", "stealthwarecore"));
        }

        enum IOFormat
        {
            Lang, CSV, GSheet
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Stealthware MCLangTool v0.1");

            IOFormat input = IOFormat.Lang;
            IOFormat output = IOFormat.CSV;

            foreach (var item in args)
            {
                if (item.StartsWith("/Input="))
                {
                    input = (IOFormat)Enum.Parse(typeof(IOFormat), item.Substring(item.IndexOf('=')));
                }
                else if (item.StartsWith("/Output="))
                {
                    output = (IOFormat)Enum.Parse(typeof(IOFormat), item.Substring(item.IndexOf('=')));
                }
            }

            Convert(input, output);
        }

        private static void Convert(IOFormat input, IOFormat output)
        {
            Console.WriteLine("Starting conversion ({0} to {1})", input, output);

            foreach (var mod in MOD_LIST)
            {
                Console.WriteLine("Mod: " + mod.Item1);

                Dictionary<string, Dictionary<string, string>> data = null;

                Console.WriteLine("Reading and processing files...");
                switch (input)
                {
                    case IOFormat.Lang:
                        data = ReadAllFiles(mod);
                        break;
                    case IOFormat.CSV:
                        data = ReadCSVFile(mod);
                        break;
                    case IOFormat.GSheet:
                        break;
                }

                switch (output)
                {
                    case IOFormat.Lang:
                        Console.WriteLine("Writing Language files...");
                        OutputLangFiles(mod, data);
                        break;
                    case IOFormat.CSV:
                        Console.WriteLine("Writing combined CSV file...");
                        OutputCSVFile(mod, data);
                        break;
                    case IOFormat.GSheet:
                        break;
                }

                Console.WriteLine();
            }

            Console.WriteLine("Conversion Complete");
        }

        private static Dictionary<string, Dictionary<string, string>> ReadAllFiles(Tuple<string, string> mod)
        {
            IList<string> files = Directory.GetFiles(DEV_DIR + mod.Item1 + ASSET_DIR + mod.Item2 + LANG_DIR, "*.lang", SearchOption.TopDirectoryOnly);

            Dictionary<string, Dictionary<string, string>> languages = new Dictionary<string, Dictionary<string, string>>();
            foreach (var file in files)
            {
                string langCode = Path.GetFileNameWithoutExtension(file);
                string[] lines = File.ReadAllLines(file, Encoding.UTF8);

                Dictionary<string, string> data = new Dictionary<string, string>();
                foreach (var line in lines)
                {
                    if (line.Length == 0)
                    {
                        // skip blank lines
                        continue;
                    }

                    string[] split = line.Split('=');
                    if (split.Length == 2)
                    {
                        string key = split[0];
                        string text = split[1];

                        if (data.ContainsKey(key))
                        {
                            throw new InvalidDataException(String.Format("Duplicate key ({0}) found in file ({1}). Cannot Convert!", key, file));
                        }

                        data.Add(key, text);
                    }
                    else
                    {
                        throw new InvalidDataException(String.Format("Invalid line found in file ({0}). Cannot Convert!", file));
                    }
                }

                languages.Add(langCode, data);
            }
            return languages;
        }

        private static void OutputCSVFile(Tuple<string, string> mod, Dictionary<string, Dictionary<string, string>> languages)
        {
            using (TextWriter tw = new StreamWriter(TRANSLATION_DIR + mod.Item1 + ".csv", false, Encoding.UTF8))
            {
                if (!languages.ContainsKey(REF_LANG))
                {
                    throw new InvalidDataException(String.Format("No '{0}' language file found. Cannot Convert!", REF_LANG));
                }

                var refLang = languages[REF_LANG];
                var keys = refLang.Keys.ToList();
                keys.Sort();

                var langCodes = languages.Keys.ToList();
                langCodes.Remove(REF_LANG);
                langCodes.Sort();

                // output row with column headers
                tw.Write("Key");
                tw.Write(',');
                tw.Write(GetLanguageDescription(REF_LANG));

                foreach (var lang in langCodes)
                {
                    tw.Write(',');
                    tw.Write(GetLanguageDescription(lang));
                }
                tw.WriteLine();
                tw.WriteLine();

                // output keys and language values
                string lastPrefix = null;
                foreach (var key in keys)
                {
                    string prefix = key.Split('.')[0];
                    if (lastPrefix != null && lastPrefix != prefix)
                    {
                        tw.WriteLine(); // add blank lines between groups
                    }
                    lastPrefix = prefix;

                    tw.Write(key);
                    tw.Write(',');
                    tw.Write(refLang[key]);

                    foreach (var lang in langCodes)
                    {
                        tw.Write(',');
                        if (languages[lang].ContainsKey(key))
                        {
                            tw.Write(languages[lang][key]);
                        }
                        else
                        {
                            tw.Write(refLang[key]);
                        }
                    }
                    tw.WriteLine();
                }
            }
        }

        private static Dictionary<string, Dictionary<string, string>> ReadCSVFile(Tuple<string, string> mod)
        {
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
            var lines = File.ReadAllLines(TRANSLATION_DIR + mod.Item1 + ".csv", Encoding.UTF8);

            // get languages from first line
            List<string> languages = new List<string>();
            string[] line = lines[0].Split(',');
            for (int i = 1; i < line.Length; i++)
            {
                string langCode = line[i].Substring(line[i].LastIndexOf('-') + 2);
                languages.Add(langCode);

                data.Add(langCode, new Dictionary<string, string>());
            }

            // read keys and values
            for (int i = 1; i < lines.Length; i++)
            {
                line = lines[i].Split(',');
                if (line.Length == 1)
                {
                    continue; // skip blank lines
                }

                string key = line[0];
                for (int j = 1; j < line.Length; j++)
                {
                    string langCode = languages[j - 1];
                    data[langCode].Add(key, line[j]);
                }
            }

            return data;
        }

        private static void OutputLangFiles(Tuple<string, string> mod, Dictionary<string, Dictionary<string, string>> languages)
        {
            foreach (var item in languages)
            {
                string fileName = DEV_DIR + mod.Item1 + ASSET_DIR + mod.Item2 + LANG_DIR + item.Key + ".lang";
                using (TextWriter tw = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    foreach (var item2 in item.Value)
                    {
                        tw.Write(item2.Key);
                        tw.Write('=');
                        tw.Write(item2.Value);
                        tw.WriteLine();
                    }
                }
            }
        }

        #region Helper Methods ...

        private static string GetLanguageDescription(string langCode)
        {
            try
            {
                string code = langCode.Replace("_", "-");
                CultureInfo culture = CultureInfo.GetCultureInfo(code);
                return culture.DisplayName + " - " + langCode;
            }
            catch (ArgumentException)
            {
            }
            return langCode;
        }

        #endregion
    }
}
