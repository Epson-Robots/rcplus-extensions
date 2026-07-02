// -----------------------------------------------------------------------
// <copyright file="Recorder.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using Windows.Win32;
    using Windows.Win32.Foundation;
    using Windows.Win32.Media.MediaFoundation;

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
                        if (!string.Equals(_videoFolder, value, StringComparison.OrdinalIgnoreCase))
                        {
                            _fileNumber = 0;
                        }

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

        /// <summary>
        /// Extension of video file
        /// </summary>
        public const string VideoFileExtension = ".mp4";

        /// <summary>
        /// Backing store of Mode
        /// </summary>
        private RecordingMode _mode = RecordingMode.Stop;

        /// <summary>
        /// Flag to stop recording
        /// </summary>
        private bool _shouldStop = false;

        /// <summary>
        /// Backing store of VideoFolder
        /// </summary>
        private string _videoFolder = ".";

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

        /// <summary>
        /// Source bit rate
        /// </summary>
        private uint _bitRate;

        /// <summary>
        /// Frame size
        /// </summary>
        private UInt64 _frameSize;

        /// <summary>
        /// Frame rate
        /// </summary>
        private UInt64 _frameRate;

        /// <summary>
        /// Aspect ratio
        /// </summary>
        private UInt64 _aspectRatio;

        /// <summary>
        /// Sink writer object
        /// </summary>
        private IMFSinkWriter? _sinkWriter;

        /// <summary>
        /// Stream index
        /// </summary>
        private uint _streamIndex;

        /// <summary>
        /// Sample object
        /// </summary>
        private IMFSample? _sample;

        /// <summary>
        /// Record time
        /// </summary>
        private long _recordTime;

        /// <summary>
        /// File number
        /// </summary>
        private int _fileNumber = 0;

        /// <summary>
        /// Maxinum file number
        /// </summary>
        private const int _maxFileNumber = 999;

        /// <summary>
        /// Initial segment time span
        /// </summary>
        private const long _initialSegmentSpan = 50_000_000;

        /// <summary>
        /// Segment time span
        /// </summary>
        private long _segmentSpan;

        /// <summary>
        /// Generated path queue
        /// </summary>
        private readonly Queue<string> _generatedPaths = [];

        /// <summary>
        /// Number of files to be kept
        /// </summary>
        private const int _numFilesKept = 2;

        /// <summary>
        /// Raise mode changed event
        /// </summary>
        private void RaisePropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mode)));
        }

        /// <summary>
        /// Create sample for writing
        /// </summary>
        /// <returns>Sample object</returns>
        private IMFSample? CreateSample()
        {
            HRESULT hr;

            hr = PInvoke.MFCreateSample(out var sample);
            if (hr.Failed)
            {
                return null;
            }

            uint length = (uint)(_height * _stride);
            hr = PInvoke.MFCreateAlignedMemoryBuffer(
                length,
                PInvoke.MF_4_BYTE_ALIGNMENT,
                out var buffer
            );
            if (hr.Failed)
            {
                Marshal.ReleaseComObject(sample);
                return null;
            }
            buffer.SetCurrentLength(length);

            sample.AddBuffer(buffer);

            Marshal.ReleaseComObject(buffer);

            return sample;
        }

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
            _bitRate = bitRate;

            _frameSize = ((UInt64)width << 32) + (UInt64)height;
            _frameRate = ((UInt64)30 << 32) + (UInt64)1;
            _aspectRatio = ((UInt64)1 << 32) + (UInt64)1;

            _sample = CreateSample();
        }

        /// <inheritdoc />
        public void Terminate()
        {
        }

        /// <summary>
        /// Create sink writer attributes
        /// </summary>
        /// <returns>Attributes object</returns>
        private static IMFAttributes? CreateSinkWriterAttributes()
        {
            HRESULT hr = PInvoke.MFCreateAttributes(out var sinkWriterAttributes, 1);
            if (hr.Failed)
            {
                return null;
            }

            sinkWriterAttributes.SetUINT32(
                PInvoke.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS,
                1
            );

            return sinkWriterAttributes;
        }

        /// <summary>
        /// Create output media type
        /// </summary>
        /// <returns>Media type object</returns>
        private IMFMediaType? CreateOutputMediaType()
        {
            HRESULT hr = PInvoke.MFCreateMediaType(out var outputMediaType);
            if (hr.Failed)
            {
                return null;
            }

            outputMediaType.SetGUID(
                PInvoke.MF_MT_MAJOR_TYPE,
                PInvoke.MFMediaType_Video
            );
            outputMediaType.SetGUID(
                PInvoke.MF_MT_SUBTYPE,
                PInvoke.MFVideoFormat_H264
            );
            outputMediaType.SetUINT32(
                PInvoke.MF_MT_AVG_BITRATE,
                _bitRate
            );
            outputMediaType.SetUINT32(
                PInvoke.MF_MT_INTERLACE_MODE,
                (uint)MFVideoInterlaceMode.MFVideoInterlace_Progressive
            );
            outputMediaType.SetUINT64(
                PInvoke.MF_MT_FRAME_SIZE,
                _frameSize
            );
            outputMediaType.SetUINT64(
                PInvoke.MF_MT_FRAME_RATE,
                _frameRate
            );
            outputMediaType.SetUINT64(
                PInvoke.MF_MT_PIXEL_ASPECT_RATIO,
                _aspectRatio
            );

            return outputMediaType;
        }

        /// <summary>
        /// Create input media type
        /// </summary>
        /// <returns>Media type object</returns>
        private IMFMediaType? CreateInputMediaType()
        {
            HRESULT hr = PInvoke.MFCreateMediaType(out var inputMediaType);
            if (hr.Failed)
            {
                return null;
            }
            inputMediaType.SetGUID(
                PInvoke.MF_MT_MAJOR_TYPE,
                PInvoke.MFMediaType_Video
            );
            inputMediaType.SetGUID(
                PInvoke.MF_MT_SUBTYPE,
                PInvoke.MFVideoFormat_RGB32
            );
            inputMediaType.SetUINT32(
                PInvoke.MF_MT_INTERLACE_MODE,
                (uint)MFVideoInterlaceMode.MFVideoInterlace_Progressive
            );
            inputMediaType.SetUINT64(
                PInvoke.MF_MT_FRAME_SIZE,
                _frameSize
            );
            inputMediaType.SetUINT64(
                PInvoke.MF_MT_FRAME_RATE,
                _frameRate
            );
            inputMediaType.SetUINT64(
                PInvoke.MF_MT_PIXEL_ASPECT_RATIO,
                _aspectRatio
            );

            return inputMediaType;
        }

        /// <summary>
        /// Create sink writer object
        /// </summary>
        /// <param name="filePath">Path of video file</param>
        /// <returns>Sink writer object</returns>
        private IMFSinkWriter? CreateSinkWriter(
            string filePath
        )
        {
            var sinkWriterAttributes = CreateSinkWriterAttributes();
            if (sinkWriterAttributes == null)
            {
                return null;
            }

            HRESULT hr = PInvoke.MFCreateSinkWriterFromURL(
                filePath,
                null,
                sinkWriterAttributes,
                out var sinkWriter
            );
            Marshal.ReleaseComObject(sinkWriterAttributes);
            if (hr.Failed)
            {
                return null;
            }

            var outputMediaType = CreateOutputMediaType();
            if (outputMediaType == null)
            {
                Marshal.ReleaseComObject(sinkWriter);
                return null;
            }
            sinkWriter.AddStream(outputMediaType, out _streamIndex);
            Marshal.ReleaseComObject(outputMediaType);

            var inputMediaType = CreateInputMediaType();
            if (inputMediaType == null)
            {
                Marshal.ReleaseComObject(sinkWriter);
                return null;
            }
            sinkWriter.SetInputMediaType(_streamIndex, inputMediaType, null);
            Marshal.ReleaseComObject(inputMediaType);

            sinkWriter.BeginWriting();

            return sinkWriter;
        }

        /// <summary>
        /// Set flipped frame to sample buffer
        /// </summary>
        /// <param name="frame">Frame data</param>
        private unsafe void SetFlippedFrame(
            byte[] frame
        )
        {
            if (_sample == null)
            {
                return;
            }

            _sample.GetBufferByIndex(0, out var buffer);
            if (buffer == null)
            {
                return;
            }

            buffer.Lock(out var data);

            for (var row = 0; row < _height; row++)
            {
                Marshal.Copy(
                    frame,
                    _stride * row,
                    (nint)(data + _stride * (_height - 1 - row)),
                    _stride
                );
            }

            buffer.Unlock();
            Marshal.ReleaseComObject(buffer);
        }

        /// <summary>
        /// Get next segment path name
        /// </summary>
        /// <returns>Path name</returns>
        private string GetNextSegmentFile()
        {
            var filePath = Path.Combine(
                _videoFolder,
                $"Video_{_fileNumber:000}{VideoFileExtension}"
            );

            if (++_fileNumber > _maxFileNumber)
            {
                _fileNumber = 0;
            }

            _generatedPaths.Enqueue(filePath);
            while (_generatedPaths.Count > _numFilesKept)
            {
                try
                {
                    var oldestPath = _generatedPaths.Dequeue();
                    if (string.Equals(_videoFolder, Path.GetDirectoryName(oldestPath), StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(oldestPath);
                    }
                }
                catch (Exception)
                {
                    // IGNORE
                }
            }

            return filePath;
        }

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

        /// <inheritdoc />
        public bool IsStopped
        {
            get
            {
                return _sinkWriter == null;
            }
        }
    }
}
