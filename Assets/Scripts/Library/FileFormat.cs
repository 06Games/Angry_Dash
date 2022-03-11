using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Nini.Config;
using Tools;
using UnityEngine;

namespace FileFormat
{
    public class JSON
    {
        public JObject jToken;
        public JSON(string plainText)
        {
            if (!string.IsNullOrEmpty(plainText))
            {
                try { jToken = JObject.Parse(plainText); }
                catch (Exception e) { Debug.LogError("Error parsing:\n" + plainText + "\nError details:\n" + e.Message); }
            }
        }
        public JSON(JToken token)
        {
            try { jToken = (JObject)token; }
            catch (Exception e) { Debug.LogError("Error parsing the token\nError details:\n" + e.Message); }
        }

        public JSON GetCategory(string token)
        {
            if (jToken == null) return new JSON(null);
            return new JSON(jToken.SelectToken(token));
        }
        public void Delete()
        {
            jToken?.Remove();
        }
        public bool ContainsValues { get
        {
            if (jToken == null) return false;
            return jToken.HasValues;
        } }

        public T Value<T>(string value)
        {
            if (jToken == null) return default;
            return jToken.Value<T>(value);
        }
        public bool ValueExist(string value)
        {
            return jToken?.Value<string>(value) != null;
        }

        public override string ToString() { return jToken.ToString(); }
    }

    namespace XML
    {
        public static class Utils
        {
            public static string ClassToXML<T>(T data, bool minimised = true)
            {
                var _serializer = new XmlSerializer(typeof(T));
                var settings = new XmlWriterSettings
                {
                    NewLineHandling = NewLineHandling.Entitize,
                    Encoding = Encoding.UTF8,
                    Indent = !minimised
                };

                using (var stream = new StringWriter())
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    _serializer.Serialize(writer, data);

                    return stream.ToString();
                }
            }
            public static T XMLtoClass<T>(string data)
            {
                var _serializer = new XmlSerializer(typeof(T));
                if (string.IsNullOrEmpty(data))
                    return default(T);

                using (var stream = new StringReader(data))
                using (var reader = XmlReader.Create(stream))
                {
                    return (T)_serializer.Deserialize(reader);
                }
            }

            public static bool IsValid(string xmlFile)
            {
                try { new XmlDocument().LoadXml(xmlFile); }
                catch { return false; }
                return true;
            }
        }

        public class XML
        {
            private XmlDocument xmlDoc;
            public XML() { xmlDoc = new XmlDocument(); }
            public XML(XmlDocument xml) { if (xml == null) xmlDoc = new XmlDocument(); else xmlDoc = xml; }
            public XML(string plainText)
            {
                xmlDoc = new XmlDocument();
                if (!string.IsNullOrEmpty(plainText)) xmlDoc.LoadXml(plainText);
            }
            public override string ToString() { return ToString(true); }
            public string ToString(bool minimised)
            {
                var settings = new XmlWriterSettings
                {
                    NewLineHandling = NewLineHandling.Entitize,
                    Encoding = Encoding.UTF8,
                    Indent = !minimised
                };
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                {
                    xmlDoc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }

            public RootElement CreateRootElement(string name)
            {
                XmlNode xmlNode = xmlDoc.CreateElement(name);
                xmlDoc.AppendChild(xmlNode);
                return new RootElement(xmlNode);
            }
            public RootElement RootElement
            {
                get
                {
                    if (xmlDoc.DocumentElement != null) return new RootElement(xmlDoc.DocumentElement);
                    throw new Exception("There is no Root Element ! Create one with CreateRootElement() function");
                }
            }
        }

        public class RootElement : Base_Collection
        {
            public RootElement(XmlNode xmlNode) { node = xmlNode; }

            public XML xmlFile => new XML(node == null ? null : node.OwnerDocument);
        }

        public class Item : Base_Collection
        {
            public Item(XmlNode xmlNode) { node = xmlNode; }
            public RootElement rootElement => new RootElement(node.OwnerDocument.DocumentElement);

            public string Attribute(string key) { return node.Attributes[key].Value; }
            public Item SetAttribute(string key, string value = "")
            {
                if (node.Attributes != null && node.Attributes[key] != null) //Set value
                    node.Attributes[key].Value = value;
                else
                { //Create attribute
                    var xmlAttribute = node.OwnerDocument.CreateAttribute(key);
                    node.Attributes.Append(xmlAttribute);
                    xmlAttribute.Value = value;
                }
                return this;
            }
            public Item RemoveAttribute(string key) {
                node?.Attributes.Remove(node.Attributes[key]);
                return this; }

            public Item Parent => new Item(node == null ? null : node.ParentNode);

            public T value<T>()
            {
                var v = Value;
                if (v == null) return default;
                try { return StringExtensions.ParseTo<T>(v); } catch { return default; }
            }
            public string Value
            {
                get
                {
                    return node?.InnerText;
                }
                set
                {
                    if (node == null) throw new Exception("This item does not exist! Can not set a value!\nCheck Item.Exist before calling this function.");
                    node.InnerText = value;
                }
            }
            public void Remove() { node.ParentNode.RemoveChild(node); }
        }

