// -----------------------------------------------------------------------
// <copyright file="ResultLogEntry.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Text;
using VanguardModelPFScrewdriver.Utils;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Represents a single tightening result record of the ModelPF screwdriver.
    /// </summary>
    public class ResultLogEntry : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Provides data for the <see cref="LogEntryChanged"/> event.
        /// </summary>
        public class ResultLogEntryEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets a value indicating whether the torque waveform of the entry should be drawn.
            /// </summary>
            public bool DrawWave { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ResultLogEntryEventArgs"/> class.
            /// </summary>
            /// <param name="draw">true to draw the waveform; otherwise, false.</param>
            public ResultLogEntryEventArgs(
                bool draw
            )
            {
                DrawWave = draw;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when the waveform drawing state of any entry has changed.
        /// </summary>
        public static EventHandler<ResultLogEntryEventArgs>? LogEntryChanged { get; set; }

        /// <summary>
        /// Defines the column order of the log fields in the source data.
        /// </summary>
        public enum FieldIndex
        {
            /// <summary>Index of the time stamp field.</summary>
            TimeStamp = 0,

            /// <summary>Index of the program number field.</summary>
            ProgramNo,

            /// <summary>Index of the number of turns field.</summary>
            NumTurns,

            /// <summary>Index of the torque field.</summary>
            Torque,

            /// <summary>Index of the elapsed time field.</summary>
            ElapsedTime,

            /// <summary>Index of the tightening result field.</summary>
            Result,

            /// <summary>Index of the error detail field.</summary>
            ErrorDetail,

            /// <summary>Index of the waveform data index field.</summary>
            WaveIndex,
        }

        /// <summary>
        /// Gets or sets the line number of this entry in the log file.
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the tightening was performed.
        /// </summary>
        [LogField]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the program number used for the tightening.
        /// </summary>
        [LogField]
        public int ProgramNo { get; set; }

        /// <summary>
        /// Gets or sets the number of turns of the driver.
        /// </summary>
        [LogField]
        public float NumTurns { get; set; }

        /// <summary>
        /// Gets or sets the measured torque value.
        /// </summary>
        [LogField]
        public float Torque { get; set; }

        /// <summary>
        /// Gets or sets the elapsed time of the tightening in seconds.
        /// </summary>
        [LogField]
        public float ElapsedTime { get; set;  }

        /// <summary>
        /// Gets or sets the tightening result (for example OK or NG).
        /// </summary>
        [LogField]
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed error information of the tightening.
        /// </summary>
        [LogField]
        public string ErrorDetail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the index of the torque waveform data associated with this entry.
        /// </summary>
        [LogField]
        public int WaveIndex { get; set; }

        /// <summary>
        /// Gets the flag indicating whether the waveform of this entry is currently drawn on the chart.
        /// </summary>
        public ReactivePropertySlim<bool> DrawWave { get; } = new(false, ReactivePropertyMode.DistinctUntilChanged);

        /// <summary>
        /// Gets the flag indicating whether waveform data is available for this entry.
        /// </summary>
        public ReactivePropertySlim<bool> CanDrawWave { get; } = new(false);

        /// <summary>
        /// Cached type converters used to parse the log field values.
        /// </summary>
        private readonly static Dictionary<Type, TypeConverter> _converters = [];

        /// <summary>
        /// Holds the subscriptions to be released on <see cref="Dispose"/>.
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Initializes the type converter cache for every property marked with <see cref="LogFieldAttribute"/>.
        /// </summary>
        static ResultLogEntry()
        {
            foreach (var propInfo in typeof(ResultLogEntry).GetProperties())
            {
                if (propInfo.CustomAttributes.Any(x => x.AttributeType == typeof(LogFieldAttribute)))
                {
                    _converters[propInfo.PropertyType] = TypeDescriptor.GetConverter(propInfo.PropertyType);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultLogEntry"/> class.
        /// </summary>
        /// <param name="lineNo">The line number of this entry in the log file.</param>
        public ResultLogEntry(
            int lineNo
        )
        {
            LineNo = lineNo;

            // Notify listeners (chart view) whenever the waveform drawing state is toggled.
            DrawWave.Subscribe((draw) =>
            {
                LogEntryChanged?.Invoke(this, new ResultLogEntryEventArgs(draw));
            })
            .AddTo(_disposables);

            // Refresh the displayed error detail when the UI language is switched.
            Main.Captions!.LanguageChanged += () =>
            {
                PropertyChanged?.Invoke(this, new(nameof(ErrorDetail)));
            };
        }

        /// <summary>
        /// Releases the subscriptions held by this instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _disposables.Dispose();
        }

        /// <summary>
        /// Fills the properties of this entry from the specified log fields.
        /// </summary>
        /// <param name="fields">The field values ordered as defined by <see cref="FieldIndex"/>.</param>
        public void Set(
            string[] fields
        )
        {
            for (int i = 0; i < fields.Length; i++)
            {
                // Skip fields that are not defined in FieldIndex.
                if (Enum.IsDefined(typeof(FieldIndex), i))
                {
                    try
                    {
                        // Resolve the target property by the field index name and convert the raw text value.
                        var propInfo = typeof(ResultLogEntry).GetProperty(((FieldIndex)i).ToString());
                        propInfo?.SetValue(this, _converters[propInfo.PropertyType].ConvertFrom(fields[i]));
                    }
                    catch (Exception)
                    {
                        // IGNORE: keep the default value when the field cannot be converted.
                    }
                }
            }
        }

        /// <summary>
        /// Builds the CSV representation of this entry.
        /// </summary>
        /// <returns>A comma separated string containing all log fields.</returns>
        public string GetCSV()
        {
            const char _separator = ',';

            StringBuilder builder = new();

            builder.Append(TimeStamp.ToString("yyyy/MM/dd HH:mm:ss"));
            builder.Append(_separator);
            builder.Append(ProgramNo);
            builder.Append(_separator);
            builder.Append($"{NumTurns:F1}");
            builder.Append(_separator);
            builder.Append($"{Torque:F1}");
            builder.Append(_separator);
            builder.Append($"{ElapsedTime:F3}");
            builder.Append(_separator);
            builder.Append(Result);
            builder.Append(_separator);
            builder.Append(ErrorDetail);
            builder.Append(_separator);
            builder.Append(WaveIndex);

            return builder.ToString();
        }
    }
}
