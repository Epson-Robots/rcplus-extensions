// -----------------------------------------------------------------------
// <copyright file="ContentViewModelAddition.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder.DockingWindow
{
    using System.Drawing;
    using System.Reactive.Disposables;
    using System.Threading.Tasks;
    using Reactive.Bindings;
    using Reactive.Bindings.Extensions;
    using static WebCamRecorder.Constants;
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
        /// Camera manager
        /// </summary>
        private readonly CameraManager _cameraManager = new();

        /// <summary>
        /// Previewer
        /// </summary>
        private readonly Previewer _previewer = new();

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
        /// <param name="previewImage"></param>
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

        /// <inheritdoc />
        public Task WindowCreated()
        {
            return Task.CompletedTask;
        }
    }
}
