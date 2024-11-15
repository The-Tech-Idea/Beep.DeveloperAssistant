
using TheTechIdea.Beep;
using TheTechIdea.Beep.Vis;

using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;

using TheTechIdea.Beep.Editor;

using TheTechIdea.Beep.Vis.Modules;

namespace Beep.DeveloperAssistant.Logic
{
    internal class FunctionandExtensionsHelpers
    {
        public IDMEEditor DMEEditor { get; set; }
        public IPassedArgs Passedargs { get; set; }
        public IVisManager Vismanager { get; set; }
        public IControlManager Controlmanager { get; set; }
        public ITree TreeEditor { get; set; }
        public IProgress<PassedArgs> progress { get; set; } 
        public CancellationToken token { get; set; }

        public IDataSource DataSource { get; set; }
        public IBranch pbr { get; set; }
        public IBranch RootBranch { get; set; }
        public IBranch ParentBranch { get; set; }
        public IBranch ViewRootBranch { get; set; }
        public IBranch DeveloperBranch { get; set; }
        public FunctionandExtensionsHelpers(IDMEEditor pdMEEditor, IVisManager pvisManager, ITree ptreeControl)
        {
            DMEEditor = pdMEEditor;
            Vismanager = pvisManager;
            TreeEditor = ptreeControl;
        }
        public void GetValues(IPassedArgs Passedarguments)
        {
            if (Passedarguments.Objects.Where(c => c.Name == "IProgress").Any())
            {
                progress = (IProgress<PassedArgs>)Passedarguments.Objects.Where(c => c.Name == "IProgress").FirstOrDefault().obj;
            }
            if (Passedarguments.Objects.Where(c => c.Name == "CancellationToken").Any())
            {
                token = (CancellationToken)Passedarguments.Objects.Where(c => c.Name == "CancellationToken").FirstOrDefault().obj;
            }
            if (progress == null)
            {
                progress = DMEEditor.progress;

            }
            if (Passedarguments.Objects.Where(c => c.Name == "VISUTIL").Any())
            {
                Vismanager = (IVisManager)Passedarguments.Objects.Where(c => c.Name == "VISUTIL").FirstOrDefault().obj;
            }
            if (Passedarguments.Objects.Where(c => c.Name == "TreeControl").Any())
            {
                TreeEditor = (ITree)Passedarguments.Objects.Where(c => c.Name == "TreeControl").FirstOrDefault().obj;
            }
           
            if (Passedarguments.Objects.Where(c => c.Name == "ControlManager").Any())
            {
                Controlmanager = (IControlManager)Passedarguments.Objects.Where(c => c.Name == "ControlManager").FirstOrDefault().obj;
            }
          

            if (Passedarguments.Objects.Where(i => i.Name == "Branch").Any())
            {
                Passedarguments.Objects.Remove(Passedarguments.Objects.Where(c => c.Name == "Branch").FirstOrDefault());
            }
            if (Passedarguments.Id > 0)
            {
                pbr = TreeEditor.treeBranchHandler.GetBranch(Passedarguments.Id);
            }

            if (Passedarguments.Objects.Where(i => i.Name == "RootBranch").Any())
            {
                Passedarguments.Objects.Remove(Passedarguments.Objects.Where(c => c.Name == "RootBranch").FirstOrDefault());
            }

            if (Passedarguments.Objects.Where(i => i.Name == "ParentBranch").Any())
            {
                Passedarguments.Objects.Remove(Passedarguments.Objects.Where(c => c.Name == "ParentBranch").FirstOrDefault());
            }
            if (pbr != null)
            {
                Passedarguments.DatasourceName = pbr.DataSourceName;
                Passedarguments.CurrentEntity = pbr.BranchText;
                if (pbr.ParentBranchID > 0)
                {
                    ParentBranch = TreeEditor.treeBranchHandler.GetBranch(pbr.ParentBranchID);
                    Passedarguments.Objects.Add(new ObjectItem() { Name = "ParentBranch", obj = ParentBranch });
                }
                Passedarguments.Objects.Add(new ObjectItem() { Name = "Branch", obj = pbr });
                if(pbr.BranchType!= EnumPointType.Root)
                {
                    int idx = TreeEditor.Branches.FindIndex(x => x.BranchClass == pbr.BranchClass && x.BranchType == EnumPointType.Root);
                    if (idx > 0)
                    {
                        RootBranch = TreeEditor.Branches[idx];
                       
                    }
                  
                }
                else
                {
                    RootBranch = pbr;
                }
                
                Passedarguments.Objects.Add(new ObjectItem() { Name = "RootBranch", obj = RootBranch });
            }


         
            if (Passedarguments.DatasourceName != null)
            {
                DataSource = DMEEditor.GetDataSource(Passedarguments.DatasourceName);
                DMEEditor.OpenDataSource(Passedarguments.DatasourceName);
            }



            ViewRootBranch = TreeEditor.Branches[TreeEditor.Branches.FindIndex(x => x.BranchClass == "VIEW" && x.BranchType == EnumPointType.Root)];
            DeveloperBranch = TreeEditor.Branches[TreeEditor.Branches.FindIndex(x => x.BranchClass == "DEV" && x.BranchType == EnumPointType.Root)];
        }
        public void AddLogMessage(string pLogType, string pLogMessage, DateTime pLogData, int pRecordID, string pMiscData, Errors pFlag)
        {
            if (progress != null)
            {
                PassedArgs passedargs = new PassedArgs()
                {
                    IsError = pFlag == Errors.Failed ? false : true,
                    Messege = pLogMessage

                };
                DMEEditor.progress.Report(passedargs);
            }
            else
                DMEEditor.AddLogMessage("Beep", pLogMessage, DateTime.Now, 0, pMiscData, pFlag);
        }
    }
}
