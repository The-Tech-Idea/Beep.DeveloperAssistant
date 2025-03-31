using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace Beep.DeveloperAssistant.Logic
{
    /// <summary>
    /// Provides reflection-based utility methods, including assembly scanning, 
    /// attribute inspection, method invocation, dynamic type creation,
    /// caching of reflection data, advanced IL generation, handling open generics,
    /// and scanning all assemblies in the current AppDomain.
    /// </summary>
    public class DeveloperReflectionUtilities
    {
        private readonly IDMEEditor _dmeEditor;

        // 1) Caching: we store property and method info in concurrent dictionaries for thread-safe access.
        // Key = Type's FullName or something unique, Value = arrays of MemberInfo
        private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyCache
            = new ConcurrentDictionary<string, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<string, MethodInfo[]> _methodCache
            = new ConcurrentDictionary<string, MethodInfo[]>();

        /// <summary>
        /// Initializes a new instance of <see cref="DeveloperReflectionUtilities"/>.
        /// </summary>
        /// <param name="dmeEditor">Reference to the DME Editor for logging and configuration.</param>
        public DeveloperReflectionUtilities(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor;
        }

        #region --- 1) Assembly Scanning (Including Current AppDomain) ---

        /// <summary>
        /// Finds all types in the given <paramref name="assembly"/> that implement or inherit
        /// the specified interface <typeparamref name="TInterface"/>.
        /// </summary>
        public List<Type> GetTypesImplementingInterface<TInterface>(Assembly assembly)
        {
            List<Type> result = new List<Type>();
            if (assembly == null)
            {
                LogError(nameof(GetTypesImplementingInterface), "Assembly is null.");
                return result;
            }

            try
            {
                Type interfaceType = typeof(TInterface);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                        continue;

                    if (interfaceType.IsAssignableFrom(type))
                    {
                        result.Add(type);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(GetTypesImplementingInterface), ex);
            }
            return result;
        }

        /// <summary>
        /// Scans *all* assemblies in the current AppDomain, returning all types that implement <typeparamref name="TInterface"/>.
        /// Helpful if you want to find plugins or classes anywhere in your application.
        /// </summary>
        public List<Type> GetTypesImplementingInterfaceInAllAssemblies<TInterface>()
        {
            List<Type> result = new List<Type>();
            try
            {
                Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in allAssemblies)
                {
                    result.AddRange(GetTypesImplementingInterface<TInterface>(assembly));
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(GetTypesImplementingInterfaceInAllAssemblies), ex);
            }
            return result;
        }

        /// <summary>
        /// Retrieves all assemblies loaded in the current AppDomain.
        /// </summary>
        public Assembly[] GetAllAssembliesInCurrentDomain()
        {
            try
            {
                return AppDomain.CurrentDomain.GetAssemblies();
            }
            catch (Exception ex)
            {
                LogException(nameof(GetAllAssembliesInCurrentDomain), ex);
                return Array.Empty<Assembly>();
            }
        }

        #endregion

        #region --- 2) Handling Partial/Open Generics ---

        /// <summary>
        /// Scans an assembly for types that match a given open generic definition.
        /// For example, if openGenericDefinition = typeof(IList&lt;&gt;),
        /// this method finds all types in <paramref name="assembly"/> that are IList&lt;T&gt; for some T.
        /// </summary>
        /// <param name="assembly">Assembly to scan.</param>
        /// <param name="openGenericDefinition">An open generic type, e.g. typeof(IList&lt;&gt;).</param>
        /// <returns>List of matching concrete types.</returns>
        public List<Type> GetTypesOfOpenGeneric(Assembly assembly, Type openGenericDefinition)
        {
            var result = new List<Type>();
            if (assembly == null)
            {
                LogError(nameof(GetTypesOfOpenGeneric), "Assembly is null.");
                return result;
            }
            if (openGenericDefinition == null || !openGenericDefinition.IsGenericTypeDefinition)
            {
                LogError(nameof(GetTypesOfOpenGeneric), "The provided type is not an open generic definition.");
                return result;
            }

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    // Check if any of the interfaces or the type itself is a closed version of openGenericDefinition
                    var allInterfaces = type.GetInterfaces();
                    foreach (var iface in allInterfaces)
                    {
                        if (iface.IsGenericType)
                        {
                            var baseDef = iface.GetGenericTypeDefinition();
                            if (baseDef == openGenericDefinition)
                            {
                                result.Add(type);
                                break;
                            }
                        }
                    }

                    // Also check if the type itself is a closed version
                    if (type.IsGenericType)
                    {
                        var baseDef = type.GetGenericTypeDefinition();
                        if (baseDef == openGenericDefinition)
                        {
                            result.Add(type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(GetTypesOfOpenGeneric), ex);
            }
            return result;
        }

        #endregion

        #region --- 3) Caching for Properties and Methods ---

        /// <summary>
        /// Retrieves all public properties for a type, using a reflection cache to speed up repeated lookups.
        /// </summary>
        public PropertyInfo[] GetCachedProperties(Type type)
        {
            if (type == null)
            {
                LogError(nameof(GetCachedProperties), "Type is null.");
                return Array.Empty<PropertyInfo>();
            }

            try
            {
                return _propertyCache.GetOrAdd(type.FullName, _ =>
                {
                    return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                });
            }
            catch (Exception ex)
            {
                LogException(nameof(GetCachedProperties), ex);
                return Array.Empty<PropertyInfo>();
            }
        }

        /// <summary>
        /// Retrieves all public methods for a type, using a reflection cache.
        /// </summary>
        public MethodInfo[] GetCachedMethods(Type type)
        {
            if (type == null)
            {
                LogError(nameof(GetCachedMethods), "Type is null.");
                return Array.Empty<MethodInfo>();
            }

            try
            {
                return _methodCache.GetOrAdd(type.FullName, _ =>
                {
                    return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                });
            }
            catch (Exception ex)
            {
                LogException(nameof(GetCachedMethods), ex);
                return Array.Empty<MethodInfo>();
            }
        }

        #endregion

        #region --- 4) Attribute Inspection & Method Invocation (Original) ---

        /// <summary>
        /// Returns all properties on the given <paramref name="type"/> that are decorated with
        /// the attribute <typeparamref name="TAttribute"/>.
        /// </summary>
        public IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TAttribute>(Type type)
            where TAttribute : Attribute
        {
            if (type == null)
            {
                LogError(nameof(GetPropertiesWithAttribute), "Type is null.");
                return Enumerable.Empty<PropertyInfo>();
            }

            try
            {
                var props = GetCachedProperties(type);
                return props.Where(p => p.GetCustomAttributes(typeof(TAttribute), true).Any());
            }
            catch (Exception ex)
            {
                LogException(nameof(GetPropertiesWithAttribute), ex);
                return Enumerable.Empty<PropertyInfo>();
            }
        }

        /// <summary>
        /// Extracts data about all attributes of the given <paramref name="member"/>.
        /// For each attribute, you can inspect constructor args and named properties.
        /// </summary>
        public Dictionary<string, List<string>> GetAttributeData(MemberInfo member)
        {
            var result = new Dictionary<string, List<string>>();
            if (member == null)
            {
                LogError(nameof(GetAttributeData), "MemberInfo is null.");
                return result;
            }

            try
            {
                var attributeDataList = member.GetCustomAttributesData();
                foreach (var attrData in attributeDataList)
                {
                    string attrTypeName = attrData.AttributeType.Name;
                    if (!result.ContainsKey(attrTypeName))
                    {
                        result[attrTypeName] = new List<string>();
                    }

                    // Constructor arguments
                    foreach (var ctorArg in attrData.ConstructorArguments)
                    {
                        result[attrTypeName].Add($"CtorArg: {ctorArg.Value}");
                    }

                    // Named arguments
                    foreach (var namedArg in attrData.NamedArguments)
                    {
                        string argInfo = $"{namedArg.MemberName} = {namedArg.TypedValue.Value}";
                        result[attrTypeName].Add($"NamedArg: {argInfo}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(nameof(GetAttributeData), ex);
            }
            return result;
        }

        /// <summary>
        /// Dynamically invokes a method by <paramref name="methodName"/> on the given <paramref name="instance"/>
        /// with optional <paramref name="parameters"/>.
        /// </summary>
        public object InvokeMethod(object instance, string methodName, params object[] parameters)
        {
            if (instance == null)
            {
                LogError(nameof(InvokeMethod), "Instance is null.");
                return null;
            }
            if (string.IsNullOrEmpty(methodName))
            {
                LogError(nameof(InvokeMethod), "Method name is null or empty.");
                return null;
            }

            try
            {
                Type type = instance.GetType();
                var methods = GetCachedMethods(type); // Using the cached method approach
                MethodInfo method = methods.FirstOrDefault(m => m.Name == methodName);

                if (method == null)
                {
                    LogError(nameof(InvokeMethod), $"Method '{methodName}' not found on type '{type.FullName}'.");
                    return null;
                }

                return method.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                LogException(nameof(InvokeMethod), ex);
                return null;
            }
        }

        #endregion

        #region --- 5) Advanced Dynamic Type Creation ---

        /// <summary>
        /// Creates a dynamic type at runtime with a specified name and a set of public properties.
        /// Extends the basic version by optionally generating:
        ///   - A constructor that sets those properties
        ///   - A sample method (e.g., "SayHello") with IL code that returns a string
        /// 
        /// This is an advanced demonstration of <see cref="System.Reflection.Emit"/>.
        /// Real usage can handle different property types, complex constructors, etc.
        /// </summary>
        /// <param name="assemblyName">In-memory assembly name.</param>
        /// <param name="moduleName">In-memory module name.</param>
        /// <param name="typeName">The name of the new dynamic type.</param>
        /// <param name="properties">
        /// A dictionary of property name to <see cref="Type"/> so you can define multiple property types.
        /// </param>
        /// <param name="generateConstructor">
        /// If true, generates a constructor that takes all these properties as parameters
        /// and sets them internally.
        /// </param>
        /// <param name="generateSampleMethod">
        /// If true, generates a method "SayHello()" returning string "Hello from {typeName}".
        /// </param>
        /// <returns>The newly created <see cref="Type"/>, or <c>null</c> on error.</returns>
        public Type CreateDynamicTypeAdvanced(
            string assemblyName,
            string moduleName,
            string typeName,
            Dictionary<string, Type> properties,
            bool generateConstructor = false,
            bool generateSampleMethod = false)
        {
            if (string.IsNullOrEmpty(assemblyName)
                || string.IsNullOrEmpty(moduleName)
                || string.IsNullOrEmpty(typeName))
            {
                LogError(nameof(CreateDynamicTypeAdvanced), "Assembly, module, or type name is null/empty.");
                return null;
            }
            if (properties == null || properties.Count == 0)
            {
                LogError(nameof(CreateDynamicTypeAdvanced), "No property definitions were provided.");
                return null;
            }

            try
            {
                // Create an in-memory assembly and module
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName(assemblyName),
                    AssemblyBuilderAccess.Run
                );
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);

                // Define a public class named <typeName>
                var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

                // 1) For each entry in the properties dictionary, create a private field + public property
                var fieldsList = new List<FieldBuilder>();
                var constructorParamTypes = new List<Type>(); // for optional constructor

                foreach (var kvp in properties)
                {
                    string propName = kvp.Key;
                    Type propType = kvp.Value;

                    // Create a private field
                    FieldBuilder fieldBuilder = typeBuilder.DefineField(
                        "_" + propName,
                        propType,
                        FieldAttributes.Private
                    );

                    fieldsList.Add(fieldBuilder);
                    constructorParamTypes.Add(propType);

                    // Create a public property
                    PropertyBuilder propBuilder = typeBuilder.DefineProperty(
                        propName,
                        PropertyAttributes.HasDefault,
                        propType,
                        null
                    );

                    // Define "get" accessor
                    MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
                        "get_" + propName,
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        propType,
                        Type.EmptyTypes
                    );

                    ILGenerator ilGenGet = getMethodBuilder.GetILGenerator();
                    ilGenGet.Emit(OpCodes.Ldarg_0);
                    ilGenGet.Emit(OpCodes.Ldfld, fieldBuilder);
                    ilGenGet.Emit(OpCodes.Ret);

                    // Define "set" accessor
                    MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
                        "set_" + propName,
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        null,
                        new Type[] { propType }
                    );

                    ILGenerator ilGenSet = setMethodBuilder.GetILGenerator();
                    ilGenSet.Emit(OpCodes.Ldarg_0);
                    ilGenSet.Emit(OpCodes.Ldarg_1);
                    ilGenSet.Emit(OpCodes.Stfld, fieldBuilder);
                    ilGenSet.Emit(OpCodes.Ret);

                    // Assign getters/setters to property
                    propBuilder.SetGetMethod(getMethodBuilder);
                    propBuilder.SetSetMethod(setMethodBuilder);
                }

                // 2) Optionally generate a constructor that sets all fields
                if (generateConstructor)
                {
                    // public .ctor(type prop1, type prop2, ...)
                    var ctorBuilder = typeBuilder.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        constructorParamTypes.ToArray()
                    );

                    ILGenerator ilCtor = ctorBuilder.GetILGenerator();

                    // call base constructor
                    ilCtor.Emit(OpCodes.Ldarg_0);
                    ConstructorInfo objectCtor = typeof(object).GetConstructor(Type.EmptyTypes);
                    ilCtor.Emit(OpCodes.Call, objectCtor);

                    // For each field, store constructor parameter
                    for (int i = 0; i < fieldsList.Count; i++)
                    {
                        // arg 0 = this, arg 1..N = constructor parameters
                        ilCtor.Emit(OpCodes.Ldarg_0);
                        ilCtor.Emit(OpCodes.Ldarg, i + 1);
                        ilCtor.Emit(OpCodes.Stfld, fieldsList[i]);
                    }

                    ilCtor.Emit(OpCodes.Ret);
                }

                // 3) Optionally generate a sample method: "public string SayHello() => 'Hello from typeName';"
                if (generateSampleMethod)
                {
                    MethodBuilder sayHelloMethod = typeBuilder.DefineMethod(
                        "SayHello",
                        MethodAttributes.Public,
                        typeof(string),
                        Type.EmptyTypes
                    );

                    ILGenerator ilSayHello = sayHelloMethod.GetILGenerator();
                    // Load the string: "Hello from <typeName>"
                    ilSayHello.Emit(OpCodes.Ldstr, $"Hello from {typeName}");
                    ilSayHello.Emit(OpCodes.Ret);
                }

                // 4) Create the type
                var generatedType = typeBuilder.CreateType();
                return generatedType;
            }
            catch (Exception ex)
            {
                LogException(nameof(CreateDynamicTypeAdvanced), ex);
                return null;
            }
        }

        #endregion

        #region --- Logging Helpers ---

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
