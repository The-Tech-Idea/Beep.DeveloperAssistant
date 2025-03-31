using System.Globalization;
using System.Reflection;
using System.Text;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Utilities;
using System.Collections.Concurrent;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;   // for thread-safe dictionary

namespace Beep.DeveloperAssistant.Logic
{
    /// <summary>
    /// Provides utility methods for conversions between strings and types,
    /// reflection-based property mapping (including deep and cached),
    /// byte/encoding conversions, and culture-specific behavior.
    /// </summary>
    public class DeveloperConversionUtilities
    {
        private readonly IDMEEditor _dmeEditor;

        /// <summary>
        /// A reflection cache storing property info for quick reuse.
        /// Key = Type FullName, Value = List of PropertyInfo
        /// </summary>
        private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyCache
            = new ConcurrentDictionary<string, PropertyInfo[]>();

        /// <summary>
        /// Creates a new instance of <see cref="DeveloperConversionUtilities"/>.
        /// </summary>
        /// <param name="dmeEditor">Reference to DME Editor for logging and configuration.</param>
        public DeveloperConversionUtilities(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor;
        }

        #region Core Conversions

        /// <summary>
        /// Converts a string to a specified target <see cref="Type"/>, using optional culture.
        /// Returns <c>null</c> if conversion fails or if <paramref name="value"/> is empty and
        /// the <paramref name="targetType"/> is a non-string type.
        /// Supports TimeSpan, Guid, and other common .NET built-ins.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="targetType">The type to convert into.</param>
        /// <param name="culture">
        /// (Optional) The culture to use for numeric / date/time parsing. 
        /// Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </param>
        /// <returns>
        /// An object of the requested type or <c>null</c> on failure. 
        /// In case of errors, logs a message via <see cref="IDMEEditor"/>.
        /// </returns>
        public object ConvertToType(string value, Type targetType, CultureInfo culture = null)
        {
            try
            {
                culture ??= CultureInfo.InvariantCulture;

                // Handle empty or whitespace for non-string targets
                if (string.IsNullOrWhiteSpace(value) && targetType != typeof(string))
                    return null;

                // Handle direct string
                if (targetType == typeof(string))
                {
                    return value;
                }

                // Handle numeric, DateTime, TimeSpan, Guid, etc.
                if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    if (int.TryParse(value, NumberStyles.Any, culture, out int i))
                        return i;
                    return null;
                }
                if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    if (double.TryParse(value, NumberStyles.Any, culture, out double d))
                        return d;
                    return null;
                }
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    if (decimal.TryParse(value, NumberStyles.Any, culture, out decimal dec))
                        return dec;
                    return null;
                }
                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(value, culture, DateTimeStyles.None, out DateTime dt))
                        return dt;
                    return null;
                }
                if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?))
                {
                    if (TimeSpan.TryParse(value, culture, out TimeSpan ts))
                        return ts;
                    return null;
                }
                if (targetType == typeof(Guid))
                {
                    if (Guid.TryParse(value, out Guid g))
                        return g;
                    return null;
                }
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, value);
                }

                // Fallback: attempt Convert.ChangeType, which can handle basic conversions
                return System.Convert.ChangeType(value, targetType, culture);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage(
                    "ConvertToType",
                    $"Failed converting '{value}' to {targetType.Name}: {ex.Message}",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return null;
            }
        }

        /// <summary>
        /// Converts a string to a specific enum type <typeparamref name="TEnum"/>.
        /// Supports parsing by name or numeric value. Returns <c>null</c> on failure.
        /// Respects an optional <paramref name="ignoreCase"/> for string comparisons.
        /// </summary>
        public TEnum? ConvertStringToEnum<TEnum>(string value, bool ignoreCase = true) where TEnum : struct, Enum
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (Enum.TryParse(value, ignoreCase, out TEnum parsed))
                    return parsed;

                // If numeric parse fails, fallback
                if (int.TryParse(value, out int numeric))
                {
                    if (Enum.IsDefined(typeof(TEnum), numeric))
                        return (TEnum)(object)numeric;
                }

                return null;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage(
                    "ConvertStringToEnum",
                    $"Failed converting '{value}' to {typeof(TEnum).Name}: {ex.Message}",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return null;
            }
        }

        /// <summary>
        /// Converts <paramref name="text"/> into a byte array using the specified <paramref name="encoding"/>.
        /// Returns <c>null</c> on error.
        /// </summary>
        public byte[] ConvertStringToBytes(string text, Encoding encoding)
        {
            try
            {
                if (text == null)
                {
                    _dmeEditor.AddLogMessage(
                        "ConvertStringToBytes",
                        "Input string is null.",
                        DateTime.Now,
                        0,
                        null,
                        Errors.Failed
                    );
                    return null;
                }
                return encoding.GetBytes(text);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage(
                    "ConvertStringToBytes",
                    $"Error: {ex.Message}",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return null;
            }
        }

        /// <summary>
        /// Converts a byte array to a string using the specified <paramref name="encoding"/>.
        /// Returns <c>null</c> if <paramref name="data"/> is null.
        /// </summary>
        public string ConvertBytesToString(byte[] data, Encoding encoding)
        {
            try
            {
                if (data == null)
                {
                    _dmeEditor.AddLogMessage(
                        "ConvertBytesToString",
                        "Input byte array is null.",
                        DateTime.Now,
                        0,
                        null,
                        Errors.Failed
                    );
                    return null;
                }
                return encoding.GetString(data);
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage(
                    "ConvertBytesToString",
                    $"Error: {ex.Message}",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return null;
            }
        }

        /// <summary>
        /// Attempts to parse a string as a DateTime using a specified format and (optional) culture.
        /// Returns <c>null</c> if parsing fails.
        /// </summary>
        public DateTime? ConvertStringToDateTime(string dateString, string format = null, CultureInfo culture = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dateString))
                    return null;

                culture ??= CultureInfo.InvariantCulture;

                if (string.IsNullOrEmpty(format))
                {
                    if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out DateTime result))
                        return result;
                    return null;
                }
                else
                {
                    if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out DateTime exactResult))
                        return exactResult;
                    return null;
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage(
                    "ConvertStringToDateTime",
                    $"Failed to parse '{dateString}' with format '{format}': {ex.Message}",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return null;
            }
        }

        #endregion

        #region Reflection Caching

        /// <summary>
        /// Retrieves the cached properties for a given <see cref="Type"/> 
        /// or adds them if not in the dictionary yet.
        /// </summary>
        /// <param name="t">Type whose properties we want.</param>
        /// <returns>Array of <see cref="PropertyInfo"/> for that type.</returns>
        private static PropertyInfo[] GetCachedProperties(Type t)
        {
            // Use FullName as key
            string key = t.FullName;

            return _propertyCache.GetOrAdd(key, _ =>
            {
                // If not found, retrieve via reflection and store
                return t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            });
        }

        #endregion

        #region Property Mapping (Shallow and Deep)

        /// <summary>
        /// Maps properties from a source object <typeparamref name="TSource"/> to
        /// a new target object <typeparamref name="TTarget"/> using reflection and caching.
        /// If <paramref name="deepMapping"/> is <c>true</c>, it attempts to recursively map
        /// nested objects and collections.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TTarget">Target type. Must have a parameterless constructor.</typeparam>
        /// <param name="source">The source instance to copy from.</param>
        /// <param name="deepMapping">
        /// If <c>true</c>, recursively create and map sub-objects and collections.
        /// </param>
        /// <returns>A new <typeparamref name="TTarget"/> instance with mapped properties,
        /// or <c>null</c> if <paramref name="source"/> is <c>null</c>.</returns>
        public TTarget MapProperties<TSource, TTarget>(TSource source, bool deepMapping = false)
            where TTarget : new()
        {
            if (source == null)
            {
                _dmeEditor.AddLogMessage(
                    "MapProperties",
                    "Source object is null.",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return default;
            }

            try
            {
                TTarget target = new TTarget();

                var srcProps = GetCachedProperties(typeof(TSource));
                var tgtProps = GetCachedProperties(typeof(TTarget));

                // Create a quick dictionary for target properties by name
                var tgtDict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var tp in tgtProps)
                {
                    if (tp.CanWrite)
                        tgtDict[tp.Name] = tp;
                }

                foreach (PropertyInfo srcProp in srcProps)
                {
                    // only proceed if we have a matching property in target
                    if (tgtDict.TryGetValue(srcProp.Name, out PropertyInfo tgtProp))
                    {
                        object srcValue = srcProp.GetValue(source, null);
                        if (srcValue == null)
                        {
                            // set null if the target type allows it
                            if (!tgtProp.PropertyType.IsValueType
                                || Nullable.GetUnderlyingType(tgtProp.PropertyType) != null)
                            {
                                tgtProp.SetValue(target, null);
                            }
                            // else skip
                        }
                        else
                        {
                            // if same type or assignable
                            if (tgtProp.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                            {
                                // If deep mapping is enabled and the property is a class, we attempt recursion
                                if (deepMapping && IsClassButNotString(srcValue.GetType()))
                                {
                                    // handle collections or single object
                                    object nestedMapped = MapNestedObject(srcValue, tgtProp.PropertyType, deepMapping);
                                    tgtProp.SetValue(target, nestedMapped);
                                }
                                else
                                {
                                    tgtProp.SetValue(target, srcValue);
                                }
                            }
                            else
                            {
                                // Attempt string-based conversion
                                string sVal = srcValue.ToString();
                                object convertedVal = ConvertToType(sVal, tgtProp.PropertyType);
                                if (convertedVal != null)
                                {
                                    tgtProp.SetValue(target, convertedVal);
                                }
                            }
                        }
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage(
                    "MapProperties",
                    $"Error mapping properties: {ex.Message}",
                    DateTime.Now,
                    0,
                    null,
                    Errors.Failed
                );
                return default;
            }
        }

        /// <summary>
        /// Helper to check if a type is a class but not a string (since string is also a class).
        /// </summary>
        private bool IsClassButNotString(Type t)
        {
            return t.IsClass && t != typeof(string);
        }

        /// <summary>
        /// Recursively maps a nested object, attempting to handle single objects or collections.
        /// If <paramref name="targetType"/> is a generic collection, iterates over source items.
        /// Otherwise, attempts a direct object-to-object map.
        /// </summary>
        /// <param name="srcValue">The source object to map from (non-null).</param>
        /// <param name="targetType">The intended target type for the property.</param>
        /// <param name="deepMapping">Whether to continue recursion.</param>
        /// <returns>A mapped object or null on error.</returns>
        private object MapNestedObject(object srcValue, Type targetType, bool deepMapping)
        {
            // If targetType is a generic list or array, handle collection
            if (IsGenericList(targetType) && IsGenericList(srcValue.GetType()))
            {
                return MapList(srcValue, targetType, deepMapping);
            }
            else if (srcValue.GetType().IsArray && targetType.IsArray)
            {
                return MapArray((Array)srcValue, targetType, deepMapping);
            }
            else
            {
                // single object: use reflection to create new instance
                try
                {
                    object nestedTarget = Activator.CreateInstance(targetType);

                    // we call the generic method MapProperties<sourceType, targetType> 
                    // using reflection
                    var mapMethod = typeof(DeveloperConversionUtilities).GetMethod(
                        nameof(MapProperties),
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new Type[] { srcValue.GetType(), typeof(bool) },
                        null
                    );

                    if (mapMethod != null)
                    {
                        var genericMap = mapMethod.MakeGenericMethod(srcValue.GetType(), targetType);
                        return genericMap.Invoke(this, new object[] { srcValue, deepMapping });
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    _dmeEditor.AddLogMessage(
                        "MapNestedObject",
                        $"Error mapping nested object: {ex.Message}",
                        DateTime.Now,
                        0,
                        null,
                        Errors.Failed
                    );
                    return null;
                }
            }
        }

        /// <summary>
        /// Maps a source array to a target array. Only supports 1D arrays for demonstration.
        /// Uses recursion for each element if <paramref name="deepMapping"/> is true.
        /// </summary>
        private object MapArray(Array sourceArray, Type targetArrayType, bool deepMapping)
        {
            try
            {
                // e.g. targetArrayType = SomeClass[]
                Type elementType = targetArrayType.GetElementType();
                if (elementType == null) return null;

                int length = sourceArray.Length;
                Array newArray = Array.CreateInstance(elementType, length);

                for (int i = 0; i < length; i++)
                {
                    object srcElem = sourceArray.GetValue(i);
                    if (srcElem != null)
                    {
                        if (deepMapping && IsClassButNotString(srcElem.GetType()))
                        {
                            newArray.SetValue(MapNestedObject(srcElem, elementType, deepMapping), i);
                        }
                        else
                        {
                            // attempt direct assign or convert
                            if (elementType.IsAssignableFrom(srcElem.GetType()))
                            {
                                newArray.SetValue(srcElem, i);
                            }
                            else
                            {
                                string sVal = srcElem.ToString();
                                object convertedVal = ConvertToType(sVal, elementType);
                                newArray.SetValue(convertedVal, i);
                            }
                        }
                    }
                }
                return newArray;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("MapArray", ex.Message, DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        /// <summary>
        /// Maps a source generic list to a target generic list if possible.
        /// </summary>
        private object MapList(object srcValue, Type targetListType, bool deepMapping)
        {
            try
            {
                // e.g. targetListType = List<SomeClass>
                Type targetElementType = targetListType.GetGenericArguments()[0];
                // Source list's element type
                Type srcElementType = srcValue.GetType().GetGenericArguments()[0];

                // create a new list<T>
                object resultList = Activator.CreateInstance(targetListType);

                // We assume both implement IEnumerable
                var addMethod = targetListType.GetMethod("Add");
                if (addMethod == null) return null;

                IEnumerable<object> sourceEnumerable = (IEnumerable<object>)srcValue;
                foreach (object srcElem in sourceEnumerable)
                {
                    if (srcElem != null)
                    {
                        if (deepMapping && IsClassButNotString(srcElem.GetType()))
                        {
                            // recursively map sub-object
                            object mappedElem = MapNestedObject(srcElem, targetElementType, deepMapping);
                            addMethod.Invoke(resultList, new[] { mappedElem });
                        }
                        else
                        {
                            // attempt direct assignment or string-based conversion
                            if (targetElementType.IsAssignableFrom(srcElem.GetType()))
                            {
                                addMethod.Invoke(resultList, new[] { srcElem });
                            }
                            else
                            {
                                string sVal = srcElem.ToString();
                                object convertedVal = ConvertToType(sVal, targetElementType);
                                addMethod.Invoke(resultList, new[] { convertedVal });
                            }
                        }
                    }
                }

                return resultList;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("MapList", ex.Message, DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        /// <summary>
        /// Helper method to determine if a type is a generic List&lt;T&gt; or implements it.
        /// </summary>
        private bool IsGenericList(Type t)
        {
            if (!t.IsGenericType)
                return false;

            Type genericDef = t.GetGenericTypeDefinition();
            return genericDef == typeof(List<>);
        }

        #endregion
    }
}
