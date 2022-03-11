using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
                for (var i = 0; i < 32 - KEY.Length; i++)
                    KEY = KEY + "X";
            }
            var KEY_BYTES = Encoding.UTF8.GetBytes(KEY);

            // Check arguments.
            if (plainText == null) throw new ArgumentNullException("plainText");

            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (var algorithm = Rijndael.Create())
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
                for (var i = 0; i < 32 - KEY.Length; i++)
                    KEY = KEY + "X";
            }
            var KEY_BYTES = Encoding.UTF8.GetBytes(KEY);

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (var algorithm = Rijndael.Create())
            {
                algorithm.Key = KEY_BYTES;

                // Get bytes from input string
                var cipherBytes = new byte[0];
                try { cipherBytes = Convert.FromBase64String(cipherText); } catch { return plaintext; }

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherBytes))
                {
                    // Read IV first
                    var IV = new byte[16];
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
        public enum Algorithm { SHA1, SHA256, SHA384, SHA512 }
        public static string SHA(Algorithm alg, string value) => Hash(sha(alg).ComputeHash(Encoding.Default.GetBytes(value))); // ComputeHash - returns byte array
        public static string SHA(Algorithm alg, Stream value) { var val = Hash(sha(alg).ComputeHash(value)); value.Close(); return val; }// ComputeHash - returns byte array

        private static HashAlgorithm sha(Algorithm alg)
        {
            if (alg == Algorithm.SHA1) return SHA1.Create();
            if (alg == Algorithm.SHA256) return SHA256.Create();
            if (alg == Algorithm.SHA384) return SHA384.Create();
            if (alg == Algorithm.SHA512) return SHA512.Create();
            return SHA1.Create();
        }

        private static string Hash(byte[] data)
        {
            var sb = new StringBuilder(); // Convert byte array to a string
            for (var i = 0; i < data.Length; i++) sb.Append(data[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
