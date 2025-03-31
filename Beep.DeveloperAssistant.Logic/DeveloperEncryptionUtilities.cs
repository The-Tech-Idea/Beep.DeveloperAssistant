using System.Security.Cryptography;
using System.Text;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace Beep.DeveloperAssistant.Logic
{
    /// <summary>
    /// Provides utility methods for hashing, HMAC, PBKDF2-based password derivation,
    /// symmetric encryption/decryption (AES in CBC or GCM mode),
    /// RSA key generation, and RSA encryption/decryption.
    /// </summary>
    public class DeveloperEncryptionUtilities
    {
        private readonly IDMEEditor _dmeEditor;

        /// <summary>
        /// Creates a new instance of <see cref="DeveloperEncryptionUtilities"/>.
        /// </summary>
        /// <param name="dmeEditor">Reference to DME Editor for logging and configuration.</param>
        public DeveloperEncryptionUtilities(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor;
        }

        #region 1) Hashing

        /// <summary>
        /// Computes the SHA256 hash of a given string, returning the hash as a hex string.
        /// </summary>
        public string ComputeSha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                LogError("ComputeSha256Hash", "Input string is null or empty.");
                return null;
            }

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(input);
                    byte[] hash = sha256.ComputeHash(bytes);
                    return ByteArrayToHexString(hash);
                }
            }
            catch (Exception ex)
            {
                LogException("ComputeSha256Hash", ex);
                return null;
            }
        }

        /// <summary>
        /// Computes the MD5 hash of a given string, returning the hash as a hex string.
        /// Note: MD5 is not cryptographically secure. Use only for checksums.
        /// </summary>
        public string ComputeMd5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                LogError("ComputeMd5Hash", "Input string is null or empty.");
                return null;
            }

            try
            {
                using (var md5 = MD5.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(input);
                    byte[] hash = md5.ComputeHash(bytes);
                    return ByteArrayToHexString(hash);
                }
            }
            catch (Exception ex)
            {
                LogException("ComputeMd5Hash", ex);
                return null;
            }
        }

        #endregion

        #region 2) HMAC / Integrity Checking

        /// <summary>
        /// Generates an HMAC-SHA256 for the given <paramref name="message"/> using <paramref name="key"/>.
        /// Returns the HMAC as a hex string. Use to verify message integrity and authenticity.
        /// </summary>
        /// <param name="message">The text to be hashed with the key.</param>
        /// <param name="key">Secret key for HMAC.</param>
        /// <returns>Hex representation of the HMAC, or null on error.</returns>
        public string GenerateHmacSha256(string message, byte[] key)
        {
            if (string.IsNullOrEmpty(message))
            {
                LogError("GenerateHmacSha256", "Message is null or empty.");
                return null;
            }
            if (key == null || key.Length == 0)
            {
                LogError("GenerateHmacSha256", "HMAC key is null or empty.");
                return null;
            }
            try
            {
                using (var hmac = new HMACSHA256(key))
                {
                    byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                    byte[] hash = hmac.ComputeHash(msgBytes);
                    return ByteArrayToHexString(hash);
                }
            }
            catch (Exception ex)
            {
                LogException("GenerateHmacSha256", ex);
                return null;
            }
        }

        #endregion

        #region 3) PBKDF2 (Password-Based Key Derivation)

        /// <summary>
        /// Derives a cryptographic key from a password using PBKDF2 (Rfc2898DeriveBytes).
        /// Typically used to generate AES keys from user passwords.
        /// </summary>
        /// <param name="password">User-provided password.</param>
        /// <param name="salt">Random salt bytes (at least 8 or 16 bytes recommended).</param>
        /// <param name="iterations">Number of iterations (e.g. 100,000+).</param>
        /// <param name="keySizeInBytes">Size of the output key in bytes (e.g. 32 for 256-bit).</param>
        /// <returns>The derived key bytes, or null on error.</returns>
        public byte[] DeriveKeyFromPassword(string password, byte[] salt, int iterations, int keySizeInBytes)
        {
            if (string.IsNullOrEmpty(password))
            {
                LogError("DeriveKeyFromPassword", "Password is null or empty.");
                return null;
            }
            if (salt == null || salt.Length == 0)
            {
                LogError("DeriveKeyFromPassword", "Salt is null or empty.");
                return null;
            }
            if (iterations < 1)
            {
                LogError("DeriveKeyFromPassword", "Iterations must be > 0.");
                return null;
            }
            if (keySizeInBytes < 1)
            {
                LogError("DeriveKeyFromPassword", "Key size in bytes must be > 0.");
                return null;
            }

            try
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    return deriveBytes.GetBytes(keySizeInBytes);
                }
            }
            catch (Exception ex)
            {
                LogException("DeriveKeyFromPassword", ex);
                return null;
            }
        }

        #endregion

        #region 4) Symmetric Encryption (AES-CBC + File Encryption)

        /// <summary>
        /// Encrypts the specified <paramref name="plainText"/> using AES (CBC mode) with the provided <paramref name="key"/> and <paramref name="iv"/>.
        /// Returns the ciphertext as a base64-encoded string.
        /// </summary>
        public string EncryptStringAesCbc(string plainText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                LogError("EncryptStringAesCbc", "PlainText is null or empty.");
                return null;
            }
            if (!ValidateAesKeyAndIv(key, iv, "EncryptStringAesCbc"))
                return null;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using var ms = new MemoryStream();
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    byte[] encrypted = ms.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
            catch (Exception ex)
            {
                LogException("EncryptStringAesCbc", ex);
                return null;
            }
        }

        /// <summary>
        /// Decrypts the specified base64-encoded <paramref name="cipherText"/> using AES (CBC mode) with <paramref name="key"/> and <paramref name="iv"/>.
        /// </summary>
        public string DecryptStringAesCbc(string cipherText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                LogError("DecryptStringAesCbc", "CipherText is null or empty.");
                return null;
            }
            if (!ValidateAesKeyAndIv(key, iv, "DecryptStringAesCbc"))
                return null;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using var ms = new MemoryStream(cipherBytes);
                    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using var sr = new StreamReader(cs);
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                LogException("DecryptStringAesCbc", ex);
                return null;
            }
        }

        /// <summary>
        /// Encrypts the contents of <paramref name="inputFile"/> using AES (CBC) and writes 
        /// the result to <paramref name="outputFile"/>.
        /// </summary>
        public bool EncryptFileAesCbc(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            if (!File.Exists(inputFile))
            {
                LogError("EncryptFileAesCbc", $"Input file not found: {inputFile}");
                return false;
            }
            if (string.IsNullOrEmpty(outputFile))
            {
                LogError("EncryptFileAesCbc", "Output file path is null or empty.");
                return false;
            }
            if (!ValidateAesKeyAndIv(key, iv, "EncryptFileAesCbc"))
                return false;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using var fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                    using var fsOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                    using var cs = new CryptoStream(fsOut, encryptor, CryptoStreamMode.Write);
                    fsIn.CopyTo(cs);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException("EncryptFileAesCbc", ex);
                return false;
            }
        }

        /// <summary>
        /// Decrypts the contents of <paramref name="inputFile"/> using AES (CBC) and writes 
        /// the result to <paramref name="outputFile"/>.
        /// </summary>
        public bool DecryptFileAesCbc(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            if (!File.Exists(inputFile))
            {
                LogError("DecryptFileAesCbc", $"Encrypted file not found: {inputFile}");
                return false;
            }
            if (string.IsNullOrEmpty(outputFile))
            {
                LogError("DecryptFileAesCbc", "Output file path is null or empty.");
                return false;
            }
            if (!ValidateAesKeyAndIv(key, iv, "DecryptFileAesCbc"))
                return false;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using var fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                    using var fsOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                    using var cs = new CryptoStream(fsIn, decryptor, CryptoStreamMode.Read);
                    cs.CopyTo(fsOut);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException("DecryptFileAesCbc", ex);
                return false;
            }
        }

        #endregion

        #region 5) AES-GCM (Authenticated Encryption)

