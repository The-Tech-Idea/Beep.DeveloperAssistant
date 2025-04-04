﻿using Beep.DeveloperAssistant.Logic;
using Beep.DeveloperAssistant.Logic.Models;

using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;

using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

using TheTechIdea.Beep.Vis.Modules;

using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Container.Services;

namespace Beep.DeveloperAssistant.Winform
{
    [AddinAttribute(Caption = "Template Editor", Name = "uc_DeveloperAssistantTemplateDesigner", misc = "AI", addinType = AddinType.Control)]
    public partial class uc_DeveloperAssistantTemplateDesigner : UserControl,IDM_Addin
    {
        public uc_DeveloperAssistantTemplateDesigner(IBeepService service)
        {
            InitializeComponent();
            Details = new AddinDetails();
            Dependencies = new Dependencies();
            beepservice = service;
            Dependencies.Logger = beepservice.lg;
            Dependencies.DMEEditor = beepservice.DMEEditor;
            DMEEditor = beepservice.DMEEditor;
            visManager = beepservice.vis;
            Details.AddinName = "Template Editor";
        }

        public string ParentName { get  ; set  ; }
        public string AddinName { get; set; } = "Template Editor";
        public string Description { get; set; } = "Template Editor";
        public string ObjectName { get; set; }
        public string ObjectType { get; set; } = "UserControl";
        public Boolean DefaultCreate { get; set; } = true;
        public string DllPath { get  ; set  ; }
        public string DllName { get  ; set  ; }
        public string NameSpace { get  ; set  ; }
        public IErrorsInfo ErrorObject { get  ; set  ; }
        public IDMLogger Logger { get  ; set  ; }
        public IDMEEditor DMEEditor { get  ; set  ; }
        public EntityStructure EntityStructure { get  ; set  ; }
        public string EntityName { get  ; set  ; }
        public IPassedArgs Passedarg { get  ; set  ; }
        public ITree TreeEditor { get; private set; }

        private IBranch CurrentBranch;
        private IBranch ParentBranch;

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
            ErrorObject = per;
            DMEEditor = pbl;

          //  TreeEditor = (ITree)visManager.Tree;
          //  if (e.Objects.Where(c => c.Name == "Branch").Any())
          //  {
          //      branch = (IBranch)e.Objects.Where(c => c.Name == "Branch").FirstOrDefault().obj;
          //  }
          //  if (e.Objects.Where(c => c.Name == "RootAppBranch").Any())
          //  {
          //      RootAppBranch = (IBranch)e.Objects.Where(c => c.Name == "RootAppBranch").FirstOrDefault().obj;
          //  }
          //  manager = new DeveloperAssistantManager(DMEEditor);
          //  manager.LoadTemplates();
          //  templatesBindingSource.DataSource=manager.Templates;
          //  this.templatesBindingNavigatorSaveItem.Click += TemplatesBindingNavigatorSaveItem_Click;
          //  this.CreateClassestoolStripButton.Click += CreateClassestoolStripButton_Click;
          ////  this.CreateEntitybutton.Click += CreateEntitybutton_Click;
          //  this.CreateTemplete.Click += CreateTemplete_Click;



         }

        private void CreateTemplete_Click(object sender, EventArgs e)
        {
            templatesBindingSource.AddNew();
        }

        private void CreateEntitybutton_Click(object sender, EventArgs e)
        {
            CodeTemplates code = (CodeTemplates)this.templatesBindingSource.Current;
            string retval = string.Empty;
            string classnamespases=string.Empty;
            List<EntityStructure> entities = new List<EntityStructure>();
            if (visManager.DialogManager.InputBox("Beep", "Enter NameSpace you like:", ref classnamespases) == BeepDialogResult.OK)
            {
                if (string.IsNullOrEmpty(classnamespases))
                {
                    classnamespases = "TheTechIdea.Beep.Classes";
                }

            }
            string filepath = string.Empty;
            if (visManager.DialogManager.InputBox("Beep", "Enter File Path you like to Save To (Leave blank to Save to Default Bin\\Entities Path ", ref filepath) == BeepDialogResult.OK)
            {
                if (string.IsNullOrEmpty(filepath))
                {
                    filepath = Path.Combine(DMEEditor.ConfigEditor.Config.EntitiesPath, classnamespases);
                }

            }
            entities = GetSelectedEntities();
            if (entities.Count > 0)
            {
              if (isRepeatedPropertyCheckBox.Checked)
                    {

                        manager.CreateClassSingleFunction(classnamespases, code, entities, classnamespases);
                    }
                    else
                    {
                        manager.CreateEntities(filepath, entities, classnamespases);
                    }

          
                DMEEditor.AddLogMessage("Success", $"Created POCO", DateTime.Now, 0, null, Errors.Ok);
                //   ExtensionsHelpers.Vismanager.Controlmanager.MsgBox("Beep", "Created POCO Successfully");
            }
            else
                MessageBox.Show("No Entites Selected", "Class Template Designer");
            MessageBox.Show("Done");

        }

