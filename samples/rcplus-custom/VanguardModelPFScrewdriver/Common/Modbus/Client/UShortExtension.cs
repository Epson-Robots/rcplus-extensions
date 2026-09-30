// -----------------------------------------------------------------------
// <copyright file="UShortExtension.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Extension for ushort
    /// </summary>
    internal static class UShortExtension
    {
        /// <summary>
        /// Write a ushort value to a byte array in big-endian order.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <param name="buffer">A byte array.</param>
        /// <param name="offset">Position to write.</param>
        internal static void SetBE(
            this ushort value,
            byte[] buffer,
            int offset
        )
        {
            ArgumentNullException.ThrowIfNull(buffer);
            if (offset < 0 || buffer.Length < offset + sizeof(ushort))
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            buffer[offset] = (byte)(value >> 8);
            buffer[offset + 1] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Write ushort values to a byte array in big-endian order.
        /// </summary>
        /// <param name="values">A ushort array.</param>
        /// <param name="buffer">A byte array.</param>
        /// <param name="offset">Position to write.</param>
        internal static void SetBE(
            this ushort[] values,
            byte[] buffer,
            int offset
        )
        {
            for (int index = 0; index < values.Length; ++index)
            {
                values[index].SetBE(buffer, offset);
                offset += sizeof(ushort);
            }
        }
    }
}
