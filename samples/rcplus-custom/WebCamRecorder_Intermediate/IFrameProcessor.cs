// -----------------------------------------------------------------------
// <copyright file="IFrameProcessor.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

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
