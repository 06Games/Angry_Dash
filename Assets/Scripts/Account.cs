using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class Account : MonoBehaviour
{

    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);

#if UNITY_EDITOR || UNITY_STANDALONE
        string path = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string path = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
        if (InternetAPI.IsConnected())
        {
            if (File.Exists(path))
            {
                string[] details = File.ReadAllLines(path);
                //details[0] = "1 = Evan";
                if (!File.Exists(Application.temporaryCachePath + "/ac.txt"))
                    Connect(details[0].Replace("1 = ", ""), details[1].Replace("2 = ", ""), true);
                else if (Security.Encrypting.Decrypt(File.ReadAllLines(Application.temporaryCachePath + "/ac.txt")[0], details[1].Replace("2 = ", "")) != details[0].Replace("1 = ", "") + BaseControl.pathToActualLogMessage())
                    Connect(details[0].Replace("1 = ", ""), details[1].Replace("2 = ", ""), true);
            }
            else
            {
                if (!Directory.Exists(path.Replace("account.account", "")))
                    Directory.CreateDirectory(path.Replace("account.account", ""));
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    public void ConnectBtn()
    {
        transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        Connect(transform.GetChild(0).GetChild(1).GetComponent<InputField>().text, transform.GetChild(0).GetChild(2).GetComponent<InputField>().text, false);
    }
    public bool Connect(string user, string mdp, bool hash)
    {
        string MDP = mdp;
        if (!hash)
            MDP = Security.Hashing.SHA384(mdp);

        string url = "https://06games.ddns.net/accounts/lite/connect.php?id=" + user + "&mdp=" + MDP + "&hash=true";
        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string Result = client.DownloadString(url);
        if (Result.Contains("Connection succesful !"))
        {
            BaseControl.LogNewMassage("Successful connection to 06Games account", true);
            transform.GetChild(0).gameObject.SetActive(false);

#if UNITY_EDITOR || UNITY_STANDALONE
            string path = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string path = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
            string[] a = Result.Split(new string[] { "<br>" }, StringSplitOptions.None);
            ConfigAPI.SetString("Account.Username", a[3]);
            File.WriteAllLines(path, new string[2] { "1 = " + user, "2 = " + MDP });
            File.WriteAllLines(Application.temporaryCachePath + "/ac.txt", new string[1] { Security.Encrypting.Encrypt(user + BaseControl.pathToActualLogMessage(), mdp) });
        }
        else
        {
            BaseControl.LogNewMassage("06Games account connection failure", true);
            transform.GetChild(0).gameObject.SetActive(true);
            if(!hash)
                transform.GetChild(0).GetChild(4).gameObject.SetActive(true);
        }

        transform.GetChild(1).gameObject.SetActive(false);
#if UNITY_EDITOR
#elif UNITY_ANDROID
        if(Result.Contains("Connection succesful !"))
                UnityThread.executeInUpdate(() => GameObject.Find("Social").GetComponent<Social>().Auth());
#endif

        return Result.Contains("Connection succesful !");
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
                throw new ArgumentNullException("plainText");

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
            return Convert.ToBase64String(encrypted);
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
                throw new ArgumentNullException("cipherText");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (Rijndael algorithm = Rijndael.Create())
            {
                algorithm.Key = KEY_BYTES;

                // Get bytes from input string
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

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
    }
}
