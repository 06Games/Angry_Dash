using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;

namespace FileFormat
{
    namespace JSON
    {
        public class JSON
        {
            public Newtonsoft.Json.Linq.JObject jObject;
            public JSON(string plainText)
            {
                dynamic stuff = JsonConvert.DeserializeObject(plainText);
                if (stuff != null)
                {
                    foreach (Newtonsoft.Json.Linq.JObject jobject in stuff)
                        if (jObject == null) jObject = jobject;
                }
            }

            public Category GetCategory(string token) { if (jObject == null) return new Category(null); else return new Category(jObject.SelectToken(token)); }
        }

        public class Category
        {
            Newtonsoft.Json.Linq.JToken jToken;
            public Category(Newtonsoft.Json.Linq.JToken token) { jToken = token; }

            public Category GetCategory(string token) { if (jToken == null) return new Category(null); else return new Category(jToken.SelectToken(token)); }
            public void Delete() { if (jToken != null) jToken.Remove(); }
            public bool ContainsValues { get { if (jToken == null) return false; else return jToken.HasValues; } }

            public T Value<T>(string value) { return jToken.Value<T>(value); }
            public bool ValueExist(string value) { if (jToken == null) return false; else return jToken.Value<string>(value) != null; }
        }
    }

    namespace XML
    {
        public static class Utils
        {
            public static string ClassToXML<T>(T data)
            {
                System.Xml.Serialization.XmlSerializer _serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                var settings = new System.Xml.XmlWriterSettings
                {
                    NewLineHandling = System.Xml.NewLineHandling.Entitize
                };

                using (var stream = new StringWriter())
                using (var writer = System.Xml.XmlWriter.Create(stream, settings))
                {
                    _serializer.Serialize(writer, data);

                    return stream.ToString();
                }
            }
            public static T XMLtoClass<T>(string data)
            {
                System.Xml.Serialization.XmlSerializer _serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                if (string.IsNullOrEmpty(data))
                    return default(T);

                using (var stream = new StringReader(data))
                using (var reader = System.Xml.XmlReader.Create(stream))
                {
                    return (T)_serializer.Deserialize(reader);
                }
            }
        }

        public class XML
        {
            System.Xml.XmlDocument xmlDoc;
            public XML() { xmlDoc = new System.Xml.XmlDocument(); }
            public XML(System.Xml.XmlDocument xml) { if (xml == null) xmlDoc = new System.Xml.XmlDocument(); else xmlDoc = xml; }
            public XML(string plainText)
            {
                xmlDoc = new System.Xml.XmlDocument();
                if (!string.IsNullOrEmpty(plainText)) xmlDoc.LoadXml(plainText);
            }
            public override string ToString()
            {
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = System.Xml.XmlWriter.Create(stringWriter))
                {
                    xmlDoc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }

            public RootElement CreateRootElement(string name)
            {
                System.Xml.XmlNode xmlNode = xmlDoc.CreateElement(name);
                xmlDoc.AppendChild(xmlNode);
                return new RootElement(xmlNode);
            }
            public RootElement RootElement
            {
                get
                {
                    if (xmlDoc.DocumentElement != null) return new RootElement(xmlDoc.DocumentElement);
                    else throw new System.Exception("There is no Root Element ! Create one with CreateRootElement() function");
                }
            }
        }

        public class RootElement : Base_Collection
        {
            public RootElement(System.Xml.XmlNode xmlNode) { node = xmlNode; }

            public XML xmlFile { get { return new XML(node.OwnerDocument); } }
        }

        public class Item : Base_Collection
        {
            public Item(System.Xml.XmlNode xmlNode) { node = xmlNode; }
            public RootElement rootElement { get { return new RootElement(node.OwnerDocument.DocumentElement); } }

            public string Attribute(string key) { return node.Attributes[key].Value; }
            public void CreateAttribute(string key, string value)
            {
                System.Xml.XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(key);
                node.Attributes.Append(xmlAttribute);
                xmlAttribute.Value = value;
            }
            public void SetAttribute(string key, string value) { node.Attributes[key].Value = value; }
            public void RemoveAttribute(string key) { node.Attributes.Remove(node.Attributes[key]); }

            public string Value { get { return node.InnerText; } set { node.InnerText = value; } }
            public void Remove() { node.ParentNode.RemoveChild(node); }
        }

