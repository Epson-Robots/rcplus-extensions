// -----------------------------------------------------------------------
// <copyright file="ModelPFDataPreference.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE definition data (preferences).
    /// </summary>
    public partial class ModelPFData
    {
        /// <summary>
        /// Represents the preference (environment settings) section of the ModelPF data file.
        /// </summary>
        public class Preference : Base
        {
            /// <summary>
            /// Maximum length of a path name stored in the file.
            /// </summary>
            public const int MaxPathLength = 200;

            /// <summary>
            /// Specifies the torque unit system.
            /// </summary>
            public enum TorqueUnitKind
            {
                /// <summary>Millinewton meter (mN&#183;m).</summary>
                MNM,

                /// <summary>Kilogram-force centimeter (kgf&#183;cm).</summary>
                KGFCM,
            }

            /// <summary>
            /// Specifies the storage location used to save log files.
            /// </summary>
            public enum StorageKind
            {
                /// <summary>Saved on the PC.</summary>
                PC = 0,

                /// <summary>Saved on the USB memory of the controller.</summary>
                ControllerUSB = 1,

                /// <summary>Saved on the internal flash memory of the controller.</summary>
                ControllerFlash = 2,
            }

            /// <summary>
            /// Gets or sets the torque unit system.
            /// </summary>
            public TorqueUnitKind TorqueUnit = TorqueUnitKind.MNM;

            /// <summary>
            /// Gets or sets the storage location where logs are saved.
            /// </summary>
            public StorageKind LogStorage = StorageKind.PC;

            /// <summary>
            /// Gets or sets the folder path name used for log output.
            /// </summary>
            public string LogFolder = string.Empty;

            /// <summary>
            /// Gets or sets the number of torque waveforms to keep for passed results, per storage location.
            /// The array is indexed by <see cref="StorageKind"/>.
            /// </summary>
            public int[] MaxNumPass = [100, 100, 10];

            /// <summary>
            /// Gets or sets the number of torque waveforms to keep for failed results, per storage location.
            /// The array is indexed by <see cref="StorageKind"/>.
            /// </summary>
            public int[] MaxNumFail = [100, 100, 10];

            /// <summary>
            /// Creates a copy of this instance.
            /// </summary>
            /// <returns>A new <see cref="Preference"/> instance with the same values.</returns>
            public Preference Clone()
            {
                return new()
                {
                    TorqueUnit = TorqueUnit,
                    LogStorage = LogStorage,
                    LogFolder = LogFolder,

                    // Copy the arrays so that the clone does not share the retention counts.
                    MaxNumPass = (int[])MaxNumPass.Clone(),
                    MaxNumFail = (int[])MaxNumFail.Clone(),
                };
            }

            /// <summary>
            /// Determines whether the logging related settings of the specified instance
            /// are the same as those of this instance.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if all the logging settings match; otherwise, <c>false</c>.</returns>
            public bool LoggingMatterEquals(
                Preference other
            )
            {
                // The folder path is compared case-insensitively because it is a Windows path.
                return (
                    other.LogStorage == LogStorage
                    && string.Equals(other.LogFolder, LogFolder, StringComparison.OrdinalIgnoreCase)
                    && other.MaxNumPass.SequenceEqual(MaxNumPass)
                    && other.MaxNumFail.SequenceEqual(MaxNumFail)
                );
            }

            /// <summary>
            /// Determines whether the specified instance has the same values as this instance.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if all the settings match; otherwise, <c>false</c>.</returns>
            public bool Equals(
                Preference other
            )
            {
                return (
                    other.TorqueUnit == TorqueUnit
                    && other.LoggingMatterEquals(this)
                );
            }

            /// <inheritdoc />
            public override void ReadFromFile(
                BinaryReader reader,
                int phase = 0
            )
            {
                // Read the fields in the same order as they are laid out in the file.
                TorqueUnit = (TorqueUnitKind)reader.ReadByte();
                LogStorage = (StorageKind)reader.ReadByte();
                LogFolder = StringExtension.ReadFromFile(reader, MaxPathLength);

                // Pass/Fail retention counts are stored as interleaved pairs per storage location.
                for (int i = 0; i < MaxNumPass.Length; i++)
                {
                    MaxNumPass[i] = (int)reader.ReadUInt16();
                    MaxNumFail[i] = (int)reader.ReadUInt16();
                }
            }

            /// <inheritdoc />
            public override void WriteToFile(
                BinaryWriter writer,
                int phase = 0
            )
            {
                // Write the fields in the same order as they are laid out in the file.
                writer.Write((byte)TorqueUnit);
                writer.Write((byte)LogStorage);
                LogFolder.WriteToFile(writer, MaxPathLength);

                // Pass/Fail retention counts are stored as interleaved pairs per storage location.
                for (int i = 0; i < MaxNumPass.Length; i++)
                {
                    writer.Write((ushort)MaxNumPass[i]);
                    writer.Write((ushort)MaxNumFail[i]);
                }
            }
        }
    }
}