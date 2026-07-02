# ウェブカメラレコーダー

Rev.1  
JAM266S8779F  

[日本語](./readme_ja.md) / [English](./readme.md)  

本readmeでは、エプソンが配信/提供している「ウェブカメラレコーダー」の実際のソースコードを題材に実装内容を解説します。  
RC+ Extensions のテンプレートからプロジェクトを作成し、機能を構築していく過程に沿って、主要なクラスの役割やExtensions API の利用方法を説明していきます。

記載しているコードは一部を抜粋したものです。  
全体の構成や完全な実装については、samples フォルダ内のソースコード一式をあわせて参照してください。

## 1. 概要

PCに繋ぐことができる周辺機器の一つである、ウェブカメラを活用するRC+ Extensionを作成してみましょう。

まず、Epson RC+ のドッキングウィンドウで、ウェブカメラの画像をプレビューできるようにします(初級編)。  
続いて、画像の録画機能を追加します(中級編)。  
SPEL+ プログラムの開始とともに録画を開始し、終了とともに録画を終了します。プログラムが長時間動くことを想定し、5秒ごとに新しいファイルを作成して録画し、最新の2ファイルだけを残します。

これは、車のドライブレコーダーと同じような機能を、システムに追加しようとするものです。装置立ち上げの過程などで、ロボットによる作業を監視しておくと、プログラムが予期せずに停止した場合に、録画された映像から何が起こったのかを、後から目視確認することが可能になります。

PCがネットワークに接続されている場合には、録画データとともに、異常を知らせる通知を送信する等の応用もできるでしょう。

それでは始めましょう。

## 2. 実装解説

### 2.1 初級編

1. RC+ Extensions の新しいプロジェクトを作成します。
    - 名前は、WebCamRecorder とします。
    - 初期機能は、**Main menu and tool bar item** および **Docking window** をチェックします。
    - ARM64 版 Windowsでは、構成を x64 にしてください。

2. ビルド、デバッグして、動作することを確認します。
    - メニュー項目に `WebCamRecorder (xx)`(xx は表示言語名)が追加され、メニュー項目を選択して、ドッキングウィンドウが表示されればOKです。

3. 一旦、Epson RC+ を終了します。

4. Visual Studio のソリューションエクスプローラーで、WebCamRecorder プロジェクトをダブルクリックし、以下の変更を行います。
    - TargetFramework を、net8.0-windows10.0.19041.0に変更します。
        - これにより、Windows Media FoundationのAPIが使えるようになります。以下の変更も、これに関連するものです。Windows Media Foundation は、DirectShow の後継として、Windows Vista 以降の OS に標準搭載されている COM ベースの API セットです。現時点では、.NET の標準ライブラリには取り込まれていませんが、次に述べる Microsoft.Windows.CsWin32 などを用いることで、標準ライブラリと同様に利用できます。
    - `<EnableWindowsTargeting>true</EnableWindowsTargeting>` の行を追加します。
    - `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` の行を追加します。

5. 「ツール」> 「NuGet パッケージマネージャー」 > 「ソリューションの NuGet パッケージの管理」を選択し、画面を開きます。
    - 「参照」タブから、Microsoft.Windows.CsWin32 パッケージを探し、最新の安定版(執筆時点で 0.3.264)をインストールします。
        - Microsoft.Windows.CsWin32 は、C# から Windows API を簡単に呼び出すためのライブラリです。  
        詳細は、<https://github.com/microsoft/CsWin32> をご覧ください。

6. プロジェクトに、NativeMethods.txt および NativeMethods.json ファイルを追加します。
    - これらは、Microsoft.Windows.CsWin32 を使って Windows API を利用するために必要なファイルです。
    - NativeMethods.txt

        ```txt
        MFStartup
        MFShutdown
        MFCreateAttributes
        MFEnumDeviceSources
        MFCreateSourceReaderFromMediaSource
        MFCreateMediaType
        MFCreateSinkWriterFromURL
        MFCreateSample
        MFCreateAlignedMemoryBuffer

        (中略)

        CoInitializeEx
        CoTaskMemFree

        COINIT
        ```

    * NativeMethods.json

        ```JSON
        {
            "$schema": "https://aka.ms/CsWin32.schema.json",
            "public": true
        }
        ```

