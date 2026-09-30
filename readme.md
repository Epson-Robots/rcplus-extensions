# Epson RC+ 8.0 <br/>RC+ Extensions

Rev.2  
ENM266S8626F  

[日本語](./readme_ja.md) / [English](./readme.md)

## 1. RC+ Extensions

RC+ Extensions is a platform that enables flexible expansion by allowing users to customize Epson RC+ 8.0 according to their applications and business processes, and to integrate with external devices and systems.

For details about using the structure of RC+ Extensions, usage, the development environment preparation, and Extension type, refer to the following manual.  
"Epson RC+ 8.0 Extensions RC+ Extensions 8.0"

In readme, an overview of API is given for each type of Extension.

For details about API specification, refer to the following RC+ Extensions API reference.  
<https://epson-robots.github.io/rcplus-extensions>

## 2. Sample Project of Extension

The `samples` folder includes the actual source code of Extension distributed by Epson.
Refer to this source code when checking the specific implementation method to develop an Extension.

For details on each sample, refer to readme that is placed in each folder.

### List of sample project

**RC+ custom**

| Project | Overview |
| --- | --- |
| SimpleJog | This allows for jog control of the robot using a custom UI or a gamepad. The project is separated into Beginner and Intermediate. |
| WebCamRecorder | You can use the web camera connected to the PC to preview and record videos. The project is separated into Beginner and Intermediate. |
| IntegratedJogPanel | While viewing the point position in 2D view, you can create and edit the continuous list of points on a single screen efficiently. |
| SMCElectricGripper | It enables you to set and control the SMC LEHR series electric grippers. |

**PC Vision Custom Vision Objects**

| Project | Overview |
| --- | --- |
| AutoContrast | This is a custom vision object that changes the color to enhance the contrast. |
| FeatureMatch | This is a custom vision object that uses the matching feature to identify positions. |

### 3. RC+ Custom

#### 3.1 Description of APIs

In RC+ Custom, you can use the Extensions API to call Epson RC+ 8.0 functions.  
The major APIs are shown below. For more details, refer to the API reference.  
<https://epson-robots.github.io/rcplus-extensions>

