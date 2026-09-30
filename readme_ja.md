# Epson RC+ 8.0<br/>RC+ Extensions

Rev.2  
JAM266S8625F  

[日本語](./readme_ja.md) / [English](./readme.md)

## 1. RC+ Extensions について

RC+ Extensionsは、ユーザーの用途や業務プロセスに合わせて、Epson RC+ 8.0をカスタマイズしたり、外部機器・システムと連携することで柔軟に拡張できるプラットフォームです。

RC+ Extensionsの全体的な構成、利用方法、開発環境の準備、およびExtensionの種類については、以下のマニュアルを参照してください。  
"Epson RC+ 8.0 拡張機能 RC+ Extensions 8.0"

本readmeでは、Extensionの種類ごとにAPIの概要について説明します。

詳細なAPI仕様に関しては、以下のRC+ Extensions APIリファレンスを参照してください。  
<https://epson-robots.github.io/rcplus-extensions>

## 2. Extensionのサンプルプロジェクト

`samples`フォルダには、エプソンが配信提供している実際のExtensionのソースコードが含まれています。  
Extensionを開発するうえで、具体的な実装方法を確認する際の参考として利用してください。

各サンプルの詳細な説明については、それぞれのフォルダ内に配置されているreadmeを参照してください。

### サンプルプロジェクト一覧

**RC+カスタム**

| プロジェクト | 概要 |
| --- | --- |
| SimpleJog | カスタムUI、およびゲームパッドによるロボットのジョグ操作を可能とします。初級編(Beginner)と中級編(Intermediate)でプロジェクトは分かれています。 |
| WebCamRecorder | PC接続のウェブカメラを使用し、映像のプレビュー表示と録画が可能です。初級編(Beginner)と中級編(Intermediate)でプロジェクトは分かれています。 |
| IntegratedJogPanel | ポイント位置を2Dビューで確認しながら、連続したポイント一覧の作成・編集を一画面で効率よく行うことが可能です。 |
| SMCElectricGripper | SMC電動グリッパ LEHRシリーズの設定および制御を可能とします。 |

**PCビジョン カスタムビジョンオブジェクト**

| プロジェクト | 概要 |
| --- | --- |
| AutoContrast | コントラストを高めるように色変換するカスタムビジョンオブジェクトです。 |
| FeatureMatch | 特徴量マッチングを使用して位置検出をおこなうカスタムビジョンオブジェクトです。 |

### 3. RC+カスタム

#### 3.1 API解説

RC+カスタムでは、Extensions APIを利用してEpson RC+ 8.0の機能を呼び出すことができます。  
主なAPIを以下に示します。より詳細な内容はAPIリファレンスを参照ください。  
<https://epson-robots.github.io/rcplus-extensions>