        public abstract class Base_Collection
        {
            public System.Xml.XmlNode node;
            public Item GetItem(string key)
            {
                System.Xml.XmlNode xmlNode = node.SelectSingleNode(key);
                if (xmlNode == null) return null;
                else return new Item(xmlNode);
            }
            public Item[] GetItems(string key)
            {
                System.Xml.XmlNodeList list = node.SelectNodes(key);
                Item[] items = new Item[list.Count];
                for (int i = 0; i < items.Length; i++)
                    items[i] = new Item(list[i]);
                if (items.Length > 0) return items;
                else return null;
            }
            public Item GetItemByAttribute(string key, string attribute, string attributeValue)
            {
                System.Xml.XmlNode xmlNode = node.SelectSingleNode(key + "[@" + attribute + " = '" + attributeValue + "']");
                if (xmlNode == null) return null;
                else return new Item(xmlNode);
            }
            public Item[] GetItemsByAttribute(string key, string attribute, string attributeValue)
            {
                System.Xml.XmlNodeList list = node.SelectNodes(key + "[@" + attribute + " = '" + attributeValue + "']");
                Item[] items = new Item[list.Count];
                for (int i = 0; i < items.Length; i++)
                    items[i] = new Item(list[i]);
                if (items.Length > 0) return items;
                else return null;
            }
            public Item CreateItem(string key)
            {
                System.Xml.XmlNode xmlNode = node.OwnerDocument.CreateElement(key);
                node.AppendChild(xmlNode);
                return new Item(xmlNode);
            }
        }
    }

    namespace INI
    {
        public class INI
        {
            Nini.Config.IConfigSource source;
            public INI(string path)
            {
                source = new Nini.Config.IniConfigSource(path);
                source.AutoSave = true;
            }
            public Category GetCategory(string token) { if (source == null) return new Category(null); else return new Category(source.Configs[token]); }
        }

        public class Category
        {
            Nini.Config.IConfig config;
            public Category(Nini.Config.IConfig iConfig) { config = iConfig; }


            public bool ContainsValues { get { if (config == null) return false; else return config.GetValues().Length > 0; } }
            public void Delete() { if (config != null) config.ConfigSource.Configs.Remove(config); }

            public T Value<T>(string key) { if (config == null) return default(T); else return Tools.StringExtensions.ParseTo<T>(config.Get(key)); }
            public T Value<T>(string key, string defaultValue) { if (config == null) return default(T); else return Tools.StringExtensions.ParseTo<T>(config.Get(key, defaultValue)); }
            public bool ValueExist(string key) { if (config == null) return false; else return config.Get(key) != null; }
            public void SetValue(string key, object value) { if (config != null) config.Set(key, value); }
            public void RemoveValue(string key) { if (config != null) config.Remove(key); }
        }
    }

    public class Binary
    {
        string chain = "";
        public Binary(byte[] data)
        {
            string binary = string.Join("", data.Select(byt => System.Convert.ToString(byt, 2).PadLeft(8, '0')));
            string onlyNumbers = System.Text.RegularExpressions.Regex.Replace(binary, "[0-9]", "");
            if (string.IsNullOrEmpty(onlyNumbers)) chain = binary;
            else throw new System.ArgumentException("The specified string is not binary");
        }
        Binary(string data) { chain = data; }
        public static Binary Parse(string data) { return new Binary(data.Replace(" ", "")); }

        public override string ToString()
        {
            string str = "";
            for (var i = 0; i < chain.Length; i += 8)
            {
                if (i < 8) str = chain.Substring(i, Mathf.Min(8, chain.Length - i));
                else str = string.Join(" ", str, chain.Substring(i, Mathf.Min(8, chain.Length - i)));
            }
            return str;
        }
        public string Decode() { return Decode(Encoding.UTF8); }
        public string Decode(Encoding encoding)
        {
            System.Collections.Generic.List<byte> byteList = new System.Collections.Generic.List<byte>();

            for (int i = 0; i < chain.Length; i += 8)
            {
                byteList.Add(System.Convert.ToByte(chain.Substring(i, 8), 2));
            }
            return encoding.GetString(byteList.ToArray());
        }
    }

    public static class ZIP
    {
        /// <summary>
        /// Compress a folder
        /// </summary>
        /// <param name="unzipPath">Path to the folder to compress</param>
        /// <param name="zipPath">Path where the zip file will be saved</param>
        public static void Compress(string unzipPath, string zipPath)
        {
            ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip
            {
                CreateEmptyDirectories = true
            };
            fastZip.CreateZip(zipPath, unzipPath, true, null);
        }

