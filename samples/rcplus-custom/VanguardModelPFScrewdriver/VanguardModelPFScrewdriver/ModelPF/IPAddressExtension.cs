// -----------------------------------------------------------------------
// <copyright file="IPAddressExtension.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Provides helper methods to read and write <see cref="IPAddress"/> values
    /// in a binary (IPv4, 4-byte) file format.
    /// </summary>
    public  static class IPAddressExtension
    {
        /// <summary>
        /// Reads an IPv4 address from the current position of the specified binary reader.
        /// </summary>
        /// <param name="reader">The binary reader used to read the 4 address bytes.</param>
        /// <returns>The <see cref="IPAddress"/> built from the bytes that were read.</returns>
        public static IPAddress ReadFromFile(
            BinaryReader reader
        )
        {
            // An IPv4 address is stored as exactly 4 bytes.
            byte[] addressBytes = new byte[4];
            reader.Read(addressBytes);

            return new IPAddress(addressBytes);
        }

        /// <summary>
        /// Writes the specified IPv4 address to the current position of the binary writer.
        /// </summary>
        /// <param name="value">The address to write. Must be an IPv4 address.</param>
        /// <param name="writer">The binary writer that receives the 4 address bytes.</param>
        /// <exception cref="InvalidDataException">
        /// Thrown when <paramref name="value"/> is not an IPv4 (<see cref="AddressFamily.InterNetwork"/>) address.
        /// </exception>
        public static void WriteToFile(
            this IPAddress value,
            BinaryWriter writer
        )
        {
            // Only IPv4 addresses are supported by this fixed 4-byte format.
            if (value.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new InvalidDataException();
            }
            writer.Write(value.GetAddressBytes());
        }
    }
}