// -----------------------------------------------------------------------
// <copyright file="ErrorLogEntry.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using System.Text;
using VanguardModelPFScrewdriver.Utils;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Represents a single error log record of the ModelPF screwdriver.
    /// </summary>
    public class ErrorLogEntry : INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the operation that caused the error.
        /// </summary>
        public enum ErrOp
        {
            /// <summary>The operation could not be determined.</summary>
            Unknown = 0,

            /// <summary>Connecting to the screwdriver.</summary>
            Connect,

            /// <summary>Stopping the current operation.</summary>
            Stop,

            /// <summary>Tightening a screw.</summary>
            Tighten,

            /// <summary>Loosening a screw.</summary>
            Loosen,

            /// <summary>Running the driver without load.</summary>
            FreeRun,

            /// <summary>Querying the device status.</summary>
            CheckStatus,

            /// <summary>Retrieving the tightening result.</summary>
            GetTighteningResult,

            /// <summary>Retrieving the torque data.</summary>
            GetTorqueData,

            /// <summary>Downloading a program to the device.</summary>
            DownloadProgam,

            /// <summary>Uploading a program from the device.</summary>
            UploadProgram,

            /// <summary>Downloading the network configuration to the device.</summary>
            DownloadNetworkConfig,

            /// <summary>Uploading the network configuration from the device.</summary>
            UploadNetworkConfig,

            /// <summary>Downloading the system information to the device.</summary>
            DownloadSysInfo,
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Defines the column order of the log fields in the source data.
        /// </summary>
        public enum FieldIndex
        {
            /// <summary>Index of the time stamp field.</summary>
            TimeStamp = 0,

            /// <summary>Index of the error operation field.</summary>
            ErrorOperation,
        }

        /// <summary>
        /// Gets or sets the line number of this entry in the log file.
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the error occurred.
        /// </summary>
        [LogField]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the operation that caused the error. See <see cref="ErrOp"/>.
        /// </summary>
        [LogField]
        public int ErrorOperation { get; set; }

        /// <summary>
        /// Cached type converters used to parse the log field values.
        /// </summary>
        private readonly static Dictionary<Type, TypeConverter> _converters = [];

        /// <summary>
        /// Initializes the type converter cache for every property marked with <see cref="LogFieldAttribute"/>.
        /// </summary>
        static ErrorLogEntry()
        {
            foreach (var propInfo in typeof(ErrorLogEntry).GetProperties())
            {
                if (propInfo.CustomAttributes.Any(x => x.AttributeType == typeof(LogFieldAttribute)))
                {
                    _converters[propInfo.PropertyType] = TypeDescriptor.GetConverter(propInfo.PropertyType);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogEntry"/> class.
        /// </summary>
        /// <param name="lineNo">The line number of this entry in the log file.</param>
        public ErrorLogEntry(
            int lineNo
        )
        {
            LineNo = lineNo;

            // Refresh the displayed operation name when the UI language is switched.
            Main.Captions!.LanguageChanged += () =>
            {
                PropertyChanged?.Invoke(this, new(nameof(ErrorOperation)));
            };
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
                        var propInfo = typeof(ErrorLogEntry).GetProperty(((FieldIndex)i).ToString());
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
        /// <returns>A comma separated string containing the time stamp and the error operation.</returns>
        public string GetCSV()
        {
            const char _separator = ',';

            StringBuilder builder = new();

            builder.Append(TimeStamp.ToString("yyyy/MM/dd HH:mm:ss"));
            builder.Append(_separator);
            builder.Append(ErrorOperation);

            return builder.ToString();
        }
    }
}
