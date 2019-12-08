using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace FindReplace
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Dictionary<string, string> mainDictionary = new Dictionary<string, string>();
                var sMainContent = File.ReadAllText(ConfigurationManager.AppSettings["MainStringsFilePath"]);
                //mainDictionary = ExtractFromFile(sMainContent, "/*", "*/", ';', '=');

                Dictionary<string, string> transDictionary;

                using (var rtf = new RichTextBox())
                {
                    //rtf.Rtf = File.ReadAllText(ConfigurationManager.AppSettings["TranslationFilePath"]);
                    //var sTransContent = rtf.Text;

                    //transDictionary = ExtractFromFile(sTransContent, "{", "}", ',', ':');
                    
                    var json = File.ReadAllText(ConfigurationManager.AppSettings["TranslationFilePath"]);
                    var parsed = JObject.Parse(json);

                    transDictionary = ExtractFromFile(json, "{", "}", ',', ':');

                }

                foreach (var keyValue in transDictionary)
                {
                    sMainContent = sMainContent.Replace(keyValue.Key, keyValue.Value);
                }

                var sAppPath = AppDomain.CurrentDomain.BaseDirectory;
                //System.IO.File.WriteAllText(sAppPath + @"\Main_Translated_" + DateTime.Now.ToFileTime() + ".strings", sMainContent);
                System.IO.File.WriteAllText(ConfigurationManager.AppSettings["TranslatedMainStringsFilePath"] + @"\Main_Translated_" + DateTime.Now.ToFileTime() + ".strings", sMainContent);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private static Dictionary<string, string> ExtractFromFile(string fileContent, string startString, string endString, char charSplitter, char charSplitOn)
        {
            var content = fileContent;
            var s = string.Empty;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            var sBlocks = ExtractFromBody(content, startString, endString);

            foreach (var sBlock in sBlocks)
            {
                var dictionary1 = SplitString(sBlock, charSplitter, charSplitOn);

                try
                {
                    if (!dictionary.ContainsKey(dictionary1.ElementAt(1).Value))
                    {
                        dictionary.Add(dictionary1.ElementAt(1).Value, dictionary1.ElementAt(2).Value);
                    }
                }
                catch (Exception ex)
                {
                    //log errors to log file
                    LogError(ex, "---" + sBlock + "---");
                }
            }

            return dictionary;
        }
        private static IEnumerable<string> ExtractFromBody(string body, string start, string end)
        {
            var matched = new List<string>();
            var exit = false;
            while (!exit)
            {
                if (body == null) continue;
                var indexStart = body.IndexOf(start);

                if (indexStart != -1)
                {
                    var indexEnd = indexStart + body.Substring(indexStart).IndexOf(end);

                    matched.Add(body.Substring(indexStart + start.Length, indexEnd - indexStart - start.Length));

                    body = body.Substring(indexEnd + end.Length);
                }
                else
                {
                    exit = true;
                }
            }

            return matched;
        }

        public static Dictionary<string, string> SplitString(string input, char charSplitter, char charSplitOn)
        {
            var output = input
                .Split(charSplitter)
                .Select(part => part.Split(charSplitOn))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);

            return output;
        }

        private static void LogError(Exception ex, string sExtraError = "")
        {
            var sPath = AppDomain.CurrentDomain.BaseDirectory + @"\ErrorLog.txt";
            var message = $"Time: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}";

            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += $"Message: {sExtraError}";
            message += Environment.NewLine;
            message += $"Message: {ex.Message}";
            message += Environment.NewLine;
            message += $"StackTrace: {ex.StackTrace}";
            message += Environment.NewLine;
            message += $"Source: {ex.Source}";
            message += Environment.NewLine;
            message += $"TargetSite: {ex.TargetSite}";
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;

            using (var writer = new StreamWriter(sPath, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
    }
}