7. プロジェクトに、以下のファイルを追加します。
    - CameraInfo.cs
        - この Extension で、カメラを表すクラス CameraInfo を記述しているファイルです。

        ```C#
        (前略)

        namespace WebCamRecorder
        {
            /// <summary>
            /// Camera information
            /// </summary>
            public class CameraInfo
            {
                /// <summary>
                /// Friendly name (may not be unique)
                /// </summary>
                public string FriendlyName { get; }

                /// <summary>
                /// Unique symbolic link
                /// </summary>
                public string SymbolicLink { get; }

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="friendlyName">Friendly name</param>
                /// <param name="symbolicLink">Symbolic link</param>
                public CameraInfo(
                    string friendlyName,
                    string symbolicLink
                )
                {
                    FriendlyName = friendlyName;
                    SymbolicLink = symbolicLink;
                }
            }
        }
        ```

    - CameraInfoCollection.cs
        - カメラの一覧を表し、指定したカメラのメディアソース(Windows Media Foundation で、データ処理の入り口となるオブジェクト)を取得するためのクラス CameraInfoCollection を記述しているファイルです。

        ```C#
        (前略)

        namespace WebCamRecorder
        {
            (中略)

            /// <summary>
            /// Camera collection object
            /// </summary>
            public sealed class CameraInfoCollection : IDisposable
            {
                /// <summary>
                /// Camera information
                /// </summary>
                public List<CameraInfo> CameraInfos = [];

                /// <summary>
                /// Source activates
                /// </summary>
                private unsafe IMFActivate_unmanaged** _sourceActivates;

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="sourceActivates">Source activates</param>
                public unsafe CameraInfoCollection(
                    IMFActivate_unmanaged** sourceActivates
                )
                {
                    _sourceActivates = sourceActivates;
                }

                /// <summary>
                /// Create media source for the specified camera
                /// </summary>
                /// <param name="cameraInfo">Selected camera</param>
                /// <returns>Media source object</returns>
                public unsafe IMFMediaSource? GetMediaSource(
                    CameraInfo cameraInfo
                )
                {
                    var index = CameraInfos.FindIndex(
                        (x) => (
                            x != null
                            && x.FriendlyName == cameraInfo.FriendlyName
                            && x.SymbolicLink == cameraInfo.SymbolicLink
                        )
                    );
                    if (index < 0)
                    {
                        return null;
                    }
                    else
                    {
                        if (Marshal.GetObjectForIUnknown((nint)_sourceActivates[index]) is not IMFActivate managedSourceActivate)
                        {
                            return null;
                        }

                        var mediaSource = managedSourceActivate.ActivateObject(typeof(IMFMediaSource).GUID) as IMFMediaSource;

                        Marshal.ReleaseComObject(managedSourceActivate);

                        return mediaSource;
                    }
                }

                (後略)
        ```

    - IFrameProcessor.cs
        - カメラから取得した画像を処理するためのインターフェイス IFrameProcessor を記述するファイルです。

        ```C#
        (前略)

        namespace WebCamRecorder
        {
            /// <summary>
            /// Frame processor interface
            /// </summary>
            public interface IFrameProcessor
            {
                /// <summary>
                /// Initialize the processor
                /// </summary>
                /// <param name="width">Frame width</param>
                /// <param name="height">Frame height</param>
                /// <param name="stride">Frame stride</param>
                /// <param name="bitRate">Bit rate</param>
                public void Initialize(
                    uint width,
                    uint height,
                    uint stride,
                    uint bitRate
                );

                /// <summary>
                /// Termniate the processor
                /// </summary>
                public void Terminate();

                /// <summary>
                /// Process the frame
                /// </summary>
                /// <param name="frame">Frame data</param>
                /// <param name="duration">Duration</param>
                public void Process(
                    byte[] frame,
                    long duration
                );

                /// <summary>
                /// Request stopping
                /// </summary>
                public void Stop();

                /// <summary>
                /// The processing is currently stopping or not
                /// </summary>
                public bool IsStopped { get; }
            }
        }
        ```

    - CameraManager.cs
        - 撮像を行い、画像処理器(上記 IFrameProcessor を実装するクラスのインスタンス)に順に画像データを渡す CameraManager クラスを記述するファイルです。

        ```C#
        (前略)

        namespace WebCamRecorder
        {
            (中略)

            /// <summary>
            /// Camera manager
            /// </summary>
            public class CameraManager
            {
                /// <summary>
                /// List of frame processors
                /// </summary>
                public List<IFrameProcessor> FrameProcessors { get; } = [];

                (中略)

                /// <summary>
                /// List available cameras
                /// </summary>
                /// <returns>Collection object</returns>
                public unsafe CameraInfoCollection? ListCameras()
                {

                (中略)

                /// <summary>
                /// Read a frame from source
                /// </summary>
                /// <param name="sourceReader">Source reader object</param>
                /// <param name="frame">Buffer</param>
                /// <param name="duration">Variable to get duration</param>
                /// <returns>1: got it, 0: not got, -1: error</returns>
                private static unsafe int ReadFrame(
                    IMFSourceReader sourceReader,
                    byte[] frame,
                    out long duration
                )
                {
                (中略)

                /// <summary>
                /// Start the processings
                /// </summary>
                /// <param name="cameraInfo">Selected camera</param>
                /// <returns>Task</returns>
                public async Task Start(
                    CameraInfo cameraInfo
                )
                {
                    while (!_done)
                    {
                        const int _waitMSec = 10;

                        await Task.Delay(_waitMSec);
                    }

                    await Task.Run(() =>
                    {
                        HRESULT hr;

                        hr = PInvoke.CoInitializeEx(COINIT.COINIT_MULTITHREADED);
                        if (hr.Failed)
                        {
                            return;
                        }

                        hr = PInvoke.MFStartup(PInvoke.MF_VERSION, PInvoke.MFSTARTUP_FULL);
                        if (hr.Succeeded)
                        {
                            _done = false;

                            var sourceReader = CreateSourceReader(cameraInfo);
                            if (sourceReader != null)
                            {
                                GetVideoInfos(
                                    sourceReader,
                                    out var width,
                                    out var height,
                                    out var stride,
                                    out var bitRate
                                );

                                var frame = new byte[stride * height];

                                foreach (var frameProcessor in FrameProcessors)
                                {
                                    frameProcessor.Initialize(width, height, stride, bitRate);
                                }

                                _stopping = false;
                                while (true)
                                {
                                    var status = ReadFrame(sourceReader, frame, out var duration);
                                    if (status < 0)
                                    {
                                        break;
                                    }
                                    else if (status > 0)
                                    {
                                        foreach (var frameProssor in FrameProcessors)
                                        {
                                            frameProssor.Process(frame, duration);
                                        }
                                    }

                                    if (_stopping && FrameProcessors.All(x => x.IsStopped))
                                    {
                                        break;
                                    }
                                }

                                foreach (var frameProcessor in FrameProcessors)
                                {
                                    frameProcessor.Terminate();
                                }

                                Marshal.ReleaseComObject(sourceReader);
                            }

                            _ = PInvoke.MFShutdown();

                            _done = true;
                            _stoppedAction?.Invoke();
                        }
                    });
                }

                (後略)
        ```

    - Previewer.cs
        - カメラ画像のプレビューを行うためのクラス Previewer を記述するファイルです。
            - PreviewImage が、ウィンドウに画像を表示するための Image コントロールです。初期化時に、ビットマップデータを作成して、PreviewImage の Source に設定し、以降、CameraManager から渡された画像データを、ビットマップに書き込んでいます。

        ```C#
        (前略)

        namespace WebCamRecorder  
        {
            (中略)

            /// <summary>
            /// Image previewer for the camera
            /// </summary>
            public class Previewer : IFrameProcessor
            {
                /// <summary>
                /// Image control
                /// </summary>
                public Image? PreviewImage;

                /// <summary>
                /// Bitmap
                /// </summary>
                private WriteableBitmap? _bitmap;

                (中略)

                /// <inheritdoc />
                public void Initialize(
                    uint width,
                    uint height,
                    uint stride,
                    uint bitRate
                )
                {
                    _width = (int)width;
                    _height = (int)height;
                    _stride = (int)stride;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _bitmap = new(
                            _width, _height,
                            96, 96,
                            PixelFormats.Bgr32,
                            null
                        );

                        if (PreviewImage != null)
                        {
                            PreviewImage.Source = _bitmap;
                        }
                    });
                }

                (中略)

                /// <inheritdoc />
                public void Process(
                    byte[] frame,
                    long duration
                )
                {
                    if (_bitmap == null)
                    {
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _bitmap.WritePixels(
                            new Int32Rect(0, 0, _width, _height),
                            frame,
                            _stride,
                            0
                        );
                    });
                }

                (後略)
        ```

