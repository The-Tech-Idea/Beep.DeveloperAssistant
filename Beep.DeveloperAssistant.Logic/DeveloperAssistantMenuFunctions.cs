
using System.CodeDom.Compiler;

using System.Text.RegularExpressions;

using Beep.DeveloperAssistant.Logic.Models;
using BeepEnterprize.Vis.Module;

using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;

using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Util;

namespace Beep.DeveloperAssistant.Logic
{
    [AddinAttribute(Caption = "Developer Assistant", Name = "DeveloperAssistantMenuFunctions", menu = "Beep", misc = "DeveloperAssistantMenuFunctions", ObjectType ="Beep",addinType = AddinType.Class, iconimage = "dev.png",order =4)]
    public class DeveloperAssistantMenuFunctions : IFunctionExtension
    {
        public IDMEEditor DMEEditor { get; set; }
        public IPassedArgs Passedargs { get; set; }
       // private CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

        private FunctionandExtensionsHelpers ExtensionsHelpers;
        public DeveloperAssistantMenuFunctions(IDMEEditor pdMEEditor,IVisManager pvisManager,ITree ptreeControl)
        {

            DMEEditor = pdMEEditor;

            ExtensionsHelpers = new FunctionandExtensionsHelpers(DMEEditor, pvisManager, ptreeControl);
        }
       

