using Beep.DeveloperAssistant.Logic;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

namespace Beep.DeveloperAssistant.MenuCommands
{
    [AddinAttribute(
        Caption = "Text File",
        Name = "DeveloperTextFileMenuCommands",
        menu = "Developer",
        misc = "DeveloperTextFileMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "textfileutilities.svg",
        order = 9,
        Showin = ShowinType.Menu
    )]
    public class DeveloperTextFileMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

       // private FunctionandExtensionsHelpers ExtensionsHelpers;

        public DeveloperTextFileMenuCommands( IAppManager pvisManager)
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
        #region Commands for DeveloperTextFileUtilities

        [CommandAttribute(
            Caption = "Read Text File",
            Name = "ReadTextFileCmd",
            Click = true,
            iconimage = "readtext.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ReadTextFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select text file to read" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var lines = textUtil.ReadAllLines(ofd.FileName);
                        if (lines != null)
                        {
                            MessageBox.Show($"Read {lines.Count} lines from {ofd.FileName}", "Read Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Read {lines.Count} lines from {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to read file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error reading text file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Write Text File",
            Name = "WriteTextFileCmd",
            Click = true,
            iconimage = "writetext.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo WriteTextFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Save text file" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string content = Microsoft.VisualBasic.Interaction.InputBox("Enter text content (separate lines with \\n):", "Write Text File", "Line1\nLine2");
                        if (!string.IsNullOrEmpty(content))
                        {
                            var lines = content.Split(new[] { "\\n" }, StringSplitOptions.None).ToList();
                            bool success = textUtil.WriteAllLines(sfd.FileName, lines);
                            if (success)
                            {
                                DMEEditor.AddLogMessage("Success", $"Wrote {lines.Count} lines to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                            }
                            else
                            {
                                DMEEditor.AddLogMessage("Fail", $"Failed to write to {sfd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error writing text file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Read CSV to DataTable",
            Name = "ReadCsvToDataTableCmd",
            Click = true,
            iconimage = "csvread.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ReadCsvToDataTableCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", Title = "Select CSV file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var dt = textUtil.ReadCsvToDataTable(ofd.FileName);
                        if (dt != null)
                        {
                            MessageBox.Show($"Read {dt.Rows.Count} rows with {dt.Columns.Count} columns from {ofd.FileName}", "CSV Read Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Read {dt.Rows.Count} rows from {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to read CSV {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error reading CSV to DataTable: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Write DataTable to CSV",
            Name = "WriteDataTableToCsvCmd",
            Click = true,
            iconimage = "csvwrite.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo WriteDataTableToCsvCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                // Sample DataTable for demo; replace with actual data source if needed
                DataTable dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("Age");
                dt.Rows.Add("Alice", 25);
                dt.Rows.Add("Bob", 30);

                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", Title = "Save CSV file" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool success = textUtil.WriteDataTableToCsv(dt, sfd.FileName);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"Wrote DataTable with {dt.Rows.Count} rows to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to write DataTable to {sfd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error writing DataTable to CSV: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Read Fixed-Width File",
            Name = "ReadFixedWidthFileCmd",
            Click = true,
            iconimage = "fixedwidth.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ReadFixedWidthFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select fixed-width file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string widthsStr = Microsoft.VisualBasic.Interaction.InputBox("Enter column widths (comma-separated, e.g., 10,20,15):", "Column Widths", "10,20,15");
                        if (!string.IsNullOrEmpty(widthsStr))
                        {
                            int[] widths = widthsStr.Split(',').Select(int.Parse).ToArray();
                            var dt = textUtil.ReadFixedWidthFile(ofd.FileName, widths);
                            if (dt != null)
                            {
                                MessageBox.Show($"Read {dt.Rows.Count} rows with {dt.Columns.Count} columns from {ofd.FileName}", "Fixed-Width Read Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                DMEEditor.AddLogMessage("Success", $"Read {dt.Rows.Count} rows from {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                            }
                            else
                            {
                                DMEEditor.AddLogMessage("Fail", $"Failed to read fixed-width file {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error reading fixed-width file: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Filter Lines by Regex",
            Name = "FilterLinesByRegexCmd",
            Click = true,
            iconimage = "filter.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo FilterLinesByRegexCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select text file to filter" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Save filtered file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        string pattern = Microsoft.VisualBasic.Interaction.InputBox("Enter regex pattern to filter lines:", "Filter by Regex", ".*test.*");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            var lines = textUtil.ReadAllLines(ofd.FileName);
                            if (lines != null)
                            {
                                var filtered = textUtil.FilterLinesByRegex(lines, pattern);
                                bool success = textUtil.WriteAllLines(sfd.FileName, filtered);
                                if (success)
                                {
                                    DMEEditor.AddLogMessage("Success", $"Filtered {filtered.Count} lines to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                                }
                                else
                                {
                                    DMEEditor.AddLogMessage("Fail", $"Failed to write filtered lines to {sfd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error filtering lines by regex: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Merge Text Files",
            Name = "MergeTextFilesCmd",
            Click = true,
            iconimage = "merge.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> MergeTextFilesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select text files to merge", Multiselect = true })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Save merged file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        string separator = Microsoft.VisualBasic.Interaction.InputBox("Enter separator between files (leave blank for none):", "Separator", "---");
                        bool includeHeaders = MessageBox.Show("Include file names as headers?", "Headers", MessageBoxButtons.YesNo) == DialogResult.Yes;

                        bool success = await textUtil.MergeTextFilesAsync(ofd.FileNames, sfd.FileName, separator, includeHeaders);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"Merged {ofd.FileNames.Length} files into {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to merge files into {sfd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error merging text files: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Convert CSV to JSON",
            Name = "ConvertCsvToJsonCmd",
            Click = true,
            iconimage = "csvtojson.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ConvertCsvToJsonCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", Title = "Select CSV file" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*", Title = "Save JSON file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        string json = textUtil.ConvertCsvToJson(ofd.FileName);
                        if (json != null)
                        {
                            File.WriteAllText(sfd.FileName, json);
                            DMEEditor.AddLogMessage("Success", $"Converted {ofd.FileName} to JSON at {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to convert {ofd.FileName} to JSON", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error converting CSV to JSON: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Remove Duplicate Lines",
            Name = "RemoveDuplicateLinesCmd",
            Click = true,
            iconimage = "deduplicate.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> RemoveDuplicateLinesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select text file" })
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Save deduplicated file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool caseSensitive = MessageBox.Show("Case-sensitive comparison?", "Case Sensitivity", MessageBoxButtons.YesNo) == DialogResult.Yes;
                        bool success = await textUtil.RemoveDuplicateLinesAsync(ofd.FileName, sfd.FileName, caseSensitive);
                        if (success)
                        {
                            DMEEditor.AddLogMessage("Success", $"Removed duplicates from {ofd.FileName} to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to remove duplicates from {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error removing duplicate lines: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Split File by Lines",
            Name = "SplitFileByLinesCmd",
            Click = true,
            iconimage = "split.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> SplitFileByLinesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select text file to split" })
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select output directory" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK && fbd.ShowDialog() == DialogResult.OK)
                    {
                        string linesPerFileStr = Microsoft.VisualBasic.Interaction.InputBox("Enter number of lines per file:", "Split by Lines", "1000");
                        if (int.TryParse(linesPerFileStr, out int linesPerFile) && linesPerFile > 0)
                        {
                            string baseFileName = Path.GetFileNameWithoutExtension(ofd.FileName);
                            var outputFiles = await textUtil.SplitFileByLinesAsync(ofd.FileName, linesPerFile, fbd.SelectedPath, baseFileName);
                            if (outputFiles.Count > 0)
                            {
                                DMEEditor.AddLogMessage("Success", $"Split {ofd.FileName} into {outputFiles.Count} files in {fbd.SelectedPath}", DateTime.Now, 0, null, Errors.Ok);
                            }
                            else
                            {
                                DMEEditor.AddLogMessage("Fail", $"Failed to split {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error splitting file by lines: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Get File Stats",
            Name = "GetFileStatsCmd",
            Click = true,
            iconimage = "stats.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> GetFileStatsCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperTextFileUtilities textUtil = new DeveloperTextFileUtilities(DMEEditor);

                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Title = "Select text file for stats" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var (lineCount, wordCount, charCount) = await textUtil.GetFileStatsAsync(ofd.FileName);
                        if (lineCount >= 0)
                        {
                            string stats = $"Lines: {lineCount}\nWords: {wordCount}\nCharacters: {charCount}";
                            MessageBox.Show(stats, "File Stats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Stats for {ofd.FileName}: {stats}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to get stats for {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error getting file stats: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion
    }
}