8. DockingWindow フォルダの DockingWindowContent.xaml ファイルを編集します。
    - 画像がウィンドウより大きい場合を考慮し、ScrollViewer を配置して、その中に DockPanel を移動します。
    - 元からある TextBlock と Grid は削除します。
    - Label と ComboBox を持つ StackPanel を追加します。
        - この Extension では、ComboBox のドロップダウンメニューを表示させるタイミングで、(もし行われていれば)撮像処理を中止し、接続されているカメラの一覧を取得します。ドロップダウンメニューからカメラが選択されたら、撮像処理を開始します。これらの処理は、後ほど記述しますが、以下のプロパティやコマンドをビューモデル追加する想定で、これらを該当箇所にバインドしておきます。
            - カメラ一覧を表す Cameras(`ReactiveCollection<CameraInfo>`)
            - 選択されているカメラの、カメラ一覧でのインデックスを表す SelectedCameraIndex(`ReactivePropertySlim<int>`)
            - カメラ一覧を(再)取得するコマンド RefreshCamerasCommand(ReactiveCommand)
        - Label の Content に注目してください。Captions[CaptionCamera].Value にバインドしています。 Extension では、Epson RC+ の表示言語に従ってローカライズしたい文字列を Captions.xlsx に記述します。詳細は後述しますが、Captions.xlsx の symbol 列で定義した名前(この場合は CaptionCamera)を使って、同様にバインドすることで、Epson RC+ の表示言語に従ったローカライズを行うことができます。
    - PreviewImage と名付けた Image を追加します。

        ```XML
        <UserControl x:Class="WebCamRecorder.DockingWindow.DockingWindowContent"
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                    xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
                    xmlns:local="clr-namespace:WebCamRecorder.DockingWindow"
                    mc:Ignorable="d"
                    d:DesignHeight="450" d:DesignWidth="800">

            <UserControl.DataContext>
                <local:DockingWindowContentViewModel />
            </UserControl.DataContext>

            <ScrollViewer
                VerticalScrollBarVisibility="Auto"
                HorizontalScrollBarVisibility="Auto">

                <DockPanel
                    Background="White"
                    LastChildFill="True">

                    <StackPanel
                        DockPanel.Dock="Top"
                        Orientation="Horizontal"
                        Margin="10">

                        <Label
                            Content="{Binding Captions[CaptionCamera].Value}" />
                        <ComboBox
                            ItemsSource="{Binding Cameras}"
                            SelectedIndex="{Binding SelectedCameraIndex.Value}"
                            IsReadOnly="True"
                            DisplayMemberPath="FriendlyName"
                            MinWidth="200"
                            Margin="10,0,0,0">
                            <i:Interaction.Triggers>
                                <i:EventTrigger
                                    EventName="DropDownOpened">
                                    <i:InvokeCommandAction
                                        Command="{Binding RefreshCamerasCommand}" />
                                </i:EventTrigger>
                            </i:Interaction.Triggers>
                        </ComboBox>

                    </StackPanel>

                    <Image
                        x:Name="PreviewImage"
                        Width="640"
                        Height="480"
                        Stretch="UniformToFill"
                        HorizontalAlignment="Left" />

                </DockPanel>
            </ScrollViewer>

        </UserControl>
        ```

