using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stealthware.MCLangTool
{
    internal class Lang
    {
        private const string FILE_EXT = ".lang";

        public static Dictionary<string, Dictionary<string, string>> ReadFiles(string devDir, Tuple<string, string> mod)
        {
            IList<string> files = Directory.GetFiles(devDir + mod.Item1 + Converter.ASSET_DIR + mod.Item2 + Converter.LANG_DIR,
                "*" + FILE_EXT, SearchOption.TopDirectoryOnly);

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

        public static void WriteFiles(string devDir, Tuple<string, string> mod, Dictionary<string, Dictionary<string, string>> languages)
        {
            foreach (var item in languages)
            {
                string fileName = devDir + mod.Item1 + Converter.ASSET_DIR + mod.Item2 + Converter.LANG_DIR + item.Key + FILE_EXT;
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

    }
}
