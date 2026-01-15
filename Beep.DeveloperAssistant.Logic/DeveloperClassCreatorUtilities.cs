using Beep.DeveloperAssistant.Logic.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Roslyn;

namespace Beep.DeveloperAssistant.Logic
{
    public class DeveloperClassCreatorUtilities
    {
        private readonly IDMEEditor DMEEditor;
        public string NameSpace { get; set; }
        public List<CodeTemplates> Templates { get; set; } = new List<CodeTemplates>();
        public DeveloperClassCreatorUtilities(IDMEEditor dmeEditor)
        {
            DMEEditor = dmeEditor ?? throw new ArgumentNullException(nameof(dmeEditor));
        }

        #region 1) DLL Creation
        public string CreateDLL(
    string dllname,
    List<EntityStructure> entities,
    string outputpath,
    IProgress<PassedArgs> progress,
    CancellationToken token,
    string nameSpacestring = "TheTechIdea.ProjectClasses",
    bool generateCSharpCodeFiles = true)
        {
            List<string> csFiles = new List<string>();
            int current = 1;
            int total = entities.Count;

            try
            {
                foreach (EntityStructure entity in entities)
                {
                    token.ThrowIfCancellationRequested();
                    string csPath = Path.Combine(outputpath, entity.EntityName + ".cs");
                    csFiles.Add(csPath);
                    CreateClass(entity.EntityName, entity.Fields, outputpath, nameSpacestring, generateCSharpCodeFiles);
                    progress?.Report(new PassedArgs { ParameterString1 = $"Created class {entity.EntityName}", EventType = "Update", ParameterInt1 = current++, ParameterInt2 = total });
                }

                string dllPath = Path.Combine(outputpath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{dllname}.dll");
                progress?.Report(new PassedArgs { ParameterString1 = "Creating DLL", EventType = "Update", ParameterInt1 = current, ParameterInt2 = total });

                bool compiledOK = RoslynCompiler.CompileCodeToDLL(csFiles, dllPath);
                if (!compiledOK)
                {
                    DMEEditor.AddLogMessage("CreateDLL", $"Error compiling DLL {dllname}", DateTime.Now, -1, null, Errors.Failed);
                    return "Compilation failed";
                }
                DMEEditor.AddLogMessage("CreateDLL", $"DLL {dllname} created successfully", DateTime.Now, 0, null, Errors.Ok);
                return "ok";
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateDLL", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return ex.Message;
            }
        }

        public async Task<string> CreateDLLAsync(
            string dllname,
            List<EntityStructure> entities,
            string outputpath,
            IProgress<PassedArgs> progress,
            CancellationToken token,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            List<string> csFiles = new List<string>();
            int current = 1;
            int total = entities.Count;

            try
            {
                foreach (EntityStructure entity in entities)
                {
                    token.ThrowIfCancellationRequested();
                    string csPath = Path.Combine(outputpath, entity.EntityName + ".cs");
                    csFiles.Add(csPath);
                    await CreateClassAsync(entity.EntityName, entity.Fields, outputpath, nameSpacestring, generateCSharpCodeFiles, progress, token);
                    progress?.Report(new PassedArgs { ParameterString1 = $"Created class {entity.EntityName}", EventType = "Update", ParameterInt1 = current++, ParameterInt2 = total });
                }

                string dllPath = Path.Combine(outputpath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{dllname}.dll");
                progress?.Report(new PassedArgs { ParameterString1 = "Creating DLL", EventType = "Update", ParameterInt1 = current, ParameterInt2 = total });

                bool compiledOK = await Task.Run(() => RoslynCompiler.CompileCodeToDLL(csFiles, dllPath), token);
                if (!compiledOK)
                {
                    DMEEditor.AddLogMessage("CreateDLL", $"Error compiling DLL {dllname}", DateTime.Now, -1, null, Errors.Failed);
                    return "Compilation failed";
                }
                DMEEditor.AddLogMessage("CreateDLL", $"DLL {dllname} created successfully", DateTime.Now, 0, null, Errors.Ok);
                return "ok";
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateDLL", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return ex.Message;
            }
        }

        public async Task<string> CreateDLLFromFilesPathAsync(
            string dllname,
            string filepath,
            string outputpath,
            IProgress<PassedArgs> progress,
            CancellationToken token,
            string nameSpacestring = "TheTechIdea.ProjectClasses")
        {
            List<string> csFiles = new List<string>();
            int current = 1;

            try
            {
                string[] files = Directory.GetFiles(filepath, "*.cs");
                int total = files.Length;
                foreach (string file in files)
                {
                    token.ThrowIfCancellationRequested();
                    csFiles.Add(file);
                    progress?.Report(new PassedArgs { ParameterString1 = $"Found class file {file}", EventType = "Update", ParameterInt1 = current++, ParameterInt2 = total });
                }

                string dllPath = Path.Combine(outputpath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{dllname}.dll");
                progress?.Report(new PassedArgs { ParameterString1 = "Creating DLL", EventType = "Update", ParameterInt1 = current, ParameterInt2 = total });

                bool compiledOK = await Task.Run(() => RoslynCompiler.CompileCodeToDLL(csFiles, dllPath), token);
                if (!compiledOK)
                {
                    DMEEditor.AddLogMessage("CreateDLLFromFilesPath", $"Error compiling DLL {dllname}", DateTime.Now, -1, null, Errors.Failed);
                    return "Compilation failed";
                }
                DMEEditor.AddLogMessage("CreateDLLFromFilesPath", $"DLL {dllname} created successfully", DateTime.Now, 0, null, Errors.Ok);
                return "ok";
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateDLLFromFilesPath", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return ex.Message;
            }
        }

        public string CreateDLLWithReferences(
            string dllname,
            List<EntityStructure> entities,
            string outputpath,
            List<string> referencePaths,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            List<string> csFiles = new List<string>();
            try
            {
                foreach (EntityStructure entity in entities)
                {
                    string csPath = Path.Combine(outputpath, entity.EntityName + ".cs");
                    csFiles.Add(csPath);
                    CreateClass(entity.EntityName, entity.Fields, outputpath, nameSpacestring);
                }

                string dllPath = Path.Combine(outputpath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{dllname}.dll");
                bool compiledOK = RoslynCompiler.CompileCodeToDLL(csFiles, dllPath, referencePaths);
                if (!compiledOK)
                {
                    DMEEditor.AddLogMessage("CreateDLLWithReferences", $"Error compiling DLL {dllname}", DateTime.Now, -1, null, Errors.Failed);
                    return "Compilation failed";
                }
                DMEEditor.AddLogMessage("CreateDLLWithReferences", $"DLL {dllname} created successfully", DateTime.Now, 0, null, Errors.Ok);
                return "ok";
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateDLLWithReferences", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return ex.Message;
            }
        }

        #endregion

        #region 2) Class Creation

        public string CreateClass(
            string classname,
            List<EntityField> flds,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            EntityStructure entity = new EntityStructure { EntityName = classname, Fields = flds };
            return CreatePOCOClass(classname, entity, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles);
        }

        public string CreateClass(
            string classname,
            EntityStructure entity,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            return CreatePOCOClass(classname, entity, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles);
        }

        public async Task<string> CreateClassAsync(
            string classname,
            List<EntityField> flds,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            EntityStructure entity = new EntityStructure { EntityName = classname, Fields = flds };
            return await CreatePOCOClassAsync(classname, entity, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles, progress, token);
        }

        public async Task<string> CreateClassAsync(
            string classname,
            EntityStructure entity,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            return await CreatePOCOClassAsync(classname, entity, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles, progress, token);
        }

        public string CreateClassRepeatedProperties(string pNameSpace, CodeTemplates codeTemplates, EntityStructure entityStructure)
        {
            if (entityStructure == null || codeTemplates == null)
            {
                DMEEditor.AddLogMessage("CreateClassRepeatedProperties", "Error: Entity or Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder classBuilder = new StringBuilder(codeTemplates.Using + Environment.NewLine + Environment.NewLine);
            classBuilder.AppendLine($"namespace {pNameSpace ?? "DefaultNamespace"} {{");
            string classDefinition = codeTemplates.ClassDefinition.Replace("{entity}", entityStructure.EntityName).Replace("{datasource}", entityStructure.DataSourceID);
            classBuilder.AppendLine(classDefinition + " {");

            if (codeTemplates.IsRepeatedProperty)
            {
                string functionHeader = codeTemplates.FunctionHeader.Replace("{entity}", entityStructure.EntityName).Replace("{datasource}", entityStructure.DataSourceID);
                classBuilder.AppendLine(functionHeader + " {");
            }

            foreach (var item in entityStructure.Fields)
            {
                string content = codeTemplates.Code.Replace("{field}", item.FieldName)
                                               .Replace("{field_type}", item.Fieldtype)
                                               .Replace("{entity}", entityStructure.EntityName)
                                               .Replace("{datasource}", entityStructure.DataSourceID);
                classBuilder.AppendLine(content);
            }

            classBuilder.AppendLine(codeTemplates.Footer.Replace("{entity}", entityStructure.EntityName).Replace("{datasource}", entityStructure.DataSourceID));
            if (codeTemplates.IsRepeatedProperty) classBuilder.AppendLine("}");
            classBuilder.AppendLine("}}");

            return classBuilder.ToString();
        }

        public string CreateClassSingleFunction(string pNameSpace, CodeTemplates codeTemplates, List<EntityStructure> entities, string dllname)
        {
            if (entities == null || codeTemplates == null)
            {
                DMEEditor.AddLogMessage("CreateClassSingleFunction", "Error: Entities or Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder classBuilder = new StringBuilder(codeTemplates.Using + Environment.NewLine + Environment.NewLine);
            classBuilder.AppendLine($"namespace {pNameSpace ?? codeTemplates.NameSpace} {{");
            classBuilder.AppendLine(codeTemplates.ClassDefinition + " {");
            classBuilder.AppendLine(codeTemplates.FunctionHeader);
            classBuilder.AppendLine(codeTemplates.Header);

            foreach (var entity in entities)
            {
                foreach (var item in entity.Fields)
                {
                    string content = codeTemplates.Code.Replace("{field}", item.FieldName)
                                                   .Replace("{field_type}", item.Fieldtype)
                                                   .Replace("{entity}", entity.EntityName)
                                                   .Replace("{datasource}", entity.DataSourceID);
                    classBuilder.AppendLine(content);
                }
            }

            classBuilder.AppendLine(codeTemplates.Footer);
            classBuilder.AppendLine("}}");

            string filepath = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, $"{dllname}.cs");
            File.WriteAllText(filepath, classBuilder.ToString());
            return classBuilder.ToString();
        }

        public List<string> WriteClasses(string pNameSpace, CodeTemplates codeTemplates, List<EntityStructure> entities, bool generateCSharpCodeFiles = true)
        {
            if (entities == null || codeTemplates == null)
            {
                DMEEditor.AddLogMessage("WriteClasses", "Error: Entities or Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            List<string> cls = new List<string>();
            foreach (var item in entities)
            {
                cls.Add(WriteClass(pNameSpace, codeTemplates, item, generateCSharpCodeFiles));
            }
            return cls;
        }

        public string WriteClass(string pNameSpace, CodeTemplates codeTemplates, EntityStructure entity, bool generateCSharpCodeFiles = true)
        {
            if (entity == null || codeTemplates == null)
            {
                DMEEditor.AddLogMessage("WriteClass", "Error: Entity or Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            string entityPath = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, entity.DataSourceID);
            Directory.CreateDirectory(entityPath);

            string cls = CreateClassRepeatedProperties(pNameSpace, codeTemplates, entity);
            if (generateCSharpCodeFiles && cls != null)
            {
                string filepath = Path.Combine(entityPath, $"{entity.EntityName}.cs");
                File.WriteAllText(filepath, cls);
            }
            return cls;
        }

        public void GenerateCSharpCode(string fileName)
        {
            if (!RoslynCompiler.CompileFile(fileName))
            {
                DMEEditor.AddLogMessage("GenerateCSharpCode", $"Error compiling {fileName}", DateTime.Now, -1, null, Errors.Failed);
            }
        }

        public string CreatePOCOClass(
            string classname,
            EntityStructure entity,
            string usingheader,
            string implementations,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            string defaultTemplate = "public :FIELDTYPE :FIELDNAME {get;set;}";
            return CreateClassFromTemplate(classname, entity, defaultTemplate, usingheader, implementations, extracode, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false);
        }

        public async Task<string> CreatePOCOClassAsync(
            string classname,
            EntityStructure entity,
            string usingheader,
            string implementations,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            string defaultTemplate = "public :FIELDTYPE :FIELDNAME {get;set;}";
            return await CreateClassFromTemplateAsync(classname, entity, defaultTemplate, usingheader, implementations, extracode, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false, progress, token);
        }

        public string CreateINotifyClass(
            EntityStructure entity,
            string usingheader,
            string implementations,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            string extraImplements = string.IsNullOrEmpty(implementations) ? "INotifyPropertyChanged" : $"{implementations}, INotifyPropertyChanged";
            string extraCodeINotify = @"
public event PropertyChangedEventHandler PropertyChanged;
private void NotifyPropertyChanged([CallerMemberName] string propertyName = """")
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            string template = @"
private :FIELDTYPE _:FIELDNAMEValue;
public :FIELDTYPE :FIELDNAME
{
    get => this._:FIELDNAMEValue;
    set { this._:FIELDNAMEValue = value; NotifyPropertyChanged(); }
}";
            return CreateClassFromTemplate(entity.EntityName, entity, template, usingheader, extraImplements, extraCodeINotify, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false);
        }

        public async Task<string> CreateINotifyClassAsync(
            EntityStructure entity,
            string usingheader,
            string implementations,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            string extraImplements = string.IsNullOrEmpty(implementations) ? "INotifyPropertyChanged" : $"{implementations}, INotifyPropertyChanged";
            string extraCodeINotify = @"
public event PropertyChangedEventHandler PropertyChanged;
private void NotifyPropertyChanged([CallerMemberName] string propertyName = """")
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            string template = @"
private :FIELDTYPE _:FIELDNAMEValue;
public :FIELDTYPE :FIELDNAME
{
    get => this._:FIELDNAMEValue;
    set { this._:FIELDNAMEValue = value; NotifyPropertyChanged(); }
}";
            return await CreateClassFromTemplateAsync(entity.EntityName, entity, template, usingheader, extraImplements, extraCodeINotify, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false, progress, token);
        }

        public string CreateEntityClass(
            EntityStructure entity,
            string usingheader,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true)
        {
            string implementations = "Entity";
            string template = @"
private :FIELDTYPE _:FIELDNAMEValue;
public :FIELDTYPE :FIELDNAME
{
    get => this._:FIELDNAMEValue;
    set => SetProperty(ref _:FIELDNAMEValue, value);
}";
            return CreateClassFromTemplate(entity.EntityName, entity, template, usingheader, implementations, extracode, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false);
        }

        public async Task<string> CreateEntityClassAsync(
            EntityStructure entity,
            string usingheader,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            string implementations = "Entity";
            string template = @"
private :FIELDTYPE _:FIELDNAMEValue;
public :FIELDTYPE :FIELDNAME
{
    get => this._:FIELDNAMEValue;
    set => SetProperty(ref _:FIELDNAMEValue, value);
}";
            return await CreateClassFromTemplateAsync(entity.EntityName, entity, template, usingheader, implementations, extracode, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false, progress, token);
        }

        public string CreateClassFromTemplate(
            string classname,
            EntityStructure entity,
            string template,
            string usingheader,
            string implementations,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            bool isPartial = false,
            bool isGeneric = false,
            string genericTypeParameter = "T",
            bool isSealed = false,
            bool isAbstract = false)
        {
            if (entity == null)
            {
                DMEEditor.AddLogMessage("CreateClassFromTemplate", "Error: Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            string finalClassName = !string.IsNullOrEmpty(classname) ? classname : entity.EntityName;
            if (isGeneric) finalClassName += $"<{genericTypeParameter}>";
            string outputFile = Path.Combine(outputpath ?? DMEEditor.ConfigEditor.Config.ScriptsPath, entity.EntityName + ".cs");

            try
            {
                StringBuilder code = new StringBuilder();
                if (!string.IsNullOrEmpty(usingheader)) code.AppendLine(usingheader);
                code.AppendLine($"namespace {nameSpacestring} {{");
                string classModifiers = (isPartial ? "partial " : "") + (isSealed ? "sealed " : "") + (isAbstract ? "abstract " : "");
                code.AppendLine(string.IsNullOrEmpty(implementations) ? $"    public {classModifiers}class {finalClassName}" : $"    public {classModifiers}class {finalClassName} : {implementations}");
                code.AppendLine("    {");
                code.AppendLine($"        public {finalClassName}() {{ }}");

                foreach (var ef in entity.Fields)
                {
                    string fieldCode = string.IsNullOrEmpty(template) ? $"        public {ef.Fieldtype}? {ef.FieldName}" : template.Replace(":FIELDNAME", ef.FieldName).Replace(":FIELDTYPE", ef.Fieldtype + "?");
                    code.AppendLine(fieldCode);
                }

                if (!string.IsNullOrEmpty(extracode)) code.AppendLine(extracode);
                code.AppendLine("    }");
                code.AppendLine("}");

                string generatedCode = code.ToString();
                if (generateCSharpCodeFiles)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                    File.WriteAllText(outputFile, generatedCode);
                }
                return generatedCode;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateClassFromTemplate", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<string> CreateClassFromTemplateAsync(
            string classname,
            EntityStructure entity,
            string template,
            string usingheader,
            string implementations,
            string extracode,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            bool generateCSharpCodeFiles = true,
            bool isPartial = false,
            bool isGeneric = false,
            string genericTypeParameter = "T",
            bool isSealed = false,
            bool isAbstract = false,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            if (entity == null)
            {
                DMEEditor.AddLogMessage("CreateClassFromTemplateAsync", "Error: Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            string finalClassName = !string.IsNullOrEmpty(classname) ? classname : entity.EntityName;
            if (isGeneric) finalClassName += $"<{genericTypeParameter}>";
            string outputFile = Path.Combine(outputpath ?? DMEEditor.ConfigEditor.Config.ScriptsPath, entity.EntityName + ".cs");

            try
            {
                StringBuilder code = new StringBuilder();
                if (!string.IsNullOrEmpty(usingheader)) code.AppendLine(usingheader);
                code.AppendLine($"namespace {nameSpacestring} {{");
                string classModifiers = (isPartial ? "partial " : "") + (isSealed ? "sealed " : "") + (isAbstract ? "abstract " : "");
                code.AppendLine(string.IsNullOrEmpty(implementations) ? $"    public {classModifiers}class {finalClassName}" : $"    public {classModifiers}class {finalClassName} : {implementations}");
                code.AppendLine("    {");
                code.AppendLine($"        public {finalClassName}() {{ }}");

                foreach (var ef in entity.Fields)
                {
                    string fieldCode = string.IsNullOrEmpty(template) ? $"        public {ef.Fieldtype}? {ef.FieldName}" : template.Replace(":FIELDNAME", ef.FieldName).Replace(":FIELDTYPE", ef.Fieldtype + "?");
                    code.AppendLine(fieldCode);
                }

                if (!string.IsNullOrEmpty(extracode)) code.AppendLine(extracode);
                code.AppendLine("    }");
                code.AppendLine("}");

                string generatedCode = code.ToString();
                if (generateCSharpCodeFiles)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                    await File.WriteAllTextAsync(outputFile, generatedCode, token);
                }
                progress?.Report(new PassedArgs { ParameterString1 = $"Generated class {finalClassName}", EventType = "Update" });
                return generatedCode;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateClassFromTemplateAsync", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        #endregion

        #region 3) In-Memory Assembly/Type Creation

        public Assembly CreateAssemblyFromCode(string code)
        {
            try
            {
                return RoslynCompiler.CreateAssembly(DMEEditor, code);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateAssemblyFromCode", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public Type CreateTypeFromCode(string code, string outputtypename)
        {
            try
            {
                var asm = CreateAssemblyFromCode(code);
                return asm?.GetType(outputtypename);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CreateTypeFromCode", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        #endregion

        #region 4) Advanced Class Generation

        public string ConvertPOCOClassToEntity(string filepath, string pocoClass, string className = null)
        {
            StringBuilder sb = new StringBuilder(pocoClass);
            int classStartIndex = pocoClass.IndexOf("class");
            if (classStartIndex < 0) return pocoClass;

            int nameStartIndex = classStartIndex + "class".Length;
            int nameEndIndex = pocoClass.IndexOfAny(new char[] { ':', '{', '\n', '\r' }, nameStartIndex);
            string originalClassName = pocoClass.Substring(nameStartIndex, nameEndIndex - nameStartIndex).Trim();

            className = className ?? originalClassName;
            string newClassDeclaration = $"class {className} : Entity";
            int interfaceStartIndex = pocoClass.IndexOf(':', nameEndIndex);
            if (interfaceStartIndex > 0)
            {
                int interfaceEndIndex = pocoClass.IndexOf('{', interfaceStartIndex);
                newClassDeclaration += pocoClass.Substring(interfaceStartIndex, interfaceEndIndex - interfaceStartIndex);
            }

            sb.Replace(pocoClass.Substring(classStartIndex, nameEndIndex - classStartIndex), newClassDeclaration);
            var propertyRegex = new Regex(@"public ([\w<>,\s]+) (\w+) { get\s*;\s*set\s*;\s*}");
            foreach (Match match in propertyRegex.Matches(pocoClass))
            {
                string propertyType = match.Groups[1].Value.Trim();
                string propertyName = match.Groups[2].Value;
                string newProperty = $@"
private {propertyType} _{propertyName.ToLower()};
public {propertyType} {propertyName}
{{
    get {{ return _{propertyName.ToLower()}; }}
    set {{ SetProperty(ref _{propertyName.ToLower()}, value); }}
}}";
                sb.Replace(match.Value, newProperty);
            }

            if (!string.IsNullOrEmpty(filepath))
            {
                File.WriteAllText(Path.Combine(filepath, $"{className}.cs"), sb.ToString());
            }
            return sb.ToString();
        }

        public string ConvertPOCOClassToEntity(string filepath, string pocoClass)
        {
            return ConvertPOCOClassToEntity(filepath, pocoClass, null);
        }

        public string ConvertPOCOClassToEntity(string filepath, string classname, EntityStructure entityStructure, string classnamespace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;\nusing System.ComponentModel;\nusing System.Runtime.CompilerServices;");
            sb.AppendLine($"namespace {classnamespace} {{");
            sb.AppendLine($"public class {classname} : Entity {{");
            foreach (var item in entityStructure.Fields)
            {
                sb.AppendLine($"private {item.Fieldtype} _{item.FieldName.ToLower()};");
                sb.AppendLine($"public {item.Fieldtype} {item.FieldName} {{");
                sb.AppendLine($"    get {{ return _{item.FieldName.ToLower()}; }}");
                sb.AppendLine($"    set {{ SetProperty(ref _{item.FieldName.ToLower()}, value); }}");
                sb.AppendLine("}");
            }
            sb.AppendLine("}}");

            if (!string.IsNullOrEmpty(filepath))
            {
                File.WriteAllText(Path.Combine(filepath, $"{classname}.cs"), sb.ToString());
            }
            return sb.ToString();
        }

        public void CreateEntities(string filepath, List<EntityStructure> entities, string classnamespace)
        {
            string outputPath = filepath ?? DMEEditor.ConfigEditor.Config.ScriptsPath;
            Directory.CreateDirectory(outputPath);

            foreach (var item in entities)
            {
                string cls = CreateEntityClass(item, null, null, outputPath, classnamespace, true);
                File.WriteAllText(Path.Combine(outputPath, $"{item.EntityName}.cs"), cls);
            }
        }

        public string GenerateInterfaceFromEntity(EntityStructure entityStructure, string namespaceName, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateInterfaceFromEntity", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    public interface I{entityStructure.EntityName} {{");
            foreach (var field in entityStructure.Fields)
            {
                sb.AppendLine($"        {field.Fieldtype} {field.FieldName} {{ get; set; }}");
            }
            sb.AppendLine("    }}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"I{entityStructure.EntityName}.cs"), code);
            }
            return code;
        }

        public string GeneratePartialClass(EntityStructure entityStructure, string namespaceName, string methodsToAdd = null, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GeneratePartialClass", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    public partial class {entityStructure.EntityName} {{");
            sb.AppendLine("        // Additional methods or properties can go here.");
            if (!string.IsNullOrEmpty(methodsToAdd))
            {
                sb.AppendLine(methodsToAdd);
            }
            sb.AppendLine("    }}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{entityStructure.EntityName}.Partial.cs"), code);
            }
            return code;
        }

        public string GenerateClassWithCustomAttributes(EntityStructure entityStructure, string namespaceName, Func<EntityField, IEnumerable<string>> useAttributes, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateClassWithCustomAttributes", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    public class {entityStructure.EntityName} {{");
            foreach (var field in entityStructure.Fields)
            {
                var attributes = useAttributes?.Invoke(field);
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        sb.AppendLine($"        {attr}");
                    }
                }
                sb.AppendLine($"        public {field.Fieldtype} {field.FieldName} {{ get; set; }}");
                sb.AppendLine();
            }
            sb.AppendLine("    }}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{entityStructure.EntityName}.cs"), code);
            }
            return code;
        }

        public string MergePartialClass(string classname, EntityStructure entity, string outputpath, string nameSpacestring = "TheTechIdea.ProjectClasses", string template = "public :FIELDTYPE :FIELDNAME {get;set;}")
        {
            string outputFile = Path.Combine(outputpath ?? DMEEditor.ConfigEditor.Config.ScriptsPath, classname + ".cs");
            try
            {
                string generatedCode;
                if (File.Exists(outputFile))
                {
                    string existingCode = File.ReadAllText(outputFile);
                    var syntaxTree = CSharpSyntaxTree.ParseText(existingCode);
                    var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
                    var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(c => c.Identifier.Text == classname);

                    if (classDecl != null && classDecl.Modifiers.Any(m => m.Text == "partial"))
                    {
                        StringBuilder newMembers = new StringBuilder();
                        foreach (var ef in entity.Fields)
                        {
                            string property = template.Replace(":FIELDNAME", ef.FieldName).Replace(":FIELDTYPE", ef.Fieldtype + "?");
                            newMembers.AppendLine($"        {property}");
                        }
                        var newClassDecl = classDecl.AddMembers(SyntaxFactory.ParseMemberDeclaration(newMembers.ToString()));
                        root = root.ReplaceNode(classDecl, newClassDecl);
                        generatedCode = root.ToFullString();
                    }
                    else
                    {
                        generatedCode = CreateClassFromTemplate(classname, entity, template, null, null, null, outputpath, nameSpacestring, false, false, false, null, false, false);
                    }
                }
                else
                {
                    generatedCode = CreateClassFromTemplate(classname, entity, template, null, null, null, outputpath, nameSpacestring, false, false, false, null, false, false);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                File.WriteAllText(outputFile, generatedCode);
                return generatedCode;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("MergePartialClass", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public string CreateRecordClass(EntityStructure entity, string outputpath, string nameSpacestring = "TheTechIdea.ProjectClasses", bool generateCSharpCodeFiles = true)
        {
            string template = "public :FIELDTYPE :FIELDNAME { get; init; }";
            return CreateClassFromTemplate(entity.EntityName, entity, template, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false).Replace("class", "record");
        }

        public string CreateSealedClass(EntityStructure entity, string outputpath, string nameSpacestring = "TheTechIdea.ProjectClasses", bool generateCSharpCodeFiles = true)
        {
            string template = "public :FIELDTYPE :FIELDNAME { get; set; }";
            return CreateClassFromTemplate(entity.EntityName, entity, template, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, true, false);
        }

        public string CreateAbstractClassWithStub(EntityStructure entity, string outputpath, string nameSpacestring = "TheTechIdea.ProjectClasses", bool generateCSharpCodeFiles = true)
        {
            string template = "public abstract :FIELDTYPE :FIELDNAME { get; set; }";
            string extracode = "public abstract void ProcessData();";
            return CreateClassFromTemplate(entity.EntityName, entity, template, null, null, extracode, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, true);
        }

        public async Task<List<string>> CreateClassesParallelAsync(
            List<EntityStructure> entities,
            string outputpath,
            string nameSpacestring = "TheTechIdea.ProjectClasses",
            string template = "public :FIELDTYPE :FIELDNAME {get;set;}",
            bool generateCSharpCodeFiles = true,
            IProgress<PassedArgs> progress = null,
            CancellationToken token = default)
        {
            List<string> generatedCodes = new List<string>();
            int total = entities.Count;
            int current = 0;

            await Task.WhenAll(entities.Select(async entity =>
            {
                token.ThrowIfCancellationRequested();
                string code = await CreateClassFromTemplateAsync(entity.EntityName, entity, template, null, null, null, outputpath, nameSpacestring, generateCSharpCodeFiles, false, false, null, false, false, progress, token);
                lock (generatedCodes)
                {
                    if (!string.IsNullOrEmpty(code))
                        generatedCodes.Add(code);
                }
                progress?.Report(new PassedArgs { ParameterString1 = $"Generated {entity.EntityName}", EventType = "Update", ParameterInt1 = Interlocked.Increment(ref current), ParameterInt2 = total });
            }));

            return generatedCodes;
        }

        public string GenerateUnitTest(string classname, EntityStructure entity, string outputpath, string nameSpacestring = "TheTechIdea.ProjectClasses.Tests", bool generateCSharpCodeFiles = true)
        {
            try
            {
                StringBuilder code = new StringBuilder();
                code.AppendLine("using Xunit;");
                code.AppendLine($"namespace {nameSpacestring} {{");
                code.AppendLine($"    public class {classname}Tests {{");
                code.AppendLine($"        [Fact]");
                code.AppendLine($"        public void Test_{classname}_Creation() {{");
                code.AppendLine($"            var instance = new {classname}();");
                foreach (var field in entity.Fields.Take(1))
                {
                    string testValue = field.Fieldtype == "string" ? "\"test\"" : "default";
                    code.AppendLine($"            instance.{field.FieldName} = {testValue};");
                    code.AppendLine($"            Assert.Equal({testValue}, instance.{field.FieldName});");
                }
                code.AppendLine("        }}");
                code.AppendLine("    }}");

                string generatedCode = code.ToString();
                string outputFile = Path.Combine(outputpath ?? DMEEditor.ConfigEditor.Config.ScriptsPath, $"{classname}Tests.cs");
                if (generateCSharpCodeFiles)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                    File.WriteAllText(outputFile, generatedCode);
                }
                return generatedCode;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("GenerateUnitTest", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public string GenerateWinFormForEntity(EntityStructure entityStructure, string namespaceName, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateWinFormForEntity", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Windows.Forms;");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    public partial class {entityStructure.EntityName}Form : Form {{");
            sb.AppendLine($"        public {entityStructure.EntityName}Form() {{ InitializeComponent(); }}");
            sb.AppendLine("        private void InitializeComponent() {{");
            sb.AppendLine("            this.SuspendLayout();");

            int topOffset = 20;
            for (int i = 0; i < entityStructure.Fields.Count; i++)
            {
                var field = entityStructure.Fields[i];
                string labelName = $"lbl{field.FieldName}";
                string textBoxName = $"txt{field.FieldName}";
                sb.AppendLine($"            var {labelName} = new Label {{ AutoSize = true, Location = new System.Drawing.Point(20, {topOffset}), Name = \"{labelName}\", Text = \"{field.FieldName}\" }};");
                sb.AppendLine($"            var {textBoxName} = new TextBox {{ Location = new System.Drawing.Point(120, {topOffset - 4}), Name = \"{textBoxName}\", Size = new System.Drawing.Size(200, 20) }};");
                sb.AppendLine($"            this.Controls.Add({labelName});");
                sb.AppendLine($"            this.Controls.Add({textBoxName});");
                topOffset += 30;
            }

            sb.AppendLine("            this.ClientSize = new System.Drawing.Size(400, 400);");
            sb.AppendLine($"            this.Name = \"{entityStructure.EntityName}Form\";");
            sb.AppendLine($"            this.Text = \"{entityStructure.EntityName} Form\";");
            sb.AppendLine("            this.ResumeLayout(false);");
            sb.AppendLine("            this.PerformLayout();");
            sb.AppendLine("        }}");
            sb.AppendLine("    }}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{entityStructure.EntityName}Form.cs"), code);
            }
            return code;
        }

        public string GenerateMvcController(EntityStructure entityStructure, string namespaceName, string modelNamespace, string controllerSuffix = "Controller", bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateMvcController", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            string entityName = entityStructure.EntityName;
            string controllerName = entityName + controllerSuffix;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine($"using {modelNamespace};");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    public class {controllerName} : Controller {{");
            sb.AppendLine($"        public IActionResult Index() {{ /* var items = _context.{entityName}.ToList(); */ return View(); }}");
            sb.AppendLine($"        public IActionResult Details(int id) {{ /* var item = _context.{entityName}.Find(id); */ return View(); }}");
            sb.AppendLine($"        public IActionResult Create() {{ return View(); }}");
            sb.AppendLine($"        [HttpPost] public IActionResult Create(/*{entityName} model*/) {{ if (ModelState.IsValid) {{ /* _context.Add(model); _context.SaveChanges(); */ return RedirectToAction(nameof(Index)); }} return View(); }}");
            sb.AppendLine($"        public IActionResult Edit(int id) {{ /* var item = _context.{entityName}.Find(id); */ return View(); }}");
            sb.AppendLine($"        [HttpPost] public IActionResult Edit(/*int id, {entityName} model*/) {{ if (ModelState.IsValid) {{ /* _context.Update(model); _context.SaveChanges(); */ return RedirectToAction(nameof(Index)); }} return View(); }}");
            sb.AppendLine($"        public IActionResult Delete(int id) {{ /* var item = _context.{entityName}.Find(id); */ return View(); }}");
            sb.AppendLine($"        [HttpPost, ActionName(\"Delete\")] public IActionResult DeleteConfirmed(int id) {{ /* var item = _context.{entityName}.Find(id); _context.Remove(item); _context.SaveChanges(); */ return RedirectToAction(nameof(Index)); }}");
            sb.AppendLine("    }}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{controllerName}.cs"), code);
            }
            return code;
        }

        public string GenerateRazorPageMarkup(EntityStructure entityStructure, string pageName, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateRazorPageMarkup", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@page");
            sb.AppendLine($"@model {pageName}Model");
            sb.AppendLine($"<h2>{entityStructure.EntityName} Page</h2>");
            sb.AppendLine("<form method=\"post\">");
            foreach (var field in entityStructure.Fields)
            {
                sb.AppendLine("    <div class=\"form-group\">");
                sb.AppendLine($"        <label asp-for=\"Entity.{field.FieldName}\"></label>");
                sb.AppendLine($"        <input asp-for=\"Entity.{field.FieldName}\" class=\"form-control\" />");
                sb.AppendLine("    </div>");
            }
            sb.AppendLine("    <button type=\"submit\" class=\"btn btn-primary\">Save</button>");
            sb.AppendLine("</form>");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{pageName}.cshtml"), code);
            }
            return code;
        }

        public string GenerateRazorPageModel(EntityStructure entityStructure, string namespaceName, string pageName, string modelNamespace, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateRazorPageModel", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc.RazorPages;");
            sb.AppendLine($"using {modelNamespace};");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"    public class {pageName}Model : PageModel {{");
            sb.AppendLine($"        [BindProperty]");
            sb.AppendLine($"        public {entityStructure.EntityName} Entity {{ get; set; }}");
            sb.AppendLine($"        public void OnGet(int? id) {{ Entity = new {entityStructure.EntityName}(); }}");
            sb.AppendLine("        public IActionResult OnPost() {{ if (!ModelState.IsValid) return Page(); /* Save Entity */ return RedirectToPage(\"Index\"); }}");
            sb.AppendLine("    }}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{pageName}.cshtml.cs"), code);
            }
            return code;
        }

        public string GenerateBlazorEditComponent(EntityStructure entityStructure, string namespaceName, string componentName, bool writeToFile = false, string outputDirectory = null)
        {
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("GenerateBlazorEditComponent", "Error: entityStructure is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"@page \"/{componentName.ToLower()}\"");
            sb.AppendLine("@using System.ComponentModel.DataAnnotations;");
            sb.AppendLine($"@namespace {namespaceName}");
            sb.AppendLine("@inherits ComponentBase");
            sb.AppendLine($"<h3>Edit {entityStructure.EntityName}</h3>");
            sb.AppendLine("<EditForm Model=\"@Entity\" OnValidSubmit=\"HandleValidSubmit\">");
            sb.AppendLine("    <DataAnnotationsValidator />");
            sb.AppendLine("    <ValidationSummary />");
            foreach (var field in entityStructure.Fields)
            {
                sb.AppendLine("    <div class=\"mb-3\">");
                sb.AppendLine($"        <label>{field.FieldName}</label><br />");
                sb.AppendLine($"        <InputText @bind-Value=\"Entity.{field.FieldName}\" />");
                sb.AppendLine("    </div>");
            }
            sb.AppendLine("    <button type=\"submit\" class=\"btn btn-primary\">Save</button>");
            sb.AppendLine("</EditForm>");
            sb.AppendLine("@code {{");
            sb.AppendLine($"    private {entityStructure.EntityName} Entity = new {entityStructure.EntityName}();");
            sb.AppendLine("    private void HandleValidSubmit() {{ /* Save logic */ }}");
            sb.AppendLine("}}");

            string code = sb.ToString();
            if (writeToFile && !string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(Path.Combine(outputDirectory, $"{componentName}.razor"), code);
            }
            return code;
        }

        public string GenerateBasicSolutionStructure(string solutionName, string rootPath)
        {
            try
            {
                string solutionDir = Path.Combine(rootPath, solutionName);
                Directory.CreateDirectory(solutionDir);
                string srcDir = Path.Combine(solutionDir, "src");
                Directory.CreateDirectory(srcDir);
                string testsDir = Path.Combine(solutionDir, "tests");
                Directory.CreateDirectory(testsDir);

                string slnPath = Path.Combine(solutionDir, $"{solutionName}.sln");
                File.WriteAllText(slnPath, $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 17
VisualStudioVersion = 17.0.0.0
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{solutionName}"", ""src\{solutionName}.csproj"", ""{{GUID-GOES-HERE}}"" 
EndProject
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
EndGlobal
");

                string csprojPath = Path.Combine(srcDir, $"{solutionName}.csproj");
                File.WriteAllText(csprojPath, $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
</Project>
");

                DMEEditor.AddLogMessage("GenerateBasicSolutionStructure", $"Solution structure created at {slnPath}", DateTime.Now, 0, null, Errors.Ok);
                return slnPath;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("GenerateBasicSolutionStructure", $"Error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        #endregion

        #region 5) Validation and Utilities

        public async Task<bool> CompileCodeAsync(
            string filespath,
            List<string> sourceFiles,
            string exeFile,
            string referencedassemblies,
            bool fromStrings,
            CancellationToken token)
        {
            string filepath = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, exeFile);
            try
            {
                bool result = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return fromStrings
                        ? RoslynCompiler.CompileCodeFromStringsToDLL(sourceFiles, filepath)
                        : RoslynCompiler.CompileCodeToDLL(Directory.GetFiles(filespath, "*.cs"), filepath);
                }, token);
                DMEEditor.AddLogMessage("CompileCode", $"Compiled {exeFile} successfully", DateTime.Now, 0, null, Errors.Ok);
                return result;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CompileCode", $"Error compiling {exeFile}: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public Assembly CompileAndLoadAssembly(string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                DMEEditor.AddLogMessage("CompileAndLoadAssembly", $"DLL not found at {dllPath}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            try
            {
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                DMEEditor.AddLogMessage("CompileAndLoadAssembly", $"Assembly loaded from {dllPath}", DateTime.Now, 0, null, Errors.Ok);
                return assembly;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("CompileAndLoadAssembly", $"Error loading assembly: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public (bool IsValid, List<string> Errors) ValidateGeneratedCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = syntaxTree.GetDiagnostics();
            List<string> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.ToString()).ToList();
            bool isValid = !errors.Any();
            if (!isValid)
            {
                DMEEditor.AddLogMessage("ValidateGeneratedCode", $"Validation failed with {errors.Count} errors", DateTime.Now, 0, null, Errors.Failed);
            }
            return (isValid, errors);
        }

        public string MergePropertiesIntoExistingClass(string existingFilePath, EntityStructure updatedEntity)
        {
            if (!File.Exists(existingFilePath))
            {
                DMEEditor.AddLogMessage("MergePropertiesIntoExistingClass", $"File not found: {existingFilePath}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            string originalCode = File.ReadAllText(existingFilePath);
            StringBuilder sb = new StringBuilder(originalCode);
            foreach (var field in updatedEntity.Fields)
            {
                if (!originalCode.Contains($"public {field.Fieldtype} {field.FieldName}"))
                {
                    int classEndIndex = sb.ToString().LastIndexOf('}');
                    if (classEndIndex > 0)
                    {
                        sb.Insert(classEndIndex, $"    public {field.Fieldtype} {field.FieldName} {{ get; set; }}\n");
                    }
                }
            }

            string mergedCode = sb.ToString();
            File.WriteAllText(existingFilePath, mergedCode);
            DMEEditor.AddLogMessage("MergePropertiesIntoExistingClass", $"Merged properties into {existingFilePath}", DateTime.Now, 0, null, Errors.Ok);
            return mergedCode;
        }

        #endregion
        #region Tempalate
        public async Task SaveTemplatesAsync()
        {
            string filename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, "DeveloperAssistantScripts.json");
            try
            {
                await Task.Run(() => DMEEditor.ConfigEditor.JsonLoader.Serialize(filename, Templates));
                DMEEditor.AddLogMessage("SaveTemplates", $"Templates saved to {filename}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("SaveTemplates", $"Error saving templates: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }

        public async Task LoadTemplatesAsync()
        {
            string filename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, "DeveloperAssistantScripts.json");
            try
            {
                Templates = await Task.Run(() => DMEEditor.ConfigEditor.JsonLoader.DeserializeObject<CodeTemplates>(filename)) ?? new List<CodeTemplates>();
                DMEEditor.AddLogMessage("LoadTemplates", $"Templates loaded from {filename}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("LoadTemplates", $"Error loading templates: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }
        public void SaveTemplates()
        {
            string filename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, "DeveloperAssistantScripts.json");
            try
            {
                DMEEditor.ConfigEditor.JsonLoader.Serialize(filename, Templates);
                DMEEditor.AddLogMessage("SaveTemplates", $"Templates saved to {filename}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("SaveTemplates", $"Error saving templates: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }

        public void LoadTemplates()
        {
            string filename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, "DeveloperAssistantScripts.json");
            try
            {
                Templates =  DMEEditor.ConfigEditor.JsonLoader.DeserializeObject<CodeTemplates>(filename) ?? new List<CodeTemplates>();
                DMEEditor.AddLogMessage("LoadTemplates", $"Templates loaded from {filename}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("LoadTemplates", $"Error loading templates: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }
        #endregion Tempalate
    }
}