        public abstract class Base_Collection
        {
            public XmlNode node;
            public Item GetItem(string key)
            {
                if (node == null) return new Item(null);
                var xmlNode = node.SelectSingleNode(key);
                if (xmlNode == null) return new Item(null);
                return new Item(xmlNode);
            }
            public Item[] GetItems()
            {
                if (node == null) return new Item[0];
                var list = node.ChildNodes;
                var items = new Item[list.Count];
                for (var i = 0; i < items.Length; i++) items[i] = new Item(list[i]);
                if (items.Length > 0) return items;
                return new Item[0];
            }
            public Item[] GetItems(string key)
            {
                if (node == null) return new Item[0];
                var list = node.SelectNodes(key);
                var items = new Item[list.Count];
                for (var i = 0; i < items.Length; i++)
                    items[i] = new Item(list[i]);
                if (items.Length > 0) return items;
                return new Item[0];
            }
            public Item GetItemByAttribute(string key, string attribute, string attributeValue = "")
            {
                if (node == null) return new Item(null);
                var xmlNode = node.SelectSingleNode(key + "[@" + attribute + " = \"" + attributeValue + "\"]");
                if (xmlNode == null) return new Item(null);
                return new Item(xmlNode);
            }
            public Item[] GetItemsByAttribute(string key, string attribute, string attributeValue = "")
            {
                if (node == null) return new Item[0];
                var list = node.SelectNodes(key + "[@" + attribute + " = '" + attributeValue + "']");
                var items = new Item[list.Count];
                for (var i = 0; i < items.Length; i++)
                    items[i] = new Item(list[i]);
                if (items.Length > 0) return items;
                return null;
            }
            public Item CreateItem(string key)
            {
                if (node == null) throw new Exception("This item does not exist! Can not create a child!\nCheck Item.Exist before calling this function.");
                XmlNode xmlNode = node.OwnerDocument.CreateElement(key);
                node.AppendChild(xmlNode);
                return new Item(xmlNode);
            }

            public bool Exist => node != null;

            public IEnumerator<Item> GetEnumerator() { return GetItems().ToList().GetEnumerator(); }
            public override string ToString() { return node.OuterXml; }
        }
    }

    namespace INI
    {
        public class INI
        {
            private IConfigSource source;
            public INI(string path)
            {
                try
                {
                    source = new IniConfigSource(path);
                    source.AutoSave = true;
                }
                catch { }
            }
            public Category GetCategory(string token)
            {
                if (source == null) return new Category(null);
                return new Category(source.Configs[token]);
            }
        }

        public class Category
        {
            private IConfig config;
            public Category(IConfig iConfig) { config = iConfig; }


            public bool ContainsValues { get
            {
                if (config == null) return false;
                return config.GetValues().Length > 0;
            } }
            public void Delete()
            {
                config?.ConfigSource.Configs.Remove(config);
            }

            public T Value<T>(string key)
            {
                if (config == null) return default;
                return StringExtensions.ParseTo<T>(config.Get(key));
            }
            public T Value<T>(string key, string defaultValue)
            {
                if (config == null) return default;
                return StringExtensions.ParseTo<T>(config.Get(key, defaultValue));
            }
            public bool ValueExist(string key)
            {
                return config?.Get(key) != null;
            }
            public void SetValue(string key, object value)
            {
                config?.Set(key, value);
            }
            public void RemoveValue(string key)
            {
                config?.Remove(key);
            }
        }
    }

    public class Binary
    {
        private string chain = "";
        public Binary(byte[] data)
        {
            var binary = string.Join("", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            var onlyNumbers = Regex.Replace(binary, "[0-9]", "");
            if (string.IsNullOrEmpty(onlyNumbers)) chain = binary;
            else throw new ArgumentException("The specified string is not binary");
        }

        private Binary(string data) { chain = data; }
        public static Binary Parse(string data) { return new Binary(data.Replace(" ", "")); }

        public override string ToString()
        {
            var str = "";
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
            var byteList = new List<byte>();

            for (var i = 0; i < chain.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(chain.Substring(i, 8), 2));
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
            var fastZip = new FastZip
            {
                CreateEmptyDirectories = true
            };
            fastZip.CreateZip(zipPath, unzipPath, true, null);
        }

        public static byte[] Compress(byte[] input)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
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
            var fastZip = new FastZip
            {
                CreateEmptyDirectories = true
            };
            fastZip.ExtractZip(zipPath, unzipPath, null);
        }

        public static byte[] Decompress(byte[] input)
        {
            var output = new MemoryStream();

            using (var compressStream = new MemoryStream(input))
            using (var decompressor = new DeflateStream(compressStream, CompressionMode.Decompress))
                decompressor.CopyTo(output);

            output.Position = 0;
            return output.ToArray();
        }

        public static async void DecompressAsync(string zipPath, string unzipPath, Action onComplete)
        {
            await Task.Run(() => Decompress(zipPath, unzipPath), new CancellationToken(false));
            onComplete();
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
            var fileToBeGZipped = new FileInfo(zipFile);
            if (gzipFile == null) gzipFile = string.Concat(fileToBeGZipped.FullName, ".gz");
            var gzipFileName = new FileInfo(gzipFile);

            using (var fileToBeZippedAsStream = fileToBeGZipped.OpenRead())
            {
                using (var gzipTargetAsStream = gzipFileName.Create())
                {
                    using (var gzipStream = new GZipStream(gzipTargetAsStream, CompressionMode.Compress))
                    {
                        try
                        {
                            fileToBeZippedAsStream.CopyTo(gzipStream);
                        }
                        catch (Exception ex)
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
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
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
            using (var fileToDecompressAsStream = new FileInfo(gzipFile).OpenRead())
            {
                using (var decompressedStream = File.Create(zipFile))
                {
                    using (var decompressionStream = new GZipStream(fileToDecompressAsStream, CompressionMode.Decompress))
                    {
                        try
                        {
                            decompressionStream.CopyTo(decompressedStream);
                        }
                        catch (Exception ex)
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
            using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream())
            {
                bigStream.CopyTo(bigStreamOut);
                return bigStreamOut.ToArray();
            }

        }
    }

    public static class Generic
    {
        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public static string CalculateMD5(this FileInfo file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = file.OpenRead())
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
