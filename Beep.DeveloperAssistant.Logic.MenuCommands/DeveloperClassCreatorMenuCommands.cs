using Beep.DeveloperAssistant.Logic;
using Beep.DeveloperAssistant.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Beep.DeveloperAssistant.MenuCommands
{
    [AddinAttribute(
        Caption = "Class Creator",
        Name = "DeveloperClassCreatorMenuCommands",
        menu = "Developer",
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

      //  private FunctionandExtensionsHelpers ExtensionsHelpers;
        private DeveloperClassCreatorUtilities _classCreator;

        public DeveloperClassCreatorMenuCommands( IAppManager pvisManager)
        {
       
            DMEEditor = pvisManager.DMEEditor;
            _classCreator = new DeveloperClassCreatorUtilities(DMEEditor);
            if (pvisManager.Tree != null)
            {
                tree = (ITree)pvisManager.Tree;
                ExtensionsHelpers = tree.ExtensionsHelpers;
            }
        }
        private ITree tree;
        public IFunctionandExtensionsHelpers ExtensionsHelpers { get; set; }

        #region Commands

        /// <summary>
        /// Creates a Plain Old CLR Object (POCO) class based on user input.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command allows developers to quickly generate a POCO class by:
        /// 1. Prompting for the class name
        /// 2. Prompting for the namespace
        /// 3. Optionally selecting an output directory for the generated file
        /// 
        /// The generated POCO class includes:
        /// - Basic properties (Id and Name by default)
        /// - Standard C# property syntax with getters and setters
        /// - No additional behaviors or dependencies
        /// 
        /// POCOs are useful for:
        /// - Data transfer objects (DTOs)
        /// - Model classes in MVVM/MVC patterns
        /// - Entity representations without ORM-specific attributes
        /// - Simple data containers
        /// 
        /// The file is saved to the selected directory if specified, otherwise it's displayed in a message box.
        /// </remarks>
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
                // Prompt for class name with validation
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create POCO Class", "MyClass");
                if (string.IsNullOrEmpty(className))
                {
                    DMEEditor.AddLogMessage("Info", "POCO class creation canceled: No class name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for namespace with validation
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                if (string.IsNullOrEmpty(namespaceName))
                {
                    DMEEditor.AddLogMessage("Info", "POCO class creation canceled: No namespace provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Optional: select output directory
                using (OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    Title = "Select output directory (click any file in target directory)",
                    CheckFileExists = false
                })
                {
                    // Define default entity structure
                    var entity = new EntityStructure
                    {
                        EntityName = className,
                        Fields = new List<EntityField>
                {
                    new EntityField {FieldName = "Id", Fieldtype = "int" },
                    new EntityField {FieldName = "Name", Fieldtype = "string" }
                }
                    };

                    // Get output directory if dialog is confirmed
                    string outputPath = null;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = Path.GetDirectoryName(ofd.FileName);
                    }

                    // Generate the POCO class
                    string code = await _classCreator.CreatePOCOClassAsync(
                        className,
                        entity,
                        null,   // Using header
                        null,   // Implementations
                        null,   // Extra code
                        outputPath,
                        namespaceName,
                        true    // Generate code files
                    );

                    // Check if generation was successful
                    if (code != null)
                    {
                        // Prepare success message based on whether file was saved or just generated
                        string successMessage = outputPath != null
                            ? $"POCO class {className} generated and saved to {outputPath}"
                            : $"POCO class {className} generated";

                        // Show the generated code
                        MessageBox.Show($"Generated POCO Class:\n\n{code}", "POCO Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", successMessage, DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to generate POCO class {className}", DateTime.Now, 0, null, Errors.Failed);
                        MessageBox.Show($"Failed to generate POCO class {className}", "Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating POCO class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                MessageBox.Show($"Error creating POCO class: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DMEEditor.ErrorObject;
        }
        /// <summary>
        /// Creates a class that implements INotifyPropertyChanged interface for property change notification.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command generates a class with the INotifyPropertyChanged implementation which is essential for:
        /// 
        /// - Data binding in WPF, UWP, and other XAML-based applications
        /// - Implementing the MVVM (Model-View-ViewModel) pattern
        /// - Creating observable properties that notify the UI when values change
        /// 
        /// The generated class includes:
        /// - Private backing fields for each property (_propertyNameValue)
        /// - Public properties with getters and setters that raise property change events
        /// - PropertyChanged event implementation
        /// - NotifyPropertyChanged helper method with CallerMemberName attribute
        /// 
        /// This pattern is widely used in .NET UI frameworks to enable automatic UI updates
        /// when property values change, simplifying two-way data binding scenarios.
        /// 
        /// If the output path is specified, the class will be saved to a file; otherwise,
        /// it will be displayed in a message box.
        /// </remarks>
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
                // Prompt for class name with validation
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create INotify Class", "MyNotifyClass");
                if (string.IsNullOrEmpty(className))
                {
                    DMEEditor.AddLogMessage("Info", "INotify class creation canceled: No class name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for namespace with validation
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                if (string.IsNullOrEmpty(namespaceName))
                {
                    DMEEditor.AddLogMessage("Info", "INotify class creation canceled: No namespace provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Optional: select output directory
                using (OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    Title = "Select output directory (click any file in target directory)",
                    CheckFileExists = false
                })
                {
                    // Define default entity structure with sample properties
                    var entity = new EntityStructure
                    {
                        EntityName = className,
                        Fields = new List<EntityField>
                {
                    new EntityField {FieldName = "Id", Fieldtype = "int" },
                    new EntityField {FieldName = "Name", Fieldtype = "string" }
                }
                    };

                    // Get output directory if dialog is confirmed
                    string outputPath = null;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = Path.GetDirectoryName(ofd.FileName);
                    }

                    // Generate the INotify class
                    string code = await _classCreator.CreateINotifyClassAsync(
                        entity,
                        "using System.ComponentModel;\nusing System.Runtime.CompilerServices;", // Required usings
                        null,   // Additional interfaces to implement
                        null,   // Extra code
                        outputPath,
                        namespaceName,
                        true    // Generate code files
                    );

                    // Check if generation was successful
                    if (code != null)
                    {
                        // Prepare success message based on whether file was saved or just generated
                        string successMessage = outputPath != null
                            ? $"INotify class {className} generated and saved to {outputPath}"
                            : $"INotify class {className} generated";

                        // Show the generated code
                        MessageBox.Show($"Generated INotify Class:\n\n{code}", "INotify Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", successMessage, DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to generate INotify class {className}", DateTime.Now, 0, null, Errors.Failed);
                        MessageBox.Show($"Failed to generate INotify class {className}", "Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating INotify class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                MessageBox.Show($"Error creating INotify class: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DMEEditor.ErrorObject;
        }
        /// <summary>
        /// Creates an Entity class based on user input, which inherits from the base Entity class.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command generates a class that inherits from the Entity base class, which is useful for:
        /// 
        /// - Data modeling in application domains
        /// - Creating persistent data entities for databases
        /// - Building business objects with consistent property change tracking
        /// 
        /// The generated Entity class includes:
        /// - Private backing fields for each property (_propertyNameValue)
        /// - Public properties that use the SetProperty method for assignments
        /// - Inheritance from the Entity base class which typically provides:
        ///   * Property change notification
        ///   * Validation support
        ///   * Persistence capabilities
        ///   * State tracking functionality
        /// 
        /// Entity classes are the foundation of domain-driven design and are used to represent
        /// business concepts and data that will be stored, retrieved, and manipulated by the application.
        /// 
        /// If the output path is specified, the class will be saved to a file; otherwise,
        /// it will be displayed in a message box.
        /// </remarks>
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
                // Prompt for class name with validation
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Create Entity Class", "MyEntity");
                if (string.IsNullOrEmpty(className))
                {
                    DMEEditor.AddLogMessage("Info", "Entity class creation canceled: No class name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for namespace with validation
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Entities");
                if (string.IsNullOrEmpty(namespaceName))
                {
                    DMEEditor.AddLogMessage("Info", "Entity class creation canceled: No namespace provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Optional: select output directory
                using (OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    Title = "Select output directory (click any file in target directory)",
                    CheckFileExists = false
                })
                {
                    // Define default entity structure with sample properties
                    var entity = new EntityStructure
                    {
                        EntityName = className,
                        Fields = new List<EntityField>
                {
                    new EntityField {FieldName = "Id", Fieldtype = "int" },
                    new EntityField {FieldName = "Name", Fieldtype = "string" }
                }
                    };

                    // Get output directory if dialog is confirmed
                    string outputPath = null;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = Path.GetDirectoryName(ofd.FileName);
                    }

                    // Generate the Entity class
                    string code = await _classCreator.CreateEntityClassAsync(
                        entity,
                        "using TheTechIdea.Beep.DataBase;", // Required using for Entity base class
                        null,   // Extra code
                        outputPath,
                        namespaceName,
                        true    // Generate code files
                    );

                    // Check if generation was successful
                    if (code != null)
                    {
                        // Prepare success message based on whether file was saved or just generated
                        string successMessage = outputPath != null
                            ? $"Entity class {className} generated and saved to {outputPath}"
                            : $"Entity class {className} generated";

                        // Show the generated code
                        MessageBox.Show($"Generated Entity Class:\n\n{code}", "Entity Class Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", successMessage, DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Failed to generate Entity class {className}", DateTime.Now, 0, null, Errors.Failed);
                        MessageBox.Show($"Failed to generate Entity class {className}", "Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating Entity class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                MessageBox.Show($"Error creating Entity class: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DMEEditor.ErrorObject;
        }
        /// <summary>
        /// Creates a DLL (Dynamic Link Library) containing entity classes based on sample entity definitions.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command generates a DLL containing multiple entity classes by:
        /// 
        /// 1. Prompting for a DLL name (without extension)
        /// 2. Prompting for an output directory path
        /// 3. Creating sample entity classes with properties
        /// 4. Compiling all generated classes into a single DLL file
        /// 
        /// The process includes:
        /// - Creating C# class files for each entity in the specified output directory
        /// - Using the Roslyn compiler to build a DLL from these source files
        /// - Providing progress updates during the compilation process
        /// 
        /// By default, the command creates two sample entities:
        /// - Sample1 with Id (int) and Name (string) properties
        /// - Sample2 with Code (string) and Value (double) properties
        /// 
        /// Creating a DLL is useful for:
        /// - Sharing class definitions across multiple projects
        /// - Creating reusable libraries of domain models
        /// - Runtime loading of entity definitions
        /// - Packaging related entity classes together
        /// 
        /// The resulting DLL can be referenced in other .NET projects and the entity 
        /// classes within it can be instantiated and used at runtime.
        /// </remarks>
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
                // Prompt for DLL name with validation
                string dllName = Microsoft.VisualBasic.Interaction.InputBox("Enter DLL name:", "Create DLL", "MyClasses");
                if (string.IsNullOrEmpty(dllName))
                {
                    DMEEditor.AddLogMessage("Info", "DLL creation canceled: No DLL name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for output directory
                string outputPath = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter output path (leave blank for default):",
                    "Output Path",
                    DMEEditor.ConfigEditor.Config.ScriptsPath);

                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = DMEEditor.ConfigEditor.Config.ScriptsPath;
                    DMEEditor.AddLogMessage("Info", $"Using default output path: {outputPath}", DateTime.Now, 0, null, Errors.Ok);
                }

                // Check if output directory exists and is writable
                if (!Directory.Exists(outputPath))
                {
                    try
                    {
                        Directory.CreateDirectory(outputPath);
                        DMEEditor.AddLogMessage("Info", $"Created output directory: {outputPath}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    catch (Exception ex)
                    {
                        DMEEditor.AddLogMessage("Error", $"Failed to create output directory: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                        MessageBox.Show($"Failed to create output directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return DMEEditor.ErrorObject;
                    }
                }

                // Create sample entity definitions
                var entities = new List<EntityStructure>
        {
            new EntityStructure {
                EntityName = "Sample1",
                Fields = new List<EntityField> {
                    new EntityField {FieldName = "Id", Fieldtype = "int" },
                    new EntityField {FieldName = "Name", Fieldtype = "string" }
                }
            },
            new EntityStructure {
                EntityName = "Sample2",
                Fields = new List<EntityField> {
                    new EntityField {FieldName = "Code", Fieldtype = "string" },
                    new EntityField {FieldName = "Value", Fieldtype = "double" }
                }
            }
        };

                // Set up progress reporting and cancellation
                var cts = new CancellationTokenSource();
                var progress = new Progress<PassedArgs>(args =>
                    DMEEditor.AddLogMessage("Progress", args.ParameterString1, DateTime.Now, 0, null, Errors.Information));

                // Show building status
                DMEEditor.AddLogMessage("Info", $"Building DLL {dllName}.dll...", DateTime.Now, 0, null, Errors.Ok);

                // Create the DLL asynchronously
                string result = await _classCreator.CreateDLLAsync(
                    dllName,
                    entities,
                    outputPath,
                    progress,
                    cts.Token);

                // Check result and display appropriate message
                if (result == "ok")
                {
                    string fullPath = Path.Combine(outputPath, $"{dllName}.dll");
                    MessageBox.Show(
                        $"DLL {dllName}.dll created successfully at:\n{fullPath}",
                        "DLL Created",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    DMEEditor.AddLogMessage("Success", $"DLL {dllName}.dll created at {outputPath}", DateTime.Now, 0, null, Errors.Ok);
                }
                else
                {
                    DMEEditor.AddLogMessage("Fail", $"Failed to create DLL {dllName}: {result}", DateTime.Now, 0, null, Errors.Failed);
                    MessageBox.Show(
                        $"Failed to create DLL {dllName}:\n{result}",
                        "DLL Creation Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating DLL: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                MessageBox.Show($"Error creating DLL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DMEEditor.ErrorObject;
        }
        /// <summary>
        /// Creates a DLL (Dynamic Link Library) from existing C# source files in a directory.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command compiles multiple C# files into a single DLL by:
        /// 
        /// 1. Prompting for a DLL name (without extension)
        /// 2. Opening a folder browser dialog to select the directory containing .cs files
        /// 3. Collecting all .cs files from the selected directory
        /// 4. Compiling them into a single DLL using the Roslyn compiler
        /// 
        /// The process includes:
        /// - Finding all C# source files in the specified directory
        /// - Using the Roslyn compiler to build a DLL from these source files
        /// - Providing progress updates during the compilation process
        /// - Saving the resulting DLL in the same directory as the source files
        /// 
        /// This command is useful for:
        /// - Creating libraries from existing code files
        /// - Packaging related classes into a distributable assembly
        /// - Converting a set of source files into a referenceable library
        /// - Building a DLL without needing a full project file
        /// 
        /// The resulting DLL can be referenced in other .NET projects and the classes 
        /// within it can be instantiated and used at runtime.
        /// 
        /// Note: All source files must be compatible with each other (no duplicate class names,
        /// no conflicting types, etc.) for successful compilation.
        /// </remarks>
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
                // Prompt for DLL name with validation
                string dllName = Microsoft.VisualBasic.Interaction.InputBox("Enter DLL name:", "Create DLL from Files", "MyFileClasses");
                if (string.IsNullOrEmpty(dllName))
                {
                    DMEEditor.AddLogMessage("Info", "DLL creation canceled: No DLL name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Open folder browser dialog to select directory with .cs files
                using (FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    Description = "Select directory containing .cs files",
                    UseDescriptionForTitle = true
                })
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        // Verify that the selected directory exists
                        if (!Directory.Exists(fbd.SelectedPath))
                        {
                            DMEEditor.AddLogMessage("Error", "Selected directory does not exist", DateTime.Now, 0, null, Errors.Failed);
                            MessageBox.Show("Selected directory does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return DMEEditor.ErrorObject;
                        }

                        // Check if directory contains any .cs files
                        string[] csFiles = Directory.GetFiles(fbd.SelectedPath, "*.cs");
                        if (csFiles.Length == 0)
                        {
                            DMEEditor.AddLogMessage("Error", "No C# files found in selected directory", DateTime.Now, 0, null, Errors.Failed);
                            MessageBox.Show("No C# files found in selected directory", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return DMEEditor.ErrorObject;
                        }

                        // Setup progress reporting and cancellation
                        var cts = new CancellationTokenSource();
                        var progress = new Progress<PassedArgs>(args =>
                            DMEEditor.AddLogMessage("Progress", args.ParameterString1, DateTime.Now, 0, null, Errors.Information));

                        // Show building status
                        DMEEditor.AddLogMessage("Info", $"Building DLL {dllName}.dll from {csFiles.Length} source files...", DateTime.Now, 0, null, Errors.Ok);

                        // Create the DLL asynchronously
                        string result = await _classCreator.CreateDLLFromFilesPathAsync(
                            dllName,
                            fbd.SelectedPath,
                            fbd.SelectedPath, // Output to same directory
                            progress,
                            cts.Token);

                        // Check result and display appropriate message
                        if (result == "ok")
                        {
                            string fullPath = Path.Combine(fbd.SelectedPath, $"{dllName}.dll");
                            MessageBox.Show(
                                $"DLL {dllName}.dll created successfully at:\n{fullPath}",
                                "DLL Created",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            DMEEditor.AddLogMessage("Success", $"DLL {dllName}.dll created from files at {fbd.SelectedPath}", DateTime.Now, 0, null, Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Fail", $"Failed to create DLL {dllName}: {result}", DateTime.Now, 0, null, Errors.Failed);
                            MessageBox.Show(
                                $"Failed to create DLL {dllName}:\n{result}",
                                "DLL Creation Failed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Info", "DLL creation canceled: No directory selected", DateTime.Now, 0, null, Errors.Ok);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error creating DLL from files: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
                MessageBox.Show($"Error creating DLL from files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DMEEditor.ErrorObject;
        }
        /// <summary>
        /// Generates a C# interface based on user input and a sample entity structure.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command allows developers to quickly generate a C# interface by:
        /// 1. Prompting for the interface name (without the 'I' prefix)
        /// 2. Prompting for the namespace
        /// 3. Optionally selecting an output directory for the generated file
        /// 
        /// The generated interface includes:
        /// - Standard C# interface syntax
        /// - Sample properties (Id and Description)
        /// - The interface name will be prefixed with 'I'
        /// 
        /// Interfaces are useful for:
        /// - Defining contracts for classes
        /// - Supporting dependency injection and testability
        /// - Enabling polymorphism and abstraction
        /// 
        /// If an output directory is selected, the interface will be saved to a file; otherwise,
        /// the generated code will be displayed in a message box.
        /// </remarks>
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
                // Prompt for interface name (without 'I' prefix)
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter interface name (without 'I'):", "Generate Interface", "MyInterface");
                if (string.IsNullOrEmpty(className))
                {
                    DMEEditor.AddLogMessage("Info", "Interface generation canceled: No interface name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for namespace
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Interfaces");
                if (string.IsNullOrEmpty(namespaceName))
                {
                    DMEEditor.AddLogMessage("Info", "Interface generation canceled: No namespace provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Optional: select output directory
                using (OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    Title = "Select output directory (optional)",
                    CheckFileExists = false
                })
                {
                    string outputPath = null;
                    bool saveToFile = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = Path.GetDirectoryName(ofd.FileName);
                        saveToFile = true;
                    }

                    // Define sample entity structure for the interface
                    var entity = new EntityStructure
                    {
                        EntityName = className,
                        Fields = new List<EntityField>
                        {
                            new EntityField {FieldName = "Id", Fieldtype = "int" },
                            new EntityField {FieldName = "Description", Fieldtype = "string" }
                        }
                    };

                    // Generate the interface code
                    string code = _classCreator.GenerateInterfaceFromEntity(entity, namespaceName, saveToFile, outputPath);
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
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating interface: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
       
        /// <summary>
        /// Generates a C# partial class based on user input and optional additional methods.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command allows developers to quickly generate a C# partial class by:
        /// 1. Prompting for the class name
        /// 2. Prompting for the namespace
        /// 3. Prompting for additional methods (optional)
        /// 4. Optionally selecting an output directory for the generated file
        /// 
        /// The generated partial class includes:
        /// - Standard C# partial class syntax
        /// - Any additional methods provided by the user
        /// - No default properties unless specified in the methods input
        /// 
        /// Partial classes are useful for:
        /// - Splitting class definitions across multiple files
        /// - Organizing large or auto-generated codebases
        /// - Enabling code generation tools to extend classes without modifying user code
        /// 
        /// If an output directory is selected, the class will be saved to a file; otherwise,
        /// the generated code will be displayed in a message box.
        /// </remarks>
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
                // Prompt for class name with validation
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Generate Partial Class", "MyPartialClass");
                if (string.IsNullOrEmpty(className))
                {
                    DMEEditor.AddLogMessage("Info", "Partial class generation canceled: No class name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for namespace with validation
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Partials");
                if (string.IsNullOrEmpty(namespaceName))
                {
                    DMEEditor.AddLogMessage("Info", "Partial class generation canceled: No namespace provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for additional methods (optional)
                string methods = Microsoft.VisualBasic.Interaction.InputBox("Enter additional methods (optional):", "Methods", "public void DoSomething() { }");

                // Optional: select output directory
                using (OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    Title = "Select output directory (optional)",
                    CheckFileExists = false
                })
                {
                    string outputPath = null;
                    bool saveToFile = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = Path.GetDirectoryName(ofd.FileName);
                        saveToFile = true;
                    }

                    // Define entity structure for the partial class (no default fields)
                    var entity = new EntityStructure { EntityName = className, Fields = new List<EntityField>() };

                    // Generate the partial class code
                    string code = _classCreator.GeneratePartialClass(entity, namespaceName, methods, saveToFile, outputPath);
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
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error generating partial class: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
        /// <summary>
        /// Generates a C# class with custom attributes on its properties based on user input.
        /// </summary>
        /// <param name="Passedarguments">Arguments passed to the command.</param>
        /// <returns>An IErrorsInfo object containing any errors that occurred during execution.</returns>
        /// <remarks>
        /// This command allows developers to quickly generate a C# class with attributes by:
        /// 1. Prompting for the class name
        /// 2. Prompting for the namespace
        /// 3. Optionally selecting an output directory for the generated file
        /// 
        /// The generated class includes:
        /// - Standard C# class syntax
        /// - Sample properties (Id and Name)
        /// - Custom attributes applied to each property (e.g., [Key] for Id, [Required] for others)
        /// 
        /// Classes with attributes are useful for:
        /// - Data annotation for validation or ORM mapping
        /// - Enabling frameworks like Entity Framework or ASP.NET validation
        /// - Adding metadata to properties for code generation or runtime reflection
        /// 
        /// If an output directory is selected, the class will be saved to a file; otherwise,
        /// the generated code will be displayed in a message box.
        /// </remarks>
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
                // Prompt for class name with validation
                string className = Microsoft.VisualBasic.Interaction.InputBox("Enter class name:", "Generate Class with Attributes", "MyAttributedClass");
                if (string.IsNullOrEmpty(className))
                {
                    DMEEditor.AddLogMessage("Info", "Class with attributes generation canceled: No class name provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Prompt for namespace with validation
                string namespaceName = Microsoft.VisualBasic.Interaction.InputBox("Enter namespace:", "Namespace", "MyApp.Models");
                if (string.IsNullOrEmpty(namespaceName))
                {
                    DMEEditor.AddLogMessage("Info", "Class with attributes generation canceled: No namespace provided", DateTime.Now, 0, null, Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                // Optional: select output directory
                using (OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    Title = "Select output directory (optional)",
                    CheckFileExists = false
                })
                {
                    string outputPath = null;
                    bool saveToFile = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = Path.GetDirectoryName(ofd.FileName);
                        saveToFile = true;
                    }

                    // Define entity structure with sample properties
                    var entity = new EntityStructure
                    {
                        EntityName = className,
                        Fields = new List<EntityField>
                        {
                            new EntityField {FieldName = "Id", Fieldtype = "int" },
                            new EntityField {FieldName = "Name", Fieldtype = "string" }
                        }
                    };

                    // Define attribute logic for each property
                    Func<EntityField, IEnumerable<string>> attributes = field =>
                        field.FieldName == "Id" ? new[] { "[Key]" } : new[] { "[Required]" };

                    // Generate the class with attributes
                    string code = _classCreator.GenerateClassWithCustomAttributes(entity, namespaceName, attributes, saveToFile, outputPath);
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
                                new EntityField {FieldName = "NewField", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "Id", Fieldtype = "int" },
                                new EntityField {FieldName = "Name", Fieldtype = "string" }
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
                                new EntityField {FieldName = "NewProp", Fieldtype = "string" }
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