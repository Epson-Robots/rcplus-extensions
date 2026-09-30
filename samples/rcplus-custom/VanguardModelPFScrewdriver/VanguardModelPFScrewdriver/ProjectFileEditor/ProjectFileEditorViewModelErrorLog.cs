// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelErrorLog.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.ModelPF;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static VanguardModelPFScrewdriver.Constants;
using V3 = Epson.RoboticsShared.ExtensionsAPI.V3;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that manages the error log:
    /// reading it from file, appending new entries and fetching it from the controller.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets the error entries shown in the error log view.
        /// </summary>
        public ReactiveCollection<ErrorLogEntry> Errors { get; } = [];

        /// <summary>
        /// Gets the currently selected error entry.
        /// </summary>
        public ReactivePropertySlim<ErrorLogEntry?> SelectedError { get; } = new(null);

        // Line number of the last error entry that has been read.
        private static int _lastErrorLineNo;

        // Size of the error log file at the last read, used for incremental reading.
        private static Int64 _lastErrorLogFileSize;

        // Entries reported by other components and not yet written to the log file.
        private static readonly Queue<ErrorLogEntry> _pendingErrorEntries = [];

        // Signaled whenever a new error entry has been enqueued.
        private static readonly AutoResetEvent _errorReported = new(false);

        // Guards access to _pendingErrorEntries.
        private static readonly object _errorReportLocker = new();

        /// <summary>
        /// Gets the file name of the error log.
        /// </summary>
        private string GetErrorLogFileName()
        {
            return GetLogFileName('E');
        }

        /// <summary>
        /// Resets the read position so that the whole error log is read again.
        /// </summary>
        private static void SetReloadErrorLog()
        {
            _lastErrorLineNo = 0;
            _lastErrorLogFileSize = 0;
        }

        /// <summary>
        /// Reads the error log entries that have been appended since the last call.
        /// </summary>
        /// <returns>The newly read error entries.</returns>
        private async Task<IEnumerable<ErrorLogEntry>> UpdateErrorLogAsync()
        {
            List<ErrorLogEntry> entries = [];

            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return entries;
            }

            var errorLogPath = Path.Combine(logFolder, GetErrorLogFileName());
            if (!File.Exists(errorLogPath))
            {
                return entries;
            }

            try
            {
                using FileStream fileStream = new(errorLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (_lastErrorLogFileSize > 0)
                {
                    // The file has shrunk, which means it was recreated: read it from the beginning.
                    if (fileStream.Length < _lastResultLogFileSize)
                    {
                        SetReloadErrorLog();
                    }
                    else
                    {
                        // Continue from the position read last time.
                        fileStream.Seek(_lastErrorLogFileSize, SeekOrigin.Begin);
                    }
                }

                using StreamReader reader = new(fileStream);

                var lineNo = _lastErrorLineNo;
                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        // End of file: remember the position for the next incremental read.
                        _lastErrorLineNo = lineNo;
                        _lastErrorLogFileSize = fileStream.Length;
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
                        ErrorLogEntry entry = new(lineNo);
                        entry.Set(fields);

                        entries.Add(entry);
                    }
                    catch (Exception)
                    {
                        // Ignore malformed lines and continue with the next one.
                    }
                }
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.LogReadingError);
            }

            return entries;
        }

        /// <summary>
        /// Reads the whole error log again from the beginning.
        /// </summary>
        /// <returns>All error entries in the log file.</returns>
        private async Task<IEnumerable<ErrorLogEntry>> ReloadErrorLogAsync()
        {
            SetReloadErrorLog();

            return await UpdateErrorLogAsync();
        }

        /// <summary>
        /// Appends a single entry to the error log file in CSV format.
        /// </summary>
        /// <param name="entry">Entry to append.</param>
        private async Task AppendErrorLogToFileAsync(
            ErrorLogEntry entry
        )
        {
            var logFolder = GetLogFolder();
            if (logFolder == null)
            {
                return;
            }

            var errorLogPath = Path.Combine(logFolder, GetErrorLogFileName());

            try
            {
                using FileStream fileStream = new(errorLogPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using StreamWriter writer = new(fileStream);

                await writer.WriteLineAsync(entry.GetCSV());
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.LogAppendingError);
            }
        }

        /// <summary>
        /// Starts the background worker that writes reported errors to the log file
        /// and adds them to <see cref="Errors"/>.
        /// </summary>
        public void StartErrorLogger()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    // Wait until at least one error has been reported.
                    _errorReported.WaitOne();

                    List<ErrorLogEntry> appended = [];

                    // Drain the queue and persist every pending entry.
                    while (true)
                    {
                        ErrorLogEntry? entry;
                        lock (_errorReportLocker)
                        {
                            if (!_pendingErrorEntries.TryDequeue(out entry))
                            {
                                break;
                            }
                        }
                        await AppendErrorLogToFileAsync(entry);
                        appended.Add(entry);
                    }

                    // Update the bound collection on the UI scheduler.
                    Errors.AddRangeOnScheduler(appended);
                }
            });
        }

        /// <summary>
        /// Enqueues an error entry for the specified operation to be logged.
        /// </summary>
        /// <param name="errorOperation">Operation in which the error occurred.</param>
        public static void ReportError(
            ErrorLogEntry.ErrOp errorOperation
        )
        {
            ErrorLogEntry entry = new(++_lastErrorLineNo)
            {
                TimeStamp = DateTime.Now,
                ErrorOperation = (int)errorOperation,
            };

            lock (_errorReportLocker)
            {
                _pendingErrorEntries.Enqueue(entry);
            }

            // Wake up the logger worker.
            _errorReported.Set();
        }

        /// <summary>
        /// Shows an error message box with the specified caption on the UI thread.
        /// </summary>
        /// <param name="captionId">Caption ID of the message text.</param>
        /// <param name="ownerWindow">Owner window of the message box.</param>
        public void Message(
            int captionId,
            Window? ownerWindow = null
        )
        {
            Application.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    _ = _windowAPI.ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, captionId),
                        ButtonType.OK,
                        IconType.Error,
                        ownerWindow
                    );
                },
                DispatcherPriority.Background
            );
        }

        /// <summary>
        /// Downloads the error log file from the robot controller into the local log folder.
        /// </summary>
        /// <returns>True if the file has been downloaded successfully.</returns>
        private bool FetchErrorLogFromController()
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

            var sourceFile = GetErrorLogFileName();
            var destinationPath = Path.Combine(logFolder, sourceFile);

            var result = controllerAPI.DownloadControllerFile(
                controllerPathType.Value,
                sourceFile,
                destinationPath
            );

            return result == RCXResult.Success;
        }
    }
}
