using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stealthware.MCLangTool
{
    internal class CSV
    {
        private const string FILE_EXT = ".csv";

        public static Dictionary<string, Dictionary<string, string>> ReadFile(string translationDir, Tuple<string, string> mod)
        {
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
            var lines = File.ReadAllLines(translationDir + mod.Item1 + FILE_EXT, Encoding.UTF8);

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

        public static void WriteFile(string translationDir, Tuple<string, string> mod, Dictionary<string, Dictionary<string, string>> languages)
        {
            Directory.CreateDirectory(translationDir);

            using (TextWriter tw = new StreamWriter(translationDir + mod.Item1 + FILE_EXT, false, Encoding.UTF8))
            {
                if (!languages.ContainsKey(Converter.REF_LANG))
                {
                    throw new InvalidDataException(String.Format("No '{0}' language found. Cannot Convert!", Converter.REF_LANG));
                }

                var refLang = languages[Converter.REF_LANG];
                var keys = refLang.Keys.ToList();
                keys.Sort();

                var langCodes = languages.Keys.ToList();
                langCodes.Remove(Converter.REF_LANG);
                langCodes.Sort();

                // output row with column headers
                tw.Write("Key");
                tw.Write(',');
                tw.Write(GetLanguageDescription(Converter.REF_LANG));

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

        #region Helper Methods ...

        public static string GetLanguageDescription(string langCode)
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
