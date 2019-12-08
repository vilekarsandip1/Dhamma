using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace FindReplace
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var sMainContent = File.ReadAllText(ConfigurationManager.AppSettings["MainStringsFilePath"]);
                Dictionary<string, string> transDictionary;

                using (var rtf = new RichTextBox())
                {
                    var json = File.ReadAllText(ConfigurationManager.AppSettings["TranslationFilePath"]);
                    var parsed = JObject.Parse(json);

                    string selectToken = ConfigurationManager.AppSettings["selectToken"] + ".translations.*";

                    var squery1 = parsed.SelectTokens(selectToken);

                    transDictionary = new Dictionary<string, string>();
                    string Key = string.Empty;
                    string Value = string.Empty;

                    foreach (var item in squery1)
                    {
                        foreach (JProperty x in (JToken)item)
                        { // if 'obj' is a JObject
                            string key = x.Name;
                            JToken value = x.Value;

                            if (key == "string")
                            {
                                Key = value.ToString();
                            }
                            if (key == "translation")
                            {
                                Value = value.ToString();

                                if (!transDictionary.ContainsKey(Key))
                                {
                                    transDictionary.Add(Key, Value);
                                }
                            }
                        }
                    }
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