#if NET5_0_OR_GREATER
        // AES-GCM is supported natively in .NET 5+.
        // If you're targeting .NET Framework, you need a third-party library or a custom approach.

        /// <summary>
        /// Encrypts <paramref name="plainText"/> using AES-GCM for authenticated encryption.
        /// Returns the ciphertext in Base64. Also outputs the <paramref name="nonce"/> and <paramref name="tag"/>.
        /// </summary>
        /// <param name="plainText">Plain text to encrypt.</param>
        /// <param name="key">AES key (16, 24, or 32 bytes for 128/192/256-bit).</param>
        /// <param name="nonce">Returns the randomly generated nonce (12 bytes recommended).</param>
        /// <param name="tag">Returns the authentication tag (16 bytes recommended).</param>
        /// <returns>Base64 string of the encrypted data, or null on error.</returns>
        public string EncryptStringAesGcm(string plainText, byte[] key, out byte[] nonce, out byte[] tag)
        {
            nonce = null;
            tag = null;

            if (string.IsNullOrEmpty(plainText))
            {
                LogError("EncryptStringAesGcm", "PlainText is null or empty.");
                return null;
            }
            if (key == null || key.Length == 0)
            {
                LogError("EncryptStringAesGcm", "AES key is null or empty.");
                return null;
            }

            try
            {
                // recommended sizes: 12 bytes for nonce, 16 bytes for tag
                nonce = GenerateRandomBytes(12);
                tag = new byte[16];

                byte[] cipherBytes;
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                using (var aesGcm = new AesGcm(key))
                {
                    cipherBytes = new byte[plainBytes.Length];
                    aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
                }

                return Convert.ToBase64String(cipherBytes);
            }
            catch (Exception ex)
            {
                LogException("EncryptStringAesGcm", ex);
                return null;
            }
        }

        /// <summary>
        /// Decrypts the given Base64-encoded ciphertext using AES-GCM.
        /// Requires the same <paramref name="key"/>, <paramref name="nonce"/>, and <paramref name="tag"/> used for encryption.
        /// </summary>
        /// <param name="cipherText">Base64-encoded encrypted data.</param>
        /// <param name="key">AES key.</param>
        /// <param name="nonce">Nonce generated during encryption.</param>
        /// <param name="tag">Authentication tag generated during encryption.</param>
        /// <returns>The decrypted plain text, or null if decryption fails.</returns>
        public string DecryptStringAesGcm(string cipherText, byte[] key, byte[] nonce, byte[] tag)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                LogError("DecryptStringAesGcm", "CipherText is null or empty.");
                return null;
            }
            if (key == null || key.Length == 0)
            {
                LogError("DecryptStringAesGcm", "AES key is null or empty.");
                return null;
            }
            if (nonce == null || nonce.Length == 0)
            {
                LogError("DecryptStringAesGcm", "Nonce is null or empty.");
                return null;
            }
            if (tag == null || tag.Length == 0)
            {
                LogError("DecryptStringAesGcm", "Tag is null or empty.");
                return null;
            }

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = new byte[cipherBytes.Length];

                using (var aesGcm = new AesGcm(key))
                {
                    aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
                }

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                LogException("DecryptStringAesGcm", ex);
                return null;
            }
        }
