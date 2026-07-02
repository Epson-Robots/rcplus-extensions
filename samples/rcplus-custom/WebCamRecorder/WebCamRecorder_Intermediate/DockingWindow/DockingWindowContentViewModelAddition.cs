// -----------------------------------------------------------------------
// <copyright file="ContentViewModelAddition.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder.DockingWindow
{
    using Epson.RoboticsShared.ExtensionsAPI;
    using Reactive.Bindings;
    using Reactive.Bindings.Extensions;
    using System.IO;
    using System.Threading.Tasks;
    using Image = System.Windows.Controls.Image;

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

        /// <summary>
        /// Camera manager
        /// </summary>
        private readonly CameraManager _cameraManager = new();

        /// <summary>
        /// Previewer
        /// </summary>
        private readonly Previewer _previewer = new();

        /// <summary>
        /// Recorder
        /// </summary>
        private readonly Recorder _recorder = new();

        /// <summary>
        /// Program execution API object
        /// </summary>
        private IRCXProgramExecutionAPI? _programExecutionAPI;

        /// <summary>
        /// Program running state
        /// </summary>
        private readonly ReactivePropertySlim<bool> _isProgramRunning = new(false, ReactivePropertyMode.DistinctUntilChanged);

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
            EnableOrDisableRecordingCommands();

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
                    _recorder.VideoFolder = GetVideoFolder();
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

            RefreshCamerasCommand.Subscribe(OnRefreshCameras).AddTo(_disposables);

            SelectedCameraIndex.Subscribe(async (index) =>
            {
                await OnSelectedCameraChanged(index);
            })
            .AddTo(_disposables);

            StartRecordingCommand = CanStartRecording
            .ToReactiveCommand()
            .WithSubscribe(() => OnStartRecording(isAuto: false))
            .AddTo(_disposables);

            StopRecordingCommand = CanStopRecording
            .ToReactiveCommand()
            .WithSubscribe(OnStopRecording)
            .AddTo(_disposables);

            _recorder.PropertyChanged += (_, _) =>
            {
                IsRecording.Value = (_recorder.Mode != Recorder.RecordingMode.Stop);
                EnableOrDisableRecordingCommands();
            };

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

        /// <inheritdoc />
        public Task WindowCreated()
        {
            _recorder.VideoFolder = GetVideoFolder();

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

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the save folder for video files.
        /// </summary>
        /// <returns>The save folder for video files.</returns>

        private string GetVideoFolder()
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
            return Path.Combine(videoFolder, "WebCamRecorder");
        }
    }
}
