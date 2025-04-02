
using System.Text;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Extensions;
using Beep.DeveloperAssistant.Logic;

namespace Beep.DeveloperAssistant.MenuCommands
{
    [AddinAttribute(
        Caption = "Compression",
        Name = "DeveloperCompressionMenuCommands",
        menu = "Developer",
        misc = "DeveloperCompressionMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "compressionutilities.svg",
        order = 5,
        Showin = ShowinType.Menu
    )]
    public class DeveloperCompressionMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

      //  private FunctionandExtensionsHelpers ExtensionsHelpers;

        public DeveloperCompressionMenuCommands( IAppManager pvisManager)
        {
            DMEEditor = pvisManager.DMEEditor;
            if (pvisManager.Tree != null)
            {
                tree = (ITree)pvisManager.Tree;
                ExtensionsHelpers = tree.ExtensionsHelpers;
            }
        }
        private ITree tree;
        public IFunctionandExtensionsHelpers ExtensionsHelpers { get; set; }

        #region Commands for DeveloperCompressionUtilities

        [CommandAttribute(
            Caption = "Zip Directory",
            Name = "ZipDirectoryCmd",
            Click = true,
            iconimage = "zipfolder.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ZipDirectoryCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select directory to zip" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "ZIP files (*.zip)|*.zip", Title = "Save ZIP file as" })
                {
                    if (fbd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.ZipDirectory(fbd.SelectedPath, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"Directory zipped to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to zip directory {fbd.SelectedPath}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error zipping directory: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Unzip File",
            Name = "UnzipFileCmd",
            Click = true,
            iconimage = "unzip.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo UnzipFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "ZIP files (*.zip)|*.zip", Title = "Select ZIP file to unzip" })
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select destination directory" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && fbd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.UnzipFile(ofd.FileName, fbd.SelectedPath);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File unzipped to {fbd.SelectedPath}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to unzip file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error unzipping file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Add File to ZIP",
            Name = "AddFileToZipCmd",
            Click = true,
            iconimage = "addtozip.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo AddFileToZipCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofdZip = new OpenFileDialog { Filter = "ZIP files (*.zip)|*.zip", Title = "Select ZIP file" })
                using (OpenFileDialog ofdFile = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to add" })
                {
                    if (ofdZip.ShowDialog() == DialogResult.OK && ofdFile.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.AddFileToZip(ofdZip.FileName, ofdFile.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File {ofdFile.FileName} added to {ofdZip.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to add file to ZIP", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error adding file to ZIP: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "GZip Compress File",
            Name = "GZipCompressFileCmd",
            Click = true,
            iconimage = "gzip.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GZipCompressFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to compress" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "GZip files (*.gz)|*.gz", Title = "Save GZip file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.GZipCompressFile(ofd.FileName, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File compressed to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to compress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error compressing file with GZip: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "GZip Decompress File",
            Name = "GZipDecompressFileCmd",
            Click = true,
            iconimage = "ungzip.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GZipDecompressFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "GZip files (*.gz)|*.gz", Title = "Select GZip file to decompress" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "All files (*.*)|*.*", Title = "Save decompressed file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.GZipDecompressFile(ofd.FileName, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File decompressed to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to decompress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error decompressing file with GZip: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Parallel Compress File",
            Name = "ParallelCompressFileCmd",
            Click = true,
            iconimage = "parallelcompress.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ParallelCompressFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to compress" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Compressed files (*.pcz)|*.pcz", Title = "Save parallel compressed file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.ParallelCompressFile(ofd.FileName, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File parallel compressed to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to parallel compress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error parallel compressing file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Parallel Decompress File",
            Name = "ParallelDecompressFileCmd",
            Click = true,
            iconimage = "paralleldecompress.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ParallelDecompressFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Compressed files (*.pcz)|*.pcz", Title = "Select parallel compressed file" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "All files (*.*)|*.*", Title = "Save decompressed file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = compressionUtil.ParallelDecompressFile(ofd.FileName, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File parallel decompressed to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to parallel decompress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error parallel decompressing file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Compress and Encrypt File",
            Name = "CompressEncryptFileCmd",
            Click = true,
            iconimage = "encrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo CompressEncryptFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor); // Assuming this exists

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to compress and encrypt" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Encrypted files (*.enc)|*.enc", Title = "Save encrypted file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Dummy AES key and IV for demonstration (in real use, generate securely)
                        byte[] aesKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes
                        byte[] aesIv = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes

                        bool success = compressionUtil.CompressThenEncryptFile(
                            ofd.FileName,
                            sfd.FileName,
                            encryptionUtil,
                            aesKey,
                            aesIv
                        );

                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File compressed and encrypted to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to compress and encrypt file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error compressing and encrypting file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Decrypt and Decompress File",
            Name = "DecryptDecompressFileCmd",
            Click = true,
            iconimage = "decrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo DecryptDecompressFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor); // Assuming this exists

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Encrypted files (*.enc)|*.enc", Title = "Select encrypted file" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "All files (*.*)|*.*", Title = "Save decrypted and decompressed file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Dummy AES key and IV (must match those used in encryption)
                        byte[] aesKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes
                        byte[] aesIv = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes

                        bool success = compressionUtil.DecryptThenDecompressFile(
                            ofd.FileName,
                            sfd.FileName,
                            encryptionUtil,
                            aesKey,
                            aesIv
                        );

                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File decrypted and decompressed to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to decrypt and decompress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error decrypting and decompressing file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "GZip Compress with Callback",
            Name = "GZipCompressWithCallbackCmd",
            Click = true,
            iconimage = "gzipcallback.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GZipCompressWithCallbackCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to compress" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "GZip files (*.gz)|*.gz", Title = "Save GZip file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        long totalBytesProcessed = 0;

                        bool success = compressionUtil.GZipCompressFileWithCallback(
                            ofd.FileName,
                            sfd.FileName,
                            cts.Token,
                            bytes => totalBytesProcessed = bytes
                        );

                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File compressed to {sfd.FileName} ({totalBytesProcessed} bytes processed)", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to compress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error compressing file with callback: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Compress File with Hash",
            Name = "CompressFileWithHashCmd",
            Click = true,
            iconimage = "hash.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo CompressFileWithHashCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperCompressionUtilities compressionUtil = new DeveloperCompressionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to compress" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "GZip files (*.gz)|*.gz", Title = "Save GZip file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        var (success, compressedPath, hash) = compressionUtil.CompressFileWithHash(ofd.FileName, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File compressed to {compressedPath} with SHA256 hash: {hash}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to compress file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error compressing file with hash: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion
    }
}