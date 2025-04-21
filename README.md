# Beep Developer Assistant

Beep Developer Assistant is a solution designed to enhance developer productivity within the [BeepDM Framework](https://github.com/The-Tech-Idea/BeepDM), developed by [The-Tech-Idea](https://github.com/The-Tech-Idea). It provides a suite of utilities and menu-driven commands for tasks like text file manipulation, scheduling, code generation, and more, seamlessly integrating with the BeepDM ecosystem. This project is in its early stages (Alpha) and aims to streamline workflows for developers using BeepDM.

## Features
- **Code Generation**: Generate C# classes, interfaces, and UI components (e.g., WinForms, Razor Pages, Blazor) from entity structures.
- **Text File Utilities**: Read, write, and manipulate CSV, fixed-width, and text files with advanced parsing and filtering.
- **Scheduling**: Manage and execute tasks with cron-based or interval-based scheduling.
- **Web Utilities**: Perform HTTP operations (GET, POST, etc.) with file downloads and JSON handling.
- **Compression**: Compress and decompress files using ZIP and GZip, with encryption support.
- **Encryption**: Hash, encrypt, and decrypt data using SHA256, AES, and RSA.
- **Localization**: Handle culture settings, resource files, and translations.
- **UI Integration**: Provides WinForms controls for template editing and code conversion.

## Installation
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/The-Tech-Idea/Beep.DeveloperAssistant.git
   ```
2. **Open in Visual Studio**:
   - Open the solution file (`Beep.DeveloperAssistant.sln`) in Visual Studio 2019 or later. (Note: The `.sln` file may not yet be in the repo; use your local copy.)
   - Build the solution to compile all projects.
3. **Integrate with BeepDM**:
   - Ensure the [BeepDM Framework](https://github.com/The-Tech-Idea/BeepDM) is installed and configured.
   - Reference the built outputs (e.g., DLLs) in your BeepDM project.

## Usage
- **Menu Commands**: Access features via the "Developer" menu in a BeepDM-integrated application, providing interactive dialogs for operations like file compression or code generation.
- **Utilities**: Use the `Beep.DeveloperAssistant.Logic` classes directly in your code for programmatic access to features.
- **WinForms Controls**: Embed `uc_DeveloperAssistantTemplateDesigner` or `uc_CodeConverter` in your WinForms application for template editing and code conversion.

## Requirements
- Visual Studio 2019 or later (for development).
- [.NET Framework](https://dotnet.microsoft.com/download) or [.NET Core](https://dotnet.microsoft.com/download) (version TBD based on BeepDM requirements).
- [BeepDM Framework](https://github.com/The-Tech-Idea/BeepDM) (specific version TBD).

## Project Structure
The `Beep.DeveloperAssistant` solution contains the following projects:

### Beep.DeveloperAssistant.Logic
- **Type**: Class Library
- **Namespace**: `Beep.DeveloperAssistant.Logic`
- **Purpose**: Core utility classes providing functionality like text file handling, scheduling, web operations, and code generation for BeepDM integration.
- **Key Files**:
  - `DeveloperTextFileUtilities.cs`: Handles text file operations (CSV, fixed-width, etc.).
  - `DeveloperSchedulingUtilities.cs`: Manages task scheduling with cron and interval support.
  - `DeveloperReflectionUtilities.cs`: Provides reflection-based utilities for type inspection and dynamic creation.
  - `DeveloperWebUtilities.cs`: Executes HTTP requests and file downloads.
  - `DeveloperNetworkUtilities.cs`: Offers network-related utilities (ping, port scanning, etc.).
  - `DeveloperConversionUtilities.cs`: Converts data types and maps properties.
  - `DeveloperLocalizationUtilities.cs`: Manages culture and resource localization.
  - `DeveloperEncryptionUtilities.cs`: Implements hashing and encryption (SHA256, AES, RSA).
  - `DeveloperCompressionUtilities.cs`: Handles file compression (ZIP, GZip) and encryption.
  - `DeveloperClassCreatorUtilities.cs`: Generates C# classes and UI components from entity structures.
- **Output**: A DLL (`Beep.DeveloperAssistant.Logic.dll`) referenced by other projects.

### Beep.DeveloperAssistant.MenuCommands
- **Type**: Class Library
- **Namespace**: `Beep.DeveloperAssistant.MenuCommands`
- **Purpose**: Provides menu-driven commands for the BeepDM "Developer" menu, exposing `Logic` utilities via interactive WinForms dialogs.
- **Key Files**:
  - `DeveloperLocalizationMenuCommands.cs`: Commands for culture and resource management.
  - `DeveloperCompressionMenuCommands.cs`: Commands for file compression and decompression.
  - `DeveloperEncryptionMenuCommands.cs`: Commands for hashing and encryption operations.
  - `DeveloperWebMenuCommands.cs`: Commands for HTTP operations and file downloads.
  - `DeveloperSchedulingMenuCommands.cs`: Commands for task scheduling and management.
  - `DeveloperTextFileMenuCommands.cs`: Commands for text file manipulation.
  - `DeveloperClassCreatorMenuCommands.cs`: Commands for code generation (POCO, Entity, UI components).
- **Output**: A DLL (`Beep.DeveloperAssistant.MenuCommands.dll`) integrated into BeepDM’s menu system.

### Beep.DeveloperAssistant.Winform
- **Type**: Windows Forms User Control Library
- **Namespace**: `Beep.DeveloperAssistant.Winform`
- **Purpose**: Provides WinForms user controls for template editing within BeepDM applications.
- **Key Files**:
  - `uc_DeveloperAssistantTemplateDesigner.cs`: A user control for designing and managing code templates, integrating with `DeveloperClassCreatorUtilities`.
- **Output**: A DLL (`Beep.DeveloperAssistant.Winform.dll`) embeddable in BeepDM WinForms applications.

### Beep.DeveloperAssistant.WinformCore
- **Type**: Windows Forms User Control Library
- **Namespace**: `Beep.DeveloperAssistant.WinformCore`
- **Purpose**: Provides WinForms user controls for code conversion within BeepDM applications.
- **Key Files**:
  - `uc_CodeConverter.cs`: A user control for converting POCO classes to Entity classes, integrating with `DeveloperClassCreatorUtilities`.
- **Output**: A DLL (`Beep.DeveloperAssistant.WinformCore.dll`) embeddable in BeepDM WinForms applications.

*(Note: Additional projects may exist locally and will be documented as they are added to the repository or clarified by the maintainers.)*

## Contributing
We welcome contributions! Please see our [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to get involved.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Status
This project is in **Alpha**. Expect rapid changes as we build out functionality. Check the [Issues](https://github.com/The-Tech-Idea/Beep.DeveloperAssistant/issues) tab for current tasks and bugs.

## Contact
For questions or suggestions, open an issue or reach out to the maintainers at [The-Tech-Idea](https://github.com/The-Tech-Idea).
