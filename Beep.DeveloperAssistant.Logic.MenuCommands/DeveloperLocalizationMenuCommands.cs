using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        Caption = "Localization",
        Name = "DeveloperLocalizationMenuCommands",
         menu = "Developer",
        misc = "DeveloperLocalizationMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "localizationutilities.svg",
        order = 7,
        Showin = ShowinType.Menu
    )]
    public class DeveloperLocalizationMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

       // private FunctionandExtensionsHelpers ExtensionsHelpers;
        private DeveloperLocalizationUtilities _locUtil;

        public DeveloperLocalizationMenuCommands(IAppManager pvisManager)
        {
           
           
            DMEEditor = pvisManager.DMEEditor;
            _locUtil = new DeveloperLocalizationUtilities(DMEEditor);
            if (pvisManager.Tree != null)
            {
                tree = (ITree)pvisManager.Tree;
                ExtensionsHelpers = tree.ExtensionsHelpers;
            }
        }
        private ITree tree;
        public IFunctionandExtensionsHelpers ExtensionsHelpers { get; set; }

        #region Commands

        [CommandAttribute(
            Caption = "Set Thread Culture",
            Name = "SetThreadCultureCmd",
            Click = true,
            iconimage = "culture.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo SetThreadCultureCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string cultureName = Microsoft.VisualBasic.Interaction.InputBox("Enter culture name (e.g., en-US, fr-FR):", "Set Thread Culture", "en-US");
                if (!string.IsNullOrEmpty(cultureName))
                {
                    _locUtil.SetThreadCulture(cultureName);
                    DMEEditor.AddLogMessage("Success", $"Thread culture set to {cultureName}", DateTime.Now, 0, null, Errors.Ok);
                    MessageBox.Show($"Current Culture: {_locUtil.GetCurrentCultureName()}\nCurrent UI Culture: {_locUtil.GetCurrentUICultureName()}", "Culture Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error setting thread culture: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Get Localized String",
            Name = "GetLocalizedStringCmd",
            Click = true,
            iconimage = "string.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> GetLocalizedStringCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string baseName = Microsoft.VisualBasic.Interaction.InputBox("Enter resource base name (e.g., MyApp.Resources.Strings):", "Get Localized String", "MyApp.Resources.Strings");
                string resourceKey = Microsoft.VisualBasic.Interaction.InputBox("Enter resource key:", "Resource Key", "Hello");
                if (!string.IsNullOrEmpty(baseName) && !string.IsNullOrEmpty(resourceKey))
                {
                    string localizedString = await _locUtil.TryGetLocalizedStringWithFallbackAsync(baseName, resourceKey);
                    if (localizedString != null)
                    {
                        MessageBox.Show($"Localized String: {localizedString}", "Localization Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Localized string retrieved: {localizedString}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to retrieve localized string for key {resourceKey}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error retrieving localized string: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Edit .resx File",
            Name = "EditResxFileCmd",
            Click = true,
            iconimage = "resx.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> EditResxFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "RESX files (*.resx)|*.resx", Title = "Select .resx file to edit" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var entries = await _locUtil.ReadResxFileAsync(ofd.FileName);
                        if (entries != null)
                        {
                            string key = Microsoft.VisualBasic.Interaction.InputBox("Enter resource key to add/edit:", "Edit .resx", "");
                            string value = Microsoft.VisualBasic.Interaction.InputBox("Enter resource value:", "Edit .resx", "");
                            if (!string.IsNullOrEmpty(key))
                            {
                                entries[key] = value;
                                bool success = await _locUtil.WriteResxFileAsync(ofd.FileName, entries);
                                if (success)
                                {
                                    DMEEditor.AddLogMessage("Success", $"Updated .resx file {ofd.FileName} with key {key}", DateTime.Now, 0, null, Errors.Ok);
                                    MessageBox.Show($"Updated {ofd.FileName}", "Edit Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    DMEEditor.AddLogMessage("Fail", $"Failed to update .resx file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                                }
                            }
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to read .resx file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error editing .resx file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Get Pluralized String",
            Name = "GetPluralizedStringCmd",
            Click = true,
            iconimage = "plural.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GetPluralizedStringCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string baseName = Microsoft.VisualBasic.Interaction.InputBox("Enter resource base name (e.g., MyApp.Resources.Strings):", "Get Pluralized String", "MyApp.Resources.Strings");
                string resourceKeyPrefix = Microsoft.VisualBasic.Interaction.InputBox("Enter resource key prefix (e.g., ItemCount):", "Resource Key Prefix", "ItemCount");
                string countStr = Microsoft.VisualBasic.Interaction.InputBox("Enter count:", "Count", "1");
                if (!string.IsNullOrEmpty(baseName) && !string.IsNullOrEmpty(resourceKeyPrefix) && int.TryParse(countStr, out int count))
                {
                    string pluralizedString = _locUtil.GetPluralizedString(baseName, resourceKeyPrefix, count);
                    if (pluralizedString != null)
                    {
                        MessageBox.Show($"Pluralized String: {pluralizedString}", "Pluralization Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Pluralized string retrieved: {pluralizedString}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to retrieve pluralized string for prefix {resourceKeyPrefix}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error retrieving pluralized string: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Load Satellite Assembly",
            Name = "LoadSatelliteAssemblyCmd",
            Click = true,
            iconimage = "satellite.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo LoadSatelliteAssemblyCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "DLL files (*.dll)|*.dll", Title = "Select satellite assembly" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string cultureName = Microsoft.VisualBasic.Interaction.InputBox("Enter culture name for the assembly (e.g., fr-FR):", "Load Satellite Assembly", "fr-FR");
                        if (!string.IsNullOrEmpty(cultureName))
                        {
                            bool success = _locUtil.LoadSatelliteAssembly(ofd.FileName, cultureName);
                            if (success)
                            {
                                DMEEditor.AddLogMessage("Success", $"Loaded satellite assembly {ofd.FileName} for culture {cultureName}", DateTime.Now, 0, null, Errors.Ok);
                                MessageBox.Show($"Loaded {ofd.FileName} for {cultureName}", "Load Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                DMEEditor.AddLogMessage("Fail", $"Failed to load satellite assembly {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error loading satellite assembly: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Load DB Translations",
            Name = "LoadDbTranslationsCmd",
            Click = true,
            iconimage = "dbtranslate.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo LoadDbTranslationsCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                // Sample DataTable for demo
                DataTable sampleTable = new DataTable();
                sampleTable.Columns.Add("Culture", typeof(string));
                sampleTable.Columns.Add("ResourceKey", typeof(string));
                sampleTable.Columns.Add("ResourceValue", typeof(string));
                sampleTable.Rows.Add("en-US", "Hello", "Hello, World!");
                sampleTable.Rows.Add("fr-FR", "Hello", "Bonjour le monde!");

                string cultureName = Microsoft.VisualBasic.Interaction.InputBox("Enter culture name (e.g., en-US):", "Load DB Translations", "en-US");
                if (!string.IsNullOrEmpty(cultureName))
                {
                    var translations = _locUtil.LoadDbTranslations(sampleTable, cultureName);
                    if (translations.Count > 0)
                    {
                        string key = Microsoft.VisualBasic.Interaction.InputBox("Enter resource key to retrieve:", "Get DB Translation", "Hello");
                        if (!string.IsNullOrEmpty(key))
                        {
                            string value = _locUtil.GetDbLocalizedString(translations, key, "Fallback Value");
                            MessageBox.Show($"Translated Value: {value}", "DB Translation Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Loaded DB translations for {cultureName}, retrieved {value} for key {key}", DateTime.Now, 0, null, Errors.Ok);
                        }
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"No translations loaded for culture {cultureName}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error loading DB translations: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Localize Form",
            Name = "LocalizeFormCmd",
            Click = true,
            iconimage = "form.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo LocalizeFormCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string baseName = Microsoft.VisualBasic.Interaction.InputBox("Enter resource base name (e.g., MyApp.Resources.Strings):", "Localize Form", "MyApp.Resources.Strings");
                if (!string.IsNullOrEmpty(baseName))
                {
                    var form = new Form { Text = "TestForm", Size = new System.Drawing.Size(300, 200) };
                    form.Controls.Add(new Button { Name = "btnTest", Text = "Click Me", Location = new System.Drawing.Point(50, 50) });
                    LocalizeControl(form, baseName);
                    form.ShowDialog();
                    DMEEditor.AddLogMessage("Success", $"Localized test form with base name {baseName}", DateTime.Now, 0, null, Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error localizing form: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Format DateTime",
            Name = "FormatDateTimeCmd",
            Click = true,
            iconimage = "datetime.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo FormatDateTimeCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string cultureName = Microsoft.VisualBasic.Interaction.InputBox("Enter culture name (e.g., en-US, leave blank for current):", "Format DateTime", "");
                string formatString = Microsoft.VisualBasic.Interaction.InputBox("Enter format string (e.g., dddd, MMMM dd, yyyy):", "Format String", "G");
                if (!string.IsNullOrEmpty(formatString))
                {
                    string formattedDate = _locUtil.FormatDateTime(DateTime.Now, cultureName, formatString);
                    MessageBox.Show($"Formatted DateTime: {formattedDate}", "Formatting Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DMEEditor.AddLogMessage("Success", $"DateTime formatted: {formattedDate}", DateTime.Now, 0, null, Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error formatting DateTime: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion

        public  void LocalizeControl(Control control, string baseName, string resourceKeyPrefix = null)
        {
            if (control == null)
            {
                LogError(nameof(LocalizeControl), "Control is null.");
                return;
            }

            string key = resourceKeyPrefix != null ? $"{resourceKeyPrefix}_{control.Name}" : control.Name;
            string localizedText = _locUtil.TryGetLocalizedStringWithFallback(baseName, key);
            if (!string.IsNullOrEmpty(localizedText))
                control.Text = localizedText;

            foreach (Control child in control.Controls)
                LocalizeControl(child, baseName, resourceKeyPrefix);
        }
        #region Logging Helpers

        private void LogMessage(string methodName, string message)
        {
            DMEEditor.AddLogMessage(methodName, message, DateTime.Now, 0, null, Errors.Ok);
        }

        private void LogError(string methodName, string message)
        {
            DMEEditor.AddLogMessage(methodName, message, DateTime.Now, 0, null, Errors.Failed);
        }

        private void LogException(string methodName, Exception ex)
        {
            DMEEditor.AddLogMessage(methodName, $"Exception: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
        }

        #endregion
    }
}