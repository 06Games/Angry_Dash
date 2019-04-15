using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Linq;

namespace FileFormat
{
    public class JSON
    {
        public Newtonsoft.Json.Linq.JObject jToken;
        public JSON(string plainText)
        {
            if (!string.IsNullOrEmpty(plainText))
            {
                try { jToken = Newtonsoft.Json.Linq.JObject.Parse(plainText); }
                catch (System.Exception e) { Debug.LogError("Error with:\n" + plainText + "\nError details:\n" + e.Message); }
            }
        }
        public JSON(Newtonsoft.Json.Linq.JToken token) { jToken = (Newtonsoft.Json.Linq.JObject)token; }

        public JSON GetCategory(string token) { if (jToken == null) return new JSON(null); else return new JSON(jToken.SelectToken(token)); }
        public void Delete() { if (jToken != null) jToken.Remove(); }
        public bool ContainsValues { get { if (jToken == null) return false; else return jToken.HasValues; } }

        public T Value<T>(string value) { return jToken.Value<T>(value); }
        public bool ValueExist(string value) { if (jToken == null) return false; else return jToken.Value<string>(value) != null; }
    }

    namespace XML
    {
        public static class Utils
        {
            public static string ClassToXML<T>(T data, bool minimised = true)
            {
                System.Xml.Serialization.XmlSerializer _serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                var settings = new System.Xml.XmlWriterSettings
                {
                    NewLineHandling = System.Xml.NewLineHandling.Entitize,
                    Encoding = Encoding.UTF8,
                    Indent = !minimised
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

            public static bool IsValid(string xmlFile)
            {
                try { new System.Xml.XmlDocument().LoadXml(xmlFile); }
                catch { return false; }
                return true;
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

            public T value<T>()
            {
                string v = Value;
                if (v == null) return default;
                else try { return Tools.StringExtensions.ParseTo<T>(v); } catch { return default; }
            }
            public string Value { get { if (node == null) return null; else return node.InnerText; } set { node.InnerText = value; } }
            public void Remove() { node.ParentNode.RemoveChild(node); }
        }

        public abstract class Base_Collection
        {
            public System.Xml.XmlNode node;
            public Item GetItem(string key)
            {
                System.Xml.XmlNode xmlNode = node.SelectSingleNode(key);
                if (xmlNode == null) return new Item(null);
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
                if (xmlNode == null) return new Item(null);
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
