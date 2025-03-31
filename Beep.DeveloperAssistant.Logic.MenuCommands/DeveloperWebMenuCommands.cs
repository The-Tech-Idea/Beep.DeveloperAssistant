using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Extensions;
using Beep.DeveloperAssistant.Logic;

namespace Beep.DeveloperAssistantMenuCommands
{
    [AddinAttribute(
        Caption = "Web",
        Name = "DeveloperWebMenuCommands",
        menu = "Beep",
        misc = "DeveloperWebMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "webutilities.svg",
        order = 10,
        Showin = ShowinType.Menu
    )]
    public class DeveloperWebMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

        private FunctionandExtensionsHelpers ExtensionsHelpers;
        private DeveloperWebUtilities _webUtil;

        public DeveloperWebMenuCommands(IDMEEditor pdMEEditor, IAppManager pvisManager, ITree ptreeControl)
        {
            DMEEditor = pdMEEditor ?? throw new ArgumentNullException(nameof(pdMEEditor));
            ExtensionsHelpers = new FunctionandExtensionsHelpers(DMEEditor, pvisManager, ptreeControl);
            _webUtil = new DeveloperWebUtilities(DMEEditor);
        }

        #region Commands for DeveloperWebUtilities

        [CommandAttribute(
            Caption = "Get Text",
            Name = "GetTextCmd",
            Click = true,
            iconimage = "gettext.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> GetTextCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Get Text", "https://example.com");
                if (!string.IsNullOrEmpty(url))
                {
                    var (content, headers) = await _webUtil.GetTextAsync(url);
                    if (content != null)
                    {
                        MessageBox.Show($"Content:\n{content}\n\nHeaders:\n{headers}", "Get Text Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Fetched text from {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to fetch text from {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error fetching text: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Get Lines",
            Name = "GetLinesCmd",
            Click = true,
            iconimage = "getlines.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> GetLinesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Get Lines", "https://example.com/lines.txt");
                if (!string.IsNullOrEmpty(url))
                {
                    var lines = await _webUtil.GetLinesAsync(url);
                    if (lines.Any())
                    {
                        MessageBox.Show($"Lines Fetched: {lines.Count}\nFirst 5 lines:\n{string.Join("\n", lines.Take(5))}", "Get Lines Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Fetched {lines.Count} lines from {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to fetch lines from {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error fetching lines: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Download File",
            Name = "DownloadFileCmd",
            Click = true,
            iconimage = "download.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> DownloadFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL to download:", "Download File", "https://example.com/file.zip");
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "All files (*.*)|*.*", Title = "Save downloaded file" })
                {
                    if (!string.IsNullOrEmpty(url) && sfd.ShowDialog() == DialogResult.OK)
                    {
                        var cts = new CancellationTokenSource();
                        bool success = await _webUtil.DownloadFileAsync(
                            url,
                            sfd.FileName,
                            (downloaded, total) => DMEEditor.AddLogMessage("Download", $"Progress: {downloaded}/{total} bytes", DateTime.Now, 0, null, Errors.Information),
                            cancellationToken: cts.Token
                        );
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"File downloaded to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                            MessageBox.Show($"File downloaded to {sfd.FileName}", "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to download file from {url}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error downloading file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Post Text",
            Name = "PostTextCmd",
            Click = true,
            iconimage = "posttext.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> PostTextCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Post Text", "https://example.com/api");
                string content = Microsoft.VisualBasic.Interaction.InputBox("Enter text content to post:", "Post Content", "Hello, World!");
                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(content))
                {
                    var (response, headers) = await _webUtil.PostTextAsync(url, content);
                    if (response != null)
                    {
                        MessageBox.Show($"Response:\n{response}\n\nHeaders:\n{headers}", "Post Text Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Posted text to {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to post text to {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error posting text: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Post Form",
            Name = "PostFormCmd",
            Click = true,
            iconimage = "postform.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> PostFormCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Post Form", "https://example.com/form");
                string formDataStr = Microsoft.VisualBasic.Interaction.InputBox("Enter form data (key=value,key2=value2):", "Form Data", "name=John,age=30");
                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(formDataStr))
                {
                    var formData = formDataStr.Split(',').Select(kv => kv.Split('=')).ToDictionary(kv => kv[0], kv => kv[1]);
                    var (response, headers) = await _webUtil.PostFormAsync(url, formData);
                    if (response != null)
                    {
                        MessageBox.Show($"Response:\n{response}\n\nHeaders:\n{headers}", "Post Form Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Posted form to {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to post form to {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error posting form: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Post Multipart (File Upload)",
            Name = "PostMultipartCmd",
            Click = true,
            iconimage = "upload.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> PostMultipartCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Post Multipart", "https://example.com/upload");
                string formDataStr = Microsoft.VisualBasic.Interaction.InputBox("Enter form data (key=value,key2=value2, optional):", "Form Data", "description=Test");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select file to upload" })
                {
                    if (!string.IsNullOrEmpty(url) && ofd.ShowDialog() == DialogResult.OK)
                    {
                        var formData = string.IsNullOrEmpty(formDataStr) ? new Dictionary<string, string>() : formDataStr.Split(',').Select(kv => kv.Split('=')).ToDictionary(kv => kv[0], kv => kv[1]);
                        var files = new Dictionary<string, (string, string)> { { "file", (ofd.FileName, "application/octet-stream") } };
                        var (response, headers) = await _webUtil.PostMultipartAsync(url, formData, files);
                        if (response != null)
                        {
                            MessageBox.Show($"Response:\n{response}\n\nHeaders:\n{headers}", "Post Multipart Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Uploaded file to {url}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to upload file to {url}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error posting multipart: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Put Text",
            Name = "PutTextCmd",
            Click = true,
            iconimage = "puttext.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> PutTextCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Put Text", "https://example.com/data/1");
                string content = Microsoft.VisualBasic.Interaction.InputBox("Enter text content to put:", "Put Content", "Updated Data");
                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(content))
                {
                    var (response, headers) = await _webUtil.PutTextAsync(url, content);
                    if (response != null)
                    {
                        MessageBox.Show($"Response:\n{response}\n\nHeaders:\n{headers}", "Put Text Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Put text to {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to put text to {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error putting text: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Delete Resource",
            Name = "DeleteCmd",
            Click = true,
            iconimage = "delete.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> DeleteCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL to delete:", "Delete Resource", "https://example.com/data/1");
                if (!string.IsNullOrEmpty(url))
                {
                    var (response, headers) = await _webUtil.DeleteAsync(url);
                    if (response != null)
                    {
                        MessageBox.Show($"Response:\n{response}\n\nHeaders:\n{headers}", "Delete Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Deleted resource at {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to delete resource at {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error deleting resource: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Get JSON",
            Name = "GetJsonCmd",
            Click = true,
            iconimage = "getjson.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> GetJsonCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Get JSON", "https://example.com/data.json");
                if (!string.IsNullOrEmpty(url))
                {
                    var data = await _webUtil.GetJsonAsync<Dictionary<string, string>>(url);
                    if (data != null)
                    {
                        var jsonStr = string.Join("\n", data.Select(kv => $"{kv.Key}: {kv.Value}"));
                        MessageBox.Show($"JSON Data:\n{jsonStr}", "Get JSON Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Fetched JSON from {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to fetch JSON from {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error fetching JSON: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Post JSON",
            Name = "PostJsonCmd",
            Click = true,
            iconimage = "postjson.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> PostJsonCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("Enter URL:", "Post JSON", "https://example.com/api");
                string jsonData = Microsoft.VisualBasic.Interaction.InputBox("Enter JSON data (e.g., {\"key\":\"value\"}):", "JSON Data", "{\"name\":\"Test\"}");
                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(jsonData))
                {
                    var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
                    var response = await _webUtil.PostJsonAsync<Dictionary<string, string>, string>(url, requestData);
                    if (response != null)
                    {
                        MessageBox.Show($"Response:\n{response}", "Post JSON Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Posted JSON to {url}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to post JSON to {url}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error posting JSON: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion
    }
}