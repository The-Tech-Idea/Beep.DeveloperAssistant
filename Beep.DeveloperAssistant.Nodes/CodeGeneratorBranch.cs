using Beep.Vis.Module;

using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Util;

namespace Beep.IDE.Nodes
{
    [AddinAttribute(Caption = "Code Generator",Name = "CodeGeneratorBranch", misc = "Beep", FileType = "Beep", iconimage = "codegen.png", menu = "DEV", ObjectType = "Beep.DEV.GEN")]
    
    public class CodeGeneratorBranch : IBranch
    {
        public CodeGeneratorBranch()
        {
            
        }
        public bool Visible { get; set; } = true;
        public bool IsDataSourceNode { get; set; } = false;
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public int Order { get; set; } = 0;
        public object TreeStrucure { get; set; }
        public IVisManager Visutil { get; set; }
        public int ID { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; } = new List<string>();
        public EntityStructure EntityStructure { get; set; }
        public int MiscID { get; set; }
        public string Name { get; set; }
        public string BranchText { get; set; } = "Code Generator";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "codegen.png";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "DEV";
         public IBranch ParentBranch { get  ; set  ; }
        public string ObjectType { get; set; } = "Beep.DEV.GEN";
        public IBranch CreateCategoryNode(CategoryFolder p)
        {
            return null;
        }

        public IErrorsInfo CreateChildNodes()
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo ExecuteBranchAction(string ActionName)
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo MenuItemClicked(string ActionNam)
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo RemoveChildNodes()
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo SetConfig(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            try
            {
                TreeEditor = pTreeEditor;
                DMEEditor = pDMEEditor;
              
             

                //   DMEEditor.AddLogMessage("Success", "Set Config OK", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                string mes = "Could not Set Config";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(Caption = "Open",Name= "OpenDeveloperTemplate", Hidden =false, iconimage = "codegen.png")]
        public IErrorsInfo OpenDeveloperTemplate()
        {

            try
            {
                Console.WriteLine("Trying Open");
                Visutil.ShowPage("uc_DeveloperAssistantTemplateDesigner", (PassedArgs)DMEEditor.Passedarguments, DisplayType.InControl);

                //   DMEEditor.AddLogMessage("Success", "Added Database Connection", DateTime.Now, 0,null, Errors.Failed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                //string mes = "Could not Add Database Connection";
                DMEEditor.AddLogMessage("Beep IDE", ex.Message, DateTime.Now, -1, null, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
    }
}
