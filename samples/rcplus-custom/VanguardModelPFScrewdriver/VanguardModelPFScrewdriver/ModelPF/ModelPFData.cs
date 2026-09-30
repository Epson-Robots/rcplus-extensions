// -----------------------------------------------------------------------
// <copyright file="ModelPFData.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using System.IO;
using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE definition data.
    /// </summary>
    public partial class ModelPFData : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the flag indicating whether the data has unsaved changes.
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    PropertyChanged?.Invoke(this, new(nameof(IsDirty)));
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="IsDirty"/>.
        /// </summary>
        private bool _isDirty = false;

        /// <summary>
        /// Updates <see cref="IsDirty"/> by comparing the current data with the original snapshot.
        /// </summary>
        public void SetDirty()
        {
            IsDirty = !(_original?.Equals(this) == true);
        }

        /// <summary>
        /// Clears the dirty flag, marking the data as saved.
        /// </summary>
        public void ResetDirty()
        {
            IsDirty = false;
        }

        // ======================================================================

        /// <summary>
        /// Base class for the nested data classes.
        /// </summary>
        public class Base
        {
            /// <summary>
            /// Reads the data from a file.
            /// </summary>
            /// <param name="reader">Binary reader.</param>
            /// <param name="phase">Phase number.</param>
            public virtual void ReadFromFile(
                BinaryReader reader,
                int phase = 0
            )
            {
            }

            /// <summary>
            /// Writes the data to a file.
            /// </summary>
            /// <param name="writer">Binary writer.</param>
            /// <param name="phase">Phase number.</param>
            public virtual void WriteToFile(
                BinaryWriter writer,
                int phase = 0
            )
            {
            }

            /// <summary>
            /// Downloads the data from the device.
            /// </summary>
            /// <param name="client">PRO-FUSE client.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> if the download succeeded; otherwise, <c>false</c>.</returns>
            public virtual Task<bool> DownloadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                return Task.FromResult(false);
            }

            /// <summary>
            /// Uploads the data to the device.
            /// </summary>
            /// <param name="client">PRO-FUSE client.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> if the upload succeeded; otherwise, <c>false</c>.</returns>
            public virtual Task<bool> UploadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                return Task.FromResult(false);
            }

            /// <summary>
            /// Clamps the data so that it stays within the valid range of the specified tool type.
            /// </summary>
            /// <param name="toolType">Tool type used to determine the valid range.</param>
            public virtual void Adjust(
                ToolKind toolType
            )
            {
            }
        }

        // ======================================================================

        /// <summary>
        /// Clamps a torque value within the valid range defined by the hint data.
        /// </summary>
        public class TorqueAdjuster
        {
            /// <summary>
            /// Hint entry that provides the valid torque range, or <c>null</c> if not found.
            /// </summary>
            private readonly HintData.Entry? _entry;

            /// <summary>
            /// Lower limit of the torque value.
            /// </summary>
            private readonly float _min = float.MinValue;

            /// <summary>
            /// Upper limit of the torque value.
            /// </summary>
            private readonly float _max = float.MaxValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="TorqueAdjuster"/> class.
            /// </summary>
            /// <param name="toolType">Tool type used to look up the hint data.</param>
            /// <param name="mode">Tightening mode used to look up the hint data.</param>
            public TorqueAdjuster(
                ToolKind toolType,
                ModeKind mode
            )
            {
                _entry = HintData.Instance.Find(HintId.Torque, toolType, mode);

                if (_entry != null)
                {
                    _min = (float)_entry.Min;
                    _max = (float)_entry.Max;

                    if (_entry.Factor != 0)
                    {
                        _min /= (float)_entry.Factor;
                        _max /= (float)_entry.Factor;
                    }
                }
            }

            /// <summary>
            /// Clamps the specified torque value into the valid range.
            /// </summary>
            /// <param name="torqueField">Torque value to be clamped.</param>
            public void Adjust(
                ref float torqueField
            )
            {
                if (torqueField < _min)
                {
                    torqueField = _min;
                }
                else if (torqueField > _max)
                {
                    torqueField = _max;
                }
            }
        }

        // ======================================================================

        /// <summary>
        /// Constants related to the file header.
        /// </summary>
        public class HeaderConsts
        {
            /// <summary>
            /// Size of the header in bytes.
            /// </summary>
            public const int HeaderSize = 16;

            /// <summary>
            /// Byte offset of the magic number.
            /// </summary>
            public const int MagicOffset = 0;

            /// <summary>
            /// Byte offset of the major version number.
            /// </summary>
            public const int VersionMajorOffset = 8;

            /// <summary>
            /// Byte offset of the minor version number.
            /// </summary>
            public const int VersionMinorOffset = 9;

            /// <summary>
            /// Byte offset of the patch version number.
            /// </summary>
            public const int VersionPatchOffset = 10;
        }

        /// <summary>
        /// Magic number identifying the file format.
        /// </summary>
        private static readonly byte[] _magic = [(byte)'V', (byte)'G', (byte)'D', (byte)'P', (byte)'F'];

        /// <summary>
        /// Major version number.
        /// </summary>
        private const byte _versionMajor = 1;

        /// <summary>
        /// Minor version number.
        /// </summary>
        private const byte _versionMinor = 0;

        /// <summary>
        /// Patch version number.
        /// </summary>
        private const byte _versionPatch = 0;

        /// <summary>
        /// Gets or sets the preference settings.
        /// </summary>
        public Preference PreferenceData = new();

        /// <summary>
        /// Gets or sets the network settings.
        /// </summary>
        public Network NetworkData = new();

        /// <summary>
        /// Gets or sets the system information.
        /// </summary>
        public SysInfo SysInfoData = new();

        /// <summary>
        /// Gets or sets the programs.
        /// </summary>
        public List<Program> ProgramData = [.. Enumerable.Range(0, 16).Select(x => new Program(x))];

        /// <summary>
        /// Gets or sets the vacuum control settings.
        /// </summary>
        public Vacuum VacuumData = new();

        /// <summary>
        /// Gets or sets the other settings.
        /// </summary>
        public Other OtherData = new();

        /// <summary>
        /// Snapshot of the data taken when it was last loaded or saved, used for dirty detection.
        /// </summary>
        private ModelPFData? _original;

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        /// <returns>A new <see cref="ModelPFData"/> instance with the same content.</returns>
        public ModelPFData Clone()
        {
            ModelPFData clone = new()
            {
                PreferenceData = PreferenceData.Clone(),
                NetworkData = NetworkData.Clone(),
                SysInfoData = SysInfoData.Clone(),
                VacuumData = VacuumData.Clone(),
                OtherData = OtherData.Clone(),
            };

            // Programs are pre-allocated in the constructor, so copy them element by element.
            for (var i = 0; i < ProgramData.Count; i++)
            {
                clone.ProgramData[i] = ProgramData[i].Clone();
            }

            return clone;
        }

        /// <summary>
        /// Determines whether the specified instance has the same content as this instance.
        /// </summary>
        /// <param name="other">Instance to compare with.</param>
        /// <returns><c>true</c> if all the data match; otherwise, <c>false</c>.</returns>
        public bool Equals(
            ModelPFData other
        )
        {
            for (var i = 0; i < ProgramData.Count; i++)
            {
                if (!other.ProgramData[i].Equals(ProgramData[i]))
                {
                    return false;
                }
            }

            return (
                other.PreferenceData.Equals(PreferenceData)
                && other.NetworkData.Equals(NetworkData)
                && other.SysInfoData.Equals(SysInfoData)
                && other.VacuumData.Equals(VacuumData)
                && other.OtherData.Equals(OtherData)
            );
        }

        /// <summary>
        /// Stores the baseline snapshot used for dirty detection.
        /// </summary>
        /// <param name="original">
        /// Snapshot to store. If <c>null</c>, a clone of the current data is used.
        /// </param>
        public void SetOriginal(
            ModelPFData? original = null
        )
        {
            _original = original ?? Clone();
        }

        /// <summary>
        /// Reads the header.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <exception cref="InvalidDataException">The header content is unexpected.</exception>
        public static void ReadHeader(
            BinaryReader reader
        )
        {
            byte[] header = new byte[HeaderConsts.HeaderSize];

            reader.Read(header);

            // Verify the magic number and the format version.
            ReadOnlySpan<byte> realMagic = header.AsSpan()[0.._magic.Length];
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
        }

        /// <summary>
        /// Reads all data from a file.
        /// </summary>
        /// <param name="fileStream">File stream.</param>
        /// <exception cref="InvalidDataException">The header content is unexpected.</exception>
        public void ReadFromFile(
            FileStream fileStream
        )
        {
            using BinaryReader reader = new(fileStream);

            // The sections must be read in the same order as they are written.
            ReadHeader(reader);
            //Debug.Print($"Header = {reader.BaseStream.Position}");
            PreferenceData.ReadFromFile(reader);
            //Debug.Print($"Preference = {reader.BaseStream.Position}");
            NetworkData.ReadFromFile(reader);
            //Debug.Print($"Network = {reader.BaseStream.Position}");
            foreach (var data in ProgramData)
            {
                data.ReadFromFile(reader);
            }
            //Debug.Print($"Program = {reader.BaseStream.Position}");
            VacuumData.ReadFromFile(reader);
            //Debug.Print($"Vacuum = {reader.BaseStream.Position}");
            OtherData.ReadFromFile(reader);
            //Debug.Print($"Other = {reader.BaseStream.Position}");

            ResetDirty();
        }

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="writer">Binary writer.</param>
        private static void WriteHeader(
            BinaryWriter writer
        )
        {
            byte[] header = new byte[HeaderConsts.HeaderSize];

            // The unused bytes are left as zero.
            Array.Copy(_magic, header, _magic.Length);
            header[HeaderConsts.VersionMajorOffset] = _versionMajor;
            header[HeaderConsts.VersionMinorOffset] = _versionMinor;
            header[HeaderConsts.VersionPatchOffset] = _versionPatch;

            writer.Write(header);
        }

        /// <summary>
        /// Writes all data to a file.
        /// </summary>
        /// <param name="fileStream">File stream.</param>
        public void WriteToFile(
            FileStream fileStream
        )
        {
            using BinaryWriter writer = new(fileStream);

            // The sections must be written in the same order as they are read.
            WriteHeader(writer);
            PreferenceData.WriteToFile(writer);
            NetworkData.WriteToFile(writer);
            foreach (var data in ProgramData)
            {
                data.WriteToFile(writer);
            }
            VacuumData.WriteToFile(writer);
            OtherData.WriteToFile(writer);

            // The saved content becomes the new baseline.
            SetOriginal();
            ResetDirty();
        }

        /// <summary>
        /// Clamps all the data so that they stay within the valid range of the specified tool type.
        /// </summary>
        /// <param name="newToolType">Tool type used to determine the valid ranges.</param>
        /// <param name="force">
        /// <c>true</c> to adjust the data even if the tool type is unchanged; otherwise, <c>false</c>.
        /// </param>
        public void Adjust(
            ToolKind newToolType,
            bool force = false
        )
        {
            // Nothing to do when the tool type has not been changed.
            if (!force && OtherData.LastToolType == newToolType)
            {
                return;
            }
            OtherData.LastToolType = newToolType;

            PreferenceData.Adjust(newToolType);
            NetworkData.Adjust(newToolType);
            foreach (var data in ProgramData)
            {
                data.Adjust(newToolType);
            }
            VacuumData.Adjust(newToolType);
            OtherData.Adjust(newToolType);

            SetDirty();
        }
    }
}
