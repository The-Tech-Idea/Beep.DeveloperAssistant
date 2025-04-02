using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        Caption = "Encryption",
        Name = "DeveloperEncryptionMenuCommands",
        menu = "Developer",
        misc = "DeveloperEncryptionMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "encryptionutilities.svg",
        order = 6,
        Showin = ShowinType.Menu
    )]
    public class DeveloperEncryptionMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

      //  private FunctionandExtensionsHelpers ExtensionsHelpers;

        public DeveloperEncryptionMenuCommands( IAppManager pvisManager)
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

        #region Commands for DeveloperEncryptionUtilities

        [CommandAttribute(
            Caption = "Compute SHA256 Hash",
            Name = "ComputeSha256HashCmd",
            Click = true,
            iconimage = "sha256.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ComputeSha256HashCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter text to hash with SHA256:", "SHA256 Hash", "");
                if (!string.IsNullOrEmpty(input))
                {
                    string hash = encryptionUtil.ComputeSha256Hash(input);
                    if (hash != null)
                    {
                        MessageBox.Show($"SHA256 Hash: {hash}", "Hash Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"SHA256 hash computed: {hash}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", "Failed to compute SHA256 hash", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error computing SHA256 hash: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate HMAC-SHA256",
            Name = "GenerateHmacSha256Cmd",
            Click = true,
            iconimage = "hmac.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateHmacSha256Cmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string message = Microsoft.VisualBasic.Interaction.InputBox("Enter message for HMAC-SHA256:", "HMAC-SHA256", "");
                string keyInput = Microsoft.VisualBasic.Interaction.InputBox("Enter HMAC key (at least 16 characters):", "HMAC Key", "mysecretkey123456");
                if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(keyInput))
                {
                    byte[] key = Encoding.UTF8.GetBytes(keyInput);
                    string hmac = encryptionUtil.GenerateHmacSha256(message, key);
                    if (hmac != null)
                    {
                        MessageBox.Show($"HMAC-SHA256: {hmac}", "HMAC Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"HMAC-SHA256 generated: {hmac}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", "Failed to generate HMAC-SHA256", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating HMAC-SHA256: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Derive Key from Password",
            Name = "DeriveKeyFromPasswordCmd",
            Click = true,
            iconimage = "pbkdf2.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo DeriveKeyFromPasswordCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string password = Microsoft.VisualBasic.Interaction.InputBox("Enter password:", "PBKDF2 Key Derivation", "");
                string saltInput = Microsoft.VisualBasic.Interaction.InputBox("Enter salt (at least 8 characters):", "Salt", "mysalt123");
                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(saltInput))
                {
                    byte[] salt = Encoding.UTF8.GetBytes(saltInput);
                    byte[] key = encryptionUtil.DeriveKeyFromPassword(password, salt, 100000, 32); // 256-bit key
                    if (key != null)
                    {
                        string keyHex = BitConverter.ToString(key).Replace("-", "").ToLower();
                        MessageBox.Show($"Derived Key (256-bit): {keyHex}", "Key Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Key derived: {keyHex}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", "Failed to derive key from password", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error deriving key: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Encrypt String (AES-CBC)",
            Name = "EncryptStringAesCbcCmd",
            Click = true,
            iconimage = "aesencrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo EncryptStringAesCbcCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string plainText = Microsoft.VisualBasic.Interaction.InputBox("Enter text to encrypt:", "AES-CBC Encryption", "");
                if (!string.IsNullOrEmpty(plainText))
                {
                    // Dummy key and IV for demonstration (32 bytes key, 16 bytes IV)
                    byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
                    byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

                    string cipherText = encryptionUtil.EncryptStringAesCbc(plainText, key, iv);
                    if (cipherText != null)
                    {
                        MessageBox.Show($"Encrypted Text: {cipherText}", "Encryption Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Text encrypted: {cipherText}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", "Failed to encrypt text with AES-CBC", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error encrypting string with AES-CBC: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Decrypt String (AES-CBC)",
            Name = "DecryptStringAesCbcCmd",
            Click = true,
            iconimage = "aesdecrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo DecryptStringAesCbcCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string cipherText = Microsoft.VisualBasic.Interaction.InputBox("Enter encrypted text (Base64):", "AES-CBC Decryption", "");
                if (!string.IsNullOrEmpty(cipherText))
                {
                    // Dummy key and IV (must match encryption)
                    byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
                    byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

                    string plainText = encryptionUtil.DecryptStringAesCbc(cipherText, key, iv);
                    if (plainText != null)
                    {
                        MessageBox.Show($"Decrypted Text: {plainText}", "Decryption Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Text decrypted: {plainText}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", "Failed to decrypt text with AES-CBC", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error decrypting string with AES-CBC: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Encrypt File (AES-CBC)",
            Name = "EncryptFileAesCbcCmd",
            Click = true,
            iconimage = "fileencrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo EncryptFileAesCbcCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to encrypt" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Encrypted files (*.enc)|*.enc", Title = "Save encrypted file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Dummy key and IV for demonstration
                        byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
                        byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

                        bool success = encryptionUtil.EncryptFileAesCbc(ofd.FileName, sfd.FileName, key, iv);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File encrypted to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to encrypt file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error encrypting file with AES-CBC: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Decrypt File (AES-CBC)",
            Name = "DecryptFileAesCbcCmd",
            Click = true,
            iconimage = "filedecrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo DecryptFileAesCbcCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Encrypted files (*.enc)|*.enc", Title = "Select encrypted file" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "All files (*.*)|*.*", Title = "Save decrypted file as" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Dummy key and IV (must match encryption)
                        byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
                        byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

                        bool success = encryptionUtil.DecryptFileAesCbc(ofd.FileName, sfd.FileName, key, iv);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File decrypted to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to decrypt file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error decrypting file with AES-CBC: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate RSA Key Pair",
            Name = "GenerateRsaKeyPairCmd",
            Click = true,
            iconimage = "rsakeys.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateRsaKeyPairCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string keySizeStr = Microsoft.VisualBasic.Interaction.InputBox("Enter RSA key size (e.g., 2048):", "RSA Key Pair Generation", "2048");
                if (int.TryParse(keySizeStr, out int keySize) && keySize >= 1024)
                {
                    var (publicKey, privateKey) = encryptionUtil.GenerateRsaKeyPair(keySize);
                    if (publicKey != null && privateKey != null)
                    {
                        using (SaveFileDialog sfdPub = new SaveFileDialog { Filter = "XML files (*.xml)|*.xml", Title = "Save public key as" })
                        using (SaveFileDialog sfdPriv = new SaveFileDialog { Filter = "XML files (*.xml)|*.xml", Title = "Save private key as" })
                        {
                            if (sfdPub.ShowDialog() == DialogResult.OK && sfdPriv.ShowDialog() == DialogResult.OK)
                            {
                                File.WriteAllText(sfdPub.FileName, publicKey);
                                File.WriteAllText(sfdPriv.FileName, privateKey);
                                DMEEditor.AddLogMessage("Success", $"RSA key pair generated: Public key at {sfdPub.FileName}, Private key at {sfdPriv.FileName}", DateTime.Now, 0, null, Errors.Ok);
                            }
                        }
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", "Failed to generate RSA key pair", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
                else
                {
                    DMEEditor.AddLogMessage("Fail", "Invalid key size entered", DateTime.Now, 0, null, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating RSA key pair: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "RSA Encrypt String",
            Name = "RsaEncryptCmd",
            Click = true,
            iconimage = "rsaencrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo RsaEncryptCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string plainText = Microsoft.VisualBasic.Interaction.InputBox("Enter text to encrypt with RSA:", "RSA Encryption", "");
                if (!string.IsNullOrEmpty(plainText))
                {
                    using (OpenFileDialog ofd = new OpenFileDialog { Filter = "XML files (*.xml)|*.xml", Title = "Select RSA public key file" })
                    {
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            string publicKeyXml = File.ReadAllText(ofd.FileName);
                            string cipherText = encryptionUtil.RsaEncrypt(plainText, publicKeyXml);
                            if (cipherText != null)
                            {
                                MessageBox.Show($"Encrypted Text: {cipherText}", "RSA Encryption Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                DMEEditor.AddLogMessage("Success", $"Text encrypted with RSA: {cipherText}", DateTime.Now, 0, null, Errors.Ok);
                            }
                            else
                            {
                                DMEEditor.AddLogMessage("Fail", "Failed to encrypt text with RSA", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error encrypting with RSA: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "RSA Decrypt String",
            Name = "RsaDecryptCmd",
            Click = true,
            iconimage = "rsadecrypt.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo RsaDecryptCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperEncryptionUtilities encryptionUtil = new DeveloperEncryptionUtilities(DMEEditor);

                string cipherText = Microsoft.VisualBasic.Interaction.InputBox("Enter encrypted text (Base64):", "RSA Decryption", "");
                if (!string.IsNullOrEmpty(cipherText))
                {
                    using (OpenFileDialog ofd = new OpenFileDialog { Filter = "XML files (*.xml)|*.xml", Title = "Select RSA private key file" })
                    {
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            string privateKeyXml = File.ReadAllText(ofd.FileName);
                            string plainText = encryptionUtil.RsaDecrypt(cipherText, privateKeyXml);
                            if (plainText != null)
                            {
                                MessageBox.Show($"Decrypted Text: {plainText}", "RSA Decryption Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                DMEEditor.AddLogMessage("Success", $"Text decrypted with RSA: {plainText}", DateTime.Now, 0, null, Errors.Ok);
                            }
                            else
                            {
                                DMEEditor.AddLogMessage("Fail", "Failed to decrypt text with RSA", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error decrypting with RSA: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion
    }
}