9. DockingWindow フォルダの DockingWindowContentViewModelAddition.cs を編集します。
    - DockingWindow フォルダには、DockingWindowContentViewModel.cs ファイルも存在し、これら 2 つのファイルで DockingWindowContentViewModel クラスを記述します。
        - DockingWindowContentViewModel.cs には、クローズや保存などの他、コピーやカット、ペーストなどのコンテンツ編集用のメソッドがあり、必要に応じて処理を記述します。
            - この Extension では、ウィンドウクローズ時に、撮像処理を停止する処理だけを加えます。

                ```C#
                (前略)

                /// <inheritdoc />
                public Task<bool> CloseAsync()
                {
                    _cameraManager.Stop();

                    return Task.FromResult(true);
                }

                (後略)
                ```

        - DockingWindowContentViewModelAddition.cs には、ビューモデルのコンストラクタと、ウィンドウ生成後に一度だけ呼ばれる WindowCreated メソッドがあります。ウィンドウ独自のプロパティやコマンド、およびそれらに関係する API の呼び出しは、このファイルにまとめることで、ビューモデル全体を見通しよく記述できます。

            ```C#
            (前略)

            namespace WebCamRecorder.DockingWindow
            {
                (中略)

                /// <summary>
                /// Extension : Docking Window (Specific Part)
                /// </summary>
                internal partial class DockingWindowContentViewModel
                {
                    /// <summary>
                    /// Camera list
                    /// </summary>
                    public ReactiveCollection<CameraInfo> Cameras { get; } = [];

                    /// <summary>
                    /// Index of the selected camera
                    /// </summary>
                    public ReactivePropertySlim<int> SelectedCameraIndex { get; } = new(-1);

                    /// <summary>
                    /// Refresh camera list command
                    /// </summary>
                    public ReactiveCommand RefreshCamerasCommand { get; } = new();

                    (中略)

                    /// <summary>
                    /// Camera manager
                    /// </summary>
                    private readonly CameraManager _cameraManager = new();

                    /// <summary>
                    /// Previewer
                    /// </summary>
                    private readonly Previewer _previewer = new();

                    (中略)

                    /// <summary>
                    /// Refresh camera list
                    /// </summary>
                    private void OnRefreshCameras()
                    {
                        SelectedCameraIndex.Value = -1;

                        Cameras.Clear();
                        var cameraInfoCollection = _cameraManager.ListCameras();
                        if (cameraInfoCollection != null)
                        {
                            foreach (var cameraInfo in cameraInfoCollection.CameraInfos)
                            {
                                Cameras.Add(cameraInfo);
                            }
                            cameraInfoCollection.Dispose();
                        }
                    }

                    /// <summary>
                    /// Change camera
                    /// </summary>
                    /// <param name="index">The index of the selected camera</param>
                    /// <returns>Task</returns>
                    private async Task OnSelectedCameraChanged(
                        int index
                    )
                    {
                        _cameraManager.Stop();

                        if (index >= 0)
                        {
                            await _cameraManager.Start(Cameras[index]);
                        }
                    }

                    /// <summary>
                    /// Set image control for previewer
                    /// </summary>
                    /// <param name="previewImage">Image control for previewing</param>
                    public void SetPreviewImage(
                        Image previewImage
                    )
                    {
                        _previewer.PreviewImage = previewImage;
                    }

                    /// <summary>
                    /// Constructor
                    /// </summary>
                    public DockingWindowContentViewModel()
                    {
                        _cameraManager.FrameProcessors.Add(_previewer);

                        RefreshCamerasCommand.Subscribe(OnRefreshCameras).AddTo(_disposables);

                        SelectedCameraIndex.Subscribe(async (index) =>
                        {
                            await OnSelectedCameraChanged(index);
                        })
                        .AddTo(_disposables);
                    }

                    (後略)
            ```

