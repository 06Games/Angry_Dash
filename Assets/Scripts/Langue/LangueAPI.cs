﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;

namespace AngryDash.Language
{
    public class LangueAPI
    {
        /// <summary> The selected language (English, French, ...) </summary>
        public static string selectedLanguage
        {
            get { return ConfigAPI.GetString("Language"); }
            set
            {
                ConfigAPI.SetString("Language", value);
                CacheManager.Dictionary.dictionary.Remove("language");
            }
        }

        /// <summary> Get a text in the current language </summary>
        /// <param name="category">Text's Category (native, mod name...)</param>
        /// <param name="id">ID of the text</param>
        /// <param name="dontExists">Text to display if the ID doesn't exists</param>
        public static string Get(string category, string id, string dontExists) { return Get(category, id, dontExists, new string[0]); }
        /// <summary> Get a text in the current language </summary>
        /// <param name="category">Text's Category (native, mod name...)</param>
        /// <param name="id">ID of the text</param>
        /// <param name="dontExists">Text to display if the ID doesn't exists</param>
        /// <param name="arg">Text parsing arguments</param>
        public static string Get(string category, string id, string dontExists, params double[] arg) { return Get(category, id, dontExists, arg.Select(x => x.ToString()).ToArray()); }
        /// <summary> Get a text in the current language </summary>
        /// <param name="category">Text's Category (native, mod name...)</param>
        /// <param name="id">ID of the text</param>
        /// <param name="dontExists">Text to display if the ID doesn't exists</param>
        /// <param name="arg">Text parsing arguments</param>
        public static string Get(string category, string id, string dontExists, params float[] arg) { return Get(category, id, dontExists, arg.Select(x => x.ToString()).ToArray()); }
        /// <summary> Get a text in the current language </summary>
        /// <param name="category">Text's Category (native, mod name...)</param>
        /// <param name="id">ID of the text</param>
        /// <param name="dontExists">Text to display if the ID doesn't exists</param>
        /// <param name="arg">Text parsing arguments</param>
        public static string Get(string category, string id, string dontExists, params string[] arg)
        {
            if (!Load(category).TryGetValue(id, out string c)) c = dontExists; //If nothing is found, return the text of the variable "dontExists"
            for (int i = 0; i < arg.Length; i++) { if (c != null) c = c.Replace("[" + i + "]", arg[i]); } //Insert the arguments in the text
            return c.Format(); //Returns the text after formatting (line breaks, tabs, ...)
        }

        public static Dictionary<string, string> Load(string category, string persistentPath = null)
        {
            var cache = new CacheManager.Cache("language");
            if (!cache.ValueExist(category))
            {
                var dic = new Dictionary<string, string>();
                for (int i = 0; i < 4; i++)
                {
                    string RP = "";
                    string Language = "";
                    if (i == 0) { RP = ConfigAPI.GetString("ressources.pack"); Language = ConfigAPI.GetString("Language"); } //First pass: Get all texts in the user's language and in the selected RP
                    else if (i == 1) { RP = "default"; Language = ConfigAPI.GetString("Language"); } //Second pass: Get all texts in the user's language and in the default RP
                    else if (i == 2) { RP = ConfigAPI.GetString("ressources.pack"); Language = "English"; } //Third pass: Get all texts in English and in the selected RP
                    else if (i == 3) { RP = "default"; Language = "English"; } //Fourth pass: Get all texts in English and in the default RP

                    if (string.IsNullOrEmpty(persistentPath)) persistentPath = UnityEngine.Application.persistentDataPath;
                    string path = persistentPath + "/Ressources/" + RP + "/languages/" + category.TrimEnd('/') + "/" + Language + ".lang";
                    if (File.Exists(path))
                    {
                        StreamReader file = new StreamReader(@path);
                        string line;
                        while ((line = file.ReadLine()) != null)
                        {
                            int equalIndex = line.IndexOf(" = ");
                            if (equalIndex < 0) continue;

                            string key = line.Substring(1, equalIndex - 1);
                            string value = line.Substring(equalIndex + 3);
                            if (!dic.ContainsKey(key)) dic.Add(key, value);
                        }
                        file.Close();
                    }
                }
                cache.Set(category, dic);
                return dic;
            }
            else return cache.Get<Dictionary<string, string>>(category);
        }
        public static async System.Threading.Tasks.Task<Dictionary<string, string>> LoadAsync(string category, string persistentPath) { return await System.Threading.Tasks.Task.Run(() => Load(category, persistentPath)); }
    }
}
