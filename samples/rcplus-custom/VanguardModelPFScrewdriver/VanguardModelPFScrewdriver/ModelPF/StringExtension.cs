// -----------------------------------------------------------------------
// <copyright file="StringExtension.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Text;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Provides helper methods to read and write fixed-size, length-prefixed UTF-8 strings.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Reads a length-prefixed UTF-8 string that occupies a fixed-size block in the stream.
        /// </summary>
        /// <param name="reader">The reader positioned at the length byte of the string block.</param>
        /// <param name="fixedSize">The size, in bytes, of the block that stores the string data.</param>
        /// <returns>The decoded string.</returns>
        public static string ReadFromFile(
            BinaryReader reader,
            byte fixedSize
        )
        {
            // The first byte holds the actual number of valid bytes in the block.
            var length = reader.ReadByte();

            // Always consume the whole fixed-size block to keep the stream position aligned.
            var buffer = new byte[fixedSize];
            reader.Read(buffer);

            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        /// <summary>
        /// Writes the string as a length-prefixed UTF-8 sequence padded to a fixed-size block.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="writer">The writer used to output the data.</param>
        /// <param name="fixedSize">The size, in bytes, of the block that stores the string data.</param>
        /// <exception cref="InvalidDataException">
        /// Thrown when the encoded string does not fit into <paramref name="fixedSize"/> bytes.
        /// </exception>
        public static void WriteToFile(
            this string value,
            BinaryWriter writer,
            byte fixedSize
        )
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > fixedSize)
            {
                // The encoded data exceeds the reserved block size.
                throw new InvalidDataException();
            }

            // Copy into a zero-filled buffer so the remaining bytes act as padding.
            var buffer = new byte[fixedSize];
            Array.Copy(bytes, buffer, bytes.Length);

            // Write the actual length first, then the padded block.
            writer.Write((byte)bytes.Length);
            writer.Write(buffer);
        }
    }
}