10. DockinwWindow フォルダの DockingWindowContent.xaml.cs ファイルを編集します。
    - プレビュー用の Image コントロールへの参照を、ビューモデルに渡します。

        ```C#
        (前略)

        if (DataContext is DockingWindowContentViewModel viewModel)
        {
            viewModel.SetPreviewImage(PreviewImage);
        }

        (後略)
        ```

11. Captions.xlsx ファイルを開いて編集します。
    - 前述したように、Epson RC+ の表示言語に従ってローカライズしたい文字列は、このファイルに記述します。  
        ![編集イメージ](images/WebCamRecorder_B_Captions.xlsx.png)
        - ID は、キャプション番号です。このファイル内で重複しないように採番してください。
        - description は、コメントです。自由に書くことができます。
        - symbol は、Extension のソースコード(.xaml、.cs)で参照するための名前です。Captions.xlsx ファイルを編集してプロジェクトをビルドすると、symbol と ID を紐づけた定数定義が Captions.cs ファイルとして生成されます。このファイルは直接編集しないでください。

            ```C#
            // <auto-generated>

            namespace WebCamRecorder
            {
                using System.Reflection;

                internal class Constants
                {
                    internal class Caption
                    {
                        (中略)

                        public const int ExtensionName = 0;
                        public const int MainMenu = 1;
                        public const int WindowTitle = 400;
                        public const int CaptionCamera = 401;
                    }
                }
            }
            ```

        - English、Japanese、… の各列には、その言語で表示したい文字列を記述します。

12. ビルド、デバッグします。
    - PC にウェブカメラを接続して、WebCamRecorder のウィンドウを表示し、カメラを選択して、画像が表示されれば成功です。

### 2.2 中級編

初級編では、生成されたソリューションに使われているものを除いて、Extensions API を利用していません。

中級編では、次の機能を提供する Extensions API を使ってみます。

- 開かれているプロジェクトの、プロジェクトフォルダのパス名を取得する(プロジェクト API)。
- SPEL+ のタスク一覧を取得する(プログラム実行 API)。

.NET の豊富なライブラリや Windows API を活用しつつ、必要な Extensions API を使うことで、Epson RC+ や SPEL+ プログラムと緊密に連携する独自のアプリケーションを Extension として作成し、利用することができます。

それでは、進めましょう。

1. Epson RC+ が起動していたら終了させ、Visual Studio を起動して、初級編で作成したソリューションを開きます。