        [CommandAttribute(Caption = "Create POCO Classes", Name = "createpoco", Click = true, iconimage = "createpoco.png", ObjectType = "Beep", PointType = EnumPointType.DataPoint)]
        public IErrorsInfo CreatePOCOlasses(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                ExtensionsHelpers.GetValues(Passedarguments);
                //string iconimage;
                ExtensionsHelpers.pbr = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(Passedarguments.Id);
                if (ExtensionsHelpers.pbr.BranchType == EnumPointType.DataPoint)
                {
                   

                    if (ExtensionsHelpers.DataSource != null)
                    {

                        if (ExtensionsHelpers.DataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                        {
                            if (ExtensionsHelpers.Vismanager.Controlmanager.InputBoxYesNo("Beep DM", "Are you sure, this might take some time?") == BeepEnterprize.Vis.Module.DialogResult.Yes)
                            {
                                PassedArgs args = new PassedArgs();
                                args.ParameterString1 = $"Creating POCO Entities Files for {Passedarguments.DatasourceName} ";
                                ExtensionsHelpers.Vismanager.ShowWaitForm(args);
                                int i = 0;
                                //    TreeEditor.ShowWaiting();
                                //    TreeEditor.ChangeWaitingCaption($"Creating POCO Entities for total:{DataSource.EntitiesNames.Count}");
                                try
                                {
                                    if (!Directory.Exists(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName)))
                                    {
                                        Directory.CreateDirectory(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName));
                                    };
                                    {
                                        if (ExtensionsHelpers.TreeEditor.SelectedBranchs.Count > 0)
                                        {
                                            foreach (int item in ExtensionsHelpers.TreeEditor.SelectedBranchs)
                                            {
                                                args.ParameterString1 = $"Addin Entity  {item} ";
                                                ExtensionsHelpers.Vismanager.PasstoWaitForm(args);
                                                IBranch br = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(item);

                                                //         TreeEditor.AddCommentsWaiting($"{i} - Added {br.BranchText} to {Passedarguments.DatasourceName}");
                                                EntityStructure ent = ExtensionsHelpers.DataSource.GetEntityStructure(br.BranchText, false);

                                                DMEEditor.classCreator.CreateINotifyClass(ent,ent.EntityName,"","", Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName));
                                                i += 1;
                                            }
                                            DMEEditor.AddLogMessage("Success", $"Created POCO", DateTime.Now, 0, null, Errors.Ok);
                                         //   ExtensionsHelpers.Vismanager.Controlmanager.MsgBox("Beep", "Created POCO Successfully");
                                        }
                                        //foreach (string tb in DataSource.EntitiesNames)
                                        //{

                                        //}
                                        ExtensionsHelpers.Vismanager.CloseWaitForm();
                                    }
                                }
                                catch (Exception ex1)
                                {

                                    DMEEditor.AddLogMessage("Fail", $"Could not Create Directory or error in Generating Class {ex1.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                                    ExtensionsHelpers.Vismanager.CloseWaitForm();
                                }

                                //   TreeEditor.HideWaiting();
                            }

                        }

                    }
                }




            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $" error in Generating Class {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                ExtensionsHelpers.Vismanager.CloseWaitForm();
            }
            return DMEEditor.ErrorObject;

        }
        [CommandAttribute(Caption = "Create POCO DLL ", Name = "createdll", Click = true, ObjectType = "Beep", iconimage = "createdll.png", PointType = EnumPointType.DataPoint)]
        public IErrorsInfo CreateDLLclasses(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                //string iconimage;
                ExtensionsHelpers.GetValues(Passedarguments);
                List<EntityStructure> ls = new List<EntityStructure>();
                EntityStructure entity = null;
               
                ExtensionsHelpers.pbr = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(Passedarguments.Id);
                if (ExtensionsHelpers.pbr.BranchType == EnumPointType.DataPoint)
                {
                    if (ExtensionsHelpers.DataSource != null)
                    {

                        if (ExtensionsHelpers.DataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                        {
                            if (ExtensionsHelpers.Vismanager.Controlmanager.InputBoxYesNo("Beep DM", "Are you sure, this might take some time?") == BeepEnterprize.Vis.Module.DialogResult.Yes)
                            {

                                int i = 0;
                                //    TreeEditor.ShowWaiting();
                                
                                //   TreeEditor.ChangeWaitingCaption($"Creating POCO Entities for total:{DataSource.EntitiesNames.Count}");
                                try
                                {
                                    if (!Directory.Exists(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName)))
                                    {
                                        Directory.CreateDirectory(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName));
                                    };
                                    PassedArgs args = new PassedArgs();
                                    args.ParameterString1 = $"Creating DLL for POCO Entities  {Passedarguments.DatasourceName} ";
                                    ExtensionsHelpers.Vismanager.ShowWaitForm(args);
                                    foreach (int item in ExtensionsHelpers.TreeEditor.SelectedBranchs)
                                    {
                                        IBranch br = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(item);
                                        IDataSource srcds = DMEEditor.GetDataSource(br.DataSourceName);

                                        if (srcds != null)
                                        {
                                            if (srcds.DatasourceName == Passedarguments.DatasourceName)
                                            {
                                                args.ParameterString1 = $"Addin Entity  {br.BranchText} ";
                                                ExtensionsHelpers.Vismanager.PasstoWaitForm(args);
                                                if (!ExtensionsHelpers.DataSource.Entities.Where(p => p.EntityName.Equals(br.BranchText, StringComparison.OrdinalIgnoreCase)).Any())
                                                {
                                                    entity = (EntityStructure)srcds.GetEntityStructure(br.BranchText, false).Clone();
                                                }
                                                else
                                                {
                                                    entity = (EntityStructure)ExtensionsHelpers.DataSource.Entities.Where(p => p.EntityName.Equals(br.BranchText, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Clone();
                                                }
                                                //     TreeEditor.AddCommentsWaiting($"{i}- Added Entity {entity.EntityName}");
                                                ls.Add(entity);
                                                i++;
                                            }

                                        }
                                    }
                                    string ret = "ok";
                                    //Control t = (Control)TreeEditor.TreeStrucure;
                                    if (ls.Count > 0)
                                    {
                                        ///   TreeEditor.AddCommentsWaiting($"Creating Entity {entity.EntityName} Files Then DLL");
                                        ret = DMEEditor.classCreator.CreateDLL(Regex.Replace(Passedarguments.DatasourceName, @"\s+", "_"), ls, Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName), ExtensionsHelpers.progress, ExtensionsHelpers.token, "TheTechIdea." + Regex.Replace(Passedarguments.DatasourceName, @"\s+", "_"));
                                    }
                                    if (ret == "ok")
                                    {
                                        DMEEditor.AddLogMessage("Success", $"Create DLL", DateTime.Now, 0, null, Errors.Ok);
                                    //    ExtensionsHelpers.Vismanager.Controlmanager.MsgBox("Beep", "Created DLL Successfully");
                                    }
                                    else
                                    {
                                        //MessageBox.Show(t, ret,"Beep");
                                    }

                                    ExtensionsHelpers.Vismanager.CloseWaitForm();
                                }
                                catch (Exception ex1)
                                {

                                    DMEEditor.AddLogMessage("Fail", $"Could not Create Directory or error in Generating DLL {ex1.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                                    ExtensionsHelpers.Vismanager.CloseWaitForm();
                                }

                                //  TreeEditor.HideWaiting();
                            }

                        }

                    }
                }





            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $" error in Generating DLL {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }
        [CommandAttribute(Caption = "Create Template DLL ", Name = "createtemplatedll", Click = true, ObjectType = "Beep", iconimage = "createtemplatedll.png", PointType = EnumPointType.DataPoint)]
        public IErrorsInfo CreateDLLFromTemplateclasses(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperAssistantManager manager = new DeveloperAssistantManager(DMEEditor);
                manager.LoadTemplates();
                ExtensionsHelpers.GetValues(Passedarguments);
                List<EntityStructure> ls = new List<EntityStructure>();
                EntityStructure entity = null;
              
                ExtensionsHelpers.pbr = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(Passedarguments.Id);
                if (ExtensionsHelpers.pbr.BranchType == EnumPointType.DataPoint)
                {
                  if (ExtensionsHelpers.DataSource != null)
                    {

                        if (ExtensionsHelpers.DataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                        {
                            if (ExtensionsHelpers.Vismanager.Controlmanager.InputBoxYesNo("Beep DM", "Are you sure, this might take some time?") == BeepEnterprize.Vis.Module.DialogResult.Yes)
                            {

                                int i = 0;
                                //    TreeEditor.ShowWaiting();

                                //   TreeEditor.ChangeWaitingCaption($"Creating POCO Entities for total:{DataSource.EntitiesNames.Count}");
                                try
                                {
                                    if (!Directory.Exists(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName)))
                                    {
                                        Directory.CreateDirectory(Path.Combine(DMEEditor.ConfigEditor.Config.ScriptsPath, Passedarguments.DatasourceName));
                                    };
                                    PassedArgs args = new PassedArgs();
                                    args.ParameterString1 = $"Creating DLL for POCO Entities  {Passedarguments.DatasourceName} ";
                                    //  ExtensionsHelpers.Vismanager.ShowWaitForm(args);
                                    //ExtensionsHelpers.progress.Report(args);
                                    ExtensionsHelpers.AddLogMessage("Beep", args.ParameterString1, DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Ok);
                                    foreach (int item in ExtensionsHelpers.TreeEditor.SelectedBranchs)
                                    {
                                        IBranch br = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(item);
                                        IDataSource srcds = DMEEditor.GetDataSource(br.DataSourceName);

                                        if (srcds != null)
                                        {
                                            if (srcds.DatasourceName == Passedarguments.DatasourceName)
                                            {
                                                args.Messege = $"Addin Entity  {br.BranchText} ";
                                                // ExtensionsHelpers.Vismanager.PasstoWaitForm(args);
                                                ExtensionsHelpers.progress.Report(args);
                                                //    DMEEditor.AddLogMessage("Beep", args.ParameterString1, DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Ok);
                                                if (!ExtensionsHelpers.DataSource.Entities.Where(p => p.EntityName.Equals(br.BranchText, StringComparison.OrdinalIgnoreCase)).Any())
                                                {
                                                    entity = (EntityStructure)srcds.GetEntityStructure(br.BranchText, false).Clone();
                                                }
                                                else
                                                {
                                                    entity = (EntityStructure)ExtensionsHelpers.DataSource.Entities.Where(p => p.EntityName.Equals(br.BranchText, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Clone();
                                                }
                                                //     TreeEditor.AddCommentsWaiting($"{i}- Added Entity {entity.EntityName}");
                                                ls.Add(entity);
                                                i++;
                                            }

                                        }
                                    }
                                    string templatename = string.Empty;
                                    string ret = "ok";
                                    //Control t = (Control)TreeEditor.TreeStrucure;
                                    if (ls.Count > 0)
                                    {
                                        if (manager.Templates.Count > 0)
                                        {
                                            if (ExtensionsHelpers.Vismanager.Controlmanager.InputComboBox("Templates", "Please Select Code Template", manager.Templates.Select(p => p.Name).ToList(), ref templatename) == DialogResult.OK)
                                            {
                                                ret = templatename;
                                                CodeTemplates codeTemplates = manager.Templates.FirstOrDefault(p => p.Name == ret);
                                                args.Messege = $"Writing DLL {ret} ";
                                                ExtensionsHelpers.progress.Report(args);
                                                string namespaeval = string.Empty;
                                                if(ExtensionsHelpers.Vismanager.Controlmanager.InputBox("Beep","Enter NameSpace you like:",ref namespaeval)== DialogResult.OK)
                                                {
                                                    if (string.IsNullOrEmpty(namespaeval))
                                                    {
                                                        namespaeval = string.Empty;
                                                    }
                                                }
                                                manager.WriteClassesToDLL(namespaeval,Regex.Replace(Passedarguments.DatasourceName, @"\s+", "_"), codeTemplates, ls, ExtensionsHelpers.progress, ExtensionsHelpers.token);
                                            }
                                        }else DMEEditor.AddLogMessage("Failed", $"Could not find any templates", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                                    }
                                    
                                    //ExtensionsHelpers.Vismanager.CloseWaitForm();
                                }
                                catch (Exception ex1)
                                {

                                    ExtensionsHelpers.AddLogMessage("Fail", $"Could not Create Directory or error in Generating DLL {ex1.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                                   // ExtensionsHelpers.Vismanager.CloseWaitForm();
                                }

                                //  TreeEditor.HideWaiting();
                            }

                        }

                    }
                }





            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $" error in Generating DLL {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }

        [CommandAttribute(Caption = "Create Entity DLL ", Name = "createEntitydll", Click = true, ObjectType = "Beep", iconimage = "createEntitydll.png", PointType = EnumPointType.DataPoint)]
        public IErrorsInfo CreateEntityclasses(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperAssistantManager manager = new DeveloperAssistantManager(DMEEditor);
                //manager.LoadTemplates();
                ExtensionsHelpers.GetValues(Passedarguments);
                List<EntityStructure> ls = new List<EntityStructure>();
                EntityStructure entity = null;

                ExtensionsHelpers.pbr = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(Passedarguments.Id);
                if (ExtensionsHelpers.pbr.BranchType == EnumPointType.DataPoint)
                {

                    if (ExtensionsHelpers.Vismanager.Controlmanager.InputBoxYesNo("Beep DM", "Are you sure, this might take some time?") == BeepEnterprize.Vis.Module.DialogResult.Yes)
                    {

                        int i = 0;
                        //    TreeEditor.ShowWaiting();

                        //   TreeEditor.ChangeWaitingCaption($"Creating POCO Entities for total:{DataSource.EntitiesNames.Count}");
                        try
                        {

                            PassedArgs args = new PassedArgs();
                            args.ParameterString1 = $"Creating Entity Classes  {Passedarguments.DatasourceName} ";
                            //  ExtensionsHelpers.Vismanager.ShowWaitForm(args);
                            //ExtensionsHelpers.progress.Report(args);
                            ExtensionsHelpers.AddLogMessage("Beep", args.ParameterString1, DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Ok);
                            foreach (int item in ExtensionsHelpers.TreeEditor.SelectedBranchs)
                            {
                                IBranch br = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(item);
                                IDataSource srcds = DMEEditor.GetDataSource(br.DataSourceName);

                                if (srcds != null)
                                {
                                      args.Messege = $"Addin Entity  {br.BranchText} ";
                                        // ExtensionsHelpers.Vismanager.PasstoWaitForm(args);
                                        ExtensionsHelpers.progress.Report(args);
                                        //    DMEEditor.AddLogMessage("Beep", args.ParameterString1, DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Ok);
                                            entity = (EntityStructure)srcds.GetEntityStructure(br.BranchText, false).Clone();
                                        //     TreeEditor.AddCommentsWaiting($"{i}- Added Entity {entity.EntityName}");
                                        ls.Add(entity);
                                        i++;
                                   

                                }
                            }
                            string templatename = string.Empty;
                            string ret = "ok";
                            //Control t = (Control)TreeEditor.TreeStrucure;
                            if (ls.Count > 0)
                            {
                                string classnamespases = string.Empty;
                                if (ExtensionsHelpers.Vismanager.Controlmanager.InputBox("Beep", "Enter NameSpace you like:", ref classnamespases) == BeepEnterprize.Vis.Module.DialogResult.OK)
                                {
                                    if (string.IsNullOrEmpty(classnamespases))
                                    {
                                        classnamespases = "TheTechIdea.Beep.Classes";
                                    }

                                }
                                string filepath = ExtensionsHelpers.Vismanager.Controlmanager.ShowSpecialDirectoriesComboBox();
                                if (string.IsNullOrEmpty(filepath))
                                {
                                    string retval = ExtensionsHelpers.Vismanager.Controlmanager.SaveFileDialog("cs", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), null);
                                    if (retval != null)
                                    {
                                        filepath = retval;
                                    }
                                }
                                if (string.IsNullOrEmpty(filepath))
                                {
                                    filepath = DMEEditor.ConfigEditor.Config.EntitiesPath;
                                }

                                filepath = Path.Combine(filepath, classnamespases);

                                manager.CreateEntities(filepath, ls, classnamespases);
                            }

                            //ExtensionsHelpers.Vismanager.CloseWaitForm();
                        }
                        catch (Exception ex1)
                        {

                            ExtensionsHelpers.AddLogMessage("Fail", $"Could not Create Directory or error in Generating DLL {ex1.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                            // ExtensionsHelpers.Vismanager.CloseWaitForm();
                        }

                        //  TreeEditor.HideWaiting();
                    }

                }





            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $" error in Generating DLL {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }
        [CommandAttribute(Caption = "Code Converter", Name = "codeconverter", Click = true, ObjectType = "Beep", iconimage = "codeconverter.png", PointType = EnumPointType.DataPoint)]
        public IErrorsInfo CodeConverter(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                DeveloperAssistantManager manager = new DeveloperAssistantManager(DMEEditor);
                //manager.LoadTemplates();
                ExtensionsHelpers.GetValues(Passedarguments);
                List<EntityStructure> ls = new List<EntityStructure>();
                EntityStructure entity = null;

                ExtensionsHelpers.pbr = ExtensionsHelpers.TreeEditor.treeBranchHandler.GetBranch(Passedarguments.Id);
                ExtensionsHelpers.Vismanager.ShowPage("uc_CodeConverter", (PassedArgs)Passedargs, DisplayType.InControl);





            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $" error in Generating DLL {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }

    }
}
