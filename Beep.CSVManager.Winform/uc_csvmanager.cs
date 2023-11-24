using Beep.CSV.Extensions;
using BeepEnterprize.Vis.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Logger;
using TheTechIdea.Util;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Beep.CSVManager.Winform
{
    [AddinAttribute(Caption = "CSV Manager", Name = "uc_csvmanager", misc = "AI", addinType = AddinType.Control)]
    public partial class uc_csvmanager : UserControl,IDM_Addin
    {
       
        public uc_csvmanager()
        {
            InitializeComponent();
        }

        public string ParentName { get ; set ; }
        public string ObjectName { get ; set ; }
        public string ObjectType { get ; set ; }
        public string AddinName { get ; set ; }
        public string Description { get ; set ; }
        public bool DefaultCreate { get ; set ; }
        public string DllPath { get ; set ; }
        public string DllName { get ; set ; }
        public string NameSpace { get ; set ; }
        public IErrorsInfo ErrorObject { get ; set ; }
        public IDMLogger Logger { get ; set ; }
        public IDMEEditor DMEEditor { get ; set ; }
        public EntityStructure EntityStructure { get ; set ; }
        public string EntityName { get ; set ; }
        public IPassedArgs Passedarg { get ; set ; }
        IVisManager visManager;

        IBranch RootAppBranch;
        IBranch branch;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        IProgress<PassedArgs> progress;
        ReadCSV cSV;
        public void Run(IPassedArgs pPassedarg)
        {
           
        }

        public void SetConfig(IDMEEditor pbl, IDMLogger plogger, IUtil putil, string[] args, IPassedArgs e, IErrorsInfo per)
        {
            Passedarg = e;
            Logger = plogger;
            ErrorObject = per;
            DMEEditor = pbl;
            
            
            visManager = (IVisManager)e.Objects.Where(c => c.Name == "VISUTIL").FirstOrDefault().obj;
          
            if (e.Objects.Where(c => c.Name == "Branch").Any())
            {
                branch = (IBranch)e.Objects.Where(c => c.Name == "Branch").FirstOrDefault().obj;
            }
            if (e.Objects.Where(c => c.Name == "RootAppBranch").Any())
            {
                RootAppBranch = (IBranch)e.Objects.Where(c => c.Name == "RootAppBranch").FirstOrDefault().obj;
            }
            this.GetFolderbutton.Click += GetFolderbutton_Click;
            cSV = new ReadCSV(DMEEditor);
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            fileDefinitionBindingSource.DataSource = cSV.fileDefinition;
          
        }

        private void GetFolderbutton_Click(object sender, EventArgs e)
        {
            string folderpath = "";
            PassedArgs p = new PassedArgs();
            if (visManager.Controlmanager.InputBox("Beep AI", $"Please Select Folder for Files to ?",ref folderpath) == BeepEnterprize.Vis.Module.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(folderpath))
                {
                    visManager.ShowWaitForm(p);
                    p.ParameterString1 = "Loading Files's";
                    visManager.PasstoWaitForm(p);
                    progress = new Progress<PassedArgs>(percent => {
                      
                      
                        p.ParameterString1 = percent.ParameterString1;
                        visManager.PasstoWaitForm(p);
                    });
                    cSV.GetColumnsFromFolder(folderpath, progress, token);
                    visManager.CloseWaitForm();
                }
            }
        }
    }
}