2. Recorder.cs を追加します。
    - Recorder クラスは、Previewer クラスと同様に、IFrameProcessor を実装しています。Recorder では、CameraManager から渡された画像データから、H.264 形式の動画ファイルを生成します。
    - 動画ファイルは、およそ 5 秒ごとに、Recorder インスタンスにパスを設定したフォルダの下に、`Video_N.mp4`(N は、000 〜 999 で、999 に達した後は 000 に戻ります)という名前で新たに作成されますが、PC のストレージを圧迫しないよう、最新の 2 ファイルだけを残すことにします。
        - この Extension では、新たに録画を開始するときは、フォルダ内の動画はすべて削除します。
    - また、ドライブレコーダー的な動作をさせるために、録画の停止を指示してからも、そこから 2 秒間は録画を続けるようにします。(つまり、最後に保存される動画は、最大 7 秒程度となります。)
    - 録画は、SPEL+ プログラムの開始・終了に連動するモード(Auto)の他に、任意のタイミングで開始・停止できるモード(Manual)も用意することとします。
    - カメラ画像のプレビューには、特に明示的な開始が必要なかったこともあり(カメラを選んだ時点で開始します)、IFrameProcessor には、停止を指示する Stop メソッドしか用意されていません。  
    このため、Recorder には、上記の録画モードを Mode プロパティとして持たせ、Mode の設定で録画が開始されるようにします。  
    さらに、Mode の変更で PropertyChanged イベントを発生するようにして、録画が実際に停止したタイミングもプログラムで捕まえられるようにします。

        ```C#
        (前略)

        namespace WebCamRecorder
        {
            (中略)

            /// <summary>
            /// Recorder
            /// </summary>
            public class Recorder : IFrameProcessor, INotifyPropertyChanged
            {
                /// <summary>
                /// Recording mode definitions
                /// </summary>
                public enum RecordingMode
                {
                    Stop,
                    Auto,
                    Manual,
                }

                /// <inheritdoc />
                public event PropertyChangedEventHandler? PropertyChanged;

                /// <summary>
                /// Recording mode
                /// </summary>
                public RecordingMode Mode
                {
                    get
                    {
                        return _mode;
                    }
                    set
                    {
                        if (_sinkWriter == null)
                        {
                            _shouldStop = false;

                            _mode = value;
                            RaisePropertyChanged();
                        }
                    }
                }

                /// <summary>
                /// Folder for video files
                /// </summary>
                public string VideoFolder
                {
                    get
                    {
                        return _videoFolder;
                    }
                    set
                    {
                        if (_sinkWriter == null)
                        {
                            try
                            {
                                Directory.CreateDirectory(value);
                                _videoFolder = value;
                            }
                            catch (Exception)
                            {
                                // EMPTY
                            }
                        }
                    }
                }

                (中略)

                /// <inheritdoc />
                public void Process(
                    byte[] frame,
                    long duration
                )
                {
                    if (_sinkWriter == null)
                    {
                        if (_mode == RecordingMode.Stop)
                        {
                            return;
                        }

                        _sinkWriter = CreateSinkWriter(GetNextSegmentFile());

                        _recordTime = 0;
                        _segmentSpan = _initialSegmentSpan;
                    }

                    if (_sinkWriter != null && _sample != null)
                    {
                        SetFlippedFrame(frame);

                        _sample.SetSampleTime(_recordTime);
                        _sample.SetSampleDuration(duration);

                        _sinkWriter.WriteSample(_streamIndex, _sample);

                        _recordTime += duration;
                        if (_recordTime > _segmentSpan)
                        {
                            _sinkWriter.Finalize();
                            Marshal.ReleaseComObject(_sinkWriter);
                            _sinkWriter = null;

                            if (_shouldStop)
                            {
                                Mode = RecordingMode.Stop;
                            }
                        }
                    }
                }

                /// <inheritdoc />
                public void Stop()
                {
                    const long _minAdditionalTime = 20_000_000;

                    if (_segmentSpan - _recordTime < _minAdditionalTime)
                    {
                        _segmentSpan = _recordTime + _minAdditionalTime;
                    }

                    _shouldStop = true;
                }

                (後略)
        ```

