// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelLog.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using System.IO;
using VanguardModelPFScrewdriver.Dialogs.Progress;
using VanguardModelPFScrewdriver.ModelPF;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using V3 = Epson.RoboticsShared.ExtensionsAPI.V3;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that manages the tightening result log:
    /// reading it from file, appending new entries and fetching it from the controller.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets the tightening results shown in the result list.
        /// </summary>
        public ReactiveCollection<ResultLogEntry> TighteningResults { get; } = [];

        /// <summary>
        /// Gets the tightening result currently selected.
        /// </summary>
        public ReactivePropertySlim<ResultLogEntry?> SelectedTighteningResult { get; } = new(null);

        /// <summary>
        /// Gets the mapping between OK waveform slots and their owning log lines.
        /// </summary>
        public RingMap RingMapOK { get; } = new();

        /// <summary>
        /// Gets the mapping between NG waveform slots and their owning log lines.
        /// </summary>
        public RingMap RingMapNG { get; } = new();

        /// <summary>
        /// Gets the line number of the last entry read from the result log.
        /// </summary>
        public int LastLineNo { get; private set; }

        /// <summary>
        /// Gets the waveform slot used by the latest OK result.
        /// </summary>
        public int LastWaveIndexOK { get; private set; }

        /// <summary>
        /// Gets the waveform slot used by the latest NG result.
        /// </summary>
        public int LastWaveIndexNG { get; private set; }

        // Size of the result log file at the last read, used for incremental reading.
        private Int64 _lastResultLogFileSize;

        /// <summary>
        /// Gets the file name of the result log.
        /// </summary>
        private string GetResultLogFileName()
        {
            return GetLogFileName('R');
        }

        /// <summary>
        /// Marks the entries whose waveform file is still available so that they can be drawn.
        /// </summary>
        /// <param name="entries">Entries to examine.</param>
        /// <param name="resultKind">Result kind to examine ("OK" or "NG").</param>
        private void CheckWaveExists(
            List<ResultLogEntry> entries,
            string resultKind
        )
        {
            if (_proFuseData == null)
            {
                return;
            }

            RingMap map;
            int numMaxWaves;

            // OK and NG waveforms use separate ring buffers with their own capacity.
            int storageKindIndex = (int)_proFuseData.PreferenceData.LogStorage;
            switch (resultKind)
            {
                case "OK":
                    map = RingMapOK;
                    numMaxWaves = _proFuseData.PreferenceData.MaxNumPass[storageKindIndex];
                    break;

                case "NG":
                    map = RingMapNG;
                    numMaxWaves = _proFuseData.PreferenceData.MaxNumFail[storageKindIndex];
                    break;

                default:
                    return;
            }

            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return;
            }

            // Only the newest entries can still own a waveform slot, so scan backwards.
            int numChecked = 0;
            for (var index = entries.Count - 1; index >= 0; index--)
            {
                var entry = entries[index];
                if (entry.Result != resultKind)
                {
                    continue;
                }

                if (++numChecked > numMaxWaves)
                {
                    break;
                }

                // The slot has already been taken over by a newer entry.
                var lineNo = map.GetLineNo(entry.WaveIndex);
                if (lineNo != entry.LineNo)
                {
                    continue;
                }

                var waveFilePath = Path.Combine(logFolder, GetWaveFileName(resultKind, entry.WaveIndex));
                if (File.Exists(waveFilePath))
                {
                    entry.CanDrawWave.Value = true;
                }
            }
        }

        /// <summary>
        /// Resets the read position and the waveform mappings so that the whole log is read again.
        /// </summary>
        private void SetReloadResultLog()
        {
            LastLineNo = 0;
            _lastResultLogFileSize = 0;
            RingMapOK.Clear();
            LastWaveIndexOK = -1;
            RingMapNG.Clear();
            LastWaveIndexNG = -1;
        }

        /// <summary>
        /// Reads the result log entries that have been appended since the last call.
        /// </summary>
        /// <returns>The newly read result entries.</returns>
        private async Task<IEnumerable<ResultLogEntry>> UpdateResultLogAsync()
        {
            List<ResultLogEntry> entries = [];

            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return entries;
            }

            var resultLogPath = Path.Combine(logFolder, GetResultLogFileName());
            if (!File.Exists(resultLogPath))
            {
                return entries;
            }

            try
            {
                using FileStream fileStream = new(resultLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (_lastResultLogFileSize > 0)
                {
                    // The file has shrunk, which means it was recreated: read it from the beginning.
                    if (fileStream.Length < _lastResultLogFileSize)
                    {
                        SetReloadResultLog();
                    }
                    else
                    {
                        // Continue from the position read last time.
                        fileStream.Seek(_lastResultLogFileSize, SeekOrigin.Begin);
                    }
                }

                using StreamReader reader = new(fileStream);

                var lineNo = LastLineNo;
                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        // End of file: remember the position for the next incremental read.
                        LastLineNo = lineNo;
                        _lastResultLogFileSize = fileStream.Length;
                        break;
                    }

                    string[] fields = line.Split(",").Select(x => x.Trim()).ToArray() ?? [];

                    // Skip empty lines and the CSV header line.
                    if (fields.Length == 0 || fields[0] == "DateTime")
                    {
                        continue;
                    }

                    lineNo++;

                    try
                    {
                        ResultLogEntry entry = new(lineNo);
                        entry.Set(fields);

                        entries.Add(entry);

                        if (entry.Result == "OK")
                        {
                            // The waveform slot is reused, so its previous owner loses its waveform.
                            var curLineNo = RingMapOK.GetLineNo(entry.WaveIndex);
                            if (curLineNo >= 0)
                            {
                                var curEntry = TighteningResults.FirstOrDefault(x => x.LineNo == curLineNo);
                                if (curEntry != null)
                                {
                                    curEntry.CanDrawWave.Value = false;
                                }
                            }
                            RingMapOK.Add(lineNo, entry.WaveIndex);
                            LastWaveIndexOK = entry.WaveIndex;
                        }
                        else
                        {
                            // The NG waveforms are managed by a separate ring buffer.
                            var curLineNo = RingMapNG.GetLineNo(entry.WaveIndex);
                            if (curLineNo >= 0)
                            {
                                var curEntry = TighteningResults.FirstOrDefault(x => x.LineNo == curLineNo);
                                if (curEntry != null)
                                {
                                    curEntry.CanDrawWave.Value = false;
                                }
                            }
                            RingMapNG.Add(lineNo, entry.WaveIndex);
                            LastWaveIndexNG = entry.WaveIndex;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore malformed lines and continue with the next one.
                    }
                }

                // Determine which of the read entries still have a waveform file.
                CheckWaveExists(entries, "OK");
                CheckWaveExists(entries, "NG");
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.LogReadingError);
            }

            return entries;
        }

        /// <summary>
        /// Reads the whole result log again from the beginning.
        /// </summary>
        /// <returns>All result entries in the log file.</returns>
        private async Task<IEnumerable<ResultLogEntry>> ReloadResultLogAsync()
        {
            SetReloadResultLog();

            return await UpdateResultLogAsync();
        }

        /// <summary>
        /// Assigns the next line number and waveform slot to the entry
        /// and appends it to the result log file in CSV format.
        /// </summary>
        /// <param name="entry">Entry to append.</param>
        private async Task AppendResultLogToFileAsync(
            ResultLogEntry entry
        )
        {
            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return;
            }

            var resultLogPath = Path.Combine(logFolder, GetResultLogFileName());

            try
            {
                using FileStream fileStream = new(resultLogPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using StreamWriter writer = new(fileStream);

                entry.LineNo = LastLineNo + 1;
                if (entry.Result == "OK")
                {
                    // Advance the OK waveform slot, wrapping around at the configured capacity.
                    LastWaveIndexOK += 1;
                    var maxNumFiles = _proFuseData.PreferenceData.MaxNumPass[(int)StorageKind.PC];
                    if (LastWaveIndexOK >= maxNumFiles)
                    {
                        LastWaveIndexOK = 0;
                    }
                    entry.WaveIndex = LastWaveIndexOK;
                }
                else
                {
                    // Advance the NG waveform slot, wrapping around at the configured capacity.
                    LastWaveIndexNG += 1;
                    var maxNumFiles = _proFuseData.PreferenceData.MaxNumFail[(int)StorageKind.PC];
                    if (LastWaveIndexNG >= maxNumFiles)
                    {
                        LastWaveIndexNG = 0;
                    }
                    entry.WaveIndex = LastWaveIndexNG;
                }

                await writer.WriteLineAsync(entry.GetCSV());
                LastLineNo++;
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.LogAppendingError);
            }
        }

        /// <summary>
        /// Downloads the result log file from the robot controller into the local log folder.
        /// </summary>
        /// <returns>True if the file has been downloaded successfully.</returns>
        private bool FetchResultLogFromController()
        {
            var controllerPathType = GetControllerPathType();
            if (controllerPathType == null)
            {
                return false;
            }

            var controllerAPI = Main.GetAPI<V3.IRCXControllerAPI>();

            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return false;
            }

            var sourceFile = GetResultLogFileName();
            var destinationPath = Path.Combine(logFolder, sourceFile);

            var result = controllerAPI.DownloadControllerFile(
                (V3.IRCXControllerAPI.ControllerPathType)controllerPathType,
                sourceFile,
                destinationPath
            );

            return result == RCXResult.Success;
        }

        /// <summary>
        /// Downloads all torque waveform files from the robot controller while showing a progress window.
        /// </summary>
        private async Task FetchWaveformDataAsync()
        {
            var controllerPathType = GetControllerPathType();
            if (controllerPathType == null)
            {
                return;
            }

            var controllerAPI = Main.GetAPI<V3.IRCXControllerAPI>();

            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return;
            }

            // Both ring buffers are transferred, so the loop runs over the larger capacity.
            var storageType = (int)_proFuseData.PreferenceData.LogStorage;
            var maxPassOK = _proFuseData.PreferenceData.MaxNumPass[storageType];
            var maxPassNG = _proFuseData.PreferenceData.MaxNumFail[storageType];
            var maxCount = Math.Max(maxPassOK, maxPassNG);

            await ProgressWindow.OpenAsync(
                (reporter) =>
                {
                    reporter.SetMessage(Captions[Caption.FetchingTorqueData]);
                    reporter.SetMaximum(maxCount);

                    for (var waveIndex = 0; waveIndex < maxCount; waveIndex++)
                    {
                        // Missing files are simply skipped by the controller API.
                        if (waveIndex < maxPassOK)
                        {
                            var sourceFile = GetWaveFileName("OK", waveIndex);
                            var destinationPath = Path.Combine(logFolder, sourceFile);
                            _ = controllerAPI.DownloadControllerFile(
                                controllerPathType.Value,
                                sourceFile,
                                destinationPath
                            );
                        }
                        if (waveIndex < maxPassNG)
                        {
                            var sourceFile = GetWaveFileName("NG", waveIndex);
                            var destinationPath = Path.Combine(logFolder, sourceFile);
                            _ = controllerAPI.DownloadControllerFile(
                                controllerPathType.Value,
                                sourceFile,
                                destinationPath
                            );
                        }

                        reporter.SetValue(waveIndex);
                    }

                    return Task.CompletedTask;
                }
            );
        }
    }
}
