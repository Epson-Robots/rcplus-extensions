// -----------------------------------------------------------------------
// <copyright file="TorqueWaveformData.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.IO;
using System.Text;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Holds a torque waveform acquired from the ModelPF screwdriver and
    /// provides serialization to and from the binary waveform file format.
    /// </summary>
    public class TorqueWaveformData
    {
        /// <summary>
        /// Defines the layout (byte offsets and sizes) of the waveform file header.
        /// </summary>
        public class HeaderConsts
        {
            /// <summary>Total size of the header in bytes.</summary>
            public const int HeaderSize = 32;

            /// <summary>Offset of the magic number.</summary>
            public const int MagicOffset = 0;

            /// <summary>Offset of the major version number.</summary>
            public const int VersionMajorOffset = 4;

            /// <summary>Offset of the minor version number.</summary>
            public const int VersionMinorOffset = 5;

            /// <summary>Offset of the patch version number.</summary>
            public const int VersionPatchOffset = 6;

            /// <summary>Offset of the sample count (16-bit little endian).</summary>
            public const int CountOffset = 8;

            /// <summary>Offset of the sampling angle (16-bit little endian).</summary>
            public const int SamplingAngleOffset = 10;

            /// <summary>Offset of the time stamp string.</summary>
            public const int TimeStampOffset = 16;

            /// <summary>Length of the time stamp string in bytes.</summary>
            public const int TimeStampLength = 14;
        }

        /// <summary>
        /// Magic number identifying the waveform file format ("PFTW").
        /// </summary>
        public static readonly byte[] _magic = [(byte)'P', (byte)'F', (byte)'T', (byte)'W'];

        /// <summary>
        /// Major version of the supported file format.
        /// </summary>
        private const byte _versionMajor = 1;

        /// <summary>
        /// Minor version of the supported file format.
        /// </summary>
        private const byte _versionMinor = 0;

        /// <summary>
        /// Patch version of the supported file format.
        /// </summary>
        private const byte _versionPatch = 0;

        /// <summary>
        /// Format of the time stamp stored in the header.
        /// </summary>
        private const string _timeStampFormat = "yyyyMMddHHmmss";

        /// <summary>
        /// Gets or sets the number of torque samples.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the sampling angle interval between two samples.
        /// </summary>
        public int SamplingAngle { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the waveform was measured.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the torque samples.
        /// </summary>
        public float[] Torques { get; set; } = [];

        /// <summary>
        /// Gets or sets the minimum and maximum torque values of <see cref="Torques"/>.
        /// </summary>
        public FloatRange TorqueRange { get; set; } = new(float.MaxValue, float.MinValue);

        /// <summary>
        /// Writes this waveform to the specified file.
        /// </summary>
        /// <param name="path">The destination file path.</param>
        public void Save(
            string path
        )
        {
            try
            {
                using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                using BinaryWriter writer = new(fileStream);

                var header = new byte[HeaderConsts.HeaderSize];

                // Magic number and format version.
                Array.Copy(_magic, 0, header, HeaderConsts.MagicOffset, _magic.Length);
                header[HeaderConsts.VersionMajorOffset] = _versionMajor;
                header[HeaderConsts.VersionMinorOffset] = _versionMinor;
                header[HeaderConsts.VersionPatchOffset] = _versionPatch;

                // Sample count and sampling angle, stored as 16-bit little endian values.
                header[HeaderConsts.CountOffset] = (byte)(Count & 0xff);
                header[HeaderConsts.CountOffset + 1] = (byte)((Count >> 8) & 0xff);
                header[HeaderConsts.SamplingAngleOffset] = (byte)(SamplingAngle & 0xff);
                header[HeaderConsts.SamplingAngleOffset + 1] = (Byte)((SamplingAngle >> 8) & 0xff);

                // Measurement time stamp as a fixed length ASCII string.
                Array.Copy(Encoding.ASCII.GetBytes(TimeStamp.ToString("yyyyMMddHHmmss")), 0, header, HeaderConsts.TimeStampOffset, HeaderConsts.TimeStampLength);

                writer.Write(header);

                // Torque values are stored in 0.1 units to fit into an unsigned 16-bit integer.
                foreach (var torque in Torques)
                {
                    writer.Write((ushort)(torque * 10f));
                }

                return;
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.SaveWaveDataFailed);
            }

            // Remove the incomplete file created before the failure.
            File.Delete(path);
        }

        /// <summary>
        /// Reads a waveform from the specified file.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <returns>The loaded waveform, or null when the file is invalid or cannot be read.</returns>
        public static TorqueWaveformData? Load(
            string path
        )
        {
            try
            {
                using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using BinaryReader reader = new(fileStream);

                var header = new byte[HeaderConsts.HeaderSize];

                reader.Read(header);

                // Reject files with an unexpected magic number or an unsupported format version.
                ReadOnlySpan<byte> realMagic = header.AsSpan()[HeaderConsts.MagicOffset.._magic.Length];
                ReadOnlySpan<byte> expectedMagic = _magic;
                if (
                    !realMagic.SequenceEqual(expectedMagic)
                    || header[HeaderConsts.VersionMajorOffset] != _versionMajor
                    || header[HeaderConsts.VersionMinorOffset] != _versionMinor
                    || header[HeaderConsts.VersionPatchOffset] != _versionPatch
                )
                {
                    throw new InvalidDataException();
                }

                // Restore the 16-bit little endian values from the header.
                TorqueWaveformData data = new()
                {
                    Count = header[HeaderConsts.CountOffset] + header[HeaderConsts.CountOffset + 1] * 256,
                    SamplingAngle = header[HeaderConsts.SamplingAngleOffset] + header[HeaderConsts.SamplingAngleOffset + 1] * 256,
                };

                var timeStampString = Encoding.ASCII.GetString(
                    header[HeaderConsts.TimeStampOffset..(HeaderConsts.TimeStampOffset + HeaderConsts.TimeStampLength)]
                );
                data.TimeStamp = DateTime.ParseExact(timeStampString, _timeStampFormat, CultureInfo.InvariantCulture);

                // Read the samples and track the value range at the same time.
                data.Torques = new float[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    var torque = (float)reader.ReadUInt16() / 10f;
                    data.Torques[i] = torque;
                    if (torque < data.TorqueRange.Min)
                    {
                        data.TorqueRange.Min = torque;
                    }
                    if (torque > data.TorqueRange.Max)
                    {
                        data.TorqueRange.Max = torque;
                    }
                }

                return data;
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.LoadWaveDataFailed);
            }

            return null;
        }
    }
}