        public static byte[] Compress(byte[] input)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new System.IO.Compression.DeflateStream(compressStream, System.IO.Compression.CompressionMode.Compress))
            {
                new MemoryStream(input).CopyTo(compressor);
                compressor.Close();
                return compressStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress a folder
        /// </summary>
        /// <param name="zipPath">ZIP file location</param>
        /// <param name="unzipPath">Path where the zip file will be extracted</param>
        public static void Decompress(string zipPath, string unzipPath)
        {
            ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip
            {
                CreateEmptyDirectories = true
            };
            fastZip.ExtractZip(zipPath, unzipPath, null);
        }


        public static byte[] Decompress(byte[] input)
        {
            var output = new MemoryStream();

            using (var compressStream = new MemoryStream(input))
            using (var decompressor = new System.IO.Compression.DeflateStream(compressStream, System.IO.Compression.CompressionMode.Decompress))
                decompressor.CopyTo(output);

            output.Position = 0;
            return output.ToArray();
        }
    }

    public static class GZIP
    {
        /// <summary>
        /// Compress a zip
        /// </summary>
        /// <param name="zipPath">ZIP file location</param>
        /// <param name="gzipFile">Path where the gzip file will be saved, null if zipPath.gz</param>
        public static void Compress(string zipFile, string gzipFile = null)
        {
            FileInfo fileToBeGZipped = new FileInfo(zipFile);
            if (gzipFile == null) gzipFile = string.Concat(fileToBeGZipped.FullName, ".gz");
            FileInfo gzipFileName = new FileInfo(gzipFile);

            using (FileStream fileToBeZippedAsStream = fileToBeGZipped.OpenRead())
            {
                using (FileStream gzipTargetAsStream = gzipFileName.Create())
                {
                    using (System.IO.Compression.GZipStream gzipStream = new System.IO.Compression.GZipStream(gzipTargetAsStream, System.IO.Compression.CompressionMode.Compress))
                    {
                        try
                        {
                            fileToBeZippedAsStream.CopyTo(gzipStream);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError(ex.Message);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Compress a zip array
        /// </summary>
        /// <param name="zip">zip byte array</param>
        public static byte[] Compress(byte[] zip)
        {
            using (var outStream = new MemoryStream())
            {
                using (var tinyStream = new System.IO.Compression.GZipStream(outStream, System.IO.Compression.CompressionMode.Compress))
                using (var mStream = new MemoryStream(zip))
                    mStream.CopyTo(tinyStream);

                return outStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress a gzip
        /// </summary>
        /// <param name="gzipFile">GZIP file location</param>
        /// <param name="zipFile">Path where the zip file will be saved</param>
        public static void Extract(string gzipFile, string zipFile)
        {
            using (FileStream fileToDecompressAsStream = new FileInfo(gzipFile).OpenRead())
            {
                using (FileStream decompressedStream = File.Create(zipFile))
                {
                    using (System.IO.Compression.GZipStream decompressionStream = new System.IO.Compression.GZipStream(fileToDecompressAsStream, System.IO.Compression.CompressionMode.Decompress))
                    {
                        try
                        {
                            decompressionStream.CopyTo(decompressedStream);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError(ex.Message);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Decompress a gzip array
        /// </summary>
        /// <param name="gzip">gzip byte array</param>
        public static byte[] Extract(byte[] gzip)
        {
            using (var inStream = new MemoryStream(gzip))
            using (var bigStream = new System.IO.Compression.GZipStream(inStream, System.IO.Compression.CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream())
            {
                bigStream.CopyTo(bigStreamOut);
                return bigStreamOut.ToArray();
            }

        }
    }
}

public class Versioning
{
    public static Versioning Actual { get { return new Versioning(Application.version); } }
    public enum Sort { Latest, Equal, Oldest, Error }
    public enum SortConditions { Latest, LatestOrEqual, Equal, OldestOrEqual, Oldest }

    string[] version;
    public Versioning(float _version) { version = _version.ToString().Split(new string[] { "." }, System.StringSplitOptions.None); }
    public Versioning(string _version) { version = _version.Split(new string[] { "." }, System.StringSplitOptions.None); }

    public override string ToString() { return string.Join(".", version); }
    public bool CompareTo(Versioning compared, SortConditions conditions)
    {
        Sort sort = CompareTo(compared);
        bool lastest = conditions == SortConditions.Latest || conditions == SortConditions.LatestOrEqual;
        bool equal = conditions == SortConditions.LatestOrEqual || conditions == SortConditions.Equal || conditions == SortConditions.OldestOrEqual;
        bool oldest = conditions == SortConditions.OldestOrEqual || conditions == SortConditions.Oldest;

        if (lastest & sort == Sort.Latest) return true;
        else if (equal & sort == Sort.Equal) return true;
        else if (oldest & sort == Sort.Oldest) return true;
        else return false;
    }
    public Sort CompareTo(Versioning compared)
    {
        for (int i = 0; (i < version.Length | i < compared.version.Length); i++)
        {
            float versionNumber = 0;
            if (version.Length > i) versionNumber = float.Parse(version[i]);
            float comparedVersion = 0;
            if (compared.version.Length > i) comparedVersion = float.Parse(compared.version[i]);

            if (versionNumber > comparedVersion) return Sort.Latest;
            if (versionNumber == comparedVersion & (i >= version.Length - 1 & i >= compared.version.Length - 1)) return Sort.Equal;
            if (versionNumber < comparedVersion) return Sort.Oldest;
        }
        Debug.LogError("Can't compare versions !");
        return Sort.Error;
    }
}

public static class InspectorUtilities
{
    public static void ClearConsole()
    {
#if UNITY_EDITOR
        var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
#endif
    }
}

namespace CacheManager
{
    public class Dictionary : MonoBehaviour
    {
        public System.Collections.Generic.Dictionary<string, Cache> dictionary;
        public Dictionary()
        { dictionary = new System.Collections.Generic.Dictionary<string, Cache>(); }

        public static Dictionary Static()
        {
            if (GameObject.Find("Cache") != null)
                return GameObject.Find("Cache").GetComponent<Dictionary>();
            else
            {
                GameObject cache = new GameObject("Cache");
                DontDestroyOnLoad(cache);
                return cache.AddComponent<Dictionary>();
            }
        }
        public static bool Exist() { return GameObject.Find("Cache") != null; }

#if UNITY_EDITOR
        public string[] key;
        public Cache[] value;
        private void Update()
        {
            key = dictionary.Keys.ToArray();
            value = dictionary.Values.ToArray();
        }
#endif
    }

    [System.Serializable]
    public class Cache
    {
        System.Collections.Generic.Dictionary<string, object> dictionary;

        public Cache(string name)
        {
            Dictionary dic = Dictionary.Static();
            if (dic.dictionary.ContainsKey(name))
                dictionary = dic.dictionary[name].dictionary;
            else
            {
                dictionary = new System.Collections.Generic.Dictionary<string, object>();
                dic.dictionary.Add(name, this);
            }
        }

        public void Set(string id, object obj)
        {
            if (string.IsNullOrEmpty(id)) return;
            else if (dictionary.ContainsKey(id)) dictionary[id] = obj;
            else dictionary.Add(id, obj);
        }

        public T Get<T>(string id) { return (T)Get(id); }
        public object Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            else if (dictionary.ContainsKey(id)) return dictionary[id];
            else return null;
        }

        public bool ValueExist(string id) { return dictionary.ContainsKey(id); }
    }
}

namespace Level
{
    [System.Serializable]
    public class LevelItem
    {
        public string Name = "";
        public string Author = "";
        public string Description = "";
        public string Music = "";
        public string[] Data = new string[0];
        public LevelItem() { }
        public LevelItem(string name, string author = "", string[] data = null, string description = "", string music = "")
        {
            Name = name;
            Author = author;
            if (data == null) Data = new string[0];
            else Data = data;
            Description = description;
            Music = music;
        }

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this); }
        public static LevelItem Parse(string data) { return FileFormat.XML.Utils.XMLtoClass<LevelItem>(data); }
    }

    [System.Serializable]
    public class SongItem
    {
        public string Name = "";
        public string Artist = "";
        public string Licence = "";
        public string URL = "";
        public SongItem() { }
        public SongItem(string name, string artist = "", string licence = "", string url = "")
        {
            Name = name;
            Artist = artist;
            Licence = licence;
            URL = url;
        }

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this); }
        public static SongItem Parse(string data) { return FileFormat.XML.Utils.XMLtoClass<SongItem>(data); }
    }
}

namespace Display
{
    public static class Screen
    {
        /// <summary>
        /// Get the main screen resolution as a Vector2
        /// </summary>
        public static Vector2 Resolution
        {
            get { return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height); }
            set { UnityEngine.Screen.SetResolution((int)value.x, (int)value.y, UnityEngine.Screen.fullScreen); }
        }
    }
}

/// <summary> All function additions to Unity native classes </summary>
namespace Tools
{
    public static class StringExtensions
    {
        /// <summary>
        /// Format a string
        /// </summary>
        /// <param name="str">The string to format</param>
        /// <returns></returns>
        public static string Format(this string str)
        {
            if (str != null)
            {
                str = str.Replace("\\n", "\n");
                str = str.Replace("\\r", "\r");
                str = str.Replace("\\t", "\t");
            }
            return str;
        }

        /// <summary>
        /// Unformat a string
        /// </summary>
        /// <param name="str">The string to unformat</param>
        public static string Unformat(this string str)
        {
            if (str != null)
            {
                str = str.Replace("\n", "\\n");
                str = str.Replace("\r", "\\r");
                str = str.Replace("\t", "\\t");
            }
            return str;
        }

        /// <summary>
        /// Parse a string to an other type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        public static T ParseTo<T>(this string str) { return (T)System.Convert.ChangeType(str, typeof(T)); }

        public static byte[] ToByte(this string str) { return ToByte(str, Encoding.UTF8); }
        public static byte[] ToByte(this string str, Encoding encoding) { return encoding.GetBytes(str); }
    }

    public static class ArrayExtensions
    {
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                System.Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                System.Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }
        public static T[] RemoveAt<T>(this T[] source, int from, int to)
        {
            T[] dest = new T[source.Length - (to - from) - 1];
            if (from > 0)
                System.Array.Copy(source, 0, dest, 0, from);

            if (to < source.Length - 1)
                System.Array.Copy(source, to + 1, dest, from, source.Length - to - 1);

            return dest;
        }

        public static T[] Get<T>(this T[] list, int from, int to)
        {
            if (from < 0) throw new System.Exception("From index can not be less than 0");
            else if (to < from) throw new System.Exception("To index can not be less than From index");
            else if (to >= list.Length) throw new System.Exception("To index can not be more than the list length");

            return list.Skip(from).Take(to - from).ToArray();
        }
    }

    public delegate void BetterEventHandler(object sender, BetterEventArgs e);
    public class BetterEventArgs : System.EventArgs
    {
        public object UserState { get; }
        public BetterEventArgs() { }
        public BetterEventArgs(object userToken) { UserState = userToken; }
    }

    public static class DateExtensions
    {
        /// <summary>
        ///  Return a DateTime as a string
        /// </summary>
        /// <param name="DT"></param>
        /// <returns></returns>
        public static string Format(this System.DateTime DT)
        {
            string a = "dd'/'MM'/'yyyy";
            return DT.ToString(a);
        }
    }

    public static class ScrollRectExtensions
    {
        /// <summary>
        /// Scroll to a child of the content
        /// </summary>
        /// <param name="scroll">The ScrollRect</param>
        /// <param name="target">The child to scroll to</param>
        /// <param name="myMonoBehaviour">Any MonoBehaviour script</param>
        public static void SnapTo(this UnityEngine.UI.ScrollRect scroll, Transform target, MonoBehaviour myMonoBehaviour) { myMonoBehaviour.StartCoroutine(Snap(scroll, target)); }
        static System.Collections.IEnumerator Snap(UnityEngine.UI.ScrollRect scroll, Transform target)
        {
            float Frames = 30;
            float normalizePosition = 1 - (target.GetSiblingIndex() - 1F) / (scroll.content.childCount - 2F);
            float actualNormalizePosition = scroll.verticalNormalizedPosition;
            for (int i = 0; i < Frames; i++)
            {
                scroll.verticalNormalizedPosition = scroll.verticalNormalizedPosition + ((normalizePosition - actualNormalizePosition) / Frames);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public static class SpriteExtensions
    {
        public static Vector2 Size(this Sprite sp) { Rect rect = sp.rect; return new Vector2(rect.width, rect.height); }
    }
}

namespace MessengerExtensions
{
    /// <summary>
    /// Broadcast messages between objects and components, including inactive ones (which Unity doesn't do)
    /// </summary>
    public static class MessengerThatIncludesInactiveElements
    {

        /// <summary>
        /// Determine if the object has the given method
        /// </summary>
        private static void InvokeIfExists(this object objectToCheck, string methodName, params object[] parameters)
        {
            System.Type type = objectToCheck.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);
            if (type.GetMethod(methodName) != null)
            {
                methodInfo.Invoke(objectToCheck, parameters);
            }
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object, even if they are inactive
        /// </summary>
        public static void BroadcastToAll(this GameObject gameobject, string methodName, params object[] parameters)
        {
            MonoBehaviour[] components = gameobject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour m in components)
            {
                m.InvokeIfExists(methodName, parameters);
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object, even if they are inactive
        /// </summary>
        public static void BroadcastToAll(this Component component, string methodName, params object[] parameters)
        {
            component.gameObject.BroadcastToAll(methodName, parameters);
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its children, even if they are inactive
        /// </summary>
        public static void SendMessageToAll(this GameObject gameobject, string methodName, params object[] parameters)
        {
            MonoBehaviour[] components = gameobject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour m in components)
            {
                m.InvokeIfExists(methodName, parameters);
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its children, even if they are inactive
        /// </summary>
        public static void SendMessageToAll(this Component component, string methodName, params object[] parameters)
        {
            component.gameObject.SendMessageToAll(methodName, parameters);
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its ancestors, even if they are inactive
        /// </summary>
        public static void SendMessageUpwardsToAll(this GameObject gameobject, string methodName, params object[] parameters)
        {
            Transform tranform = gameobject.transform;
            while (tranform != null)
            {
                tranform.gameObject.BroadcastToAll(methodName, parameters);
                tranform = tranform.parent;
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its ancestors, even if they are inactive
        /// </summary>
        public static void SendMessageUpwardsToAll(this Component component, string methodName, params object[] parameters)
        {
            component.gameObject.SendMessageUpwardsToAll(methodName, parameters);
        }
    }
}

namespace Security
{
    /// <summary>
    /// Encrypting class, the returned string can be decrypt
    /// Warning : Do not use this for passwords or other sensitive elements
    /// </summary>
    public static class Encrypting
    {
        public static string Encrypt(string plainText, string KEY)
        {
            if (KEY.Length > 32)
                KEY = KEY.Substring(0, 32);
            else if (KEY.Length < 32)
            {
                for (int i = 0; i < 32 - KEY.Length; i++)
                    KEY = KEY + "X";
            }
            byte[] KEY_BYTES = Encoding.UTF8.GetBytes(KEY);

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new System.ArgumentNullException("plainText");

            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (Rijndael algorithm = Rijndael.Create())
            {
                algorithm.Key = KEY_BYTES;

                // Create a decrytor to perform the stream transform.
                var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write IV first
                            msEncrypt.Write(algorithm.IV, 0, algorithm.IV.Length);
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return System.Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText, string KEY)
        {
            if (KEY.Length > 32)
                KEY = KEY.Substring(0, 32);
            else if (KEY.Length < 32)
            {
                for (int i = 0; i < 32 - KEY.Length; i++)
                    KEY = KEY + "X";
            }
            byte[] KEY_BYTES = Encoding.UTF8.GetBytes(KEY);

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new System.ArgumentNullException("cipherText");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (Rijndael algorithm = Rijndael.Create())
            {
                algorithm.Key = KEY_BYTES;

                // Get bytes from input string
                byte[] cipherBytes = System.Convert.FromBase64String(cipherText);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    // Read IV first
                    byte[] IV = new byte[16];
                    msDecrypt.Read(IV, 0, IV.Length);

                    // Assign IV to an algorithm
                    algorithm.IV = IV;

                    // Create a decrytor to perform the stream transform.
                    var decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);

                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }

    /// <summary>
    /// Hashing class, the returned string can't be "unhashed"
    /// </summary>
    public static class Hashing
    {
        public static string SHA1(string value)
        {
            SHA1 sha = System.Security.Cryptography.SHA1.Create();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string SHA384(string value)
        {
            SHA384 sha = System.Security.Cryptography.SHA384.Create();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string SHA512(string value)
        {
            SHA512 sha = System.Security.Cryptography.SHA512.Create();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