| API | Overview |
| --- | --- |
|IRCXProjectAPI |An API set related to the SPEL+ project. See [Project](#311-project). |
|IRCXPointAPI |An API set related to point data operation. See [Points](#312-points). |
|IRCXProgramEditorAPI |An API set related to Epson RC+ 8.0 program editor operation. See [Program editor](#313-program-editor). |
|IRCXControllerConnectionAPI |An API set related to controller connection. See [Controller connection](#314-controller-connection). |
|IRCXControllerAPI |An API set for acquiring controller information and setting up the controller. See [Controller settings](#315-controller-settings). |
|IRCXIOAPI |An API set related to I/O operation. See [I/O](#316-io). |
|IRCXRobotManagerAPI |An API set related to robot operation. See [Robot operation](#317-robot-operation). |
|IRCXProgramExecutionAPI |An API set related to Epson RC+ 8.0 program execution. See [Program execution](#318-program-execution). |
|IRCXPreferencesAPI |An API set related to setting up the Epson RC+ 8.0 development environment. See [Setting the development environment](#319-setting-the-development-environment). |

To use the Extensions API, declare the following to acquire the API instance.

```C#
var extensionAPI = Main.GetAPI<IRCXProjectAPI>();
```

##### 3.1.1 Project

An API set related to project operation.

```C#
// Retrieves the API instance.
IRCXProjectAPI api = Main.GetAPI<IRCXProjectAPI>()!;  

// Opens the specified project.
var ret = await api.OpenAsync("C:\\EpsonRC80\\projects\\SimulatorDemos\\C4_Sample", false);

// Opens the specified project file.
ret = await api.OpenProjectFileAsync("Main.prg");

// Builds the project.
ret = await api.BuildAsync();

// Closes the specified project file.
ret = await api.CloseProjectFileAsync("Main.prg");

// Closes the project.
ret = await api.CloseAsync();
```

##### 3.1.2 Points

An API set related to point data operation.

```C#
// Retrieves the API instance.
IRCXPointAPI api = Main.GetAPI<IRCXPointAPI>()!;  

// Retrieves the list of point file names included in the project.
var pointFileNames = api.PointFileDescriptors.Select(x => x.FileName);  
if (pointFileNames.Count() == 0) return;

var pointFileName = pointFileNames.ElementAt(0);

// Retrieves the list of point data defined in the specified point file.
var (ret, points) = api.GetPoints(pointFileName);

// Adds a point.
Dictionary<string, IRCXPointAPI.RCXPointElement> point = new() {
    ["Number"] = new(typeof(int), 10),
    ["Label"] = new(typeof(string), "MyPointLabel"),
    ["X"] = new(typeof(double), 100.0),
    ["Hand"] = new(typeof(IRCXPointAPI.RCXPointElement.Hand), IRCXPointAPI.RCXPointElement.Hand.Lefty)
};
var addResult = api.AddPoint(pointFileName, point);

// Deletes a point.
var deleteResult = api.DeletePoint(pointFileName, 10);
```

##### 3.1.3 Program editor

An API set related to program editor operation.

```C#
// Retrieves the API instance.
IRCXProgramEditorAPI api = Main.GetAPI<IRCXProgramEditorAPI>()!;  

// List of program editors currently opened in Epson RC+ 8.0.
var programEditors = api.ProgramEditors;  
if (programEditors == null || programEditors.Count() <= 0) return;

var editor = programEditors.ElementAt(0)!;

// Retrieves the content of the specified line number.
var (ret, content) = await editor.GetLineContentAsync(1);

// Sets a breakpoint at the specified line number.
ret = await editor.SetBreakpointAsync(1);

// Clears the breakpoint set at the specified line number.
ret = await editor.ClearBreakpointAsync(1);

// Clears all breakpoints set in the program editor.
ret = await editor.ClearAllBreakpointsAsync();
```

##### 3.1.4 Controller connection

An API set related to controller connection.

```C#
// Retrieves the API instance.
IRCXControllerConnectionAPI? api = Main.GetAPI<IRCXControllerConnectionAPI>();  

// Retrieves the connection information of the controller last connected.
IRCXControllerConnectionAPI.IRCXConnection? lastConnection = api?.GetLastConnection();  
if (lastConnection == null) return;

// The controller number last connected.
int number = lastConnection.Number;  

// Connects to the controller.  
// Specify the controller number to connect to as the first argument.
bool connectResult = await api?.ConnectionControllerAsync(lastConnection).ConfigureAwait(false);

if (connectResult) {
    // Process executed when the connection succeeds.
} else {
    // Process executed when the connection fails.
}

bool? isConnected = api?.IsOnline;  // The current connection state of the controller.
if (isConnected == true) {
    // Disconnects from the controller.
    await api?.DisconnectControllerAsync().ConfigureAwait(false);  
}
```

##### 3.1.5 Controller settings

An API set for acquiring controller information and setting up the controller.  
A connection to a controller is required. For information on connecting a controller, refer to [Controller connection](#314-controller-connection).

```C#
// Retrieves the API instance.
IRCXControllerAPI api = Main.GetAPI<IRCXControllerAPI>()!;

// Retrieves the list of controller setting categories.
var categoryNames = api.GetControllerSettingsCategoryNames(); 
var categoryName = categoryNames.ElementAt(1); // "Configuration" category.

// Retrieves the controller settings for the specified category.
var (result, settings) = api.GetControllerSettings(null, categoryName); 

// Procedure for applying new values to the controller.
// Initiates a controller settings update session.
var (ret, id) = await api.StartSetControllerSettingsAsync(); 

// Updates the controller name.
settings["Name"].Value = "MyController"; 

// Submits the updated values for the specified category.
var result = await api.SetControllerSettingsAsync(id, categoryName, settings); 

// Commits the changes and applies them to the controller.
var setResult = await api.CommitSetControllerSettingsAsync(id); 
```

##### 3.1.6 I/O

An API set related to I/O operation.  
A connection to a controller is required. For information on connecting a controller, refer to [Controller connection](#314-controller-connection).

```C#
// Retrieves the API instance.
IRCXIOAPI api = Main.GetAPI<IRCXIOAPI>()!;  

var ioLabels = api.IOLabels;  // List of I/O label objects.

// Adds a new I/O label.
_ = api.AddIOLabel("MyLabel", IRCXIOAPI.RCXIOKind.Input, typeof(bool), 0, "MyComment");  
if (ioLabels?.FirstOrDefault(i => i.Label == "MyLabel") is IRCXIOAPI.IRCXIOLabel ioLabel) {
    // Updates the existing I/O label.
    _ = api.AddIOLabel("MyLabel", IRCXIOAPI.RCXIOKind.Input, typeof(bool), 0, "MyComment2");  
}

// Creates an object that monitors the I/O state.
var ret = api.CreateWatcher<bool>(IRCXIOAPI.RCXIOKind.Input, 0, (watcher, oldData, newData) => {
    // Called when the I/O state changes.
});

// To stop monitoring the I/O state, dispose of the watcher object.
if (ret != null) {
    IRCXIOAPI.IRCXIOWatcher? watcher = ret.Value.Item2;
    _watcher?.Dispose();
}
```

##### 3.1.7 Robot operation

An API set related to robot operation.

```C#
// Retrieves the API instance.
IRCXRobotManagerAPI api = Main.GetAPI<IRCXRobotManagerAPI>()!;  

// Retrieves the current robot number.
var currentRobotNumber = api.CurrentRobotNumber;

// Sets the current robot.
var ret = await api.SetCurrentRobotAsync(1);

// Retrieves the current joint position of the current robot.
var currentJointPosition = api.JointPosition;

// Monitors changes in the current robot’s joint position and performs processing when it changes.
api.ObserveProperty(x => x.JointPosition).Subscribe(position => {
    // Do something
});

// Retrieves the jogger object.
var jogger = await api.CreateJoggerAsync();

// Executes a jog operation.
// When using an actual robot, ensure the emergency stop button can be pressed before executing.
var _ = jogger.StartJointJogAsync(IRCXRobotManagerAPI.RCXJogJointAxis.J1);
```

##### 3.1.8 Program execution

An API set related to program execution.

```C#
// Retrieves the API instance.
IRCXProgramExecutionAPI api = Main.GetAPI<IRCXProgramExecutionAPI>()!;  

// Retrieves the list of user-defined functions registered in Epson RC+ 8.0.
var userFunctions = api.UserFunctions;  
if (userFunctions == null || userFunctions.Count() <= 0) return;

// Executes the specified user function.
await api.StartFunctionAsync(userFunctions.ElementAt(0), false, false, Id);

// Executes a SPEL command.
await api.ExecuteSpelCommandAsync("Motor ON");
```

##### 3.1.9 Setting the development environment

An API set related to setting up the development environment.

```C#
// Retrieves the API instance.
IRCXPreferencesAPI api = Main.GetAPI<IRCXPreferencesAPI>();  

// Retrieves the list of preference category names.
var preferenceCategories = api.GetPreferencesCategoryNames();  

// Retrieves the preference values for the specified category.
var (ret, preferences) = api.GetPreferences(preferenceCategories.ElementAt(0));  
if (preferences == null) return;

// Modifies the development environment settings.
preferences["IsAutoSave"].Value = false;
await api.SetPreferencesAsync(preferenceCategories.ElementAt(0), preferences);
```

##### 3.1.10 Windows

An API set for displaying docking windows and message boxes.

```C#
// Retrieves the API instance.
IRCXWindowAPI? api = Main.GetAPI<IRCXWindowAPI>();

// Show message box.
var response = api?.ShowMessageBox(
    new RCXCaption(Main.CommonId, Caption.ExtensionName),
    new RCXCaption("Are you OK?"),
    IRCXWindowAPI.ButtonType.Yes_No,
    IRCXWindowAPI.IconType.Question
);
if (response == IRCXWindowAPI.ResponseType.OK)
{
    // Process when the response is OK.
}
```

There is also an API for docking windows that host WPF WebView2 controls.

- The following URIs can be handled.
  - External (Internet, Intranet) sites
  - Static sites located locally on the PC running Epson RC+
    - It is also possible to start a dedicated HTTP server to dynamically import JavaScript modules.
  - Web applications running locally on the PC running Epson RC+
    - You can specify the application startup command (such as node) and parameters.
      - When \${port} and \${lang} are specified as parameters, they are replaced with the port number (automatically assigned) and the display language name, respectively.
- Features can be implemented using web technologies such as HTML, CSS, and JavaScript.
  - Script execution when closing a window or saving content
  - Editing content
    - Acquire a selected string and transfer it to the clipboard
    - Sending messages corresponding to the edit menu ("RCX.Clear", "RCX.Paste", "RCX.SelectAll", "RCX.Undo", "RCX.Redo")
  - Sending a message ("RCX.ShowHelp") corresponding to displaying help by pressing the F1 key
- Once a host object has been created, you can interact with it from C#.
  - You can also use the built-in host object chrome.webview.hostObjects.BuiltinBridge, which implements the following functions.
    - `string GetCaption(int number): Acquires the caption string for the specified number.
    - `string ReadConfiguration(bool global): Reads the extension's configuration data as a Base64 encoded string.
    - `string WriteConfiguration(bool global): Writes the extension configuration data provided as a Base64 encoded string.
      - If the global flag is true, the configuration data is common to all logged-in Windows users; if it is false, it applies to the logged-in user.

```C#
// Retrieves the API instance.
IRCXWindowAPI? api = Main.GetAPI<IRCXWindowAPI>();

if (api != null)
{
    // Show "Epson Global Portal" site in a docking window
    var webVewInfo = new IRCXWindowAPI.WebViewInfo(
        (_, _, _) => new Uri("https://epson.com/"),
        Id,
        $"{Id}.External",
        new RCXCaption(CommonId, Caption.WindowTitle_External),
        Main.CommonIcon
    );

    await api.ShowDockingWebViewWindowAsync(webViewInfo).ConfigureAwait(true);
}
```

#### 3.2 Description of Extension Points

RC+ Custom provides a mechanism (extension points) for incorporating unique features into Epson RC+, such as adding menu items, managing project files, and displaying docking windows.  
These work by implementing the interfaces for extension points provided by the Extensions API as exports of the Managed Extensibility Framework (MEF), which is .NET's built-in extension framework.  
When creating an Extension project, you can select the extension point you want to use as the Initial Feature, or you can add it as a new item even after the project is created.

This section describes how to implement the extension points.

##### 3.2.1 Main menu items and toolbar buttons

Extensions can add their own menu items to the main menu. The menu items can also be hierarchically organized using submenus.

You can also add a corresponding toolbar button for each menu item. (Adding only a toolbar button is not supported.)

Selecting a menu item or clicking a toolbar button invokes the extension's command. The command invoked from a menu item and a toolbar button is the same; however, it is possible to distinguish whether it was invoked from a toolbar button.

To use this extension point, create a class that implements the IRCXMainMenuItemProvider interface and export it.

```C#
[Export(typeof(IRCXMainMenuItemProvider))]
public class MainMenuItem : IRCXMainMenuItemProvider
{
    /// <inheritdoc />
    public string Id => Main.CommonId;

    /// <inheritdoc />
    public string MenuItemId => "MyExtension.MainMenuItem";

    /// <inheritdoc />
    public IRCXMainMenuItemProvider.MenuItem MainMenuRootItem
    {
        get
        {
            return new IRCXMainMenuItemProvider.MenuItem
            {
                Caption = new RCXCaption(Main.CommonId, Caption.MainMenu),
                Icon = Main.CommonIcon,
                CommandName = "Main",
                ToolTip = new RCXCaption(Main.CommonId, Caption.MainMenu),
            };
        }
    }

    /// <inheritdoc />
    public IRCXMainMenuItemProvider.TopLevelMenu TopLevel => IRCXMainMenuItemProvider.TopLevelMenu.Default;

    /// <inheritdoc />
    public Task ExecuteMainMenuItemCommandAsync(
        string commandName,
        bool fromToolBar
    )
    {
        // (Code here)
        return Task.CompletedTask;
    }
}
```

If there are multiple menu items, make sure that each MenuItemId is unique.

An example of adding a submenu is shown below.

```C#
    public IRCXMainMenuItemProvider.MenuItem MainMenuRootItem
    {
        get
        {
            return new IRCXMainMenuItemProvider.MenuItem
            {
                Caption = new RCXCaption(Main.CommonId, Caption.MainMenu),
                Icon = Icon,
                CommandName = "Main",
                Children =
                [
                    new()
                    {
                        Caption = new RCXCaption(Main.CommonId, Caption.MainMenu_Sub1),
                        Icon = Icon,
                        CommandName = "Sub1",
                        ToolTip = new RCXCaption(Main.CommonId, Caption.ToolTip_Sub1),
                    },
                    new()
                    {
                        Caption = new RCXCaption(Main.CommonId, Caption.MainMenu_Sub2),
                        Icon = Icon,
                        CommandName = "Sub2",
                        ToolTip = null,
                    },
                ]
            };
        }
    }
```

If ToolTip is null, no toolbar button is displayed for that menu item.

At present, there is no way to dynamically change the strings displayed in menu items.

##### 3.2.2 Docking window

An extension can display a docking window by providing a user control as its content and its view model.

Since dialog windows (modal or modeless) can be displayed using standard WPF features, no API support is provided for them.

The view model class for a user control must implement the interface IRCXUserControlViewModel.

```C#
internal partial class DockingWindowContentViewModel : IRCUserControlViewModel
{
    /// <inheritdoc />
    public string Id => Main.CommonId;

    /// <inheritdoc />
    public string ViewModelId => $"MyExtension.DockingWindow";

    /// <inheritdoc />
    public bool KeepOpenWhenProjectClosing => false;

    /// <summary>
    /// Captions
    /// </summary>
    public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

    /// <inheritdoc />
    public RCXCaption WindowCaption { get; set; } = new(Main.CommonId, Caption.WindowTitle);

    /// <inheritdoc />
    public ImageSource? WindowIcon { get; set; } = Main.CommonIcon;

    /// <inheritdoc />
    public Task<bool> CloseAsync()
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> SaveAsync()
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public void Reload()
    {
    }

    /// <inheritdoc />
    public void Copy()
    {
    }

    /// <inheritdoc />
    public void Cut()
    {
    }

    /// <inheritdoc />
    public void Paste()
    {
    }

    /// <inheritdoc />
    public void SelectAll()
    {
    }

    /// <inheritdoc />
    public void Undo()
    {
    }

    /// <inheritdoc />
    public void Redo()
    {
    }

    /// <inheritdoc />
    public void ShowHelp()
    {
    }

    /// <summary>
    /// Show docking window
    /// </summary>
    /// <returns>Task</returns>
    public static async Task Show()
    {
        DockingWindowContent control = new();

        if (control.DataContext is DockingWindowContentViewModel controlViewModel)
        {
            await Main.GetAPI<IRCXWindowAPI>().ShowDockingWindowAsync(controlViewModel, control);
        }
    }
}
```

Editing commands (Copy, Cut, Paste, SelectAll, Undo, Redo) can be invoked by selecting the corresponding item in the edit menu of the main window.

To acquire or set the enable/disable status of each of these menu items, use the GetContentState or SetContentState method of the Window API, respectively.

An example of enabling Undo in the DockingWindowContentViewModel above is shown below.

```C#
var api = Main.GetAPI<IRCXWindowAPI>();

var state = api.GetContentState(this);
api.SetContentState(this, state | ContentState.CanUndo);
```

Pressing F1 when the window is displayed invokes the ShowHelp method.

An example of using ShowHelp to display the PDF manual held by the extension is shown below. (This example assumes that the PDF manual file is located in the Resources folder of your project. To include a file in an extension, in Visual Studio set the file property "Build Action" to "Content" and set "Copy to Output Directory" to "Copy always" or "Copy if newer."

```C#
public void ShowHelp()
{
    try
    {
        var helpFilePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources",
            "Manual.pdf"
        );

        ProcessStartInfo processStartInfo = new()
        {
            FileName = helpFilePath,
            UseShellExecute = true,
        };
        Process.Start(processStartInfo);
    }
    catch (Exception)
    {
        // Error handling
    }
}
```

##### 3.2.3 Project files

You can add custom files managed by the extension to an Epson RC+ project.

To use this extension point, create a class that implements the IRCXProjectFileProvider interface and export it.

```C#
[Export(typeof(IRCXProjectFileProvider))]
public partial class ProjectFileEditorProjectFile : IRCXProjectFileProvider
{
    /// <inheritdoc />
    public string Id => Main.CommonId;

    /// <inheritdoc />
    public string FileTypeName => "MyExtensionFiles";

    /// <inheritdoc />
    public string Extension => ".ext";

    /// <inheritdoc />
    public bool UseDefaultProjectExplorerItem => true;

    /// <inheritdoc />
    public RCXCaption ProjectExplorerRootItemCaption => new(Main.CommonId, Caption.FileCategory);

    /// <inheritdoc />
    public ImageSource? ProjectExplorerRootItemIconData => Main.CommonIcon;

    /// <inheritdoc />
    public ImageSource? FileIcon => Main.CommonIcon;

    /// <inheritdoc />
    public RCXCaption FileTypeNameCaption => new(Main.CommonId, Caption.FileTypeName);

    /// <inheritdoc />
    public async Task OpenAsync(
        string fileName
    )
    {
        // Open project file editor window
        ProjectFileEditor editorControl = new();

        if (editorControl.DataContext is ProjectFileEditorViewModel editorViewModel)
        {
            editorViewModel.FileName = fileName;
            editorViewModel.LoadContent();
            await Main.GetAPI<IRCXWindowAPI>().ShowDockingWindowAsync(editorViewModel, editorControl);
        }
    }

    /// <inheritdoc />
    public void WriteInitialContent(
        FileStream fileStream
    )
    {
        try
        {
            // Write initial content of the file (optional)
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);
            writer.WriteLine("ProjectFile Initial Content");
        }
        catch (Exception)
        {
            // Handle Error
        }
    }
}
```

In an extension, the following processes are implemented.

1. File open process (OpenAsync)
   - Custom project files are typically supplied together with an editor used to edit those files. An editor that uses a docking window is implemented almost in the same way as the docking windows described above. A typical implementation would be to create an editor control, pass the contents read from the file to its view model, and display the editor window, as shown in the example above.
2. Processing to write initial content when creating a file (WriteInitialContent)
   - When this method is called, an empty file is created and the FileStream associated with the file is passed. If the initial content is empty, no special processing is required in this method.

When the UseDefaultProjectExplorerItem flag is set to true, a tree item for the extension can be added to the Project Explorer (if you do not use this feature, you must use the extension points described in the next section).

The tree items are configured as follows.

- Parent item indicating the extension file type
  - It has an "Add New Item" context menu consisting of two submenus: "New File..." and "Existing File...".
- Child item indicating the file name of the file being added
  - It has a context menu consisting of three items: "Open," "Exclude from Project," and "Delete."

##### 3.2.4 Project Explorer tree items

Extensions can add their own tree items to the Project Explorer of an open Epson RC+ project. If the tree items are linked to your own files, use the "Project files" extension point mentioned above.

Multiple tree items can be added and each item can be organized hierarchically.

Double-clicking a tree item:  
- If the item is a parent item, its child items are expanded or collapsed.
- If the item is a child item, the extension command is invoked.

A tree item can have a context menu. Selecting an item in the context menu invokes the extension's command.

To use this extension point, create a class that implements the IRCXProjectExplorerItemProvide interface and export it.

```C#
[Export(typeof(IRCXProjectExplorerItemProvider))]
public partial class ProjectExplorerItem : IRCXProjectExplorerItemProvider
{
    /// <inheritdoc />
    public string Id => Main.CommonId;

    /// <inheritdoc />
    public IRCXProjectExplorerItemProvider.Item ProjectExplorerRootItem
    {
        get
        {
            return new()
            {
                Caption = new(Main.CommonId, Caption.PECategory),
                Icon = Main.CommonIcon,
                Children =
                [
                    new()
                    {
                        Caption = new(Main.CommonId, Caption.PEItem),
                        Icon = Main.CommonIcon,
                        CommandName = "ItemCommand",
                        CommandParameter = "ItemCommandParameter",
                        ContextMenuItems =
                        [
                            new()
                            {
                                Caption = new(Main.CommonId, Caption.PEItemMenu),
                                CommandName = "ContextMenuCommand",
                                CommandParameter = "ContextMenuCommandParameter",
                            }
                        ]
                    }
                ]
            };
        }
    }

    /// <inheritdoc />
    public Task ExecuteProjectExplorerItemCommandAsync(
        string commandName,
        object? commandParameter = null
    )
    {
        // (Code here)
        return Task.CompletedTask;
    }
}
```

##### 3.2.5 External functions

A SPEL+ program can use Declare statements to invoke external functions defined in a DLL. This mechanism has existed since earlier versions of Epson RC+ and is subject to the constraint that DLLs must be 32-bit native.

The Extension Development Kit provides a bridge DLL that enables SPEL+ programs to call internal functions of the 64-bit extension assembly. This is the "External functions" extension point.

This "External functions" is invoked using the following format.

- Insert the following Declare statement into your SPEL+ program.
    ```
    Declare CallExternal, "C:\EpsonRC80\ExternalFunctionBridge.dll", "CallExternal",(commandLine$ As String, ByRef output$ As String) As Int32
    ```
  - commandLine$ is a string containing the command line, which is the function name and arguments delimited by spaces.
  - output$ is a string variable that stores the output of the function
  - The return value is the result code of the function execution.
    - Zero (0) indicates normal completion (success)
    - Non-zero indicates an error
  - (Example)
    ```
    Int32 ret
    String output$
    ret = CallExternal("CubeRoot 10.0", ByRef output$)
    Print output$
    ```
    - 2.154434690031884 is displayed in the Run window.

To use this extension point, implement and export the delegate RCXExternalFunction and export the function name as metadata.

```C#
[Export(typeof(RCXExternalFunction))]
[ExportMetadata("Name", "CubeRoot")]
public RCXExternalFunction CubeRoot = (command, parameters) =>
{
    if (parameters.Count == 0 || !double.TryParse(parameters[0], out var input))
    {
        return ValueTuple.Create(RCXResult.BadArgument, string.Empty);
    }

    double output = Math.Cbrt(input);

    return ValueTuple.Create(RCXResult.Success, output.ToString());
};
```

This extension point allows SPEL+ programs to invoke various functions that can be implemented in an extension.

The following are built-in "External functions" that are always available in Epson RC+ editions that support extensions.

1. Execute external program (waits for completion and returns the first line of output)
    ```
    ret = CallExternal("Execute program [arg(s)]", output$)
    ```
2. Start external program (does not wait for completion)
    ```
    ret = CallExternal("ExecuteNoWait program [arg(s)]", output$)
    ```
3. Acquire the string corresponding to the CallExternal result code
    ```
    ret = CallExternal("ErrorStr Str$(retCode)" output$)
    ```
    - (Example)
        ```
        Int32 ret
        String output$
        ret = CallExternal("ErrorStr 0", ByRef output$)
        Print output$
        ```
      - "Success" is displayed in the Run window.

(For advanced users)

In addition, the bridge DLL provides functions for controlling the operation of external functions (Requires a Declare statement similar to CallExternal).

1. GetTimeout(ByRef timeout As Int32) As Int32
    - Acquires the CallExternal timeout period. The unit is milliseconds, and the initial value is 30,000.
2. SetTimeout(timeout As Int32) As Int32
    - Sets the CallExternal timeout period. The unit is milliseconds.

## 4. Developing PC Vision Custom Vision Objects

### 4.1 Description of APIs

The API comprises four functions.  
Refer to the following description and implement it.  
For more details, refer to the API reference.  
<https://epson-robots.github.io/rcplus-extensions>

#### 4.1.1 CVOGetAPIVersion

This function is called to check the API version when a package is loaded. Use this function as it is without changing its implementation.

```c
CVOAPI int32_t CVOGetAPIVersion()
{
    return CVO_API_VERSION;
}
```

#### 4.1.2 CVOGetProfile

This function is called when Epson RC+ starts. Set the profile information for your custom vision object in the CVO_PROFILE structure and return it.  
For details on each setting, refer to the API Reference. Specify Properties and ResultProperties as character strings delimited by commas. For details on the properties that can be set, see "4.1.5 Properties."

```c
CVOAPI int32_t CVOGetProfile(CVO_PROFILE* profile)
{
    strcpy_s(profile->Name, CVO_VALUE_MAX_LENGTH, "InvertColor");
    strcpy_s(profile->NamePrefix, CVO_VALUE_MAX_LENGTH, "Invert");
    strcpy_s(profile->IconFileName, CVO_VALUE_MAX_LENGTH, "InvertColor.ico");
    profile->GrayscaleOnly = true;
    profile->UpdateImage = true;
    profile->DetectItems = true;
    profile->HasTeach = false;
    profile->SaveModelFileInProjectFolder = false;
    strcpy_s(profile->Properties, CVO_VALUE_MAX_LENGTH, "");
    strcpy_s(profile->ResultProperties, CVO_VALUE_MAX_LENGTH, "");

    return CVO_NOERROR;
}
```

#### 4.1.3 CVOTeach

This function is called when teaching an object. It does not need to be implemented if teaching is not required.

```c
CVOAPI int32_t CVOTeach(CVO_PARAMS* inParams, CVO_PARAMS* outParams, CVO_IMG* img, CVO_MODEL* mdl)
{
    return CVO_NOERROR;
}
```

Pass the image information of the model window to CVO_IMG. Use it as the model information. Taught model information can also be managed by Epson RC+. To manage this information by Epson RC+, return the binary information to CVO_MODEL.

```c
CVOAPI int32_t CVOTeach(CVO_IMG* img, CVO_MODEL* mdl)
{
    int32_t width = img->Width;
    int32_t height = img->Height;
    int32_t stride = img->Stride;

    mdl->Size = 4 + 4 + 4 + stride * height;
    memcpy(mdl->pBuffer + 0, &width, 4);
    memcpy(mdl->pBuffer + 4, &height, 4);
    memcpy(mdl->pBuffer + 8, &stride, 4);
    memcpy(mdl->pBuffer + 12, img->pBuffer, stride * height);

    return CVO_NOERROR;
}
```

#### 4.1.4 CVORun

This function is called when an object is run. Pass the image information of the search window to CVO_IMG. If you want to convert the image, rewrite the information in CVO_IMG.

```c
CVOAPI int32_t CVORun(CVO_PARAMS* inParams, CVO_PARAMS* outParams, CVO_DETECT_ITEMS* detectItems, CVO_IMG* img, CVO_MODEL* mdl)
{
    for (int y = 0; y < img->Height; y++)
    {
        for (int x = 0; x < img->Width; x++)
        {
            int idx = img->Stride * y + img->BytesPerPixel * x;

            if (img->BytesPerPixel == 3)
            {
                img->pBuffer[idx + 0] = 255 - img->pBuffer[idx + 0];
                img->pBuffer[idx + 1] = 255 - img->pBuffer[idx + 1];
                img->pBuffer[idx + 2] = 255 - img->pBuffer[idx + 2];
            }
            else if (img->BytesPerPixel == 1)
            {
                img->pBuffer[idx] = 255 - img->pBuffer[idx];
            }
        }
    }

    return CVO_NOERROR;
}
```

To provide detection position information, set the information in CVO_DETECT_ITEMS.

```c
CVOAPI int32_t CVORun(CVO_PARAMS* inParams, CVO_PARAMS* outParams, CVO_DETECT_ITEMS* detectItems, CVO_IMG* img, CVO_MODEL* mdl)
{
    detectItems->Count = 10;

    for (auto i = 0; i < detectItems->Count; i++)
    {
        detectItems->Items[i].PixelX = rand() % img->Width;
        detectItems->Items[i].PixelY = rand() % img->Height;
    }

    return CVO_NOERROR;
}
```

#### 4.1.5 Properties

The properties that can be displayed on the Vision Guide screen vary according to the profile information settings of the custom vision object. Check the items below.

##### 4.1.5.1 Properties

|Property |Extension Accessible |Display Always |Display DetectItems |Display HasTeach |Display by Extension Profile Settings|
|- |:-: |:-: |:-: |:-: |:-:|
|AbortSeqOnFail| |X| | | |
|Accept |X| | | |X|
|Caption| |X| | | |
|CurrentResult| | |X| | |
|Description |X |X| | | |
|Enabled| |X| | | |
|FailColor| |X| | | |
|Graphics| |X| | | |
|LabelBackColor| |X| | | |
|ModelWinCenterX| | | |X| |
|ModelWinCenterY| | | |X| |
|ModelWinHeight| | | |X| |
|ModelWinLeft| | | |X| |
|ModelWinTop| | | |X| |
|ModelWinWidth| | | |X| |
|Name| |X| | | |
|NumberToFind |X| |X| | |
|PassColor| |X| | | |
|PassType| | |X| | |
|SearchWinCenterX| |X| | | |
|SearchWinCenterY| |X| | | |
|SearchWinHeight| |X| | | |
|SearchWinLeft| |X| | | |
|SearchWinTop| |X| | | |
|SearchWinWidth| |X| | | |
|Sort| | |X| | |
|ThresholdHigh |X| | | |X|
|ThresholdLow |X| | | |X|

The following properties can be set in the profile information Properties.

- Accept
- ThresholdHigh
- ThresholdLow

##### 4.1.5.2 Result properties

|Property |Extension Accessible |Display Always |Display DetectItems |Display HasTeach |Display by Extension Profile Settings|
|- |:-: |:-: |:-: |:-: |:-:|
|Angle |X| |X *1| |X|
|CameraX| | |X| | |
|CameraY| | |X| | |
|Found| | |X| | |
|MaxX |X| |X *1| |X|
|MaxY |X| |X *1| |X|
|MinX |X| |X *1| |X|
|MinY |X| |X *1| |X|
|NumberFound| | |X| | |
|Passed| |X| | | |
|PixelX |X| |X| | |
|PixelY |X| |X| | |
|RobotX| | |X| | |
|RobotY| | |X| | |
|RobotU| | |X| | |
|Scale |X| |X *1| |X|
|Score |X| | | |X|
|Text |X| | | |X|
|Time| |X| | | |

*1 Displayed when both DetectItems and ResultProperties are set.

The following properties can be set in the ResultProperties of the profile information.

- Angle
- MaxX/MaxY/MinX/MinY
- Scale
- Score
- Text
