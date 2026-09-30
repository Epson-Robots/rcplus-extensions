// -----------------------------------------------------------------------
// <copyright file="RingMap.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Maintains a bidirectional mapping between a result log line number and
    /// the waveform slot index of the device ring buffer.
    /// Because the ring buffer slots are reused, a slot always refers to the newest line number.
    /// </summary>
    public class RingMap
    {
        /// <summary>
        /// Maps a log line number to the waveform slot index.
        /// </summary>
        private readonly Dictionary<int, int> _lineNoToWaveIndex = [];

        /// <summary>
        /// Maps a waveform slot index to the log line number.
        /// </summary>
        private readonly Dictionary<int, int> _waveIndexToLineNo = [];

        /// <summary>
        /// Removes all mappings.
        /// </summary>
        public void Clear()
        {
            _lineNoToWaveIndex.Clear();
            _waveIndexToLineNo.Clear();
        }

        /// <summary>
        /// Registers the mapping between the specified line number and waveform slot index.
        /// </summary>
        /// <param name="lineNo">The line number of the result log entry.</param>
        /// <param name="waveIndex">The waveform slot index in the ring buffer.</param>
        public void Add(
            int lineNo,
            int waveIndex
        )
        {
            // The slot has been overwritten, so drop the mapping of the previous owner line.
            if (_waveIndexToLineNo.TryGetValue(waveIndex, out var curLineNo))
            {
                _lineNoToWaveIndex.Remove(curLineNo);
            }
            _lineNoToWaveIndex[lineNo] = waveIndex;
            _waveIndexToLineNo[waveIndex] = lineNo;
        }

        /// <summary>
        /// Gets the waveform slot index mapped to the specified line number.
        /// </summary>
        /// <param name="lineNo">The line number of the result log entry.</param>
        /// <returns>The waveform slot index, or -1 if no mapping exists.</returns>
        public int GetWaveIndex(
            int lineNo
        )
        {
            if (_lineNoToWaveIndex.TryGetValue(lineNo, out var waveIndex))
            {
                return waveIndex;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the line number mapped to the specified waveform slot index.
        /// </summary>
        /// <param name="waveIndex">The waveform slot index in the ring buffer.</param>
        /// <returns>The line number, or -1 if no mapping exists.</returns>
        public int GetLineNo(
            int waveIndex
        )
        {
            if (_waveIndexToLineNo.TryGetValue(waveIndex, out var lineNo))
            {
                return lineNo;
            }
            else
            {
                return -1;
            }
        }
    }
}
