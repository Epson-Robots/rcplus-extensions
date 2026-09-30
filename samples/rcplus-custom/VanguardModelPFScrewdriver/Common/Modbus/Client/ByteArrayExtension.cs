// -----------------------------------------------------------------------
// <copyright file="ByteArrayExtension.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Extention for a Byte Array
    /// </summary>
    internal static class ByteArrayExtension
    {
        /// <summary>
        /// Retrieve a big-endian ushort value from a byte array.
        /// </summary>
        /// <param name="buffer">A byte array.</param>
        /// <param name="offset">Position to retrieve.</param>
        /// <returns>A value.</returns>
        internal static ushort GetBE(
            this byte[] buffer,
            int offset
        )
        {
            return (ushort)(
                ((ushort)buffer[offset] << 8)
                + (ushort)buffer[offset + 1]
            );
        }

        /// <summary>
        /// Retrieve big-endian ushort values from a byte array.
        /// </summary>
        /// <param name="buffer">A byte array.</param>
        /// <param name="offset">Position to retrieve.</param>
        /// <param name="count">Number of values to retrieve.</param>
        /// <returns>An array.</returns>
        internal static ushort[] GetBE(
            this byte[] buffer,
            int offset,
            int count
        )
        {
            var values = new ushort[count];

            for (int index = 0; index < count; index++)
            {
                values[index] = buffer.GetBE(offset);
                offset += sizeof(ushort);
            }

            return values;
        }
    }
}
