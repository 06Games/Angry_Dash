using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Ionic.Zip;

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
            public bool ContainsValues { get { if (jToken == null) return false; else return jToken.HasValues; } }
            public T Value<T>(string value) { return jToken.Value<T>(value); }
            public bool ValueExist(string value) { if (jToken == null) return false; else return jToken.Value<string>(value) != null; }
        }
    }

    public static class ZIP
    {

#if UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void unzip (string zipFilePath, string location);

	[DllImport("__Internal")]
	private static extern void zip (string zipFilePath);

	[DllImport("__Internal")]
	private static extern void addZipFile (string addFile);

#endif

        public static void Compress(string unzipPath, string zipPath)
        {
            string[] files = Directory.GetFiles(unzipPath);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            string path = Path.GetDirectoryName(zipPath);
            Directory.CreateDirectory(path);

            using (ZipFile zip = new ZipFile())
            {
                foreach (string file in files)
                {
                    zip.AddFile(file, "");
                }
                zip.Save(zipPath);
            }
#elif UNITY_ANDROID
		using (AndroidJavaClass zipper = new AndroidJavaClass ("com.tsw.zipper")) {
			{
				zipper.CallStatic ("zip", zipPath, files);
			}
		}
#elif UNITY_IPHONE
		foreach (string file in files) {
			addZipFile (file);
		}
		zip (zipPath);
#endif
        }

        public static void Decompress(string zipPath, string unzipPath)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            Directory.CreateDirectory(unzipPath);

            using (ZipFile zip = ZipFile.Read(zipPath))
            {

                zip.ExtractAll(unzipPath, ExtractExistingFileAction.OverwriteSilently);
            }
#elif UNITY_ANDROID
		using (AndroidJavaClass zipper = new AndroidJavaClass ("com.tsw.zipper")) {
			zipper.CallStatic ("unzip", zipPath, unzipPath);
		}
#elif UNITY_IPHONE
		unzip (zipPath, unzipPath);
#endif
        }
    }
}

namespace Display
{
    public static class Screen
    {
        public static Vector2 resolution {
            get { return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height); }
            set { UnityEngine.Screen.SetResolution((int)resolution.x, (int)resolution.y, UnityEngine.Screen.fullScreen); }
        }
    }
}

namespace Tools
{
    public class String
    {
        string str;
        public String(string _string) { str = _string; }

        public String Format
        {
            get
            {
                if (str != null)
                {
                    str = str.Replace("\\n", "\n");
                    str = str.Replace("\\t", "\t");
                }
                return new String(str);
            }
        }
        public string GetString { get { string st = str; return st; } }
        public T ParseTo<T>() { return (T)System.Convert.ChangeType(str, typeof(T)); }
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
