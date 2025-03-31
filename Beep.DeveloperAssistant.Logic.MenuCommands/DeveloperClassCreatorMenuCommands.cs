using Beep.DeveloperAssistant.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Extensions;
using Beep.DeveloperAssistant.Logic;

namespace Beep.DeveloperAssistant.MenuCommands
{
    [AddinAttribute(
        Caption = "Class Creator",
        Name = "DeveloperClassCreatorMenuCommands",
        menu = "Beep",
        misc = "DeveloperClassCreatorMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "classcreatorutilities.svg",
        order = 11,
        Showin = ShowinType.Menu
    )]
    public class DeveloperClassCreatorMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

        private FunctionandExtensionsHelpers ExtensionsHelpers;
        private DeveloperClassCreatorUtilities _classCreator;

        public DeveloperClassCreatorMenuCommands(IDMEEditor pdMEEditor, IAppManager pvisManager, ITree ptreeControl)
        {
            DMEEditor = pdMEEditor ?? throw new ArgumentNullException(nameof(pdMEEditor));
            ExtensionsHelpers = new FunctionandExtensionsHelpers(DMEEditor, pvisManager, ptreeControl);
            _classCreator = new DeveloperClassCreatorUtilities(DMEEditor);
        }

        #region Commands

        [CommandAttribute(
            Caption = "Create POCO Class",
            Name = "CreatePOCOClassCmd",
            Click = true,
            iconimage = "poco.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> CreatePOCOClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create POCO Class", "MyClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = await _classCreator.CreatePOCOClassAsync(className, entity, null, null, null, ofd.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(ofd.FileName) : null, namespaceName, true);
                        if (code != null)
                        {
                            MessageBox.Show($"Generated POCO Class:\n{code}", "POCO Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"POCO class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate POCO class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating POCO class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create INotify Class",
            Name = "CreateINotifyClassCmd",
            Click = true,
            iconimage = "inotify.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> CreateINotifyClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create INotify Class", "MyNotifyClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = await _classCreator.CreateINotifyClassAsync(entity, null, null, null, ofd.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(ofd.FileName) : null, namespaceName, true);
                        if (code != null)
                        {
                            MessageBox.Show($"Generated INotify Class:\n{code}", "INotify Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"INotify class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate INotify class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating INotify class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create Entity Class",
            Name = "CreateEntityClassCmd",
            Click = true,
            iconimage = "entity.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> CreateEntityClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create Entity Class", "MyEntity");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Entities");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = await _classCreator.CreateEntityClassAsync(entity, null, null, ofd.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(ofd.FileName) : null, namespaceName, true);
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Entity Class:\n{code}", "Entity Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Entity class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate Entity class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating Entity class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create DLL from Entities",
            Name = "CreateDLLFromEntitiesCmd",
            Click = true,
            iconimage = "dll.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> CreateDLLFromEntitiesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string dllName = Microsoft.VisualBasic.Interaction.InputBox("Enter DLL name:", "Create DLL", "MyClasses");
                string outputPath = Microsoft.VisualBasic.Interaction.InputBox("Enter output path (leave blank for default):", "Output Path", DMEEditor.ConfigEditor.Config.ScriptsPath);
                if (!string.IsNullOrEmpty(dllName))
                {
                    var entities = new List<EntityStructure>
                    {
                        new EntityStructure { EntityName = "Sample1", Fields = new List<EntityField> { new EntityField { fieldname = "Id", fieldtype = "int" }, new EntityField { fieldname = "Name", fieldtype = "string" } } },
                        new EntityStructure { EntityName = "Sample2", Fields = new List<EntityField> { new EntityField { fieldname = "Code", fieldtype = "string" }, new EntityField { fieldname = "Value", fieldtype = "double" } } }
                    };

                    var cts = new CancellationTokenSource();
                    string result = await _classCreator.CreateDLLAsync(dllName, entities, outputPath, new Progress<PassedArgs>(args => DMEEditor.AddLogMessage("Progress", args.ParameterString1, DateTime.Now, 0, null, Errors.Information)), cts.Token);
                    if (result == "ok")
                    {
                        MessageBox.Show($"DLL {dllName} created successfully at {outputPath}", "DLL Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"DLL {dllName} created", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to create DLL {dllName}: {result}", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating DLL: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create DLL from Files",
            Name = "CreateDLLFromFilesCmd",
            Click = true,
            iconimage = "dllfiles.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> CreateDLLFromFilesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string dllName = Microsoft.VisualBasic.Interaction.InputBox("Enter DLL name:", "Create DLL from Files", "MyFileClasses");
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select directory containing .cs files" })
                {
                    if (!string.IsNullOrEmpty(dllName) && fbd.ShowDialog() == DialogResult.OK)
                    {
                        var cts = new CancellationTokenSource();
                        string result = await _classCreator.CreateDLLFromFilesPathAsync(dllName, fbd.SelectedPath, fbd.SelectedPath, new Progress<PassedArgs>(args => DMEEditor.AddLogMessage("Progress", args.ParameterString1, DateTime.Now, 0, null, Errors.Information)), cts.Token);
                        if (result == "ok")
                        {
                            MessageBox.Show($"DLL {dllName} created successfully at {fbd.SelectedPath}", "DLL Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"DLL {dllName} created from files", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to create DLL {dllName}: {result}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating DLL from files: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate Interface",
            Name = "GenerateInterfaceCmd",
            Click = true,
            iconimage = "interface.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateInterfaceCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter interface name (without 'I'):", "Generate Interface", "MyInterface");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Interfaces");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Description", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.GenerateInterfaceFromEntity(entity, namespaceName, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Interface:\n{code}", "Interface Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Interface I{className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate interface I{className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating interface: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate Partial Class",
            Name = "GeneratePartialClassCmd",
            Click = true,
            iconimage = "partial.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GeneratePartialClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Generate Partial Class", "MyPartialClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Partials");
                string methods = Microsoft.VisualBasic.Interaction.InputBox("Enter additional methods (optional):", "Methods", "public void DoSomething() { }");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure { EntityName = className, Fields = new List<EntityField>() };
                        string code = _classCreator.GeneratePartialClass(entity, namespaceName, methods, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Partial Class:\n{code}", "Partial Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Partial class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate partial class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating partial class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate Class with Attributes",
            Name = "GenerateClassWithAttributesCmd",
            Click = true,
            iconimage = "attributes.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateClassWithAttributesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Generate Class with Attributes", "MyAttributedClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        Func<EntityField, IEnumerable<string>> attributes = field =>
                            field.fieldname == "Id" ? new[] { "[Key]" } : new[] { "[Required]" };

                        string code = _classCreator.GenerateClassWithCustomAttributes(entity, namespaceName, attributes, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Class with Attributes:\n{code}", "Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Class {className} with attributes generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate class {className} with attributes", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating class with attributes: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Merge Partial Class",
            Name = "MergePartialClassCmd",
            Click = true,
            iconimage = "merge.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo MergePartialClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Merge Partial Class", "MyPartialClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Partials");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "C# files (*.cs)|*.cs", Title = "Select existing class file to merge into" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName) && ofd.ShowDialog() == DialogResult.OK)
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "NewField", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.MergePartialClass(className, entity, Path.GetDirectoryName(ofd.FileName), namespaceName);
                        if (code != null)
                        {
                            MessageBox.Show($"Merged Partial Class:\n{code}", "Partial Class Merged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Partial class {className} merged into {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to merge partial class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error merging partial class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create Record Class",
            Name = "CreateRecordClassCmd",
            Click = true,
            iconimage = "record.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo CreateRecordClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter record name:", "Create Record Class", "MyRecord");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.CreateRecordClass(entity, ofd.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(ofd.FileName) : null, namespaceName, true);
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Record Class:\n{code}", "Record Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Record class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate record class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating record class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create Sealed Class",
            Name = "CreateSealedClassCmd",
            Click = true,
            iconimage = "sealed.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo CreateSealedClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create Sealed Class", "MySealedClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.CreateSealedClass(entity, ofd.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(ofd.FileName) : null, namespaceName, true);
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Sealed Class:\n{code}", "Sealed Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Sealed class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate sealed class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating sealed class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Create Abstract Class",
            Name = "CreateAbstractClassCmd",
            Click = true,
            iconimage = "abstract.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo CreateAbstractClassCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create Abstract Class", "MyAbstractClass");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.CreateAbstractClassWithStub(entity, ofd.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(ofd.FileName) : null, namespaceName, true);
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Abstract Class:\n{code}", "Abstract Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Abstract class {className} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate abstract class {className}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating abstract class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate WinForms Form",
            Name = "GenerateWinFormsFormCmd",
            Click = true,
            iconimage = "winform.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateWinFormsFormCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter form name (without 'Form'):", "Generate WinForms Form", "MyForm");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Forms");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.GenerateWinFormForEntity(entity, namespaceName, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (code != null)
                        {
                            MessageBox.Show($"Generated WinForms Form:\n{code}", "Form Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"WinForms form {className}Form generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate WinForms form {className}Form", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating WinForms form: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate MVC Controller",
            Name = "GenerateMVCControllerCmd",
            Click = true,
            iconimage = "controller.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateMVCControllerCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter controller name (without 'Controller'):", "Generate MVC Controller", "MyController");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Controllers");
                string modelNamespace = Microsoft.VisualBasic.Interaction.InputBox("Enter model namespace:", "Model Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName) && !string.IsNullOrEmpty(modelNamespace))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = className,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.GenerateMvcController(entity, namespaceName, modelNamespace, "Controller", ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (code != null)
                        {
                            MessageBox.Show($"Generated MVC Controller:\n{code}", "Controller Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"MVC Controller {className}Controller generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate MVC Controller {className}Controller", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating MVC controller: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate Razor Page",
            Name = "GenerateRazorPageCmd",
            Click = true,
            iconimage = "razor.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateRazorPageCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string pageName = Microsoft.VisualBasic.Interaction.InputBox("Enter page name:", "Generate Razor Page", "MyPage");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Pages");
                string modelNamespace = Microsoft.VisualBasic.Interaction.InputBox("Enter model namespace:", "Model Namespace", "MyApp.Models");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(pageName) && !string.IsNullOrEmpty(namespaceName) && !string.IsNullOrEmpty(modelNamespace))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = pageName,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string markup = _classCreator.GenerateRazorPageMarkup(entity, pageName, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        string modelCode = _classCreator.GenerateRazorPageModel(entity, namespaceName, pageName, modelNamespace, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (markup != null && modelCode != null)
                        {
                            MessageBox.Show($"Generated Razor Page Markup:\n{markup}\n\nPage Model:\n{modelCode}", "Razor Page Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Razor page {pageName} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate Razor page {pageName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating Razor page: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate Blazor Component",
            Name = "GenerateBlazorComponentCmd",
            Click = true,
            iconimage = "blazor.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateBlazorComponentCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string componentName = Microsoft.VisualBasic.Interaction.InputBox("Enter component name:", "Generate Blazor Component", "MyComponent");
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Components");
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Select output directory (optional)" })
                {
                    if (!string.IsNullOrEmpty(componentName) && !string.IsNullOrEmpty(namespaceName))
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = componentName,
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "Id", fieldtype = "int" },
                                new EntityField { fieldname = "Name", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.GenerateBlazorEditComponent(entity, namespaceName, componentName, ofd.ShowDialog() == DialogResult.OK, Path.GetDirectoryName(ofd.FileName));
                        if (code != null)
                        {
                            MessageBox.Show($"Generated Blazor Component:\n{code}", "Blazor Component Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Blazor component {componentName} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate Blazor component {componentName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating Blazor component: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Generate Solution Structure",
            Name = "GenerateSolutionStructureCmd",
            Click = true,
            iconimage = "solution.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo GenerateSolutionStructureCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string solutionName = Microsoft.VisualBasic.Interaction.InputBox("Enter solution name:", "Generate Solution Structure", "MySolution");
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select root directory for solution" })
                {
                    if (!string.IsNullOrEmpty(solutionName) && fbd.ShowDialog() == DialogResult.OK)
                    {
                        string result = _classCreator.GenerateBasicSolutionStructure(solutionName, fbd.SelectedPath);
                        if (result != null)
                        {
                            MessageBox.Show($"Solution structure created at:\n{result}", "Solution Structure Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Solution structure {solutionName} generated", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to generate solution structure {solutionName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating solution structure: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Validate Generated Code",
            Name = "ValidateCodeCmd",
            Click = true,
            iconimage = "validate.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ValidateCodeCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "C# files (*.cs)|*.cs", Title = "Select C# file to validate" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string code = File.ReadAllText(ofd.FileName);
                        var (isValid, errors) = _classCreator.ValidateGeneratedCode(code);
                        if (isValid)
                        {
                            MessageBox.Show("Code is syntactically valid.", "Validation Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Code in {ofd.FileName} validated successfully", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            MessageBox.Show($"Code validation failed with {errors.Count} errors:\n{string.Join("\n", errors)}", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            DMEEditor.AddLogMessage("Fail", $"Code validation failed for {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error validating code: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Merge Properties into Class",
            Name = "MergePropertiesCmd",
            Click = true,
            iconimage = "mergeprops.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo MergePropertiesCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "C# files (*.cs)|*.cs", Title = "Select existing class file to merge properties into" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var entity = new EntityStructure
                        {
                            EntityName = Path.GetFileNameWithoutExtension(ofd.FileName),
                            Fields = new List<EntityField>
                            {
                                new EntityField { fieldname = "NewProp", fieldtype = "string" }
                            }
                        };

                        string code = _classCreator.MergePropertiesIntoExistingClass(ofd.FileName, entity);
                        if (code != null)
                        {
                            MessageBox.Show($"Merged Properties into Class:\n{code}", "Properties Merged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"Properties merged into {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to merge properties into {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error merging properties: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Convert POCO to Entity",
            Name = "ConvertPOCOToEntityCmd",
            Click = true,
            iconimage = "convert.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ConvertPOCOToEntityCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "C# files (*.cs)|*.cs", Title = "Select POCO class file to convert" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string pocoCode = File.ReadAllText(ofd.FileName);
                        string className = Microsoft.VisualBasic.Interaction.InputBox("Enter new class name (optional):", "Convert POCO to Entity", Path.GetFileNameWithoutExtension(ofd.FileName));
                        string outputPath = Microsoft.VisualBasic.Interaction.InputBox("Enter output directory (leave blank to overwrite):", "Output Path", Path.GetDirectoryName(ofd.FileName));

                        string code = _classCreator.ConvertPOCOClassToEntity(outputPath, pocoCode, className);
                        if (code != null)
                        {
                            MessageBox.Show($"Converted POCO to Entity:\n{code}", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"POCO class converted to Entity in {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to convert POCO to Entity in {ofd.FileName}", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error converting POCO to Entity: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion
    }
}