3. DockingWindow フォルダの DockingWindowContent.xaml ファイルを編集します。
    - 画面に、録画中であることを示すインジケータの表示と、手動(Manual モード)で録画を開始、停止できるようにするボタンを追加します。
    - ビューモデルには、以下のプロパティとコマンドを後で追加します。
        - 録画中を示す IsRecording(`ReactivePropertySlim<bool>`)
        - 録画が開始可能であることを示す CanStartRecording(`ReactivePropertySlim<bool>`)と、録画開始を指示する StartRecordingCommand(`ReactiveCommand`)
        - 録画が停止可能であることを示す CanStopRecording(`ReactivePropertySlim<bool>`)と、録画停止を指示する StopRecordingCommand(`ReactiveCommand`)
        - この Extension では、Auto モードでの録画中は、手動での録画開始・停止はできないようにし、逆に Manual モードでの録画中は、Auto モードでの録画は機能させないようにします。
            - ただし、どちらの場合も、ドッキングウィンドウを閉じることで、録画停止となります。

        ```XML
        (前略)

        </ComboBox>
        <Border
            CornerRadius="10"
            Width="60"
            Height="20"
            Margin="20,0,0,0"
            VerticalAlignment="Center">
            <TextBlock
                Text="REC"
                HorizontalAlignment="Center"
                VerticalAlignment="Center">
                <TextBlock.Style>
                    <Style
                        TargetType="TextBlock">
                        <Style.Triggers>
                            <DataTrigger
                                Binding="{Binding IsRecording.Value}"
                                Value="True">
                                <Setter
                                    Property="Foreground"
                                    Value="White" />
                            </DataTrigger>
                            <DataTrigger
                                Binding="{Binding IsRecording.Value}"
                                Value="False">
                                <Setter
                                    Property="Foreground"
                                    Value="Black" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </TextBlock.Style>
            </TextBlock>
            <Border.Style>
                <Style
                    TargetType="Border">
                    <Style.Triggers>
                        <DataTrigger
                            Binding="{Binding IsRecording.Value}"
                            Value="True">
                            <Setter
                                Property="Background"
                                Value="Red" />
                        </DataTrigger>
                        <DataTrigger
                            Binding="{Binding IsRecording.Value}"
                            Value="False">
                            <Setter
                                Property="Background"
                                Value="LightGray" />
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </Border.Style>
        </Border>
        <Button
            Command="{Binding StartRecordingCommand}"
            IsEnabled="{Binding CanStartRecording.Value}"
            Content="{Binding Captions[LabelStart].Value}"
            Width="80"
            VerticalAlignment="Center"
            Margin="10,0,0,0" />
        <Button
            Command="{Binding StopRecordingCommand}"
            IsEnabled="{Binding CanStopRecording.Value}"
            Content="{Binding Captions[LabelStop].Value}"
            Width="80"
            VerticalAlignment="Center"
            Margin="10,0,0,0" />
        ```

4. DockingWindow フォルダの DockingWindowContentViewModelAddition.cs ファイルを編集します。
    - ビュー側(.xaml)でバインドしたプロパティやコマンドを追加します。
    - Recorder クラスのインスタンスも CameraManager に追加します。
    - WindowCreated メソッドにご注目ください。ここで、Extensions API の、プロジェクト API を使って、開かれているプロジェクトのプロジェクトフォルダ(のパス名)を取得しています。
        - API オブジェクトは、Main.GetAPI メソッドで取得します。
        - プロジェクト API オブジェクトの ProjectFolder プロパティは、開いているプロジェクトの、プロジェクトフォルダのパス名です。プロジェクトが開かれていないときは null です。
            - プロジェクトが開かれていなかった場合は、プロジェクトフォルダのかわりに、Windows のログインユーザーの「ビデオ」フォルダを使うことにします。
        - 録画ファイルは、プロジェクトフォルダまたはログインユーザーの「ビデオ」フォルダに、サブフォルダ WebCamRecorder を作成して、その中に保存するよう、Recorder に設定しています。

        ```C#
        (前略)
        /// <summary>
        /// Recording in progress or not
        /// </summary>
        public ReactivePropertySlim<bool> IsRecording { get; } = new(false);

        /// <summary>
        /// Can start recording or not
        /// </summary>
        public ReactivePropertySlim<bool> CanStartRecording { get; } = new(false);

        /// <summary>
        /// Start recording command
        /// </summary>
        public ReactiveCommand StartRecordingCommand { get; }

        /// <summary>
        /// Can stop recording or not
        /// </summary>
        public ReactivePropertySlim<bool> CanStopRecording { get; } = new(false);

        /// <summary>
        /// Stop recording command
        /// </summary>
        public ReactiveCommand StopRecordingCommand { get; }

        (中略)

        /// <summary>
        /// Recorder
        /// </summary>
        private readonly Recorder _recorder = new();

        (中略)

        /// <summary>
        /// Change camera
        /// </summary>
        /// <param name="index">The index of the selected camera</param>
        /// <returns>Task</returns>
        private async Task OnSelectedCameraChanged(
            int index
        )
        {
            EnableOrDisableRecordingCommands();

            _cameraManager.Stop();

            if (index >= 0)
            {
                await _cameraManager.Start(Cameras[index]);
            }
        }

        (中略)

        /// <summary>
        /// Update recording command possibilities
        /// </summary>
        private void EnableOrDisableRecordingCommands()
        {
            CanStartRecording.Value = (SelectedCameraIndex.Value >= 0 && _recorder.Mode == Recorder.RecordingMode.Stop);
            CanStopRecording.Value = (_recorder.Mode == Recorder.RecordingMode.Manual);
        }
        /// <summary>
        /// Start recording
        /// </summary>
        private void OnStartRecording(
            bool isAuto
        )
        {
            if (_recorder.Mode == Recorder.RecordingMode.Stop)
            {
                try
                {
                    var files = Directory.EnumerateFiles(
                        _recorder.VideoFolder,
                        $"*{Recorder.VideoFileExtension}"
                    );
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    // IGNORE
                }
                _recorder.Mode = isAuto ? Recorder.RecordingMode.Auto : Recorder.RecordingMode.Manual;

                EnableOrDisableRecordingCommands();
            }
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        private void OnStopRecording()
        {
            _recorder.Stop();

            CanStopRecording.Value = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DockingWindowContentViewModel()
        {
            _cameraManager.FrameProcessors.Add(_previewer);
            _cameraManager.FrameProcessors.Add(_recorder);

            (中略)

            _recorder.PropertyChanged += (_, _) =>
            {
                IsRecording.Value = (_recorder.Mode != Recorder.RecordingMode.Stop);
                EnableOrDisableRecordingCommands();
            };

            (中略)

        }

        /// <inheritdoc />
        public Task WindowCreated()
        {
            string videoFolder;

            var projectAPI = Main.GetAPI<IRCXProjectAPI>();
            if (projectAPI != null && projectAPI.ProjectFolder != null)
            {
                videoFolder = projectAPI.ProjectFolder;
            }
            else
            {
                videoFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);    
            }
            _recorder.VideoFolder = Path.Combine(videoFolder, "WebCamRecorder");

            (後略)
        ```

