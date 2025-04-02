using System;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

using System.Xml.Linq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace Beep.DeveloperAssistant.Logic
{
    public class DeveloperLocalizationUtilities
    {
        private readonly IDMEEditor _dmeEditor;
        private static readonly ConcurrentDictionary<string, (ResourceManager Manager, DateTime Expiration)> _resourceManagerCache
            = new ConcurrentDictionary<string, (ResourceManager, DateTime)>();

        public DeveloperLocalizationUtilities(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor ?? throw new ArgumentNullException(nameof(dmeEditor));
        }

        #region 1) Basic Culture Management

        public void SetThreadCulture(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                LogError(nameof(SetThreadCulture), "Culture name is null or empty.");
                return;
            }
            try
            {
                var culture = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                LogMessage(nameof(SetThreadCulture), $"Culture set to {cultureName}");
            }
            catch (CultureNotFoundException ex)
            {
                LogException(nameof(SetThreadCulture), ex);
            }
        }

        public string GetCurrentCultureName() => Thread.CurrentThread.CurrentCulture.Name;
        public string GetCurrentUICultureName() => Thread.CurrentThread.CurrentUICulture.Name;

        #endregion

        #region 2) Loading Compiled Resources (with Fallback Logic)

        public string TryGetLocalizedStringWithFallback(string baseName, string resourceKey, TimeSpan? cacheExpiration = null)
        {
            if (string.IsNullOrEmpty(baseName) || string.IsNullOrEmpty(resourceKey))
            {
                LogError(nameof(TryGetLocalizedStringWithFallback), "baseName or resourceKey is null/empty.");
                return null;
            }

            try
            {
                ResourceManager rm = GetOrAddResourceManager(baseName, cacheExpiration ?? TimeSpan.FromMinutes(10));
                string value = rm.GetString(resourceKey, Thread.CurrentThread.CurrentUICulture) ??
                              rm.GetString(resourceKey, Thread.CurrentThread.CurrentUICulture.Parent);

                if (value == null)
                {
                    LogError(nameof(TryGetLocalizedStringWithFallback), $"Resource key '{resourceKey}' not found in '{baseName}' for culture or fallback.");
                }
                return value;
            }
            catch (Exception ex)
            {
                LogException(nameof(TryGetLocalizedStringWithFallback), ex);
                return null;
            }
        }

        public async Task<string> TryGetLocalizedStringWithFallbackAsync(string baseName, string resourceKey, TimeSpan? cacheExpiration = null)
        {
            return await Task.Run(() => TryGetLocalizedStringWithFallback(baseName, resourceKey, cacheExpiration));
        }

        #endregion

        #region 3) Raw .resx Editing

        public async Task<Dictionary<string, string>> ReadResxFileAsync(string resxFilePath)
        {
            if (!File.Exists(resxFilePath))
            {
                LogError(nameof(ReadResxFileAsync), $"File not found: {resxFilePath}");
                return null;
            }
            try
            {
                using (var stream = File.OpenRead(resxFilePath))
                {
                    var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
                    var dict = doc.Descendants("data")
                        .Where(d => d.Attribute("name") != null)
                        .ToDictionary(
                            d => d.Attribute("name")!.Value,
                            d => d.Element("value")?.Value ?? string.Empty);
                    return dict;
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(ReadResxFileAsync), ex);
                return null;
            }
        }

        public async Task<bool> WriteResxFileAsync(string resxFilePath, Dictionary<string, string> entries)
        {
            if (string.IsNullOrEmpty(resxFilePath) || entries == null || entries.Count == 0)
            {
                LogError(nameof(WriteResxFileAsync), "resxFilePath or entries invalid.");
                return false;
            }
            try
            {
                XDocument doc = File.Exists(resxFilePath) ? XDocument.Load(resxFilePath) : CreateNewResxDocument();
                XElement root = doc.Element("root") ?? throw new InvalidOperationException("Missing 'root' element.");

                var existingDataNodes = root.Elements("data").ToDictionary(
                    el => (string)el.Attribute("name")!,
                    el => el,
                    StringComparer.OrdinalIgnoreCase);

                foreach (var kvp in entries)
                {
                    if (existingDataNodes.TryGetValue(kvp.Key, out XElement existingEl))
                    {
                        var valueEl = existingEl.Element("value") ?? new XElement("value");
                        valueEl.Value = kvp.Value ?? string.Empty;
                        if (!existingEl.Elements().Contains(valueEl)) existingEl.Add(valueEl);
                    }
                    else
                    {
                        root.Add(new XElement("data",
                            new XAttribute("name", kvp.Key),
                            new XAttribute(XNamespace.Xml + "space", "preserve"),
                            new XElement("value", kvp.Value ?? string.Empty)));
                    }
                }

                using (var stream = File.OpenWrite(resxFilePath))
                {
                    await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(WriteResxFileAsync), ex);
                return false;
            }
        }

        public bool ValidateResxFile(string resxFilePath)
        {
            try
            {
                var doc = XDocument.Load(resxFilePath);
                return doc.Element("root") != null && doc.Descendants("data").All(d => d.Attribute("name") != null);
            }
            catch (Exception ex)
            {
                LogException(nameof(ValidateResxFile), ex);
                return false;
            }
        }

        #endregion

        #region 4) Advanced Pluralization

        public string GetPluralizedString(string baseName, string resourceKeyPrefix, int count)
        {
            var culture = CultureInfo.CurrentUICulture;
            string suffix = count == 1 ? "One" : "Other"; // Simplified; extend with PluralRules for complex cases
            string key = $"{resourceKeyPrefix}_{suffix}";
            string formatString = TryGetLocalizedStringWithFallback(baseName, key) ?? $"{count} items";
            return string.Format(culture, formatString, count);
        }

        #endregion

        #region 5) Advanced Resource Deployment: Satellite Assemblies

        public bool LoadSatelliteAssembly(string assemblyPath, string cultureName)
        {
            if (!File.Exists(assemblyPath))
            {
                LogError(nameof(LoadSatelliteAssembly), $"Satellite assembly not found: {assemblyPath}");
                return false;
            }
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                CultureInfo culture = new CultureInfo(cultureName);
                LogMessage(nameof(LoadSatelliteAssembly), $"Loaded satellite assembly for culture {cultureName}: {assembly.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                LogException(nameof(LoadSatelliteAssembly), ex);
                return false;
            }
        }

        #endregion

        #region 6) User-Defined / DB-Backed Translations

        public Dictionary<string, string> LoadDbTranslations(DataTable dataTable, string cultureName)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (dataTable == null || string.IsNullOrEmpty(cultureName))
            {
                LogError(nameof(LoadDbTranslations), "dataTable or cultureName is null/empty.");
                return dict;
            }

            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["Culture"]?.ToString() == cultureName)
                    {
                        string key = row["ResourceKey"]?.ToString();
                        string val = row["ResourceValue"]?.ToString();
                        if (!string.IsNullOrEmpty(key))
                            dict[key] = val;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(LoadDbTranslations), ex);
            }
            return dict;
        }

        public string GetDbLocalizedString(Dictionary<string, string> dbDict, string resourceKey, string fallback = null)
        {
            if (dbDict == null || string.IsNullOrEmpty(resourceKey))
            {
                LogError(nameof(GetDbLocalizedString), "dbDict or resourceKey is null/empty.");
                return fallback;
            }
            return dbDict.TryGetValue(resourceKey, out string value) ? value : fallback ?? LogErrorAndReturn(nameof(GetDbLocalizedString), $"Key '{resourceKey}' not found.", null);
        }

        #endregion

        #region 7) UI Localization

        //public void LocalizeControl(Control control, string baseName, string resourceKeyPrefix = null)
        //{
        //    if (control == null)
        //    {
        //        LogError(nameof(LocalizeControl), "Control is null.");
        //        return;
        //    }

        //    string key = resourceKeyPrefix != null ? $"{resourceKeyPrefix}_{control.Name}" : control.Name;
        //    string localizedText = TryGetLocalizedStringWithFallback(baseName, key);
        //    if (!string.IsNullOrEmpty(localizedText))
        //        control.Text = localizedText;

        //    foreach (Control child in control.Controls)
        //        LocalizeControl(child, baseName, resourceKeyPrefix);
        //}

        #endregion

        #region 8) Date/Number Formatting

        public string FormatDateTime(DateTime dateTime, string cultureName = null, string formatString = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            try
            {
                var culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);
                return dateTime.ToString(formatString ?? "G", culture);
            }
            catch (CultureNotFoundException ex)
            {
                LogException(nameof(FormatDateTime), ex);
                return dateTime.ToString();
            }
        }

        public DateTime? ParseDateTime(string dateString, string cultureName = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;
            try
            {
                var culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);
                return DateTime.TryParse(dateString, culture, styles, out DateTime dt) ? dt : null;
            }
            catch (CultureNotFoundException ex)
            {
                LogException(nameof(ParseDateTime), ex);
                return null;
            }
        }

        public string FormatNumber(double number, string cultureName = null, string formatString = null, NumberStyles styles = NumberStyles.Any)
        {
            try
            {
                var culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);
                return number.ToString(formatString ?? "N", culture);
            }
            catch (CultureNotFoundException ex)
            {
                LogException(nameof(FormatNumber), ex);
                return number.ToString();
            }
        }

        #endregion

        #region Helper Methods

        private ResourceManager GetOrAddResourceManager(string baseName, TimeSpan expiration)
        {
            if (_resourceManagerCache.TryGetValue(baseName, out var cached) && DateTime.Now < cached.Expiration)
                return cached.Manager;

            var rm = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
            _resourceManagerCache[baseName] = (rm, DateTime.Now + expiration);
            return rm;
        }

        private XDocument CreateNewResxDocument()
        {
            return new XDocument(
                new XElement("root",
                    new XElement("resheader", new XAttribute("name", "resmimetype"), new XElement("value", "text/microsoft-resx")),
                    new XElement("resheader", new XAttribute("name", "version"), new XElement("value", "2.0")),
                    new XElement("resheader", new XAttribute("name", "reader"), new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, ...")),
                    new XElement("resheader", new XAttribute("name", "writer"), new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, ..."))
                )
            );
        }

        private string LogErrorAndReturn(string methodName, string message, string fallback)
        {
            LogError(methodName, message);
            return fallback;
        }

        #endregion

        #region Logging Helpers

        private void LogMessage(string methodName, string message)
        {
            _dmeEditor.AddLogMessage(methodName, message, DateTime.Now, 0, null, Errors.Ok);
        }

        private void LogError(string methodName, string message)
        {
            _dmeEditor.AddLogMessage(methodName, message, DateTime.Now, 0, null, Errors.Failed);
        }

        private void LogException(string methodName, Exception ex)
        {
            _dmeEditor.AddLogMessage(methodName, $"Exception: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
        }

        #endregion
    }
}