using Beep.DeveloperAssistant.Logic.Models;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Roslyn;
using TheTechIdea.Util;

namespace Beep.DeveloperAssistant.Logic
{
    public class DeveloperAssistantManager
    {
       
        private IDMEEditor DMEEditor;
        public DeveloperAssistantManager(IDMEEditor pDMEEditor)
        {
            DMEEditor = pDMEEditor;
        }
        public string NameSpace { get; set; }
        public List<CodeTemplates> Templates { get; set; }  =new List<CodeTemplates>();
        public void SaveTemplates()
        {
            string filename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, "DeveloperAssistantScripts.json");
            DMEEditor.ConfigEditor.JsonLoader.Serialize(filename, Templates);
        }
        public void LoadTemplates()
        {
            string filename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, "DeveloperAssistantScripts.json");
            Templates = DMEEditor.ConfigEditor.JsonLoader.DeserializeObject<CodeTemplates>(filename);
        }
        public string CreateClassRepeatedProperties(string pNameSpace,CodeTemplates codeTemplates, EntityStructure entityStructure)
        {
            if (!string.IsNullOrEmpty(pNameSpace))
            {
                NameSpace = pNameSpace;
            }
            if (entityStructure == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
            if (codeTemplates == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
            string classbuilder = codeTemplates.Using + Environment.NewLine;
            classbuilder = classbuilder + Environment.NewLine;


            if (string.IsNullOrEmpty(NameSpace))
            {
                string namesapce = codeTemplates.NameSpace.Replace("{datasource}", entityStructure.DataSourceID);
                classbuilder = classbuilder+namesapce + Environment.NewLine;
            }
            else
                classbuilder = classbuilder + $" namespace {NameSpace} " + Environment.NewLine;
            classbuilder = classbuilder + "{" + Environment.NewLine;
            string ClassDefinition = codeTemplates.ClassDefinition.Replace("{entity}", entityStructure.EntityName);
            ClassDefinition = ClassDefinition.Replace("{datasource}", entityStructure.DataSourceID) ;
            classbuilder = classbuilder + ClassDefinition + Environment.NewLine;
            if (codeTemplates.IsRepeatedProperty)
            {
                string functionheader = codeTemplates.FunctionHeader.Replace("{entity}", entityStructure.EntityName);
                functionheader = functionheader.Replace("{datasource}", entityStructure.DataSourceID);
                classbuilder = classbuilder + functionheader + Environment.NewLine;
                classbuilder = classbuilder + "{" + Environment.NewLine;
            }
            string header = codeTemplates.FunctionHeader.Replace("{entity}", entityStructure.EntityName);
            header= header.Replace("{datasource}", entityStructure.DataSourceID);
            classbuilder = classbuilder + header + Environment.NewLine;
            foreach (var item in entityStructure.Fields)
            {
                string content = codeTemplates.Code.Replace("{field}", item.fieldname);
                content = content.Replace("{field_type}", item.fieldtype);
                content = content.Replace("{entity}", entityStructure.EntityName);
                content = content.Replace("{datasource}", entityStructure.DataSourceID);
                classbuilder = classbuilder + content + Environment.NewLine;
            }
            //if (codeTemplates.IsRepeatedProperty)
            //{
               
            //    classbuilder = classbuilder + "}" + Environment.NewLine;
            //}
            classbuilder = classbuilder + Environment.NewLine;
            string footer= codeTemplates.Footer + Environment.NewLine;
            footer= footer.Replace("{entity}", entityStructure.EntityName);
            footer = footer.Replace("{datasource}", entityStructure.DataSourceID);
            classbuilder = classbuilder + footer + Environment.NewLine;
            classbuilder = classbuilder + "  }" + Environment.NewLine;
            classbuilder = classbuilder + "}" + Environment.NewLine;
            return classbuilder;
        }
        public string CreateClassSingleFunction(string pNameSpace, CodeTemplates codeTemplates, List<EntityStructure> entities,string dllname)
        {
            
            if (!string.IsNullOrEmpty(pNameSpace))
            {
                NameSpace = pNameSpace;
            }
            if (entities == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
            if (codeTemplates == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
            string classbuilder = codeTemplates.Using + Environment.NewLine;
            classbuilder = classbuilder + Environment.NewLine;

            string namesapce = codeTemplates.NameSpace;
            if(string.IsNullOrEmpty(NameSpace))
            {
                classbuilder = classbuilder + $" namespace {namesapce} " + Environment.NewLine;
            }else
                classbuilder = classbuilder + $" namespace {pNameSpace} " + Environment.NewLine;

            classbuilder = classbuilder + "{" + Environment.NewLine;
            string ClassDefinition = codeTemplates.ClassDefinition + "{";
            classbuilder = classbuilder + ClassDefinition + Environment.NewLine;
          
                string functionheader = codeTemplates.FunctionHeader;
                classbuilder = classbuilder + functionheader + Environment.NewLine;
           
            string header = codeTemplates.Header;
            classbuilder = classbuilder + header + Environment.NewLine;
            string content = string.Empty;
            foreach (EntityStructure entity in entities)
            {
                foreach (var item in entity.Fields)
                {
                    content = codeTemplates.Code.Replace("{field}", item.fieldname);
                    content = content.Replace("{field_type}", item.fieldtype);
                }
                content = content.Replace("{entity}", entity.EntityName);
                content = content.Replace("{datasource}", entity.DatasourceEntityName);
                classbuilder = classbuilder + content + Environment.NewLine;
            }
            classbuilder= classbuilder+  Environment.NewLine;
           
            classbuilder = classbuilder + Environment.NewLine;
            string footer = codeTemplates.Footer + Environment.NewLine;
          
            classbuilder = classbuilder + footer + Environment.NewLine;
            classbuilder = classbuilder + "  }" + Environment.NewLine;
            classbuilder = classbuilder + "}" + Environment.NewLine;
            classbuilder = classbuilder + "}" + Environment.NewLine;

            string filepath = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, $"{dllname}.cs");
            StreamWriter streamWriter = new StreamWriter(filepath);
            streamWriter.WriteLine(classbuilder);
            streamWriter.Close();

            return classbuilder;
        }
        public List<string> WriteClasses(string pNameSpace,CodeTemplates codeTemplates, List<EntityStructure> entities,  bool GenerateCSharpCodeFiles = true)
        {
            if (!string.IsNullOrEmpty(pNameSpace))
            {
                NameSpace = pNameSpace;
            }
            List<string> cls=new List<string>();
            if (entities == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
            if (codeTemplates == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
           
            foreach (var item in entities)
            {
                if (!Directory.Exists(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, item.DataSourceID)))
                {
                    Directory.CreateDirectory(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, item.DataSourceID));
                };
                cls.Add(WriteClass(pNameSpace,codeTemplates, item,GenerateCSharpCodeFiles));
            }
            return cls;
        }
        public string WriteClass(string pNameSpace, CodeTemplates codeTemplates, EntityStructure entity, bool GenerateCSharpCodeFiles = true)
        {
            if (entity == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
            if (codeTemplates == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Template is null", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

           
            if (!Directory.Exists(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, entity.DataSourceID)))
            {
                 Directory.CreateDirectory(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, entity.DataSourceID));
            };
            string cls = CreateClassRepeatedProperties(pNameSpace, codeTemplates, entity);
            if(GenerateCSharpCodeFiles)
            {
                string filepath = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, entity.DataSourceID, $"{entity.EntityName}.cs");
                StreamWriter streamWriter = new StreamWriter(filepath);
                streamWriter.WriteLine(cls);
                streamWriter.Close();
            }

            return cls;
        }
        public void WriteClassesToDLL(string pNameSpace,string dllname,CodeTemplates codeTemplates, List<EntityStructure> entities, IProgress<PassedArgs> progress, CancellationToken token, string NameSpacestring = "Beep.ProjectClasses", bool GenerateCSharpCodeFiles = false)
        {
            if (!string.IsNullOrEmpty(pNameSpace))
            {
                NameSpace = pNameSpace;
            }
            PassedArgs args = new PassedArgs();
            if (entities == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Entity is null", DateTime.Now, 0, null, Errors.Failed);
                return;
            }
            if (codeTemplates == null)
            {
                DMEEditor.AddLogMessage("Temelate Class Designer", "Error Template is null", DateTime.Now, 0, null, Errors.Failed);
                return;
            }


            args.Messege = $"Creating DLL {dllname}";
            progress.Report(args);
            List<string> classes = new List<string>();
            List<string> TestClass = new List<string>();
            if (codeTemplates.IsRepeatedProperty)
            {
                classes= WriteClasses(NameSpace,codeTemplates, entities);
            }
            else
            {
                classes.Add(CreateClassSingleFunction(NameSpace,codeTemplates, entities,dllname));
            }
           
            TestClass.Add(classes.FirstOrDefault());
            if (!Directory.Exists(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, dllname)))
            {
                Directory.CreateDirectory(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, dllname));
            };
            string filespath= Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, dllname);
            string dllfilename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, dllname,$"{dllname}.dll");
            string testfilename = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, dllname, $"test.dll");
            //----------------- Test One to see template ----
           bool retval = CompileCode(filespath, TestClass, testfilename, codeTemplates.ReferencedAssemblies,false);
            if(!retval)
            {
                args.Messege = $"Error : Creating DLL {dllname}";
                progress.Report(args);
            }
          
            //-----------------------------------------------
           
            args.Messege = $"Finished Creating DLL {dllname}";
            progress.Report(args);
        }
        public bool CompileCode(string filespath,List<string> sourceFiles,String exeFile,string referencedassemblies,bool FromStrings)
        {


            string filepath = Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, exeFile);
            bool retval=true;
            if (FromStrings)
            {
                retval = RoslynCompiler.CompileCodeFromStringsToDLL(sourceFiles, filepath);
            }else
            {
              
                string[] files = Directory.GetFiles(filespath, "*.cs");
                retval = RoslynCompiler.CompileCodeToDLL( files, filepath);
            }
               


            return retval;
        }
        public  string ConvertPOCOClassToEntity(string filepath,string pocoClass, string className)
        {
            var stringBuilder = new StringBuilder(pocoClass);

            // Find the opening of the class declaration
            int classStartIndex = pocoClass.IndexOf("class");
            if (classStartIndex < 0) return pocoClass; // Return original if no class keyword is found

            // Extract the original class name
            int nameStartIndex = classStartIndex + "class".Length;
            int nameEndIndex = pocoClass.IndexOfAny(new char[] { ':', '{', '\n', '\r' }, nameStartIndex);
            string originalClassName = pocoClass.Substring(nameStartIndex, nameEndIndex - nameStartIndex).Trim();

            // If className is null or empty, use the original class name
            if (string.IsNullOrEmpty(className))
            {
                className = originalClassName;
            }

            // Prepare the new class declaration with "Entity" as the base class
            string newClassDeclaration = $"class {className} : Entity";

            // Check if there are existing interfaces, and append them after "Entity"
            int interfaceStartIndex = pocoClass.IndexOf(':', nameEndIndex);
            if (interfaceStartIndex > 0)
            {
                int interfaceEndIndex = pocoClass.IndexOf('{', interfaceStartIndex);
                string interfaces = pocoClass.Substring(interfaceStartIndex, interfaceEndIndex - interfaceStartIndex);
                newClassDeclaration += interfaces;
            }

            // Replace the original class declaration with the new one
            stringBuilder.Replace(pocoClass.Substring(classStartIndex, nameEndIndex - classStartIndex), newClassDeclaration);

            // Use regex to find all property declarations, allowing optional spaces around semicolons
            // Use regex to find all property declarations, allowing optional spaces around semicolons
            var propertyRegex = new Regex(@"public ([\w<>,\s]+) (\w+) { get\s*;\s*set\s*;\s*}");
            foreach (Match match in propertyRegex.Matches(pocoClass))
            {
                string propertyType = match.Groups[1].Value.Trim();
                string propertyName = match.Groups[2].Value;

                // Replace the auto-property with a full property that uses the SetProperty method
                string newProperty = $@"
private {propertyType} _{propertyName.ToLower()};
public {propertyType} {propertyName}
{{
    get {{ return _{propertyName.ToLower()}; }}
    set {{ SetProperty(ref _{propertyName.ToLower()}, value); }}
}}";

                stringBuilder.Replace(match.Value, newProperty);
            }


            if (!string.IsNullOrEmpty(filepath))
            {
                string savepath = string.IsNullOrEmpty(filepath) ? DMEEditor.ConfigEditor.Config.ScriptsPath : filepath;
                string file = Path.Combine(savepath, $"{className}.cs");
                StreamWriter streamWriter = new StreamWriter(file);
                streamWriter.WriteLine(stringBuilder.ToString());
                streamWriter.Close();
            }

            return stringBuilder.ToString();
        }
        public string ConvertPOCOClassToEntity(string filepath, string pocoClass)
        {
            var stringBuilder = new StringBuilder(pocoClass);


            // Find the opening of the class declaration
            int classStartIndex = pocoClass.IndexOf("class");
            if (classStartIndex < 0) return pocoClass; // Return original if no class keyword is found

            // Extract the original class name
            int nameStartIndex = classStartIndex + "class".Length;
            int nameEndIndex = pocoClass.IndexOfAny(new char[] { ':', '{', '\n', '\r' }, nameStartIndex);
            string originalClassName = pocoClass.Substring(nameStartIndex, nameEndIndex - nameStartIndex).Trim();

            // If className is null or empty, use the original class name
           
            
             string   className = originalClassName;
           

            // Prepare the new class declaration with "Entity" as the base class
            string newClassDeclaration = $"class {className} : Entity";

            // Check if there are existing interfaces, and append them after "Entity"
            int interfaceStartIndex = pocoClass.IndexOf(':', nameEndIndex);
            if (interfaceStartIndex > 0)
            {
                int interfaceEndIndex = pocoClass.IndexOf('{', interfaceStartIndex);
                string interfaces = pocoClass.Substring(interfaceStartIndex, interfaceEndIndex - interfaceStartIndex);
                newClassDeclaration += interfaces;
            }

            // Replace the original class declaration with the new one
            stringBuilder.Replace(pocoClass.Substring(classStartIndex, nameEndIndex - classStartIndex), newClassDeclaration);

            // Use regex to find all property declarations, allowing optional spaces around semicolons
            // Use regex to find all property declarations, allowing optional spaces around semicolons
            var propertyRegex = new Regex(@"public ([\w<>,\s]+) (\w+) { get\s*;\s*set\s*;\s*}");
            foreach (Match match in propertyRegex.Matches(pocoClass))
            {
                string propertyType = match.Groups[1].Value.Trim();
                string propertyName = match.Groups[2].Value;

                // Replace the auto-property with a full property that uses the SetProperty method
                string newProperty = $@"
private {propertyType} _{propertyName.ToLower()};
public {propertyType} {propertyName}
{{
    get {{ return _{propertyName.ToLower()}; }}
    set {{ SetProperty(ref _{propertyName.ToLower()}, value); }}
}}";

                stringBuilder.Replace(match.Value, newProperty);
            }
            if (!string.IsNullOrEmpty(filepath))
            {
                string savepath = string.IsNullOrEmpty(filepath) ? DMEEditor.ConfigEditor.Config.ScriptsPath : filepath;
                string file = Path.Combine(savepath, $"{className}.cs");
                StreamWriter streamWriter = new StreamWriter(file);
                streamWriter.WriteLine(stringBuilder.ToString());
                streamWriter.Close();
            }
           

            return stringBuilder.ToString();
        }
        public string ConvertPOCOClassToEntity(string filepath,string classname,  EntityStructure entityStructure,string classnamespace)
        {
            return  DMTypeBuilder.ConvertPOCOClassToEntity(DMEEditor, entityStructure, classnamespace);
        }
        public void CreateEntities( string filepath, List<EntityStructure> entities, string classnamespace)
        {
            if (!Directory.Exists(Path.Combine(filepath)))
            {
                Directory.CreateDirectory(Path.Combine(filepath));
            };
            foreach (var item in entities)
            {
                
               
                string cls =DMTypeBuilder.ConvertPOCOClassToEntity(DMEEditor, item, classnamespace);
                string file = Path.Combine(filepath,  $"{item.EntityName}.cs");
                    StreamWriter streamWriter = new StreamWriter(file);
                    streamWriter.WriteLine(cls);
                    streamWriter.Close();
              
            }
            
        }
    }
}
