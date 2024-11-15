using TheTechIdea.Beep.Vis;

using Beep.DeveloperAssistant.Logic;
using TheTechIdea.Beep;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Logger;

namespace Beep.DeveloperAssistant.WinformCore
{
    [AddinAttribute(Caption = "Code Converter", Name = "uc_CodeConverter", misc = "AI", addinType = AddinType.Control)]
    public partial class uc_CodeConverter : UserControl, IDM_Addin
    {
        public uc_CodeConverter()
        {
            InitializeComponent();
        }
        public string ParentName { get; set; }
        public string AddinName { get; set; } = "Template Editor";
        public string Description { get; set; } = "Template Editor";
        public string ObjectName { get; set; }
        public string ObjectType { get; set; } = "UserControl";
        public Boolean DefaultCreate { get; set; } = true;
        public string DllPath { get; set; }
        public string DllName { get; set; }
        public string NameSpace { get; set; }
        public IErrorsInfo ErrorObject { get; set; }
        public IDMLogger Logger { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public string EntityName { get; set; }
        public IPassedArgs Passedarg { get; set; }
        public ITree TreeEditor { get; private set; }
        public IBranch ParentBranch { get; private set; }

        IVisManager visManager;

        IBranch RootAppBranch;
        IBranch branch;
        DeveloperAssistantManager manager;
        IDataSource ds;
        public void Run(IPassedArgs pPassedarg)
        {

        }

        public void SetConfig(IDMEEditor pbl, IDMLogger plogger, IUtil putil, string[] args, IPassedArgs e, IErrorsInfo per)
        {
            Passedarg = e;
            visManager = (IVisManager)e.Objects.Where(c => c.Name == "VISUTIL").FirstOrDefault().obj;
            Logger = plogger;
            DMEEditor = pbl;
            ErrorObject = per;
            TreeEditor = (ITree)visManager.Tree;
            if (e.Objects.Where(c => c.Name == "Branch").Any())
            {
                branch = (IBranch)e.Objects.Where(c => c.Name == "Branch").FirstOrDefault().obj;
            }
            if (e.Objects.Where(i => i.Name == "RootBranch").Any())
            {
                RootAppBranch = (IBranch)e.Objects.Where(c => c.Name == "RootBranch").FirstOrDefault().obj;
            }

            if (e.Objects.Where(i => i.Name == "ParentBranch").Any())
            {
                ParentBranch = (IBranch)e.Objects.Where(c => c.Name == "ParentBranch").FirstOrDefault().obj;
            }
            manager = new DeveloperAssistantManager(DMEEditor);
            manager.LoadTemplates();
            this.ToEntitybutton.Click += ToEntitybutton_Click;

        }

        private void ToEntitybutton_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(SourcetextBox.Text))
            {
                return;
            }
            TargettextBox.Text= manager.ConvertPOCOClassToEntity(null,SourcetextBox.Text);
        }
    }
}
