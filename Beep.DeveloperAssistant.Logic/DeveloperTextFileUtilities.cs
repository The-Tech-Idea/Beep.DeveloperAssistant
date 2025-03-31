using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;


namespace Beep.DeveloperAssistant.Logic
{
    /// <summary>
    /// Provides helper methods for reading and writing CSV or other text-based files,
    /// including fixed-width parsing, filtering, and asynchronous operations.
    /// </summary>
    public class DeveloperTextFileUtilities
    {
        private readonly IDMEEditor _dmeEditor;

        public DeveloperTextFileUtilities(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor ?? throw new ArgumentNullException(nameof(dmeEditor));
           
        }

        #region Basic Text I/O (Sync)

        public List<string> ReadAllLines(string filePath, Encoding encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadAllLines", $"Invalid or missing file: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }
                return File.ReadAllLines(filePath, encoding ?? Encoding.UTF8).ToList();
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadAllLines", $"Error reading file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public bool WriteAllLines(string filePath, List<string> lines, bool append = false, Encoding encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _dmeEditor.AddLogMessage("WriteAllLines", "File path is null or empty.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }
                if (lines == null)
                {
                    _dmeEditor.AddLogMessage("WriteAllLines", "Lines list is null.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                if (append)
                    File.AppendAllLines(filePath, lines, encoding ?? Encoding.UTF8);
                else
                    File.WriteAllLines(filePath, lines, encoding ?? Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("WriteAllLines", $"Error writing file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        #endregion

        #region Basic Text I/O (Async)

        public async Task<List<string>> ReadAllLinesAsync(string filePath, Encoding encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadAllLinesAsync", $"Invalid or missing file: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }
                return (await File.ReadAllLinesAsync(filePath, encoding ?? Encoding.UTF8)).ToList();
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadAllLinesAsync", $"Error reading file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<bool> WriteAllLinesAsync(string filePath, List<string> lines, bool append = false, Encoding encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _dmeEditor.AddLogMessage("WriteAllLinesAsync", "File path is null or empty.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }
                if (lines == null)
                {
                    _dmeEditor.AddLogMessage("WriteAllLinesAsync", "Lines list is null.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                if (!append)
                    await File.WriteAllLinesAsync(filePath, lines, encoding ?? Encoding.UTF8);
                else
                    using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
                        foreach (var line in lines)
                            await writer.WriteLineAsync(line);
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("WriteAllLinesAsync", $"Error writing file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        #endregion

        #region CSV Parsing/Export (Sync)

        public DataTable ReadCsvToDataTable(string filePath, char delimiter = ',', bool hasHeader = true, Encoding encoding = null)
        {
            DataTable dt = new DataTable();
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadCsvToDataTable", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }

                var lines = File.ReadAllLines(filePath, encoding ?? Encoding.UTF8);
                if (lines.Length == 0) return dt;

                int startLine = 0;
                string[] headers = ParseCsvLine(lines[0], delimiter);
                if (hasHeader)
                {
                    foreach (var header in headers)
                        dt.Columns.Add(header.Trim());
                    startLine = 1;
                }
                else
                {
                    for (int i = 0; i < headers.Length; i++)
                        dt.Columns.Add($"Column{i + 1}");
                }

                for (int i = startLine; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    string[] fields = ParseCsvLine(lines[i], delimiter);
                    var row = dt.NewRow();
                    for (int colIndex = 0; colIndex < fields.Length && colIndex < dt.Columns.Count; colIndex++)
                        row[colIndex] = fields[colIndex].Trim();
                    dt.Rows.Add(row);
                }
                return dt;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadCsvToDataTable", $"Error reading CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public bool WriteDataTableToCsv(DataTable table, string filePath, char delimiter = ',', bool includeHeader = true, Encoding encoding = null)
        {
            try
            {
                if (table == null)
                {
                    _dmeEditor.AddLogMessage("WriteDataTableToCsv", "DataTable is null.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                using (var writer = new StreamWriter(filePath, false, encoding ?? Encoding.UTF8))
                {
                    if (includeHeader)
                    {
                        var headers = table.Columns.Cast<DataColumn>().Select(col => EscapeCsvField(col.ColumnName, delimiter));
                        writer.WriteLine(string.Join(delimiter, headers));
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        var fields = row.ItemArray.Select(field => EscapeCsvField(field?.ToString() ?? "", delimiter));
                        writer.WriteLine(string.Join(delimiter, fields));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("WriteDataTableToCsv", $"Error writing CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        #endregion

        #region CSV Parsing (Async)

        public async Task<DataTable> ReadCsvToDataTableAsync(string filePath, char delimiter = ',', bool hasHeader = true, Encoding encoding = null)
        {
            DataTable dt = new DataTable();
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadCsvToDataTableAsync", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }

                var lines = await File.ReadAllLinesAsync(filePath, encoding ?? Encoding.UTF8);
                if (lines.Length == 0) return dt;

                int startLine = 0;
                string[] headers = ParseCsvLine(lines[0], delimiter);
                if (hasHeader)
                {
                    foreach (var header in headers)
                        dt.Columns.Add(header.Trim());
                    startLine = 1;
                }
                else
                {
                    for (int i = 0; i < headers.Length; i++)
                        dt.Columns.Add($"Column{i + 1}");
                }

                for (int i = startLine; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    string[] fields = ParseCsvLine(lines[i], delimiter);
                    var row = dt.NewRow();
                    for (int colIndex = 0; colIndex < fields.Length && colIndex < dt.Columns.Count; colIndex++)
                        row[colIndex] = fields[colIndex].Trim();
                    dt.Rows.Add(row);
                }
                return dt;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadCsvToDataTableAsync", $"Error reading CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<bool> WriteDataTableToCsvAsync(DataTable table, string filePath, char delimiter = ',', bool includeHeader = true, Encoding encoding = null)
        {
            try
            {
                if (table == null)
                {
                    _dmeEditor.AddLogMessage("WriteDataTableToCsvAsync", "DataTable is null.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
                {
                    if (includeHeader)
                    {
                        var headers = table.Columns.Cast<DataColumn>().Select(col => EscapeCsvField(col.ColumnName, delimiter));
                        await writer.WriteLineAsync(string.Join(delimiter, headers));
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        var fields = row.ItemArray.Select(field => EscapeCsvField(field?.ToString() ?? "", delimiter));
                        await writer.WriteLineAsync(string.Join(delimiter, fields));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("WriteDataTableToCsvAsync", $"Error writing CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        #endregion

        #region Fixed-Width Parsing

        public DataTable ReadFixedWidthFile(string filePath, int[] columnWidths, string[] columnNames = null, Encoding encoding = null)
        {
            DataTable dt = new DataTable();
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadFixedWidthFile", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }
                if (columnWidths == null || columnWidths.Length == 0)
                {
                    _dmeEditor.AddLogMessage("ReadFixedWidthFile", "columnWidths is null or empty.", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }

                for (int i = 0; i < columnWidths.Length; i++)
                    dt.Columns.Add(columnNames != null && i < columnNames.Length ? columnNames[i] : $"Column{i + 1}");

                foreach (var line in File.ReadAllLines(filePath, encoding ?? Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var row = dt.NewRow();
                    int position = 0;
                    for (int colIndex = 0; colIndex < columnWidths.Length; colIndex++)
                    {
                        int width = columnWidths[colIndex];
                        if (position >= line.Length) break;
                        int remainingLength = Math.Min(width, line.Length - position);
                        row[colIndex] = line.Substring(position, remainingLength).Trim();
                        position += width;
                    }
                    dt.Rows.Add(row);
                }
                return dt;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadFixedWidthFile", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<DataTable> ReadFixedWidthFileAsync(string filePath, int[] columnWidths, string[] columnNames = null, Encoding encoding = null)
        {
            DataTable dt = new DataTable();
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadFixedWidthFileAsync", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }
                if (columnWidths == null || columnWidths.Length == 0)
                {
                    _dmeEditor.AddLogMessage("ReadFixedWidthFileAsync", "columnWidths is null or empty.", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }

                for (int i = 0; i < columnWidths.Length; i++)
                    dt.Columns.Add(columnNames != null && i < columnNames.Length ? columnNames[i] : $"Column{i + 1}");

                var lines = await File.ReadAllLinesAsync(filePath, encoding ?? Encoding.UTF8);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var row = dt.NewRow();
                    int position = 0;
                    for (int colIndex = 0; colIndex < columnWidths.Length; colIndex++)
                    {
                        int width = columnWidths[colIndex];
                        if (position >= line.Length) break;
                        int remainingLength = Math.Min(width, line.Length - position);
                        row[colIndex] = line.Substring(position, remainingLength).Trim();
                        position += width;
                    }
                    dt.Rows.Add(row);
                }
                return dt;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadFixedWidthFileAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        #endregion

        #region Filtering or Searching

        public List<string> FilterLines(List<string> lines, Func<string, bool> predicate)
        {
            if (lines == null || predicate == null)
            {
                _dmeEditor.AddLogMessage("FilterLines", "Input lines list or predicate is null.", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
            return lines.Where(predicate).ToList();
        }

        public List<string> FilterLinesByRegex(List<string> lines, string pattern, RegexOptions options = RegexOptions.None)
        {
            if (lines == null)
            {
                _dmeEditor.AddLogMessage("FilterLinesByRegex", "Input lines list is null.", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
            if (string.IsNullOrEmpty(pattern))
                return new List<string>(lines);

            try
            {
                var regex = new Regex(pattern, options);
                return lines.Where(line => regex.IsMatch(line)).ToList();
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("FilterLinesByRegex", $"Invalid regex pattern: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
        }

        #endregion

        #region CSV <-> Object Mapping (Sync)

        public List<T> ReadCsvToObjectList<T>(string filePath, char delimiter = ',', bool hasHeader = true, Encoding encoding = null) where T : class, new()
        {
            List<T> result = new List<T>();
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadCsvToObjectList", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return result;
                }

                var lines = File.ReadAllLines(filePath, encoding ?? Encoding.UTF8);
                if (lines.Length == 0) return result;

                string[] headers = ParseCsvLine(lines[0], delimiter);
                int startLine = hasHeader ? 1 : 0;
                if (!hasHeader)
                {
                    headers = new string[headers.Length];
                    for (int c = 0; c < headers.Length; c++)
                        headers[c] = $"Column{c + 1}";
                }

                var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var headerPropMap = props.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

                for (int i = startLine; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    string[] fields = ParseCsvLine(lines[i], delimiter);
                    T obj = new T();
                    for (int c = 0; c < fields.Length && c < headers.Length; c++)
                    {
                        var headerName = headers[c].Trim();
                        if (headerPropMap.TryGetValue(headerName, out var prop))
                        {
                            object convertedValue = ConvertFieldValue(fields[c].Trim(), prop.PropertyType);
                            prop.SetValue(obj, convertedValue);
                        }
                    }
                    result.Add(obj);
                }
                return result;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadCsvToObjectList", $"Error reading CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return result;
            }
        }

        public bool WriteObjectListToCsv<T>(List<T> items, string filePath, char delimiter = ',', Encoding encoding = null)
        {
            try
            {
                if (items == null || !items.Any())
                {
                    _dmeEditor.AddLogMessage("WriteObjectListToCsv", "No items to write or null list.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                using (var writer = new StreamWriter(filePath, false, encoding ?? Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(delimiter, props.Select(p => EscapeCsvField(p.Name, delimiter))));
                    foreach (var item in items)
                    {
                        var fields = props.Select(p => EscapeCsvField(p.GetValue(item)?.ToString() ?? "", delimiter));
                        writer.WriteLine(string.Join(delimiter, fields));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("WriteObjectListToCsv", $"Error writing CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        #endregion

        #region New Utility Methods

        /// <summary>
        /// Counts the total number of lines in a file efficiently.
        /// </summary>
        public async Task<long> CountLinesAsync(string filePath, Encoding encoding = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("CountLinesAsync", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return -1;
                }

                long count = 0;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
                {
                    while (await reader.ReadLineAsync() != null)
                        count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("CountLinesAsync", $"Error counting lines: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return -1;
            }
        }

        /// <summary>
        /// Parses a CSV line handling quoted fields and escaped delimiters.
        /// </summary>
        private string[] ParseCsvLine(string line, char delimiter)
        {
            if (string.IsNullOrEmpty(line))
                return Array.Empty<string>();

            List<string> fields = new List<string>();
            bool inQuotes = false;
            StringBuilder field = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"' && (i == 0 || line[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == delimiter && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                field.Append(c);
            }
            fields.Add(field.ToString()); // Add last field
            return fields.ToArray();
        }

        /// <summary>
        /// Escapes a CSV field if it contains special characters.
        /// </summary>
        private string EscapeCsvField(string field, char delimiter)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            if (field.Contains(delimiter) || field.Contains("\"") || field.Contains("\n"))
                return $"\"{field.Replace("\"", "\"\"")}\"";
            return field;
        }

        private object ConvertFieldValue(string fieldValue, Type targetType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldValue) && targetType != typeof(string))
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

                if (targetType == typeof(string))
                    return fieldValue;

                if (targetType == typeof(int) || targetType == typeof(int?))
                    return int.TryParse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int i) ? i : (int?)null;

                if (targetType == typeof(double) || targetType == typeof(double?))
                    return double.TryParse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : (double?)null;

                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return decimal.TryParse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) ? m : (decimal?)null;

                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    return DateTime.TryParse(fieldValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? dt : (DateTime?)null;

                if (targetType.IsEnum)
                    return Enum.TryParse(targetType, fieldValue, true, out object result) ? result : null;

                return Convert.ChangeType(fieldValue, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }

        #endregion
        public async Task<bool> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    _dmeEditor.AddLogMessage("CopyFileAsync", $"Source file not found: {sourcePath}", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }
                await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite));
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("CopyFileAsync", $"Error copying file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public async Task<DataTable> ReadAllCsvsInDirectoryAsync(string directoryPath, char delimiter = ',', bool hasHeader = true)
        {
            DataTable combined = new DataTable();
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    _dmeEditor.AddLogMessage("ReadAllCsvsInDirectoryAsync", $"Directory not found: {directoryPath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }

                var csvFiles = Directory.GetFiles(directoryPath, "*.csv");
                foreach (var file in csvFiles)
                {
                    var dt = await ReadCsvToDataTableAsync(file, delimiter, hasHeader);
                    if (dt != null)
                    {
                        if (combined.Columns.Count == 0)
                            combined = dt.Clone(); // Set schema from first file
                        foreach (DataRow row in dt.Rows)
                            combined.ImportRow(row);
                    }
                }
                return combined;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadAllCsvsInDirectoryAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }
        public async Task<bool> ReplaceTextInFileAsync(string filePath, string searchText, string replaceText, bool caseSensitive = false)
        {
            try
            {
                var lines = await ReadAllLinesAsync(filePath);
                if (lines == null) return false;

                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var updatedLines = lines.Select(line => line.Replace(searchText, replaceText, comparison)).ToList();
                return await WriteAllLinesAsync(filePath, updatedLines);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReplaceTextInFileAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public List<string> TransformLines(List<string> lines, Func<string, string> transformer)
        {
            if (lines == null || transformer == null)
            {
                _dmeEditor.AddLogMessage("TransformLines", "Lines or transformer is null.", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
            return lines.Select(transformer).ToList();
        }
        public async Task<List<string>> CompareFilesAsync(string filePath1, string filePath2)
        {
            try
            {
                var lines1 = await ReadAllLinesAsync(filePath1);
                var lines2 = await ReadAllLinesAsync(filePath2);
                if (lines1 == null || lines2 == null) return new List<string>();

                List<string> differences = new List<string>();
                int maxLines = Math.Max(lines1.Count, lines2.Count);
                for (int i = 0; i < maxLines; i++)
                {
                    string line1 = i < lines1.Count ? lines1[i] : "[EOF]";
                    string line2 = i < lines2.Count ? lines2[i] : "[EOF]";
                    if (line1 != line2)
                        differences.Add($"Line {i + 1}: '{line1}' vs '{line2}'");
                }
                return differences;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("CompareFilesAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
        }
        public async Task ProcessLinesAsync(string filePath, Func<string, Task> lineProcessor, Encoding encoding = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ProcessLinesAsync", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return;
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                        await lineProcessor(line);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ProcessLinesAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }
        public async Task<(long LineCount, long WordCount, long CharCount)> GetFileStatsAsync(string filePath, Encoding encoding = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("GetFileStatsAsync", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return (-1, -1, -1);
                }

                long lines = 0, words = 0, chars = 0;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lines++;
                        chars += line.Length;
                        words += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    }
                }
                return (lines, words, chars);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("GetFileStatsAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return (-1, -1, -1);
            }
        }
        public async Task<bool> MergeTextFilesAsync(string[] sourceFiles, string outputFilePath, string separator = "", bool includeFileNamesAsHeaders = false, Encoding encoding = null)
        {
            try
            {
                if (sourceFiles == null || sourceFiles.Length == 0 || string.IsNullOrEmpty(outputFilePath))
                {
                    _dmeEditor.AddLogMessage("MergeTextFilesAsync", "Invalid input parameters.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                using (var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
                {
                    for (int i = 0; i < sourceFiles.Length; i++)
                    {
                        if (!File.Exists(sourceFiles[i]))
                        {
                            _dmeEditor.AddLogMessage("MergeTextFilesAsync", $"Source file not found: {sourceFiles[i]}", DateTime.Now, 0, null, Errors.Failed);
                            continue;
                        }

                        if (includeFileNamesAsHeaders)
                            await writer.WriteLineAsync($"--- {Path.GetFileName(sourceFiles[i])} ---");

                        var lines = await File.ReadAllLinesAsync(sourceFiles[i], encoding ?? Encoding.UTF8);
                        foreach (var line in lines)
                            await writer.WriteLineAsync(line);

                        if (i < sourceFiles.Length - 1 && !string.IsNullOrEmpty(separator))
                            await writer.WriteLineAsync(separator);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("MergeTextFilesAsync", $"Error merging files: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public string ConvertCsvToJson(string filePath, char delimiter = ',', bool hasHeader = true, Encoding encoding = null)
        {
            try
            {
                var dt = ReadCsvToDataTable(filePath, delimiter, hasHeader, encoding);
                if (dt == null) return null;

                var jsonArray = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, string>();
                    for (int i = 0; i < dt.Columns.Count; i++)
                        dict[dt.Columns[i].ColumnName] = row[i]?.ToString() ?? "";
                    jsonArray.Add(dict);
                }
                return System.Text.Json.JsonSerializer.Serialize(jsonArray, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ConvertCsvToJson", $"Error converting CSV to JSON: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public bool ConvertJsonToCsv(string jsonContent, string outputFilePath, char delimiter = ',', Encoding encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonContent))
                {
                    _dmeEditor.AddLogMessage("ConvertJsonToCsv", "JSON content is empty.", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                var jsonArray = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonContent);
                if (jsonArray == null || jsonArray.Count == 0) return false;

                var dt = new DataTable();
                var headers = jsonArray[0].Keys;
                foreach (var header in headers)
                    dt.Columns.Add(header);

                foreach (var item in jsonArray)
                {
                    var row = dt.NewRow();
                    foreach (var kvp in item)
                        row[kvp.Key] = kvp.Value;
                    dt.Rows.Add(row);
                }

                return WriteDataTableToCsv(dt, outputFilePath, delimiter, true, encoding);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ConvertJsonToCsv", $"Error converting JSON to CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public async Task<bool> RemoveDuplicateLinesAsync(string filePath, string outputFilePath, bool caseSensitive = false, Encoding encoding = null)
        {
            try
            {
                var lines = await ReadAllLinesAsync(filePath, encoding);
                if (lines == null) return false;

                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var uniqueLines = lines.Distinct(StringComparer.FromComparison(comparison)).ToList();
                return await WriteAllLinesAsync(outputFilePath, uniqueLines, false, encoding);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("RemoveDuplicateLinesAsync", $"Error removing duplicates: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public DataTable DeduplicateCsvRows(DataTable dt, string[] keyColumns)
        {
            try
            {
                if (dt == null || keyColumns == null || keyColumns.Length == 0)
                {
                    _dmeEditor.AddLogMessage("DeduplicateCsvRows", "Invalid DataTable or key columns.", DateTime.Now, 0, null, Errors.Failed);
                    return dt;
                }

                var uniqueRows = new HashSet<string>();
                var result = dt.Clone();

                foreach (DataRow row in dt.Rows)
                {
                    var key = string.Join("|", keyColumns.Select(col => row[col]?.ToString() ?? ""));
                    if (uniqueRows.Add(key))
                        result.ImportRow(row);
                }
                return result;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("DeduplicateCsvRows", $"Error deduplicating rows: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }
        public async Task<List<string>> SplitFileByLinesAsync(string sourceFilePath, int linesPerFile, string outputDirectory, string baseFileName, Encoding encoding = null)
        {
            try
            {
                var lines = await ReadAllLinesAsync(sourceFilePath, encoding);
                if (lines == null) return new List<string>();

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                List<string> outputFiles = new List<string>();
                for (int i = 0; i < lines.Count; i += linesPerFile)
                {
                    var chunk = lines.Skip(i).Take(linesPerFile).ToList();
                    string outputFile = Path.Combine(outputDirectory, $"{baseFileName}_{outputFiles.Count + 1}.txt");
                    await WriteAllLinesAsync(outputFile, chunk, false, encoding);
                    outputFiles.Add(outputFile);
                }
                return outputFiles;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("SplitFileByLinesAsync", $"Error splitting file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
        }
        public bool ValidateFileEncoding(string filePath, Encoding expectedEncoding)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ValidateFileEncoding", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream, expectedEncoding, true))
                {
                    reader.ReadToEnd(); // Attempt to read entire file
                    return reader.CurrentEncoding.Equals(expectedEncoding);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ValidateFileEncoding", $"Error validating encoding: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public bool ValidateCsvStructure(string filePath, char delimiter = ',', bool hasHeader = true, Encoding encoding = null)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, encoding ?? Encoding.UTF8);
                if (lines.Length <= (hasHeader ? 1 : 0)) return true;

                int expectedColumns = ParseCsvLine(lines[0], delimiter).Length;
                int startLine = hasHeader ? 1 : 0;
                for (int i = startLine; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    if (ParseCsvLine(lines[i], delimiter).Length != expectedColumns)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ValidateCsvStructure", $"Error validating CSV: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public async Task<bool> ExtractMatchingLinesAsync(string sourceFilePath, string outputFilePath, string pattern, RegexOptions options = RegexOptions.None, Encoding encoding = null)
        {
            try
            {
                var lines = await ReadAllLinesAsync(sourceFilePath, encoding);
                if (lines == null) return false;

                var filtered = FilterLinesByRegex(lines, pattern, options);
                return await WriteAllLinesAsync(outputFilePath, filtered, false, encoding);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ExtractMatchingLinesAsync", $"Error extracting lines: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public async Task<List<string>> SearchWithContextAsync(string filePath, string searchTerm, int contextLines = 2, bool caseSensitive = false, Encoding encoding = null)
        {
            try
            {
                var lines = await ReadAllLinesAsync(filePath, encoding);
                if (lines == null) return new List<string>();

                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                List<string> result = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains(searchTerm, comparison))
                    {
                        int start = Math.Max(0, i - contextLines);
                        int end = Math.Min(lines.Count - 1, i + contextLines);
                        for (int j = start; j <= end; j++)
                            if (!result.Contains(lines[j]))
                                result.Add($"{(j == i ? ">>" : "  ")} {j + 1}: {lines[j]}");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("SearchWithContextAsync", $"Error searching with context: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
        }
        public async Task<List<string>> ReadCompressedLinesAsync(string filePath, Encoding encoding = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor.AddLogMessage("ReadCompressedLinesAsync", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return null;
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                using (var reader = new StreamReader(gzip, encoding ?? Encoding.UTF8))
                {
                    List<string> lines = new List<string>();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                        lines.Add(line);
                    return lines;
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadCompressedLinesAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }
        #region CSV <-> Object Mapping (Async)
        public List<T> ReadCsvToObjectListWithMapping<T>(string filePath, Dictionary<string, string> headerToPropertyMap, char delimiter = ',', bool hasHeader = true) where T : class, new()
        {
            List<T> result = new List<T>();
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length == 0) return result;

                string[] headers = ParseCsvLine(lines[0], delimiter);
                int startLine = hasHeader ? 1 : 0;
                var props = typeof(T).GetProperties();

                for (int i = startLine; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    string[] fields = ParseCsvLine(lines[i], delimiter);
                    T obj = new T();

                    for (int c = 0; c < headers.Length && c < fields.Length; c++)
                    {
                        if (headerToPropertyMap.TryGetValue(headers[c].Trim(), out string propName))
                        {
                            var prop = props.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
                            if (prop != null)
                                prop.SetValue(obj, ConvertFieldValue(fields[c].Trim(), prop.PropertyType));
                        }
                    }
                    result.Add(obj);
                }
                return result;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ReadCsvToObjectListWithMapping", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return result;
            }
        }

        #endregion CSV <-> Object Mapping (Async)
    // Add these to the top of the file if not already present


}
}