// -----------------------------------------------------------------------
// <copyright file="Previewer.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder  
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Image = System.Windows.Controls.Image;

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

        /// <summary>
        /// Source width
        /// </summary>
        private int _width;

        /// <summary>
        /// Source height
        /// </summary>
        private int _height;

        /// <summary>
        /// Source stride
        /// </summary>
        private int _stride;

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

        /// <inheritdoc />
        public void Terminate()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (PreviewImage != null)
                {
                    PreviewImage.Source = null;
                }

                _bitmap = null;
            });
        }

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

        /// <inheritdoc />
        public void Stop()
        {
        }

        /// <inheritdoc />
        public bool IsStopped
        {
            get
            {
                return true;
            }
        }
    }
}
