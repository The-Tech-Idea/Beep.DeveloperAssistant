using Beep.Vis.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Util;

namespace Beep.CSV.Extensions
{
    [AddinAttribute(Caption = "CSV Management", Name = "CSVManagerFunctions", ObjectType = "Beep", menu = "Beep", misc = "IFunctionExtension", addinType = AddinType.Class, iconimage = "File.ico", order = 1)]
    public class CSVManagerFunctions : IFunctionExtension
    {
        public IDMEEditor DMEEditor { get  ; set  ; }
        public IPassedArgs Passedargs { get  ; set  ; }

        private FunctionandExtensionsHelpers ExtensionsHelpers;


        public CSVManagerFunctions(IDMEEditor pdMEEditor, IVisManager pvisManager, ITree ptreeControl)
        {
            DMEEditor = pdMEEditor;

            ExtensionsHelpers = new FunctionandExtensionsHelpers(DMEEditor, pvisManager, ptreeControl);
        }
        [CommandAttribute(Caption = "CSV Manager", Name = "CSVManager", Click = true, iconimage = "newproject.ico", ObjectType = "Beep", PointType = EnumPointType.Global)]
        public IErrorsInfo CSVManager(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {

                ExtensionsHelpers.GetValues(Passedarguments);
                ExtensionsHelpers.Vismanager.ShowPage("uc_csvmanager", (PassedArgs)DMEEditor.Passedarguments, DisplayType.InControl);
                // DMEEditor.AddLogMessage("Success", $"Open Data Connection", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Could not create new project {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }
    }
}
