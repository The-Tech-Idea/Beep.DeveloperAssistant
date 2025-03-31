using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace TheTechIdea.Beep.Roslyn
{
    public static class RoslynCompiler
    {
        private static readonly Dictionary<string, Tuple<Type, Assembly>> CompiledTypes = new Dictionary<string, Tuple<Type, Assembly>>();

        #region 1) Compile Single File to DLL

        public static bool CompileCodeToDLL(string sourceFile, string outputFile)
        {
            string code = File.ReadAllText(sourceFile);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return CompileToDLL(new[] { syntaxTree }, outputFile);
        }

        #endregion

        #region 2) Compile Multiple Files to DLL

        public static bool CompileCodeToDLL(IEnumerable<string> sourceFiles, string outputFile)
        {
            var syntaxTrees = sourceFiles.Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file))).ToList();
            return CompileToDLL(syntaxTrees, outputFile);
        }

        // New Overload for Additional References
        public static bool CompileCodeToDLL(IEnumerable<string> sourceFiles, string outputFile, List<string> referencePaths)
        {
            var syntaxTrees = sourceFiles.Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file))).ToList();
            var references = GetDefaultReferences();
            if (referencePaths != null)
            {
                references.AddRange(referencePaths.Select(path => MetadataReference.CreateFromFile(path)));
            }
            return CompileToDLL(syntaxTrees, outputFile, references);
        }

        #endregion

        #region 3) Compile From String Source(s) to DLL

        public static bool CompileCodeFromStringsToDLL(IEnumerable<string> sourceCodes, string outputFile)
        {
            var syntaxTrees = sourceCodes.Select(code => CSharpSyntaxTree.ParseText(code)).ToList();
            return CompileToDLL(syntaxTrees, outputFile);
        }

        #endregion

        #region 4) Compile to In-Memory Assembly and Retrieve Type

        public static Tuple<Type, Assembly> CompileClassTypeandAssembly(string classname, string code)
        {
            if (CompiledTypes.TryGetValue(classname, out var existingType))
            {
                return existingType;
            }

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            string assemblyName = Path.GetRandomFileName();
            var references = GetDefaultReferences();

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    foreach (var diag in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                    {
                        Console.Error.WriteLine(diag.ToString());
                    }
                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());
                var compiledType = assembly.GetTypes().FirstOrDefault(p => p.Name.Contains(classname));
                if (compiledType != null)
                {
                    CompiledTypes[classname] = new Tuple<Type, Assembly>(compiledType, assembly);
                }
                return new Tuple<Type, Assembly>(compiledType, assembly);
            }
        }

        #endregion

        #region 5) Compile And Get a Specific Type (File + Code)

        public static Type CompileGetClassType(string filepath, string classname, string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            string assemblyName = Path.GetFileNameWithoutExtension(filepath);
            var references = GetDefaultReferences();

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            string filename = Path.Combine(filepath, classname + ".dll");
            EmitResult result = compilation.Emit(filename);

            if (!result.Success)
            {
                foreach (var diagnostic in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                {
                    Console.Error.WriteLine(diagnostic.ToString());
                }
                return null;
            }

            Assembly assembly = Assembly.LoadFrom(filename);
            return assembly.GetTypes().FirstOrDefault(p => p.Name.Contains(classname));
        }

        #endregion

        #region 6) Compile Class From File/String to DLL

        public static bool CompileClassFromFileToDLL(string filePath, string outputFile)
        {
            string sourceCode = File.ReadAllText(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            return CompileToDLL(new[] { syntaxTree }, outputFile);
        }

        public static bool CompileClassFromStringToDLL(string sourceCode, string outputFile)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            return CompileToDLL(new[] { syntaxTree }, outputFile);
        }

        #endregion

        #region 7) In-Memory Compilation from Multiple Files

        public static bool CompileFiles(IEnumerable<string> filePaths)
        {
            var syntaxTrees = filePaths.Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file))).ToList();
            return CompileInMemory(syntaxTrees);
        }

        #endregion

        #region 8) Simple Helpers: Compile a Single File or Source In-Memory

        public static bool CompileFile(string filePath)
        {
            string sourceCode = File.ReadAllText(filePath);
            return CompileSource(sourceCode);
        }

        public static bool CompileSource(string sourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            return CompileInMemory(new[] { syntaxTree });
        }

        #endregion

        #region 9) Create Assembly from Code (In-Memory) with Logging

        public static Assembly CreateAssembly(IDMEEditor DMEEditor, string code)
        {
            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var references = GetDefaultReferences();

                var compilation = CSharpCompilation.Create(
                    "InMemoryAssembly",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);
                    if (!result.Success)
                    {
                        foreach (var diagnostic in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                        {
                            DMEEditor.AddLogMessage("Error", diagnostic.ToString(), DateTime.Now, 0, null, Errors.Failed);
                        }
                        return null;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    return Assembly.Load(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Error compiling code: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public static Assembly CreateAssembly(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var references = GetDefaultReferences();

            var compilation = CSharpCompilation.Create(
                "InMemoryAssembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine(diagnostic.ToString());
                    }
                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin);
                return Assembly.Load(ms.ToArray());
            }
        }

        #endregion

        #region 10) POSSIBLE ENHANCEMENTS

        public static void AddAdditionalReference(List<MetadataReference> references, Type type)
        {
            string assemblyPath = type.Assembly.Location;
            if (!references.Any(r => r.Display == assemblyPath))
            {
                references.Add(MetadataReference.CreateFromFile(assemblyPath));
            }
        }

        public class CompilationResult
        {
            public bool Success { get; set; }
            public Assembly Assembly { get; set; }
            public List<string> Diagnostics { get; set; } = new List<string>();
        }

        public static CompilationResult CompileCodeWithDetailedResult(IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<MetadataReference> references, CSharpCompilationOptions options)
        {
            var result = new CompilationResult();
            var compilation = CSharpCompilation.Create("DetailedAssembly", syntaxTrees, references, options);
            using (var ms = new MemoryStream())
            {
                EmitResult emitResult = compilation.Emit(ms);
                foreach (var diag in emitResult.Diagnostics)
                {
                    if (diag.Severity == DiagnosticSeverity.Error || diag.IsWarningAsError)
                    {
                        result.Diagnostics.Add(diag.ToString());
                    }
                }
                result.Success = emitResult.Success;
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    result.Assembly = Assembly.Load(ms.ToArray());
                }
            }
            return result;
        }

        private static readonly ConcurrentDictionary<string, SyntaxTree> CachedSyntaxTrees = new ConcurrentDictionary<string, SyntaxTree>();
        public static SyntaxTree GetSyntaxTreeForFile(string filePath)
        {
            return CachedSyntaxTrees.GetOrAdd(filePath, fp => CSharpSyntaxTree.ParseText(File.ReadAllText(fp)));
        }

        public static async Task<object> ExecuteScriptAsync(string code, object globals = null)
        {
            try
            {
                var scriptOptions = ScriptOptions.Default
                    .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)))
                    .WithImports("System", "System.Linq", "System.Collections.Generic");
                return await CSharpScript.EvaluateAsync(code, scriptOptions, globals);
            }
            catch (CompilationErrorException ex)
            {
                Console.WriteLine(string.Join(Environment.NewLine, ex.Diagnostics));
                throw;
            }
        }

        public static bool CompileCodeToDLLWithStrongName(IEnumerable<SyntaxTree> syntaxTrees, string outputFile, string snkFile)
        {
            var references = GetDefaultReferences();
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithPlatform(Platform.AnyCpu)
                .WithStrongNameProvider(new DesktopStrongNameProvider())
                .WithCryptoKeyFile(snkFile);

            return CompileToDLL(syntaxTrees, outputFile);
        }

        public static string PrependAssemblyAttributes(string code, string attributesCode)
        {
            return attributesCode + Environment.NewLine + code;
        }

        public static List<MetadataReference> GetDefaultReferences()
        {
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            };

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
                string systemRuntimePath = Path.Combine(runtimePath, "System.Runtime.dll");
                if (File.Exists(systemRuntimePath))
                {
                    references.Add(MetadataReference.CreateFromFile(systemRuntimePath));
                }
            }
            return references;
        }

        public static async Task<CompilationResult> CompileFilesAsync(IEnumerable<string> filePaths, string outputFile)
        {
            return await Task.Run(() =>
            {
                var syntaxTrees = filePaths.Select(file => GetSyntaxTreeForFile(file)).ToList();
                var references = GetDefaultReferences();
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    .WithPlatform(Platform.AnyCpu);
                return CompileCodeWithDetailedResult(syntaxTrees, references, options);
            });
        }

        public static CompilationResult CompileSourceWithDetailedResult(string sourceCode, string outputFile = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var references = GetDefaultReferences();
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithPlatform(Platform.AnyCpu);

            var result = CompileCodeWithDetailedResult(new[] { syntaxTree }, references, options);

            if (!string.IsNullOrEmpty(outputFile) && result.Success)
            {
                using (var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    var compilation = CSharpCompilation.Create(
                        Path.GetFileNameWithoutExtension(outputFile),
                        new[] { syntaxTree },
                        references,
                        options);
                    EmitResult emitResult = compilation.Emit(fs);
                    foreach (var diag in emitResult.Diagnostics)
                    {
                        if (diag.Severity == DiagnosticSeverity.Error || diag.IsWarningAsError)
                        {
                            result.Diagnostics.Add(diag.ToString());
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        #region Private Helpers

        private static bool CompileToDLL(IEnumerable<SyntaxTree> syntaxTrees, string outputFile)
        {
            var references = GetDefaultReferences();
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithPlatform(Platform.AnyCpu);

            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(outputFile),
                syntaxTrees,
                references,
                compilationOptions);

            using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                EmitResult result = compilation.Emit(outputStream);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine(diagnostic.ToString());
                    }
                    return false;
                }
                return true;
            }
        }

        private static bool CompileToDLL(IEnumerable<SyntaxTree> syntaxTrees, string outputFile, IEnumerable<MetadataReference> references)
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithPlatform(Platform.AnyCpu);

            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(outputFile),
                syntaxTrees,
                references,
                compilationOptions);

            using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                EmitResult result = compilation.Emit(outputStream);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine(diagnostic.ToString());
                    }
                    return false;
                }
                return true;
            }
        }

        private static bool CompileInMemory(IEnumerable<SyntaxTree> syntaxTrees)
        {
            var references = GetDefaultReferences();
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithPlatform(Platform.AnyCpu);

            var compilation = CSharpCompilation.Create(
                "InMemoryAssembly",
                syntaxTrees,
                references,
                compilationOptions);

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly.Load(ms.ToArray());
                    return true;
                }
                foreach (var diagnostic in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                {
                    Console.WriteLine(diagnostic.ToString());
                }
                return false;
            }
        }

        #endregion
    }
}