| API | 概要 |
| --- | --- |
|IRCXProjectAPI|SPEL+プロジェクトに関するAPI群です。[プロジェクト](#311-プロジェクト)参照|
|IRCXPointAPI|ポイントデータの操作に関するAPI群です。[ポイント](#312-ポイント)参照|
|IRCXProgramEditorAPI|Epson RC+ 8.0のプログラムエディター操作に関するAPI群です。[プログラムエディター](#313-プログラムエディター)参照|
|IRCXControllerConnectionAPI|コントローラーの接続に関するAPI群です。[コントローラー接続](#314-コントローラー接続)参照|
|IRCXControllerAPI|コントローラーの情報を取得したり、コントローラーに設定をしたりするためのAPI群です。[コントローラー設定](#315-コントローラー設定)参照|
|IRCXIOAPI|I/O操作に関するAPI群です。[I/O](#316-io)参照|
|IRCXRobotManagerAPI|ロボット操作に関するAPI群です。[ロボット操作](#317-ロボット操作)参照|
|IRCXProgramExecutionAPI|Epson RC+ 8.0のプログラム実行操作に関するAPI群です。[プログラム実行](#318-プログラム実行)参照|
|IRCXPreferencesAPI|Epson RC+ 8.0の開発環境設定に関するAPI群です。[開発環境設定](#319-開発環境設定)参照|

Extensions APIを利用するには、以下のように記述し、APIインスタンスを取得します。

```C#
var extensionAPI = Main.GetAPI<IRCXProjectAPI>();
```

##### 3.1.1 プロジェクト

プロジェクト操作に関するAPI群です。

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

##### 3.1.2 ポイント

ポイントデータの操作に関するAPI群です。

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

##### 3.1.3 プログラムエディター

プログラムエディター操作に関するAPI群です。

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

##### 3.1.4 コントローラー接続

コントローラーの接続に関するAPI群です。

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

##### 3.1.5 コントローラー設定

コントローラーの情報を取得したり、コントローラーに設定をしたりするためのAPI群です。  
コントローラーに接続している必要があります。コントローラー接続に関しては[コントローラー接続](#314-コントローラー接続)を参照ください。

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

I/O操作に関するAPI群です。  
コントローラーに接続している必要があります。コントローラー接続に関しては[コントローラー接続](#314-コントローラー接続)を参照ください。

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

##### 3.1.7 ロボット操作

ロボット操作に関するAPI群です。

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

##### 3.1.8 プログラム実行

プログラム実行操作に関するAPI群です。

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

##### 3.1.9 開発環境設定

開発環境設定に関するAPI群です。

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

##### 3.1.10 ウィンドウ

ドッキングウィンドウやメッセージボックスを表示する等のAPI群です。

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

WPF の WebView2 コントロールをホストするドッキングウィンドウ用の API もあります。

- 以下の URI を扱うことができます。
  - 外部(インターネット、イントラネット)のサイト
  - Epson RC+を動かすPCのローカルに配置した静的サイト
    - JavaScriptモジュールの動的インポートを行うために、専用のHTTPサーバーを起動することも可能です。
  - Epson RC+を動かすPCのローカルで動作するWebアプリケーション
    - アプリケーションの起動コマンド(node など)と、パラメーターが指定できます。
      - パラメーターとして、\${port}、\${lang} を与えると、それぞれポート番号(自動割り当て)、表示言語名に置き換えます。
- Web技術(HTML, CSS, JavaScript など)を用いて、機能を実装することができます。
  - ウィンドウクローズ、コンテンツ保存時のスクリプト実行
  - コンテンツ編集
    - 選択されている文字列の取得およびクリップボードへの転送
    - 編集メニューに対応したメッセージ("RCX.Clear", "RCX.Paste", "RCX.SelectAll", "RCX.Undo", "RCX.Redo")の送信
  - F1キーによるヘルプ表示に対応したメッセージ("RCX.ShowHelp")の送信
- ホストオブジェクトを作成すると、C#側とやりとりすることができます。
  - 以下の関数を実装した組み込みのホストオブジェクト chrome.webview.hostObjects.BuiltinBridge も使えます。
    - `string GetCaption(int number)`: 指定番号のキャプション文字列を取得する。
    - `string ReadConfiguration(bool global)`: Extension の設定データを、Base64 エンコード文字列として読み出す。
    - `string WriteConfiguration(bool global)`: Base64 エンコード文字列で与えた Extension の設定データを書き込む。
      - global フラグが true の場合、設定データは、Windows のログインユーザーに共通となり、false の場合は、ログインユーザーごととなります。

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

#### 3.2 拡張ポイント解説

RC+カスタムでは、メニュー項目の追加、プロジェクトファイルの管理、ドッキングウィンドウの表示など、Epson RC+に独自の機能を組み込むための仕組み(拡張ポイント)を提供しています。  
これらは、Extensions APIが用意する拡張ポイントのためのインターフェースを、.NET の組み込み拡張フレームワークであるManaged Extensibility Framework(MEF)のExportとして実装することで動作します。  
Extensionプロジェクト作成時には、利用したい拡張ポイントをInitial Feature として選択でき、プロジェクト作成後でも新しい項目として追加することが可能です。

ここでは、各拡張ポイントの実装方法について説明します。

##### 3.2.1 メインメニュー項目およびツールバーボタン

Extensionは、メインメニューにExtension用のメニュー項目を追加できます。メニュー項目は、サブメニューを使って階層化することもできます。

また、各メニュー項目について、対応するツールバーボタンの追加もできます。(ツールバーボタンのみの追加はサポートされていません。)

メニュー項目を選択するか、ツールバーボタンをクリックすると、Extensionのコマンドが呼び出されます。呼び出されるコマンドは、メニュー項目とツールバーボタンとで同じものですが、ツールバーボタンから呼び出されたかどうかは区別することができます。

この拡張ポイントを使うには、インターフェイスIRCXMainMenuItemProviderを実装したクラスを作成し、エクスポートします。

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

複数のメニュー項目を持つ場合、MenuItemIdは他と重複しないようにしてください。

以下は、サブメニューを持たせる場合の例です。

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

ToolTipをnullにすると、そのメニュー項目に対するツールバーボタンは表示しません。

今のところ、メニュー項目に表示する文字列等を、動的に変更する手段は用意されていません。

##### 3.2.2 ドッキングウィンドウ

Extensionは、コンテンツとなるユーザーコントロールと、そのビューモデルを提供することで、ドッキングウィンドウを表示できます。

ダイアログウィンドウ(モーダルまたはモードレス)は、WPFの標準機能で表示させることが可能ですので、APIでのサポートはありません。

ユーザーコントロール用のビューモデルクラスは、インターフェイスIRCXUserControlViewModelを実装する必要があります。

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

編集コマンド(Copy, Cut, Paste, SelectAll, Undo, Redo)は、メインウィンドウの編集メニューの、該当項目の選択で呼び出されます。

これらの各メニュー項目の有効／無効を取得または設定するには、ウィンドウAPIのGetContentStateまたはSetContentStateメソッドを用います。

以下は、上記のDockingWindowContentViewModelの中で、Undoを有効にする例です。

```C#
var api = Main.GetAPI<IRCXWindowAPI>();

var state = api.GetContentState(this);
api.SetContentState(this, state | ContentState.CanUndo);
```

ウィンドウ表示中に、F1キーを押すと、ShowHelpメソッドが呼び出されます。

以下は、ShowHelpで、Extensionが持つPDFマニュアルを表示させる例です。この例では、PDFマニュアルのファイルは、プロジェクトのResourcesフォルダにあるものとします。ファイルをExtensionに含めるには、Visual Studioで、ファイルのプロパティの「ビルドアクション」を「コンテンツ」とし、「出力ディレクトリにコピー」を「常にコピーする」か「新しい場合はコピーする」に設定します。

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

##### 3.2.3 プロジェクトファイル

Epson RC+のプロジェクトに、Extensionが管理する独自のファイルを追加することができます。

この拡張ポイントを使うには、インターフェイス IRCXProjectFileProvider を実装したクラスを作成し、エクスポートします。

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

Extension では、以下の処理を記述します。

1. ファイルを「開く」処理(OpenAsync)
   - 独自のプロジェクトファイルの提供は、通常そのファイルを編集するエディターの提供とセットで行います。ドッキングウィンドウを使うエディターについては、先に述べたドッキングウィンドウの実装方法とほとんど同じです。典型的な実装は、上記の例のように、エディターコントロールを作成して、そのビューモデルにファイルから読み込んだ内容を渡し、エディターウィンドウを表示する、というものです。
2. ファイル作成時に初期内容を書き込む処理(WriteInitialContent)
   - このメソッドの呼び出し時、ファイルは空の状態で作成され、ファイルに紐づく FileStream が渡されます。初期内容が空でよい場合は、このメソッドで特段の処理は必要ありません。

UseDefaultProjectExplorerItemフラグをtrueにすると、プロジェクトエクスプローラーに、Extension のツリー項目を追加することができます(この機能を使わない場合は、次節で述べる拡張ポイントを併用しなければなりません)。

この場合のツリー項目は、以下で構成されます。

- Extension のファイル種類を示す親項目
  - サブメニューとして「新しいファイル...」、「既存のファイル...」の2項目からなる「新しいアイテムの追加」コンテキストメニューを持ちます。
- 追加されているファイルの、ファイル名を示す子項目
  - 「開く」「プロジェクトから除外」「削除」の3項目からなるコンテキストメニューを持ちます。

##### 3.2.4 プロジェクトエクスプローラーのツリー項目

Extension は、開いているEpson RC+ プロジェクトの、プロジェクトエクスプローラーに、独自のツリー項目を追加できます。ツリー項目が、独自のファイルに紐づく場合は、前述の「プロジェクトファイル」拡張ポイントを用いてください。

ツリー項目は、複数追加でき、それぞれの項目が階層化できます。

ツリー項目のダブルクリックは、  
- 親項目の場合、子項目の展開または折込となります。
- 末端の子項目の場合、Extensionのコマンドが呼び出されます。

ツリー項目は、コンテキストメニューを持つことができます。コンテキストメニュー項目を選択すると、Extensionのコマンドが呼び出されます。

この拡張ポイントを使うには、インターフェイス IRCXProjectExplorerItemProvide を実装したクラスを作成し、エクスポートします。

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

##### 3.2.5 外部ファンクション

SPEL+ プログラムは、Declareステートメントを使って、DLLで定義されている外部ファンクションを実行することができます。この仕組みは、以前のバージョンのEpson RC+ から存在するもので、DLLには 32 ビットネイティブでなければならないという制約があります。

Extensionの開発キットでは、ブリッジ DLLを提供し、SPEL+ プログラムが、64 ビットアセンブリであるExtensionの内部関数を呼び出せる仕組みを用意しました。これがExtensionの「外部ファンクション」拡張ポイントです。

この「外部ファンクション」の呼び出しは、以下の形式となります。

- SPEL+ プログラムに、以下の Declare ステートメントを挿入する。
    ```
    Declare CallExternal, "C:\EpsonRC80\ExternalFunctionBridge.dll", "CallExternal",(commandLine$ As String, ByRef output$ As String) As Int32
    ```
  - commandLine$ は、ファンクション名と引数を空白で区切って与える、コマンド行を格納した文字列
  - output$は、ファンクションの出力を格納する文字列変数
  - 戻り値は、ファンクション実行の結果コード
    - 0 は、正常終了(成功)
    - 非 0 は、エラー
  - (例)
    ```
    Int32 ret
    String output$
    ret = CallExternal("CubeRoot 10.0", ByRef output$)
    Print output$
    ```
    - Run ウィンドウに 2.154434690031884 と出力されます。

この拡張ポイントを使うには、デリゲート RCXExternalFunction を実装して、エクスポートし、かつメタデータとして、ファンクション名をエクスポートします。

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

この拡張ポイントにより、Extensionで実装可能なさまざまな関数を、SPEL+ プログラムから呼び出せるようになるでしょう。

なお、以下は、組み込みの「外部ファンクション」として、ExtensionをサポートしているエディションのEpson RC+ では常に利用できます。

1. 外部プログラム実行(終了を待って、出力の1行目を返します)
    ```
    ret = CallExternal("Execute program [arg(s)]", output$)
    ```
2. 外部プログラム起動(終了を待ちません)
    ```
    ret = CallExternal("ExecuteNoWait program [arg(s)]", output$)
    ```
3. CallExternalの結果コードに対応する文字列の取得
    ```
    ret = CallExternal("ErrorStr Str$(retCode)" output$)
    ```
    - 例
        ```
        Int32 ret
        String output$
        ret = CallExternal("ErrorStr 0", ByRef output$)
        Print output$
        ```
      - Run ウィンドウに Success と出力されます。

(上級者向け)

このほか、ブリッジ DLL には、外部ファンクションの動作を制御するための関数があります(CallExternalと同様なDeclare ステートメントが必要です)。

1. GetTimeout(ByRef timeout As Int32) As Int32
    - CallExternalのタイムアウト時間を取得します。単位はミリ秒で、初期値は 30,000 です。
2. SetTimeout(timeout As Int32) As Int32
    - CallExternalのタイムアウト時間を設定します。単位はミリ秒です。

## 4. PCビジョン カスタムビジョンオブジェクトの開発

### 4.1 API解説

APIは4つの関数から構成されています。  
以下の説明を参考に実装を行ってください。  
より詳細な内容はAPIリファレンスを参照ください。  
<https://epson-robots.github.io/rcplus-extensions>

#### 4.1.1 CVOGetAPIVersion

この関数はパッケージを読み込むときにAPIのバージョンを確認するためにコールされます。この関数の実装は変更せずこのまま利用ください。

```c
CVOAPI int32_t CVOGetAPIVersion()
{
    return CVO_API_VERSION;
}
```

#### 4.1.2 CVOGetProfile

この関数はEpson RC+起動時にコールされます。あなたのカスタムビジョンオブジェクトのプロファイル情報をCVO_PROFILE構造体に設定して返却ください。  
それぞれの設定は、APIリファレンスを参照してください。Properties、ResultPropertiesは、カンマ区切りでプロパティを文字列で指定しますが、設定可能なプロパティについては「4.1.5 プロパティ」の章を参照してください。

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

この関数はオブジェクトのティーチでコールされます。ティーチが不要であれば実装は不要です。

```c
CVOAPI int32_t CVOTeach(CVO_PARAMS* inParams, CVO_PARAMS* outParams, CVO_IMG* img, CVO_MODEL* mdl)
{
    return CVO_NOERROR;
}
```

CVO_IMGにモデルウィンドウの画像情報を渡します。モデル情報として利用ください。ティーチしたモデル情報をEpson RC+側で管理することも可能です。Epson RC+側で管理させたい場合はCVO_MODELにバイナリ情報を返却ください。

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

この関数はオブジェクトが実行されるときにコールされます。サーチウィンドウの画像情報がCVO_IMGで渡されます。画像変換を行う場合は、このCVO_IMGの情報を書き替えてください。

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

検出位置情報を提供する場合は、CVO_DETECT_ITEMSに情報を設定してください。

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

#### 4.1.5 プロパティ

Vision Guide画面で表示できるプロパティは、カスタムビジョンオブジェクトのプロファイル情報の設定状況によって変化します。以下を確認ください。

##### 4.1.5.1 プロパティ

|Property|Extension Accessible|Display Always|Display DetectItems|Display HasTeach|Display by Extension Profile Settings|
|-|:-:|:-:|:-:|:-:|:-:|
|AbortSeqOnFail| |X| | | |
|Accept|X| | | |X|
|Caption| |X| | | |
|CurrentResult| | |X| | |
|Description|X|X| | | |
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
|NumberToFind|X| |X| | |
|PassColor| |X| | | |
|PassType| | |X| | |
|SearchWinCenterX| |X| | | |
|SearchWinCenterY| |X| | | |
|SearchWinHeight| |X| | | |
|SearchWinLeft| |X| | | |
|SearchWinTop| |X| | | |
|SearchWinWidth| |X| | | |
|Sort| | |X| | |
|ThresholdHigh|X| | | |X|
|ThresholdLow|X| | | |X|

プロファイル情報のPropertiesで設定できるプロパティは以下です。

- Accept
- ThresholdHigh
- ThresholdLow

##### 4.1.5.2 リザルトプロパティ

|Property|Extension Accessible|Display Always|Display DetectItems|Display HasTeach|Display by Extension Profile Settings|
|-|:-:|:-:|:-:|:-:|:-:|
|Angle|X| |X *1| |X|
|CameraX| | |X| | |
|CameraY| | |X| | |
|Found| | |X| | |
|MaxX|X| |X *1| |X|
|MaxY|X| |X *1| |X|
|MinX|X| |X *1| |X|
|MinY|X| |X *1| |X|
|NumberFound| | |X| | |
|Passed| |X| | | |
|PixelX|X| |X| | |
|PixelY|X| |X| | |
|RobotX| | |X| | |
|RobotY| | |X| | |
|RobotU| | |X| | |
|Scale|X| |X *1| |X|
|Score|X| | | |X|
|Text|X| | | |X|
|Time| |X| | | |

*1 DetectItemsとResultProperties両方の設定がされている場合に表示されます。

プロファイル情報のResultProperties設定できるプロパティ情報は以下です。

- Angle
- MaxX/MaxY/MinX/MinY
- Scale
- Score
- Text
