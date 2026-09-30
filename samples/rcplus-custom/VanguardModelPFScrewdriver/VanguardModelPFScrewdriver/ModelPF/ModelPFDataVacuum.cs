// -----------------------------------------------------------------------
// <copyright file="ModelPFDataVacuum.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE definition data (vacuum control).
    /// </summary>
    public partial class ModelPFData
    {
        /// <summary>
        /// Represents the vacuum control section of the ModelPF data file.
        /// </summary>
        public class Vacuum : Base
        {
            /// <summary>
            /// Maximum length of an I/O label stored in the file.
            /// </summary>
            public const int MaxLabelLength = 32;

            /// <summary>
            /// Gets or sets the I/O label used to turn the vacuum on.
            /// </summary>
            public string IOLabelVacuumOn = string.Empty;

            /// <summary>
            /// Gets or sets the I/O label used to turn the vacuum break on.
            /// </summary>
            public string IOLabelVacuumBreakOn = string.Empty;

            /// <summary>
            /// Gets or sets the I/O label used to monitor the suction state.
            /// </summary>
            public string IOLabelSuctionState = string.Empty;

            /// <summary>
            /// Creates a copy of this instance.
            /// </summary>
            /// <returns>A new <see cref="Vacuum"/> instance with the same values.</returns>
            public Vacuum Clone()
            {
                return new()
                {
                    IOLabelVacuumOn = IOLabelVacuumOn,
                    IOLabelVacuumBreakOn = IOLabelVacuumBreakOn,
                    IOLabelSuctionState = IOLabelSuctionState,
                };
            }

            /// <summary>
            /// Determines whether the specified instance has the same values as this instance.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if all the labels match; otherwise, <c>false</c>.</returns>
            public bool Equals(
                Vacuum other
            )
            {
                // The labels are compared case-insensitively because I/O labels are case-insensitive.
                return (
                    string.Equals(other.IOLabelVacuumOn, IOLabelVacuumOn, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(other.IOLabelVacuumBreakOn, IOLabelVacuumBreakOn, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(other.IOLabelSuctionState, IOLabelSuctionState, StringComparison.OrdinalIgnoreCase)
                );
            }

            /// <inheritdoc />
            public override void ReadFromFile(
                BinaryReader reader,
                int phase = 0
            )
            {
                // Read the labels in the same order as they are laid out in the file.
                IOLabelVacuumOn = StringExtension.ReadFromFile(reader, MaxLabelLength);
                IOLabelVacuumBreakOn = StringExtension.ReadFromFile(reader, MaxLabelLength);
                IOLabelSuctionState = StringExtension.ReadFromFile(reader, MaxLabelLength);
            }

            /// <inheritdoc />
            public override void WriteToFile(
                BinaryWriter writer,
                int phase = 0
            )
            {
                // Write the labels in the same order as they are laid out in the file.
                IOLabelVacuumOn.WriteToFile(writer, MaxLabelLength);
                IOLabelVacuumBreakOn.WriteToFile(writer, MaxLabelLength);
                IOLabelSuctionState.WriteToFile(writer, MaxLabelLength);
            }
        }
    }
}