#endif

        #endregion

        #region 6) Asymmetric Encryption (RSA)

        /// <summary>
        /// Generates an RSA public/private key pair in XML format (for .NET Framework)
        /// or in standard RSA parameters. Key size default = 2048 bits.
        /// 
        /// For .NET 5+ and cross-platform, you might prefer using PEM-encoded keys 
        /// and Import/Export subjectPublicKeyInfo / PKCS#8. Shown here with XML for simplicity.
        /// </summary>
        /// <param name="keySize">Typically 2048 or 4096.</param>
        /// <returns>(publicKeyXML, privateKeyXML)</returns>
        public (string publicKey, string privateKey) GenerateRsaKeyPair(int keySize = 2048)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(keySize))
                {
                    rsa.PersistKeyInCsp = false;
                    string publicKey = rsa.ToXmlString(false);  // public only
                    string privateKey = rsa.ToXmlString(true);  // includes private
                    return (publicKey, privateKey);
                }
            }
            catch (Exception ex)
            {
                LogException("GenerateRsaKeyPair", ex);
                return (null, null);
            }
        }

        /// <summary>
        /// Encrypts the given <paramref name="plainText"/> with RSA using the public key XML.
        /// Returns a Base64-encoded string of the ciphertext.
        /// 
        /// RSA is typically used to encrypt small amounts of data (like an AES key), 
        /// not large messages. 
        /// </summary>
        public string RsaEncrypt(string plainText, string publicKeyXml)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                LogError("RsaEncrypt", "PlainText is null or empty.");
                return null;
            }
            if (string.IsNullOrEmpty(publicKeyXml))
            {
                LogError("RsaEncrypt", "publicKeyXml is null or empty.");
                return null;
            }

            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(publicKeyXml);
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] cipherBytes = rsa.Encrypt(plainBytes, false);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
            catch (Exception ex)
            {
                LogException("RsaEncrypt", ex);
                return null;
            }
        }

        /// <summary>
        /// Decrypts the given Base64-encoded RSA ciphertext using the private key XML.
        /// Returns the original plain text.
        /// </summary>
        public string RsaDecrypt(string cipherTextBase64, string privateKeyXml)
        {
            if (string.IsNullOrEmpty(cipherTextBase64))
            {
                LogError("RsaDecrypt", "CipherTextBase64 is null or empty.");
                return null;
            }
            if (string.IsNullOrEmpty(privateKeyXml))
            {
                LogError("RsaDecrypt", "privateKeyXml is null or empty.");
                return null;
            }

            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(privateKeyXml);
                    byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
                    byte[] plainBytes = rsa.Decrypt(cipherBytes, false);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch (Exception ex)
            {
                LogException("RsaDecrypt", ex);
                return null;
            }
        }

        #endregion

        #region Utility / Helpers

        /// <summary>
        /// Converts a byte array to a lowercase hex string (e.g., "1a2b3c...").
        /// </summary>
        private string ByteArrayToHexString(byte[] array)
        {
            var sb = new StringBuilder(array.Length * 2);
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates random bytes of a specified length using a secure RNG.
        /// </summary>
        public byte[] GenerateRandomBytes(int length)
        {
            byte[] data = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
            return data;
        }

        /// <summary>
        /// Simple helper to validate that AES key and IV are not null/empty and log accordingly.
        /// </summary>
        private bool ValidateAesKeyAndIv(byte[] key, byte[] iv, string methodName)
        {
            if (key == null || key.Length == 0)
            {
                LogError(methodName, "AES key is null or empty.");
                return false;
            }
            if (iv == null || iv.Length == 0)
            {
                LogError(methodName, "AES IV is null or empty.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Logs an exception using the DMEEditor.
        /// </summary>
        private void LogException(string methodName, Exception ex)
        {
            _dmeEditor.AddLogMessage(
                methodName,
                $"Error: {ex.Message}",
                DateTime.Now,
                0,
                null,
                Errors.Failed
            );
        }

        /// <summary>
        /// Logs an error message using the DMEEditor.
        /// </summary>
        private void LogError(string methodName, string message)
        {
            _dmeEditor.AddLogMessage(
                methodName,
                message,
                DateTime.Now,
                0,
                null,
                Errors.Failed
            );
        }

        #endregion
    }
}