        private void CreateClassestoolStripButton_Click(object sender, EventArgs e)
        {
            List<EntityStructure> entities = new List<EntityStructure>();
            CodeTemplates codeTemplates = (CodeTemplates)this.templatesBindingSource.Current;
            if (codeTemplates == null)
            {
                MessageBox.Show("No Template Selected", "Class Template Designer");
                return;
            }
            string namespaeval = string.Empty;
            if (visManager.DialogManager.InputBox("Beep", "Enter NameSpace you like:", ref namespaeval) == BeepDialogResult.OK)
            {
                if (string.IsNullOrEmpty(namespaeval))
                {
                    namespaeval = string.Empty;
                }
            }
            entities=GetSelectedEntities();
            if (entities.Count > 0)
            {
                if (isRepeatedPropertyCheckBox.Checked)
                {

                    manager.CreateClassSingleFunction(namespaeval, codeTemplates, entities, namespaeval);
                }
                else
                {
                    manager.WriteClasses(namespaeval, codeTemplates, entities);
                }

              
                //   ExtensionsHelpers.Vismanager.Controlmanager.MsgBox("Beep", "Created POCO Successfully");
            }
            else
                MessageBox.Show("No Entites Selected", "Class Template Designer");
        }

        private void TemplatesBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            manager.SaveTemplates();
            MessageBox.Show("Save Successfull", "Class Template Designer");
        }
        private List<EntityStructure> GetSelectedEntities()
        {
            List<EntityStructure> entities = new List<EntityStructure>();
            if (TreeEditor.SelectedBranchs.Count > 0)
            {
                PassedArgs args = new PassedArgs();
                args.ParameterString1 = $"Creating  Entities Files  ";
                visManager.ShowWaitForm(args);
              
                foreach (int item in TreeEditor.SelectedBranchs)
                {

                    IBranch br = TreeEditor.Treebranchhandler.GetBranch(item);
                    ds = DMEEditor.GetDataSource(br.DataSourceName);
                    if (ds != null)
                    {
                        EntityStructure ent = (EntityStructure)ds.GetEntityStructure(br.BranchText, false).Clone();
                        if (ent != null)
                        {
                            entities.Add(ent);
                            args.ParameterString1 = $"Processing  Entity {ent.EntityName}";
                            visManager.PasstoWaitForm(args);

                        }
                        else
                        {
                            args.ParameterString1 = $"Could not get Entity {ent.EntityName}";
                            visManager.PasstoWaitForm(args);
                            DMEEditor.AddLogMessage("Fail", $"Could not get Entity {ent.EntityName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        args.ParameterString1 = $"Could not get Datasource {br.DataSourceName}";
                        visManager.PasstoWaitForm(args);
                        DMEEditor.AddLogMessage("Fail", $"Could not get Datasource {br.DataSourceName}", DateTime.Now, 0, null, Errors.Failed);
                    }

                }
             
                visManager.CloseWaitForm();
              //  DMEEditor.AddLogMessage("Success", $"Created POCO", DateTime.Now, 0, null, Errors.Ok);
                //   ExtensionsHelpers.Vismanager.Controlmanager.MsgBox("Beep", "Created POCO Successfully");
            }
            else
                MessageBox.Show("No Entites Selected", "Class Template Designer");
            return entities;
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
           return string.Empty;
        }

        public void Run(params object[] args)
        {
             
        }

        public Task<IErrorsInfo> RunAsync(IPassedArgs pPassedarg)
        {
            return Task.FromResult<IErrorsInfo>(null);
        }

        public Task<IErrorsInfo> RunAsync(params object[] args)
        {
            return Task.FromResult<IErrorsInfo>(null);
        }

        public void Configure(Dictionary<string, object> settings)
        {
            TreeEditor = (ITree)visManager.Tree;
            CurrentBranch = TreeEditor.CurrentBranch;
            ParentBranch = TreeEditor.CurrentBranch.ParentBranch;
            manager = new DeveloperClassCreatorUtilities(DMEEditor);
            manager.LoadTemplates();
            templatesBindingSource.DataSource = manager.Templates;
            this.templatesBindingNavigatorSaveItem.Click += TemplatesBindingNavigatorSaveItem_Click;
            this.CreateClassestoolStripButton.Click += CreateClassestoolStripButton_Click;
            //  this.CreateEntitybutton.Click += CreateEntitybutton_Click;
            this.CreateTemplete.Click += CreateTemplete_Click;
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
