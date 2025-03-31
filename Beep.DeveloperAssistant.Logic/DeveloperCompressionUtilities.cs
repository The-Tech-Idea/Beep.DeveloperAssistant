using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace Beep.DeveloperAssistant.Logic
{
    /// <summary>
    /// Provides extensive utility methods for compressing/decompressing files and byte arrays,
    /// plus advanced enhancements: parallel compression, encryption, callbacks/cancellation,
    /// and checksum validation.
    /// </summary>
    public class DeveloperCompressionUtilities
    {
        private readonly IDMEEditor _dmeEditor;

        /// <summary>
        /// Creates a new instance of <see cref="DeveloperCompressionUtilities"/>.
        /// </summary>
        /// <param name="dmeEditor">Reference to DME Editor for logging and configuration.</param>
        public DeveloperCompressionUtilities(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor;
        }

        #region Basic ZIP Operations (Directory & Single File)

        public bool ZipDirectory(string sourceDirectory, string destinationZipPath)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                LogError(nameof(ZipDirectory), $"Source directory not found: {sourceDirectory}");
                return false;
            }
            try
            {
                ZipFile.CreateFromDirectory(
                    sourceDirectory,
                    destinationZipPath,
                    CompressionLevel.Optimal,
                    includeBaseDirectory: true);
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(ZipDirectory), ex);
                return false;
            }
        }

        public bool UnzipFile(string zipPath, string destinationDirectory)
        {
            if (!File.Exists(zipPath))
            {
                LogError(nameof(UnzipFile), $"Zip file not found: {zipPath}");
                return false;
            }
            try
            {
                ZipFile.ExtractToDirectory(zipPath, destinationDirectory);
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(UnzipFile), ex);
                return false;
            }
        }

        public bool AddFileToZip(string zipPath, string fileToAdd, string entryName = null)
        {
            if (!File.Exists(fileToAdd))
            {
                LogError(nameof(AddFileToZip), $"File to add not found: {fileToAdd}");
                return false;
            }
            try
            {
                using (FileStream fs = new FileStream(zipPath, FileMode.OpenOrCreate))
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    string zipEntryName = string.IsNullOrEmpty(entryName)
                        ? Path.GetFileName(fileToAdd)
                        : entryName;

                    // If entry exists, delete it first
                    ZipArchiveEntry existing = archive.GetEntry(zipEntryName);
                    existing?.Delete();

                    archive.CreateEntryFromFile(fileToAdd, zipEntryName, CompressionLevel.Optimal);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(AddFileToZip), ex);
                return false;
            }
        }

        #endregion

        #region Partial Extraction & Listing Zip Contents

        public List<string> ListZipArchiveEntries(string zipPath)
        {
            if (!File.Exists(zipPath))
            {
                LogError(nameof(ListZipArchiveEntries), $"Zip file not found: {zipPath}");
                return null;
            }
            var entries = new List<string>();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        entries.Add(entry.FullName);
                    }
                }
                return entries;
            }
            catch (Exception ex)
            {
                LogException(nameof(ListZipArchiveEntries), ex);
                return null;
            }
        }

        public bool ExtractSpecificEntryFromZip(
            string zipPath,
            string entryToExtract,
            string destinationDirectory)
        {
            if (!File.Exists(zipPath))
            {
                LogError(nameof(ExtractSpecificEntryFromZip), $"Zip file not found: {zipPath}");
                return false;
            }
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    var entry = archive.GetEntry(entryToExtract);
                    if (entry == null)
                    {
                        LogError(nameof(ExtractSpecificEntryFromZip), $"Entry not found: {entryToExtract}");
                        return false;
                    }
                    string fullPath = Path.Combine(destinationDirectory, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    entry.ExtractToFile(fullPath, overwrite: true);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(ExtractSpecificEntryFromZip), ex);
                return false;
            }
        }

        #endregion

        #region GZip Basic

        public bool GZipCompressFile(string sourceFilePath, string destinationGzipPath)
        {
            if (!File.Exists(sourceFilePath))
            {
                LogError(nameof(GZipCompressFile), $"File not found: {sourceFilePath}");
                return false;
            }
            try
            {
                using (FileStream inputFileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                using (FileStream outputFileStream = new FileStream(destinationGzipPath, FileMode.Create, FileAccess.Write))
                using (GZipStream gzipStream = new GZipStream(outputFileStream, CompressionMode.Compress))
                {
                    inputFileStream.CopyTo(gzipStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(GZipCompressFile), ex);
                return false;
            }
        }

        public bool GZipDecompressFile(string gzipFilePath, string destinationFilePath)
        {
            if (!File.Exists(gzipFilePath))
            {
                LogError(nameof(GZipDecompressFile), $"GZip file not found: {gzipFilePath}");
                return false;
            }
            try
            {
                using (FileStream inputFileStream = new FileStream(gzipFilePath, FileMode.Open, FileAccess.Read))
                using (GZipStream gzipStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
                using (FileStream outputFileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
                {
                    gzipStream.CopyTo(outputFileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(GZipDecompressFile), ex);
                return false;
            }
        }

        public byte[] GZipCompressBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                LogError(nameof(GZipCompressBytes), "Data is null or empty.");
                return null;
            }

            try
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(data, 0, data.Length);
                    }
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(GZipCompressBytes), ex);
                return null;
            }
        }

        public byte[] GZipDecompressBytes(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                LogError(nameof(GZipDecompressBytes), "Compressed data is null or empty.");
                return null;
            }

            try
            {
                using (var inputStream = new MemoryStream(compressedData))
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    gzipStream.CopyTo(outputStream);
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(GZipDecompressBytes), ex);
                return null;
            }
        }

        #endregion

        #region 1) Multi-Threaded or Parallel Compression (Simple Example)

        /// <summary>
        /// Demonstrates a naive approach to multi-threaded compression by splitting a file 
        /// into chunks, compressing each chunk in parallel, and reassembling. 
        /// This is a simplified example. Real usage could handle chunk alignment, 
        /// partial GZip headers, etc.
        /// </summary>
        /// <param name="sourceFile">File to compress.</param>
        /// <param name="destinationFile">Resulting compressed file.</param>
        /// <param name="chunkSize">Size of each chunk in bytes (e.g. 1MB = 1,048,576).</param>
        /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
        public bool ParallelCompressFile(string sourceFile, string destinationFile, int chunkSize = 1048576)
        {
            if (!File.Exists(sourceFile))
            {
                LogError(nameof(ParallelCompressFile), $"File not found: {sourceFile}");
                return false;
            }
            try
            {
                byte[] allBytes = File.ReadAllBytes(sourceFile);
                int totalChunks = (int)Math.Ceiling((double)allBytes.Length / chunkSize);
                var compressedChunks = new byte[totalChunks][];

                Parallel.For(0, totalChunks, i =>
                {
                    int offset = i * chunkSize;
                    int length = Math.Min(chunkSize, allBytes.Length - offset);
                    var slice = new byte[length];
                    Buffer.BlockCopy(allBytes, offset, slice, 0, length);

                    compressedChunks[i] = GZipCompressBytes(slice);
                });

                // Reassemble the compressed chunks
                using (var fs = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                {
                    for (int i = 0; i < totalChunks; i++)
                    {
                        // Write length + data
                        byte[] chunk = compressedChunks[i];
                        // 4 bytes to store length of chunk
                        fs.Write(BitConverter.GetBytes(chunk.Length), 0, 4);
                        fs.Write(chunk, 0, chunk.Length);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(ParallelCompressFile), ex);
                return false;
            }
        }

        /// <summary>
        /// Decompresses a file created by <see cref="ParallelCompressFile"/>.
        /// Reads each chunk length, chunk data, and decompresses in sequence.
        /// </summary>
        public bool ParallelDecompressFile(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
            {
                LogError(nameof(ParallelDecompressFile), $"File not found: {sourceFile}");
                return false;
            }
            try
            {
                byte[] compressedBytes = File.ReadAllBytes(sourceFile);
                using (var ms = new MemoryStream(compressedBytes))
                using (var outFile = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                {
                    // read chunks until EOF
                    while (ms.Position < ms.Length)
                    {
                        // read 4 bytes for chunk length
                        byte[] lengthBytes = new byte[4];
                        int readLen = ms.Read(lengthBytes, 0, 4);
                        if (readLen < 4) break;

                        int chunkLen = BitConverter.ToInt32(lengthBytes, 0);
                        byte[] chunkData = new byte[chunkLen];
                        ms.Read(chunkData, 0, chunkLen);

                        byte[] decompressed = GZipDecompressBytes(chunkData);
                        outFile.Write(decompressed, 0, decompressed.Length);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(ParallelDecompressFile), ex);
                return false;
            }
        }

        #endregion

        #region 2) Encryption Beyond Password-Protected Archives (Example Using AES)

        /// <summary>
        /// Example: Compresses a file with GZip, then encrypts it with AES using 
        /// an external <see cref="DeveloperEncryptionUtilities"/> or local AES code.
        /// 
        /// Real usage requires secure key management, IV storage, and so on.
        /// </summary>
        /// <param name="sourceFile">Plain file to compress+encrypt.</param>
        /// <param name="destFile">Destination for the encrypted data.</param>
        /// <param name="encryptUtil">Reference to your encryption utilities class.</param>
        /// <param name="aesKey">AES key.</param>
        /// <param name="aesIv">AES IV.</param>
        /// <returns><c>true</c> if success, else <c>false</c>.</returns>
        public bool CompressThenEncryptFile(
            string sourceFile,
            string destFile,
            DeveloperEncryptionUtilities encryptUtil,
            byte[] aesKey,
            byte[] aesIv)
        {
            if (!File.Exists(sourceFile))
            {
                LogError(nameof(CompressThenEncryptFile), $"Source file not found: {sourceFile}");
                return false;
            }
            if (encryptUtil == null)
            {
                LogError(nameof(CompressThenEncryptFile), "encryptUtil is null.");
                return false;
            }

            try
            {
                // 1) GZip compress to in-memory
                byte[] fileBytes = File.ReadAllBytes(sourceFile);
                byte[] compressed = GZipCompressBytes(fileBytes);
                if (compressed == null)
                {
                    LogError(nameof(CompressThenEncryptFile), "Compression returned null.");
                    return false;
                }

                // 2) AES encrypt
                // We'll pretend DeveloperEncryptionUtilities has an "EncryptStringAes" or "EncryptBytesAes" method.
                // For demonstration, let's do base64 of the raw compressed, then encrypt string. 
                // Alternatively, your encryption utility might accept raw bytes directly.
                string base64Compressed = Convert.ToBase64String(compressed);
                string cipherText = EncryptStringAes(base64Compressed, aesKey, aesIv);
                if (cipherText == null)
                {
                    LogError(nameof(CompressThenEncryptFile), "Encryption returned null.");
                    return false;
                }

                // 3) Write cipherText to destFile
                File.WriteAllText(destFile, cipherText);
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(CompressThenEncryptFile), ex);
                return false;
            }
        }

        /// <summary>
        /// Decrypts an AES-encrypted file (created by <see cref="CompressThenEncryptFile"/>),
        /// then GZip-decompresses the result back to the original data.
        /// </summary>
        public bool DecryptThenDecompressFile(
            string sourceFile,
            string destFile,
            DeveloperEncryptionUtilities encryptUtil,
            byte[] aesKey,
            byte[] aesIv)
        {
            if (!File.Exists(sourceFile))
            {
                LogError(nameof(DecryptThenDecompressFile), $"Encrypted file not found: {sourceFile}");
                return false;
            }
            if (encryptUtil == null)
            {
                LogError(nameof(DecryptThenDecompressFile), "encryptUtil is null.");
                return false;
            }

            try
            {
                // 1) Read cipherText
                string cipherText = File.ReadAllText(sourceFile);
                if (string.IsNullOrEmpty(cipherText))
                {
                    LogError(nameof(DecryptThenDecompressFile), "Cipher text is empty.");
                    return false;
                }

                // 2) Decrypt to base64 string
                string base64Compressed = DecryptStringAes(cipherText, aesKey, aesIv);
                if (base64Compressed == null)
                {
                    LogError(nameof(DecryptThenDecompressFile), "Decryption returned null.");
                    return false;
                }

                // 3) Convert from base64 -> compressed bytes
                byte[] compressed = Convert.FromBase64String(base64Compressed);

                // 4) GZip decompress
                byte[] originalBytes = GZipDecompressBytes(compressed);
                if (originalBytes == null)
                {
                    LogError(nameof(DecryptThenDecompressFile), "Decompression returned null.");
                    return false;
                }

                // 5) Write to destFile
                File.WriteAllBytes(destFile, originalBytes);
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(DecryptThenDecompressFile), ex);
                return false;
            }
        }

        #endregion

        #region 3) Event Callbacks & Cancellation

        /// <summary>
        /// Compresses a file into GZip format, supporting a <see cref="CancellationToken"/> 
        /// and a callback for advanced progress or event handling.
        /// If <paramref name="token"/> is canceled, the method stops and returns <c>false</c>.
        /// </summary>
        public bool GZipCompressFileWithCallback(
            string sourceFile,
            string destinationFile,
            CancellationToken token,
            Action<long> onProgressBytes = null)
        {
            if (!File.Exists(sourceFile))
            {
                LogError(nameof(GZipCompressFileWithCallback), $"File not found: {sourceFile}");
                return false;
            }

            const int bufferSize = 81920; // 80 KB
            try
            {
                using (var input = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                using (var output = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    byte[] buffer = new byte[bufferSize];
                    long totalBytes = 0;
                    int bytesRead;

                    while ((bytesRead = input.Read(buffer, 0, bufferSize)) > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            LogError(nameof(GZipCompressFileWithCallback), "Operation canceled.");
                            return false;
                        }
                        gzip.Write(buffer, 0, bytesRead);
                        totalBytes += bytesRead;
                        onProgressBytes?.Invoke(totalBytes);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(GZipCompressFileWithCallback), ex);
                return false;
            }
        }

        #endregion

        #region 4) Validation or Checksums

        /// <summary>
        /// Computes the SHA256 hash of a file, for verifying data integrity or comparing
        /// pre/post compression. 
        /// </summary>
        public string ComputeFileSha256Hash(string filePath)
        {
            if (!File.Exists(filePath))
            {
                LogError(nameof(ComputeFileSha256Hash), $"File not found: {filePath}");
                return null;
            }
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = sha256.ComputeHash(fs);
                    return ByteArrayToHexString(hash);
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(ComputeFileSha256Hash), ex);
                return null;
            }
        }

        /// <summary>
        /// Example method that compresses a file, returns the compressed file's path plus a hash
        /// that can be used to verify correctness after decompression.
        /// The caller might store/compare this hash with the final extracted file.
        /// </summary>
        public (bool Success, string CompressedPath, string Sha256Hash) CompressFileWithHash(
            string sourceFile,
            string destinationGzipFile)
        {
            if (!File.Exists(sourceFile))
            {
                LogError(nameof(CompressFileWithHash), $"File not found: {sourceFile}");
                return (false, null, null);
            }
            try
            {
                if (!GZipCompressFile(sourceFile, destinationGzipFile))
                {
                    return (false, null, null);
                }
                // compute hash of the compressed file
                string hash = ComputeFileSha256Hash(destinationGzipFile);
                return (true, destinationGzipFile, hash);
            }
            catch (Exception ex)
            {
                LogException(nameof(CompressFileWithHash), ex);
                return (false, null, null);
            }
        }

        /// <summary>
        /// Utility to convert a byte array to hex. 
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

        #endregion

        #region 5) Password Protection Placeholder

        // If you want to handle password-protected ZIPs with encryption, you need a 3rd-party library 
        // like DotNetZip or SharpZipLib. The built-in System.IO.Compression doesn't natively support it.

        public bool CreatePasswordProtectedZipPlaceholder(IEnumerable<string> sourceFiles, string destinationZipPath, string password)
        {
            LogError(nameof(CreatePasswordProtectedZipPlaceholder),
                "System.IO.Compression does not support password-protected zips. Use a 3rd-party library.");
            return false;
        }
        public string EncryptStringAes(string plainText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText)) return null;
            if (key == null || key.Length == 0) return null;
            if (iv == null || iv.Length == 0) return null;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        byte[] encrypted = ms.ToArray();
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public string DecryptStringAes(string cipherText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(cipherText)) return null;
            if (key == null || key.Length == 0) return null;
            if (iv == null || iv.Length == 0) return null;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(cipherBytes))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Logging Helpers

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

        private void LogException(string methodName, Exception ex)
        {
            _dmeEditor.AddLogMessage(
                methodName,
                $"Exception: {ex.Message}",
                DateTime.Now,
                0,
                null,
                Errors.Failed
            );
        }

        #endregion
    }
}