5. ビルド、デバッグします。
    - ドッキングウィンドウを開いて、カメラを選択し、録画開始と停止を試してください。
    ![ウィンドウ](images/WebCamRecorder_Window.png)
    - 所定のフォルダー\*に、動画ファイルが保存されており、再生できれば成功です。  
        \*: プロジェクトを開いている場合: プロジェクトフォルダ内の「WebCamRecorder」フォルダ  
        プロジェクトを開いていない場合: Windows ログインユーザーの「ビデオ」フォルダ
6. DockingWindow フォルダの DockingWindowContentViewModelAddition.cs ファイルを編集します。
    - Auto モードでの録画機能を追加します。このためには、Extension で、SPEL+ プログラムの実行開始と、実行終了のタイミングを捉える必要があります。
        - SPEL+ プログラムは、マルチタスクです。この Extension では、SPEL+ プログラムの開始・終了は、通常タスクの開始・終了であるとします。
        - タスクの一覧は、プログラム実行 API オブジェクトの Tasks プロパティで取得します。
            - Tasks は `IEnumerable<IRCXTask>` 型のコレクションで、IRCXTask インスタンスは、タスク状態を示す State と、タスク種別を示す Kind プロパティを持っています。
            - SPEL+ のタスクに何らかの変更があると、Tasks プロパティの PropertyChanged イベントが発生するようになっています。
    - WindowCreated メソッドに、実行中の通常タスクの有無を監視して、`ReactivePropertySlim<bool>` の _isProgramRunning を更新する以下のコードを追加します。

        ```C#
        (前略)

        /// <summary>
        /// Program execution API object
        /// </summary>
        private IRCXProgramExecutionAPI? _programExecutionAPI;

        /// <summary>
        /// Program running state
        /// </summary>
        private readonly ReactivePropertySlim<bool> _isProgramRunning = new(false, ReactivePropertyMode.DistinctUntilChanged);

        (中略)

        /// <inheritdoc />
        public Task WindowCreated()
        {
            (中略)

            _programExecutionAPI = Main.GetAPI<IRCXProgramExecutionAPI>();

            _programExecutionAPI?.ObserveProperty(x => x.Tasks).Subscribe((tasks) =>
            {
                _isProgramRunning.Value = tasks
                .Any(
                    x => (
                        x.Kind == IRCXProgramExecutionAPI.IRCXTask.RCXTaskKind.Normal
                        && x.State == IRCXProgramExecutionAPI.IRCXTask.RCXTaskState.Run
                    )
                );
            })
            .AddTo(_disposables);

        (後略)
        ```

    - さらに、上記で更新される _isProgramRunning プロパティの変更を検知して、録画の開始、終了を呼び出すコードをコンストラクタに加えます。

        ```C#
        (前略)

        /// <summary>
        /// Constructor
        /// </summary>
        public DockingWindowContentViewModel()
        {

        (中略)
            _isProgramRunning.Subscribe((isRunning) =>
            {
                if (SelectedCameraIndex.Value >= 0)
                {
                    if (isRunning)
                    {
                        OnStartRecording(isAuto: true);
                    }
                    else
                    {
                        OnStopRecording();
                    }

                    EnableOrDisableRecordingCommands();
                }
            })
            .AddTo(_disposables);
        }

        (後略)
        ```

7. ビルド、デバッグします。
    - Extension のウィンドウを開いて、カメラを選択し、プレビュー画像が表示されるのを確認します。
    - Run ウィンドウを開いてプログラムを実行します。
        - プログラムの開始とともに録画が開始され、終了の約 2 秒後に録画が停止され、所定のフォルダに動画ファイルが保存されていれば成功です。
