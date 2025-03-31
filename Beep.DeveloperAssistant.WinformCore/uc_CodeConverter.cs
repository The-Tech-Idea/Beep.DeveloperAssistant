using TheTechIdea.Beep.Vis;

using Beep.DeveloperAssistant.Logic;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Container.Services;

namespace Beep.DeveloperAssistant.WinformCore
{
    [AddinAttribute(Caption = "Code Converter", Name = "uc_CodeConverter", misc = "AI", addinType = AddinType.Control)]
    public partial class uc_CodeConverter : UserControl, IDM_Addin
    {
        public uc_CodeConverter(IBeepService service)
        {
            InitializeComponent();
            Details = new AddinDetails();
            Dependencies = new Dependencies();
            beepservice = service;
            Dependencies.Logger = beepservice.lg;
            Dependencies.DMEEditor = beepservice.DMEEditor;
            DMEEditor = beepservice.DMEEditor;
            visManager = beepservice.vis;
            Details.AddinName = "Code Converter";
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

        private IBranch CurrentBranch;

        public IBranch ParentBranch { get; private set; }
        public AddinDetails Details { get  ; set  ; }
        public Dependencies Dependencies { get  ; set  ; }

        private IBeepService beepservice;

        public string GuidID { get  ; set  ; }

        IAppManager visManager;

        IBranch RootAppBranch;
        IBranch branch;
        DeveloperClassCreatorUtilities manager;
        IDataSource ds;

        public event EventHandler OnStart;
        public event EventHandler OnStop;
        public event EventHandler<ErrorEventArgs> OnError;

        public void Run(IPassedArgs pPassedarg)
        {

        }

        public void SetConfig(IDMEEditor pbl, IDMLogger plogger, IUtil putil, string[] args, IPassedArgs e, IErrorsInfo per)
        {
            Passedarg = e;
          
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
         

        }

        private void ToEntitybutton_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(SourcetextBox.Text))
            {
                return;
            }
            TargettextBox.Text= manager.ConvertPOCOClassToEntity(null,SourcetextBox.Text);
        }

        public void Initialize()
        {
            
        }

        public void Suspend()
        {
            
        }

        public void Resume()
        {
            
        }

        public string GetErrorDetails()
        {
            return "";
        }

        public void Run(params object[] args)
        {
           
        }

        public Task<IErrorsInfo> RunAsync(IPassedArgs pPassedarg)
        {
            return null;
        }

        public Task<IErrorsInfo> RunAsync(params object[] args)
        {
            return null;
        }

        public void Configure(Dictionary<string, object> settings)
        {
            TreeEditor = (ITree)visManager.Tree;
            CurrentBranch=TreeEditor.CurrentBranch;
            ParentBranch = TreeEditor.CurrentBranch.ParentBranch;
       
            manager = new DeveloperClassCreatorUtilities(DMEEditor);
            manager.LoadTemplates();
            this.ToEntitybutton.Click += ToEntitybutton_Click;
        }

        public void ApplyTheme()
        {
            
        }

        public void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            
        }

        public void SetError(string message)
        {
            
        }
    }
}
