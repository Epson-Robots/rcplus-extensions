// -----------------------------------------------------------------------
// <copyright file="ModelPFDataOther.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE definition data (other).
    /// </summary>
    public partial class ModelPFData
    {
        /// <summary>
        /// Represents the miscellaneous ("other") section of the ModelPF data file.
        /// </summary>
        public class Other : Base
        {
            /// <summary>
            /// Gets or sets the identifier of the last used control table contents.
            /// Stored in the file as an unsigned 16-bit value.
            /// </summary>
            public int LastCTContsId = 0;

            /// <summary>
            /// Gets or sets the tool type that was used last.
            /// Stored in the file as a single byte.
            /// </summary>
            public ToolKind LastToolType = ToolKind.S;

            /// <summary>
            /// Gets or sets the reserved bytes kept for future extensions.
            /// The array length defines the number of bytes read from / written to the file.
            /// </summary>
            public byte[] Reserved = new byte[5];

            /// <summary>
            /// Creates a copy of this instance.
            /// </summary>
            /// <returns>A new <see cref="Other"/> instance with the same values.</returns>
            public Other Clone()
            {
                return new()
                {
                    LastCTContsId = LastCTContsId,
                    LastToolType = LastToolType,

                    // Copy the array so that the clone does not share the reserved buffer.
                    Reserved = (byte[])Reserved.Clone(),
                };
            }

            /// <summary>
            /// Determines whether the specified instance has the same values as this instance.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
            public bool Equals(
                Other other
            )
            {
                return (
                    other.LastCTContsId == LastCTContsId
                    && other.LastToolType == LastToolType
                    && other.Reserved.SequenceEqual(Reserved)
                );
            }

            /// <inheritdoc />
            public override void ReadFromFile(
                BinaryReader reader,
                int phase = 0
            )
            {
                // Read the fields in the same order as they are laid out in the file.
                LastCTContsId = (int)reader.ReadUInt16();
                LastToolType = (ToolKind)reader.ReadByte();

                // Skip over the reserved area by reading it into the buffer.
                Reserved = reader.ReadBytes(Reserved.Length);
            }

            /// <inheritdoc />
            public override void WriteToFile(
                BinaryWriter writer,
                int phase = 0
            )
            {
                // Write the fields in the same order as they are laid out in the file.
                writer.Write((UInt16)LastCTContsId);
                writer.Write((byte)LastToolType);

                // Preserve the reserved area so the file layout stays fixed size.
                writer.Write(Reserved);
            }
        }
    }
}
