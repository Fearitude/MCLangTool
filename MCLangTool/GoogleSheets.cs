using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace Stealthware.MCLangTool
{
    internal class GoogleSheets
    {
        const string SCOPE = "https://spreadsheets.google.com/feeds";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        static GOAuth2RequestFactory requestFactory;

        public static void Authorise()
        {
            if (requestFactory != null)
            {
                return; // factory already created
            }

            // File with client id, client secret and possibly refresh token
            string[] clientInfo = File.ReadAllLines("googleapi.txt");

            // setup parameters for authorisation
            OAuth2Parameters parameters = new OAuth2Parameters();
            parameters.ClientId = clientInfo[0];
            parameters.ClientSecret = clientInfo[1];
            parameters.RedirectUri = REDIRECT_URI;
            parameters.Scope = SCOPE;

            // check if we have already been authorised and have a refresh token
            if (clientInfo.Length >= 3)
            {
                parameters.RefreshToken = clientInfo[2];

                // request a new access token
                OAuthUtil.RefreshAccessToken(parameters);

                // create factory for spreadsheet requests
                requestFactory = new GOAuth2RequestFactory(null, "MCLangTool", parameters);

                return;
            }

            // otherwise, request authorisation
            string url = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
            Console.WriteLine("You must authorise MCLangTool to access Google Docs by logging into your account.");
            Console.WriteLine("Press return to open your browser now.");
            Console.ReadLine();
            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to open your browser, please visit the url below.");
                Console.WriteLine(url);
            }

            Console.WriteLine("Once completed, please paste in your access code and press return.");
            parameters.AccessCode = Console.ReadLine();

            // Get the Access Token
            OAuthUtil.GetAccessToken(parameters);

            // keep the refresh token for later
            File.WriteAllLines("googleapi.txt", new[] { parameters.ClientId, parameters.ClientSecret, parameters.RefreshToken });

            // create factory for spreadsheet requests
            requestFactory = new GOAuth2RequestFactory(null, "MCLangTool", parameters);
        }

        public static Dictionary<string, Dictionary<string, string>> ReadSheet(string sheetId, Tuple<string, string> mod)
        {
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

            SpreadsheetsService service = new SpreadsheetsService("MCLangTool");
            service.RequestFactory = requestFactory;

            // get the spreadsheet
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Uri = new Uri("https://spreadsheets.google.com/feeds/spreadsheets/private/full/" + sheetId);
            SpreadsheetFeed feed = service.Query(query);

            if (feed.Entries.Count != 1)
            {
                throw new Exception("Google Docs spreadsheet not found. Cannot Convert!");
            }

            SpreadsheetEntry spreadsheet = (SpreadsheetEntry)feed.Entries[0];

            // get the sheet
            WorksheetEntry worksheet = FindWorkSheet(mod.Item1, spreadsheet);
            if (worksheet == null)
            {
                throw new Exception("Worksheet not found. Cannot Convert!");
            }

            // fetch all cells
            CellFeed cellFeed = worksheet.QueryCellFeed(ReturnEmptyCells.yes);

            List<string> languages = new List<string>();
            string lastKey = null;
            foreach (CellEntry cell in cellFeed.Entries)
            {
                // get languages from first line
                if (cell.Row == 1)
                {
                    if (cell.Column > 1)
                    {
                        string langCode = cell.Value.Substring(cell.Value.LastIndexOf('-') + 2);
                        languages.Add(langCode);

                        data.Add(langCode, new Dictionary<string, string>());
                    }
                    continue;
                }

                if (cell.Value == "")
                {
                    continue; // skip blank cells
                }

                if (cell.Column == 1)
                {
                    // read a key
                    lastKey = cell.Value;
                }
                else
                {
                    // read a value
                    string langCode = languages[(int)cell.Column - 2];
                    data[langCode].Add(lastKey, cell.Value);
                }
            }

            return data;
        }

        public static void WriteSheet(string sheetId, Tuple<string, string> mod, Dictionary<string, Dictionary<string, string>> languages)
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

            SpreadsheetsService service = new SpreadsheetsService("MCLangTool");
            service.RequestFactory = requestFactory;

            // get the spreadsheet
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Uri = new Uri("https://spreadsheets.google.com/feeds/spreadsheets/private/full/" + sheetId);
            SpreadsheetFeed feed = service.Query(query);

            if (feed.Entries.Count != 1)
            {
                throw new Exception("Google Docs spreadsheet not found. Cannot Convert!");
            }

            SpreadsheetEntry spreadsheet = (SpreadsheetEntry)feed.Entries[0];

            // delete the sheet for this mod
            WorksheetEntry worksheet = FindWorkSheet(mod.Item1, spreadsheet);
            if (worksheet != null)
            {
                worksheet.Delete();
            }

            // then create a new one
            int rowCount = GetExpectedRowCount(keys) + 2;
            CreateWorkSheet(mod.Item1, spreadsheet.Worksheets, langCodes.Count + 2, rowCount, service);

            // get new sheet
            worksheet = FindWorkSheet(mod.Item1, spreadsheet);
            if (worksheet == null)
            {
                throw new Exception("Failed to create new worksheet. Cannot Convert!");
            }

            // fetch all cells
            Dictionary<Tuple<uint, uint>, CellEntry> cellMap = new Dictionary<Tuple<uint, uint>, CellEntry>();

            CellFeed cellFeed = worksheet.QueryCellFeed(ReturnEmptyCells.yes);
            foreach (CellEntry cell in cellFeed.Entries)
            {
                cellMap.Add(Tuple.Create(cell.Column, cell.Row), cell);
            }

            // populate header cells
            SetCellValue(cellMap, 1, 1, "Key");
            SetCellValue(cellMap, 2, 1, CSV.GetLanguageDescription(Converter.REF_LANG));
            uint column = 3;
            foreach (var lang in langCodes)
            {
                SetCellValue(cellMap, column++, 1, CSV.GetLanguageDescription(lang));
            }

            // populate key and language value cells
            string lastPrefix = null;
            uint row = 3;
            foreach (var key in keys)
            {
                string prefix = key.Split('.')[0];
                if (lastPrefix != null && lastPrefix != prefix)
                {
                    row++; // add blank lines between groups
                }
                lastPrefix = prefix;

                SetCellValue(cellMap, 1, row, key);
                SetCellValue(cellMap, 2, row, refLang[key]);

                column = 3;
                foreach (var lang in langCodes)
                {
                    string value;
                    if (languages[lang].ContainsKey(key))
                    {
                        value = languages[lang][key];
                    }
                    else
                    {
                        value = refLang[key];
                    }
                    SetCellValue(cellMap, column++, row, value);
                }
                row++;
            }
        }

        private static int GetExpectedRowCount(List<string> keys)
        {
            string lastPrefix = null;
            int blankCount = 0;
            foreach (var key in keys)
            {
                string prefix = key.Split('.')[0];
                if (lastPrefix != null && lastPrefix != prefix)
                {
                    blankCount++; // add blank lines between groups
                }
                lastPrefix = prefix;
            }
            return keys.Count + blankCount;
        }

        private static void SetCellValue(Dictionary<Tuple<uint, uint>, CellEntry> cellMap, uint col, uint row, string value)
        {
            CellEntry cell = cellMap[Tuple.Create(col, row)];
            cell.InputValue = value;
            cell.Update();
        }

        private static WorksheetEntry FindWorkSheet(string name, SpreadsheetEntry spreadsheet)
        {
            WorksheetEntry worksheet = null;
            foreach (WorksheetEntry sheet in spreadsheet.Worksheets.Entries)
            {
                if (sheet.Title.Text == name)
                {
                    worksheet = sheet;
                    break;
                }
            }
            return worksheet;
        }

        private static void CreateWorkSheet(string name, WorksheetFeed wsFeed, int cols, int rows, SpreadsheetsService service)
        {
            // create a new sheet
            WorksheetEntry worksheet = new WorksheetEntry();
            worksheet.Title.Text = name;
            worksheet.Cols = (uint)cols;
            worksheet.Rows = (uint)rows;

            // insert the sheet
            service.Insert(wsFeed, worksheet);
        }
    }
}
