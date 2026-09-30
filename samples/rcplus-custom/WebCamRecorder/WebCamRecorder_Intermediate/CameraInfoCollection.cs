// -----------------------------------------------------------------------
// <copyright file="CameraInfoCollection.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Windows.Win32;
    using Windows.Win32.Media.MediaFoundation;

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

        /// <inheritdoc />
        public unsafe void Dispose()
        {
            if (_sourceActivates != null)
            {
                for (var i = 0; i < CameraInfos.Count; i++)
                {
                    _sourceActivates[i]->Release();
                }

                PInvoke.CoTaskMemFree(_sourceActivates);
                _sourceActivates = null;
            }
        }
    }
}
