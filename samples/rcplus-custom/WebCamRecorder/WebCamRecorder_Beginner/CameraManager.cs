// -----------------------------------------------------------------------
// <copyright file="CameraManager.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Windows.Win32;
    using Windows.Win32.Foundation;
    using Windows.Win32.Media.MediaFoundation;
    using Windows.Win32.System.Com;

    /// <summary>
    /// Camera manager
    /// </summary>
    public class CameraManager
    {
        /// <summary>
        /// List of frame processors
        /// </summary>
        public List<IFrameProcessor> FrameProcessors { get; } = [];

        /// <summary>
        /// Will stop soon or not
        /// </summary>
        public bool IsStopping
        {
            get
            {
                return _stopping;
            }
        }

        /// <summary>
        /// Flags to indicate Stop() called
        /// </summary>
        private bool _stopping = false;

        /// <summary>
        /// Flags to indicate the task has done
        /// </summary>
        private bool _done = true;

        /// <summary>
        /// Action when stopped
        /// </summary>
        private Action? _stoppedAction;

        /// <summary>
        /// Create video device attributes
        /// </summary>
        /// <returns>Attributes object</returns>
        private static IMFAttributes? CreateVideoDeviceAttributes()
        {
            HRESULT hr = PInvoke.MFCreateAttributes(out var deviceAttribtes, 1);
            if (hr.Failed)
            {
                return null;
            }

            deviceAttribtes.SetGUID(
                PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE,
                PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID
            );

            return deviceAttribtes;
        }

        /// <summary>
        /// List available cameras
        /// </summary>
        /// <returns>Collection object</returns>
        public unsafe CameraInfoCollection? ListCameras()
        {
            var videoDeviceAttributes = CreateVideoDeviceAttributes();
            if (videoDeviceAttributes == null)
            {
                return null;
            }
            else
            {
                HRESULT hr = PInvoke.MFEnumDeviceSources(
                    videoDeviceAttributes,
                    out var sourceActivates,
                    out var sourceActivatesCount
                );

                CameraInfoCollection cameraInfoCollection = new(sourceActivates);

                for (var i = 0; i < sourceActivatesCount; i++)
                {
                    sourceActivates[i]->GetAllocatedString(
                        PInvoke.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME,
                        out var friendlyNamePtr,
                        out var _
                    );
                    string friendlyName = friendlyNamePtr.ToString();
                    PInvoke.CoTaskMemFree(friendlyNamePtr);

                    sourceActivates[i]->GetAllocatedString(
                        PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK,
                        out var symbolicLinkPtr,
                        out var _
                    );
                    string symbolicLink = symbolicLinkPtr.ToString();
                    PInvoke.CoTaskMemFree(symbolicLinkPtr);

                    cameraInfoCollection.CameraInfos.Add(
                        new(friendlyName, symbolicLink)
                    );
                }

                return cameraInfoCollection;
            }
        }

        /// <summary>
        /// Create source reader attributes
        /// </summary>
        /// <returns>Attributes object</returns>
        private static IMFAttributes? CreateSourceReaderAttributes()
        {
            HRESULT hr = PInvoke.MFCreateAttributes(out var readerAttributes, 2);
            if (hr.Failed)
            {
                return null;
            }

            readerAttributes.SetUINT32(
                PInvoke.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS,
                1
            );
            readerAttributes.SetUINT32(
                PInvoke.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING,
                1
            );
            

            return readerAttributes;
        }

        /// <summary>
        /// Create source reader media type
        /// </summary>
        /// <returns>Media type object</returns>
        private static IMFMediaType? CreateSourceReaderMediaType()
        {
            HRESULT hr = PInvoke.MFCreateMediaType(out var mediaType);
            if (hr.Failed)
            {
                return null;
            }

            mediaType.SetGUID(
                PInvoke.MF_MT_MAJOR_TYPE,
                PInvoke.MFMediaType_Video
            );
            mediaType.SetGUID(
                PInvoke.MF_MT_SUBTYPE,
                PInvoke.MFVideoFormat_RGB32
            );

            return mediaType;
        }

        /// <summary>
        /// Create source reader for the specified camera
        /// </summary>
        /// <param name="cameraInfo">Selected camera</param>
        /// <returns>Source reader object</returns>
        private unsafe IMFSourceReader? CreateSourceReader(
            CameraInfo cameraInfo
        )
        {
            var cameraInfoCollection = ListCameras();
            if (cameraInfoCollection == null)
            {
                return null;
            }

            var mediaSource = cameraInfoCollection.GetMediaSource(cameraInfo);
            cameraInfoCollection.Dispose();
            if (mediaSource == null)
            {
                return null;
            }

            var readerAttributes = CreateSourceReaderAttributes();
            if (readerAttributes == null)
            {
                mediaSource.Shutdown();
                Marshal.ReleaseComObject(mediaSource);
                return null;
            }

            HRESULT hr = PInvoke.MFCreateSourceReaderFromMediaSource(
                mediaSource,
                readerAttributes,
                out var sourceReader
            );
            if (hr.Failed)
            {
                mediaSource.Shutdown();
                Marshal.ReleaseComObject(mediaSource);
                return null;
            }

            var mediaType = CreateSourceReaderMediaType();
            if (mediaType == null)
            {
                mediaSource.Shutdown();
                Marshal.ReleaseComObject(mediaSource);
                return null;
            }
            unchecked
            {
                sourceReader.SetCurrentMediaType(
                    (uint)MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                    null,
                    mediaType
                );
            }
            Marshal.ReleaseComObject(mediaType);

            return sourceReader;
        }

        /// <summary>
        /// Get video information
        /// </summary>
        /// <param name="sourceReader">Source reader object</param>
        /// <param name="width">Variable to get width</param>
        /// <param name="height">Variable to get height</param>
        /// <param name="stride">Variable to get stride</param>
        /// <param name="bitRate">Variable to get bit rate</param>
        private static unsafe void GetVideoInfos(
            IMFSourceReader sourceReader,
            out uint width,
            out uint height,
            out uint stride,
            out uint bitRate
        )
        {
            width = 0;
            height = 0;
            stride = 0;
            bitRate = 0;

            unchecked
            {
                sourceReader.GetCurrentMediaType(
                    (uint)MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                    out var actualMediaType
                );
                if (actualMediaType == null)
                {
                    return;
                }

                actualMediaType.GetUINT64(
                    PInvoke.MF_MT_FRAME_SIZE,
                    out var frameSize
                );
                width = (uint)(frameSize >> 32);
                height = (uint)(frameSize & 0xffffffff);

                actualMediaType.GetUINT32(
                    PInvoke.MF_MT_DEFAULT_STRIDE,
                    out stride
                );

                try
                {
                    actualMediaType.GetUINT32(
                        PInvoke.MF_MT_AVG_BITRATE,
                        out bitRate
                    );
                }
                catch (Exception)
                {
                    const uint _defaultBitRate = 4_000_000;
                    bitRate = _defaultBitRate;
                }

                Marshal.ReleaseComObject(actualMediaType);
            }
        }

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
            const long _defaultDuration = 333_333;
            duration = _defaultDuration;

            try
            {
                unchecked
                {
                    sourceReader.ReadSample(
                        (uint)MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                        0,
                        out var _,
                        out uint streamFlags,
                        out var _,
                        out IMFSample sample
                    );

                    if (sample == null)
                    {
                        return 0;
                    }
                    else if ((streamFlags & (uint)MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_ENDOFSTREAM) != 0)
                    {
                        return -1;
                    }

                    sample.GetSampleDuration(out duration);

                    sample.ConvertToContiguousBuffer(out var mediaBuffer);

                    if (mediaBuffer != null)
                    {
                        mediaBuffer.Lock(out var data, out var _, out var currentLength);

                        if (currentLength <= frame.Length)
                        {
                            Marshal.Copy((nint)data, frame, 0, (int)currentLength);
                        }

                        mediaBuffer.Unlock();
                        Marshal.ReleaseComObject(mediaBuffer);
                    }

                    Marshal.ReleaseComObject(sample);

                    return 1;
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"ReadSample: {ex}");
            }

            return -1;
        }

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

        /// <summary>
        /// Command to stop all processings
        /// </summary>
        public void Stop(
            Action? stoppedAction = null
        )
        {
            _stoppedAction = stoppedAction;

            if (_done)
            {
                _stoppedAction?.Invoke();
            }
            else
            {
                foreach (var frameProcessor in FrameProcessors)
                {
                    frameProcessor.Stop();
                }
                _stopping = true;
            }
        }
    }
}
