// -----------------------------------------------------------------------
// <copyright file="ModelPFDataProgram.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFRegister;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE definition data (program).
    /// </summary>
    public partial class ModelPFData
    {
        /// <summary>
        /// Represents a single screw fastening program.
        /// </summary>
        public class Program : Base
        {
            /// <summary>
            /// Maximum length of a program name.
            /// </summary>
            public const int MaxProgramNameLength = 16;

            /// <summary>
            /// Specifies the rotating direction of the driver.
            /// </summary>
            public enum DirectionKind
            {
                /// <summary>Clockwise.</summary>
                CW = 0,

                /// <summary>Counterclockwise.</summary>
                CCW = 1,
            }

            /// <summary>
            /// Judgement window applied to the torque waveform.
            /// </summary>
            public class Window : Base
            {
                /// <summary>
                /// Zero-based index of this window within the program.
                /// </summary>
                public int Index;

                /// <summary>
                /// Gets or sets a value indicating whether this window is enabled.
                /// </summary>
                public bool IsEnabled = false;

                /// <summary>
                /// Gets or sets the number of turns at which the window starts.
                /// </summary>
                public float StartTurn = 1f;

                /// <summary>
                /// Gets or sets the number of turns at which the window ends.
                /// </summary>
                public float EndTurn = 2f;

                /// <summary>
                /// Gets or sets the torque at the start of the window.
                /// </summary>
                public float TorqueAtStart;

                /// <summary>
                /// Gets or sets the torque at the end of the window.
                /// </summary>
                public float TorqueAtEnd;

                /// <summary>
                /// Gets or sets the torque tolerance width of the window.
                /// </summary>
                public float TorqueWidth = 5f;

                /// <summary>
                /// Initializes a new instance of the <see cref="Window"/> class.
                /// </summary>
                /// <param name="index">Zero-based index of the window within the program.</param>
                public Window(
                    int index
                )
                {
                    Index = index;
                }

                /// <summary>
                /// Creates a copy of this instance.
                /// </summary>
                /// <returns>A new <see cref="Window"/> instance with the same values.</returns>
                public Window Clone()
                {
                    return new(Index)
                    {
                        IsEnabled = IsEnabled,
                        StartTurn = StartTurn,
                        EndTurn = EndTurn,
                        TorqueAtStart = TorqueAtStart,
                        TorqueAtEnd = TorqueAtEnd,
                        TorqueWidth = TorqueWidth,
                    };
                }

                /// <summary>
                /// Determines whether the specified instance has the same values as this instance.
                /// </summary>
                /// <param name="other">Instance to compare with.</param>
                /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
                public bool Equals(
                    Window other
                )
                {
                    return (
                        other.Index == Index
                        && other.IsEnabled == IsEnabled
                        && other.StartTurn == StartTurn
                        && other.EndTurn == EndTurn
                        && other.TorqueAtStart == TorqueAtStart
                        && other.TorqueAtEnd == TorqueAtEnd
                        && other.TorqueWidth == TorqueWidth
                    );
                }

                /// <inheritdoc />
                public override void ReadFromFile(
                    BinaryReader reader,
                    int phase = 0
                )
                {
                    // The values are stored as fixed point numbers scaled by 10.
                    IsEnabled = reader.ReadUInt16() != 0;
                    StartTurn = (float)reader.ReadUInt16() / 10f;
                    EndTurn = (float)reader.ReadUInt16() / 10f;
                    TorqueAtStart = (float)reader.ReadUInt16() / 10f;
                    TorqueAtEnd = (float)reader.ReadUInt16() / 10f;
                    TorqueWidth = (float)reader.ReadUInt16() / 10f;
                }

                /// <inheritdoc />
                public override void WriteToFile(
                    BinaryWriter writer,
                    int phase = 0
                )
                {
                    // The values are stored as fixed point numbers scaled by 10.
                    writer.Write((ushort)(IsEnabled ? 1 : 0));
                    writer.Write((ushort)(StartTurn * 10f));
                    writer.Write((ushort)(EndTurn * 10f));
                    writer.Write((ushort)(TorqueAtStart * 10f));
                    writer.Write((ushort)(TorqueAtEnd * 10f));
                    writer.Write((ushort)(TorqueWidth * 10f));
                }

                /// <summary>
                /// Restores this window from the register values read from the device.
                /// </summary>
                /// <param name="regValues">Register values read from the device.</param>
                /// <param name="offset">Index of the first register of this window.</param>
                public void SetFromRegValues(
                    ushort[] regValues,
                    int offset
                )
                {
                    IsEnabled = regValues[offset + Holding.OffsetWindowIsEnabled] != 0;
                    StartTurn = (float)regValues[offset + Holding.OffsetWindowStartTurn] / 10f;
                    EndTurn = (float)regValues[offset + Holding.OffsetWindowEndTurn] / 10f;
                    TorqueAtStart = (float)regValues[offset + Holding.OffsetWindowTorqueAtStart] / 10f;
                    TorqueAtEnd = (float)regValues[offset + Holding.OffsetWindowTorqueAtEnd] / 10f;
                    TorqueWidth = (float)regValues[offset + Holding.OffsetWindowTorqueWidth] / 10f;
                }

                /// <summary>
                /// Stores this window into the register buffer to be written to the device.
                /// </summary>
                /// <param name="regValues">Register buffer to write to.</param>
                /// <param name="offset">Index of the first register of this window.</param>
                public void SetToRegValues(
                    ushort[] regValues,
                    int offset
                )
                {
                    regValues[offset + Holding.OffsetWindowIsEnabled] = (ushort)(IsEnabled ? 1 : 0);
                    regValues[offset + Holding.OffsetWindowStartTurn] = (ushort)(StartTurn * 10f);
                    regValues[offset + Holding.OffsetWindowEndTurn] = (ushort)(EndTurn * 10f);
                    regValues[offset + Holding.OffsetWindowTorqueAtStart] = (ushort)(TorqueAtStart * 10f);
                    regValues[offset + Holding.OffsetWindowTorqueAtEnd] = (ushort)(TorqueAtEnd * 10f);
                    regValues[offset + Holding.OffsetWindowTorqueWidth] = (ushort)(TorqueWidth * 10f);
                }

                /// <inheritdoc />
                public override void Adjust(
                    ToolKind toolType
                )
                {
                    // The adjuster is prepared by the owning Program before this call.
                    if (_torqueAdjuster != null)
                    {
                        _torqueAdjuster.Adjust(ref TorqueAtStart);
                        _torqueAdjuster.Adjust(ref TorqueAtEnd);
                    }
                }
            }

            /// <summary>
            /// Single step of a rotating sequence.
            /// </summary>
            public class Step : Base
            {
                /// <summary>
                /// Gets or sets the number of turns of this step.
                /// </summary>
                public float NumTurns { get; set; } = 0f;

                /// <summary>
                /// Gets or sets the rotating speed of this step.
                /// </summary>
                public int Speed { get; set; } = 60;

                /// <summary>
                /// Creates a copy of this instance.
                /// </summary>
                /// <returns>A new <see cref="Step"/> instance with the same values.</returns>
                public Step Clone()
                {
                    return new()
                    {
                        NumTurns = NumTurns,
                        Speed = Speed,
                    };
                }

                /// <summary>
                /// Determines whether the specified instance has the same values as this instance.
                /// </summary>
                /// <param name="other">Instance to compare with.</param>
                /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
                public bool Equals(
                    Step other
                )
                {
                    return (
                        other.NumTurns == NumTurns
                        && other.Speed == Speed
                    );
                }

                /// <inheritdoc />
                public override void ReadFromFile(
                    BinaryReader reader,
                    int phase = 0
                )
                {
                    // The number of turns is stored as a fixed point number scaled by 10.
                    NumTurns = (float)reader.ReadUInt16() / 10f;
                    Speed = reader.ReadUInt16();
                }

                /// <inheritdoc />
                public override void WriteToFile(
                    BinaryWriter writer,
                    int phase = 0
                )
                {
                    // The number of turns is stored as a fixed point number scaled by 10.
                    writer.Write((ushort)(NumTurns * 10f));
                    writer.Write((ushort)Speed);
                }

                /// <summary>
                /// Restores this step from the register values read from the device.
                /// </summary>
                /// <param name="regValues">Register values read from the device.</param>
                /// <param name="offset">Index of the first register of this step.</param>
                public void SetFromRegValues(
                    ushort[] regValues,
                    int offset
                )
                {
                    NumTurns = (float)regValues[offset + Holding.SubOffsetNumTurns] / 10f;
                    Speed = (int)regValues[offset + Holding.SubOffsetSpeed];
                }

                /// <summary>
                /// Stores this step into the register buffer to be written to the device.
                /// </summary>
                /// <param name="regValues">Register buffer to write to.</param>
                /// <param name="offset">Index of the first register of this step.</param>
                public void SetToRegValues(
                    ushort[] regValues,
                    int offset
                )
                {
                    regValues[offset + Holding.SubOffsetNumTurns] = (ushort)(NumTurns * 10f);
                    regValues[offset + Holding.SubOffsetSpeed] = (ushort)Speed;
                }
            }

            /// <summary>
            /// Screw tightening program.
            /// </summary>
            public class TighteningProgram : Base
            {
                /// <summary>
                /// Flags indicating which special parameters are enabled.
                /// </summary>
                [Flags]
                public enum Special
                {
                    /// <summary>No special parameter is enabled.</summary>
                    None = 0,

                    /// <summary>The amount used to detect the start of tightening is enabled.</summary>
                    StartDetectionAmount = (1 << 0),

                    /// <summary>The initial tapping torque is enabled.</summary>
                    InitialTappingTorque = (1 << 1),

                    /// <summary>The torque-up detection time is enabled.</summary>
                    TorqueUpDetectionTime = (1 << 2),

                    /// <summary>The permissible number of turns on the minus side is enabled.</summary>
                    PermissibleTurnsBelow = (1 << 3),

                    /// <summary>The permissible number of turns on the plus side is enabled.</summary>
                    PermissibleTurnsAbove = (1 << 4),

                    /// <summary>The further tightening angle is enabled.</summary>
                    FurtherTighteningAngle = (1 << 5),

                    /// <summary>The further tightening angle applied after the screw protrudes is enabled.</summary>
                    FurtherTighteningAngleForProtruding = (1 << 6),

                    /// <summary>The torque threshold used to change the speed is enabled.</summary>
                    TorqueThresholdToChangeSpeed = (1 << 7),

                    /// <summary>The speed applied after the change is enabled.</summary>
                    SpeedAfterChanged = (1 << 8),
                }

                /// <summary>
                /// Specifies the tightening mode.
                /// </summary>
                public enum ModeKind
                {
                    /// <summary>Normal tightening.</summary>
                    Normal = 0,

                    /// <summary>Tapping tightening.</summary>
                    Tapping = 1,

                    /// <summary>Fast tightening.</summary>
                    Fast = 2,
                }

                /// <summary>
                /// Gets or sets the judgement windows.
                /// </summary>
                public List<Window> Windows = [.. Enumerable.Range(0, 8).Select(x => new Window(x))];

                /// <summary>
                /// Gets or sets the rotating direction.
                /// </summary>
                public DirectionKind Direction = DirectionKind.CW;

                /// <summary>
                /// Gets or sets the target torque.
                /// </summary>
                public float TargetTorque;

                /// <summary>
                /// Gets or sets the tightening mode.
                /// </summary>
                public ModeKind Mode = ModeKind.Normal;

                /// <summary>
                /// Gets or sets the flags indicating which special parameters are enabled.
                /// </summary>
                public Special SpecialFlags = Special.None;

                /// <summary>
                /// Gets or sets the amount used to detect the start of tightening.
                /// </summary>
                public float StartDetectionAmount;

                /// <summary>
                /// Gets or sets the initial tapping torque.
                /// </summary>
                public float InitialTappingTorque;

                /// <summary>
                /// Gets or sets the torque-up detection time.
                /// </summary>
                public float TorqueUpDetectionTime;

                /// <summary>
                /// Gets or sets the permissible number of turns on the minus side.
                /// </summary>
                public float PermissibleTurnsBelow;

                /// <summary>
                /// Gets or sets the permissible number of turns on the plus side.
                /// </summary>
                public float PermissibleTurnsAbove;

                /// <summary>
                /// Gets or sets the further tightening angle.
                /// </summary>
                public float FurtherTighteningAngle;

                /// <summary>
                /// Gets or sets the further tightening angle applied after the screw protrudes.
                /// </summary>
                public float FurtherTighteningAngleForProtruding;

                /// <summary>
                /// Gets or sets the torque threshold used to change the speed.
                /// </summary>
                public float TorqueThresholdToChangeSpeed;

                /// <summary>
                /// Gets or sets the speed applied after the change.
                /// </summary>
                public int SpeedAfterChanged = 10;

                /// <summary>
                /// Gets or sets the rotating steps.
                /// </summary>
                public List<Step> Steps = [.. Enumerable.Range(0, 9).Select(_ => new Step())];

                /// <summary>
                /// Gets or sets the angle interval used to sample the torque waveform.
                /// </summary>
                public int SamplingAngle = 1;

                /// <summary>
                /// Gets or sets the torque correction rate.
                /// </summary>
                public int TorqueAdjustRate;

                /// <summary>
                /// Creates a deep copy of this instance.
                /// </summary>
                /// <returns>A new <see cref="TighteningProgram"/> instance with the same content.</returns>
                public TighteningProgram Clone()
                {
                    TighteningProgram clone = new()
                    {
                        Direction = Direction,
                        TargetTorque = TargetTorque,
                        Mode = Mode,
                        SpecialFlags = SpecialFlags,
                        StartDetectionAmount = StartDetectionAmount,
                        InitialTappingTorque = InitialTappingTorque,
                        TorqueUpDetectionTime = TorqueUpDetectionTime,
                        PermissibleTurnsBelow = PermissibleTurnsBelow,
                        PermissibleTurnsAbove = PermissibleTurnsAbove,
                        FurtherTighteningAngle = FurtherTighteningAngle,
                        FurtherTighteningAngleForProtruding = FurtherTighteningAngleForProtruding,
                        TorqueThresholdToChangeSpeed = TorqueThresholdToChangeSpeed,
                        SpeedAfterChanged = SpeedAfterChanged,
                        SamplingAngle = SamplingAngle,
                        TorqueAdjustRate = TorqueAdjustRate,
                    };

                    // The lists are pre-allocated in the field initializers, so copy them element by element.
                    for (var i = 0; i < Windows.Count; i++)
                    {
                        clone.Windows[i] = Windows[i].Clone();
                    }

                    for (var i = 0; i < Steps.Count; i++)
                    {
                        clone.Steps[i] = Steps[i].Clone();
                    }

                    return clone;
                }

                /// <summary>
                /// Determines whether the specified instance has the same content as this instance.
                /// </summary>
                /// <param name="other">Instance to compare with.</param>
                /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
                public bool Equals(
                    TighteningProgram other
                )
                {
                    for (var i = 0; i < Windows.Count; i++)
                    {
                        if (!other.Windows[i].Equals(Windows[i]))
                        {
                            return false;
                        }
                    }

                    for (var i = 0; i < Steps.Count; i++)
                    {
                        if (other.Steps[i].NumTurns != Steps[i].NumTurns)
                        {
                            return false;
                        }
                        else if (other.Steps[i].NumTurns == 0)
                        {
                            // A step without turns terminates the sequence, so the rest is not used.
                            break;
                        }
                        else if (!other.Steps[i].Equals(Steps[i]))
                        {
                            return false;
                        }
                    }

                    return (
                        other.Direction == Direction
                        && other.TargetTorque == TargetTorque
                        && other.SpecialFlags == SpecialFlags
                        && other.Mode == Mode
                        && other.StartDetectionAmount == StartDetectionAmount
                        && other.InitialTappingTorque == InitialTappingTorque
                        && other.TorqueUpDetectionTime == TorqueUpDetectionTime
                        && other.PermissibleTurnsBelow == PermissibleTurnsBelow
                        && other.PermissibleTurnsAbove == PermissibleTurnsAbove
                        && other.FurtherTighteningAngle == FurtherTighteningAngle
                        && other.FurtherTighteningAngleForProtruding == FurtherTighteningAngleForProtruding
                        && other.TorqueThresholdToChangeSpeed == TorqueThresholdToChangeSpeed
                        && other.SpeedAfterChanged == SpeedAfterChanged
                        && other.SamplingAngle == SamplingAngle
                        && other.TorqueAdjustRate == TorqueAdjustRate
                    );
                }

                /// <inheritdoc />
                public override void ReadFromFile(
                    BinaryReader reader,
                    int phase
                )
                {
                    switch (phase)
                    {
                        // Phase 0: the main tightening parameters.
                        case 0:
                            foreach (var window in Windows)
                            {
                                window.ReadFromFile(reader);
                            }
                            Direction = (DirectionKind)reader.ReadUInt16();
                            TargetTorque = (float)reader.ReadUInt16() / 10f;
                            SpecialFlags = (Special)reader.ReadUInt16();
                            Mode = (ModeKind)reader.ReadUInt16();
                            StartDetectionAmount = (float)reader.ReadUInt16() / 10f;
                            InitialTappingTorque = (float)reader.ReadUInt16() / 10f;

                            // The detection time is scaled by 100 while the others are scaled by 10.
                            TorqueUpDetectionTime = (float)reader.ReadUInt16() / 100f;
                            PermissibleTurnsBelow = (float)reader.ReadInt16() / 10f;
                            PermissibleTurnsAbove = (float)reader.ReadInt16() / 10f;
                            FurtherTighteningAngle = (float)reader.ReadInt16() / 10f;
                            FurtherTighteningAngleForProtruding = (float)reader.ReadInt16() / 10f;
                            TorqueThresholdToChangeSpeed = (float)reader.ReadUInt16() / 10f;
                            SpeedAfterChanged = (int)reader.ReadUInt16();

                            // Skip the reserved register.
                            _ = reader.ReadUInt16();
                            foreach (var step in Steps)
                            {
                                step.ReadFromFile(reader);
                            }
                            break;

                        // Phase 1: the parameters stored after the loosening and free run sections.
                        case 1:
                            SamplingAngle = reader.ReadUInt16();
                            TorqueAdjustRate = reader.ReadInt16();
                            break;
                    }
                }

                /// <inheritdoc />
                public override void WriteToFile(
                    BinaryWriter writer,
                    int phase
                )
                {
                    switch (phase)
                    {
                        // Phase 0: the main tightening parameters.
                        case 0:
                            foreach (var window in Windows)
                            {
                                window.WriteToFile(writer);
                            }
                            writer.Write((ushort)Direction);
                            writer.Write((ushort)(TargetTorque * 10f));
                            writer.Write((ushort)SpecialFlags);
                            writer.Write((ushort)Mode);
                            writer.Write((ushort)(StartDetectionAmount * 10f));
                            writer.Write((ushort)(InitialTappingTorque * 10f));

                            // The detection time is scaled by 100 while the others are scaled by 10.
                            writer.Write((ushort)(TorqueUpDetectionTime * 100f));
                            writer.Write((short)(PermissibleTurnsBelow * 10f));
                            writer.Write((short)(PermissibleTurnsAbove * 10f));
                            writer.Write((short)(FurtherTighteningAngle * 10f));
                            writer.Write((short)(FurtherTighteningAngleForProtruding * 10f));
                            writer.Write((ushort)(TorqueThresholdToChangeSpeed * 10f));
                            writer.Write((ushort)SpeedAfterChanged);

                            // Fill the reserved register with zero.
                            writer.Write((ushort)0);
                            foreach (var step in Steps)
                            {
                                step.WriteToFile(writer);
                            }
                            break;

                        // Phase 1: the parameters stored after the loosening and free run sections.
                        case 1:
                            writer.Write((ushort)SamplingAngle);
                            writer.Write((short)TorqueAdjustRate);
                            break;
                    }
                }

                /// <summary>
                /// Downloads the tightening program from the device.
                /// </summary>
                /// <param name="client">Client used to communicate with the device.</param>
                /// <param name="cancellationToken">Token used to cancel the operation.</param>
                /// <returns><c>true</c> if the download succeeded; otherwise, <c>false</c>.</returns>
                public override async Task<bool> DownloadAsync(
                    ModelPFClient client,
                    CancellationToken cancellationToken = default
                )
                {
                    ushort count;
                    ushort[]? regValues;
                    int offset;

                    // Read the window definition block at once.
                    count = (ushort)(Holding.WindowDefinitionEnd - Holding.WindowDefinitionStart + 1);
                    regValues = await client.ReadHoldingRegistersAsync(Holding.WindowDefinitionStart, count, cancellationToken);
                    if (regValues == null)
                    {
                        return false;
                    }
                    offset = 0;
                    foreach (var window in Windows)
                    {
                        window.SetFromRegValues(regValues, offset);
                        offset += Holding.NumRegsPerWindow;
                    }

                    // Read the tightening parameter block at once.
                    count = (ushort)(Holding.TighteningPartEnd - Holding.TighteningPartStart + 1);
                    regValues = await client.ReadHoldingRegistersAsync(Holding.TighteningPartStart, count, cancellationToken);
                    if (regValues == null)
                    {
                        return false;
                    }
                    Direction = (DirectionKind)regValues[Holding.OffsetDirection];
                    TargetTorque = (float)regValues[Holding.OffsetTargetTorque] / 10f;
                    SpecialFlags = (Special)regValues[Holding.OffsetSpecialFlags];
                    Mode = (ModeKind)regValues[Holding.OffsetMode];
                    StartDetectionAmount = (float)regValues[Holding.OffsetStartDetectionAmount] / 10f;
                    InitialTappingTorque = (float)regValues[Holding.OffsetInitialTappingTorque] / 10f;
                    TorqueUpDetectionTime = (float)regValues[Holding.OffsetTorqueUpDetectionTime] / 100f;

                    // Some registers hold signed values, so reinterpret them as signed.
                    PermissibleTurnsBelow = (float)unchecked((short)regValues[Holding.OffsetPermissibleTurnsBelow]) / 10f;
                    PermissibleTurnsAbove = (float)regValues[Holding.OffsetPermissibleTurnsAbove] / 10f;
                    FurtherTighteningAngle = (float)unchecked((short)regValues[Holding.OffsetFurtherTighteningAngle]) / 10f;
                    FurtherTighteningAngleForProtruding = (float)regValues[Holding.OffsetFurtherTighteningAngleForProtruding] / 10f;
                    TorqueThresholdToChangeSpeed = (float)regValues[Holding.OffsetTorqueThresholdToChangeSpeed] / 10f;
                    SpeedAfterChanged = (int)regValues[Holding.OffsetSpeedAfterChanged];
                    offset = Holding.OffsetTightningSteps;
                    foreach (var step in Steps)
                    {
                        step.SetFromRegValues(regValues, offset);
                        offset += Holding.NumRegsPerStep;
                    }

                    // The sampling angle and the torque correction rate are stored in two consecutive registers.
                    regValues = await client.ReadHoldingRegistersAsync(Holding.SampleingAngle, 2, cancellationToken);
                    if (regValues == null)
                    {
                        return false;
                    }
                    SamplingAngle = (int)regValues[0];
                    TorqueAdjustRate = (int)unchecked((short)regValues[1]);

                    return true;
                }

                /// <summary>
                /// Uploads the tightening program to the device.
                /// </summary>
                /// <param name="client">Client used to communicate with the device.</param>
                /// <param name="cancellationToken">Token used to cancel the operation.</param>
                /// <returns><c>true</c> if the upload succeeded; otherwise, <c>false</c>.</returns>
                public override async Task<bool> UploadAsync(
                    ModelPFClient client,
                    CancellationToken cancellationToken = default
                )
                {
                    ushort count;
                    ushort[] regValues;
                    int offset;
                    bool isSuccess;

                    // Write the window definition block at once.
                    count = (ushort)(Holding.WindowDefinitionEnd - Holding.WindowDefinitionStart + 1);
                    regValues = new ushort[count];
                    offset = 0;
                    foreach (var window in Windows)
                    {
                        window.SetToRegValues(regValues, offset);
                        offset += Holding.NumRegsPerWindow;
                    }
                    isSuccess = await client.WriteMultipleRegistersAsync(Holding.WindowDefinitionStart, regValues, cancellationToken);
                    if (!isSuccess)
                    {
                        return false;
                    }

                    // Write the tightening parameter block at once.
                    count = (ushort)(Holding.TighteningPartEnd - Holding.TighteningPartStart + 1);
                    regValues = new ushort[count];
                    regValues[Holding.OffsetDirection] = (ushort)Direction;
                    regValues[Holding.OffsetTargetTorque] = (ushort)(TargetTorque * 10f);
                    regValues[Holding.OffsetSpecialFlags] = (ushort)SpecialFlags;
                    regValues[Holding.OffsetMode] = (ushort)Mode;
                    regValues[Holding.OffsetStartDetectionAmount] = (ushort)(StartDetectionAmount * 10f);
                    regValues[Holding.OffsetInitialTappingTorque] = (ushort)(InitialTappingTorque * 10f);
                    regValues[Holding.OffsetTorqueUpDetectionTime] = (ushort)(TorqueUpDetectionTime * 100f);
                    regValues[Holding.OffsetPermissibleTurnsBelow] = (ushort)(PermissibleTurnsBelow * 10f);
                    regValues[Holding.OffsetPermissibleTurnsAbove] = (ushort)(PermissibleTurnsAbove * 10f);
                    regValues[Holding.OffsetFurtherTighteningAngle] = (ushort)(FurtherTighteningAngle * 10f);
                    regValues[Holding.OffsetFurtherTighteningAngleForProtruding] = (ushort)(FurtherTighteningAngleForProtruding * 10f);
                    regValues[Holding.OffsetTorqueThresholdToChangeSpeed] = (ushort)(TorqueThresholdToChangeSpeed * 10f);
                    regValues[Holding.OffsetSpeedAfterChanged] = (ushort)SpeedAfterChanged;
                    offset = Holding.OffsetTightningSteps;
                    foreach (var step in Steps)
                    {
                        step.SetToRegValues(regValues, offset);
                        offset += Holding.NumRegsPerStep;
                    }

                    isSuccess = await client.WriteMultipleRegistersAsync(Holding.TighteningPartStart, regValues, cancellationToken);
                    if (!isSuccess)
                    {
                        return false;
                    }

                    // The sampling angle and the torque correction rate are stored in two consecutive registers.
                    regValues = new ushort[2];
                    regValues[0] = (ushort)SamplingAngle;
                    regValues[1] = (ushort)TorqueAdjustRate;
                    isSuccess = await client.WriteMultipleRegistersAsync(Holding.SampleingAngle, regValues, cancellationToken);
                    if (!isSuccess)
                    {
                        return false;
                    }

                    return true;
                }

                /// <inheritdoc />
                public override void Adjust(
                    ToolKind toolType
                )
                {
                    // The adjuster is prepared by the owning Program before this call.
                    if (_torqueAdjuster != null)
                    {
                        foreach (var window in Windows)
                        {
                            window.Adjust(toolType);
                        }

                        _torqueAdjuster.Adjust(ref TargetTorque);
                        _torqueAdjuster.Adjust(ref InitialTappingTorque);
                    }
                }
            }

            /// <summary>
            /// Screw loosening program.
            /// </summary>
            public class LooseningProgram : Base
            {
                /// <summary>
                /// Gets or sets the rotating direction.
                /// </summary>
                public DirectionKind Direction = DirectionKind.CCW;

                /// <summary>
                /// Gets or sets the target torque.
                /// </summary>
                public float TargetTorque;

                /// <summary>
                /// Gets or sets the rotating steps.
                /// </summary>
                public List<Step> Steps = [.. Enumerable.Range(0, 2).Select(_ => new Step())];

                /// <summary>
                /// Creates a deep copy of this instance.
                /// </summary>
                /// <returns>A new <see cref="LooseningProgram"/> instance with the same content.</returns>
                public LooseningProgram Clone()
                {
                    LooseningProgram clone = new()
                    {
                        Direction = Direction,
                        TargetTorque = TargetTorque,
                    };

                    // The list is pre-allocated in the field initializer, so copy it element by element.
                    for (var i = 0; i < Steps.Count; i++)
                    {
                        clone.Steps[i] = Steps[i].Clone();
                    }

                    return clone;
                }

                /// <summary>
                /// Determines whether the specified instance has the same content as this instance.
                /// </summary>
                /// <param name="other">Instance to compare with.</param>
                /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
                public bool Equals(
                    LooseningProgram other
                )
                {
                    for (var i = 0; i < Steps.Count; i++)
                    {
                        if (other.Steps[i].NumTurns != Steps[i].NumTurns)
                        {
                            return false;
                        }
                        else if (other.Steps[i].NumTurns == 0)
                        {
                            // A step without turns terminates the sequence, so the rest is not used.
                            break;
                        }
                        else if (!other.Steps[i].Equals(Steps[i]))
                        {
                            return false;
                        }
                    }

                    return (
                        other.Direction == Direction
                        && other.TargetTorque == TargetTorque
                    );
                }

                /// <inheritdoc />
                public override void ReadFromFile(
                    BinaryReader reader,
                    int phase = 0
                )
                {
                    Direction = (DirectionKind)reader.ReadUInt16();
                    TargetTorque = (float)reader.ReadUInt16() / 10f;
                    foreach (var step in Steps)
                    {
                        step.ReadFromFile(reader);
                    }
                }

                /// <inheritdoc />
                public override void WriteToFile(
                    BinaryWriter writer,
                    int phase = 0
                )
                {
                    writer.Write((ushort)Direction);
                    writer.Write((ushort)(TargetTorque * 10f));
                    foreach (var step in Steps)
                    {
                        step.WriteToFile(writer);
                    }
                }

                /// <summary>
                /// Downloads the loosening program from the device.
                /// </summary>
                /// <param name="client">Client used to communicate with the device.</param>
                /// <param name="cancellationToken">Token used to cancel the operation.</param>
                /// <returns><c>true</c> if the download succeeded; otherwise, <c>false</c>.</returns>
                public override async Task<bool> DownloadAsync(
                    ModelPFClient client,
                    CancellationToken cancellationToken = default
                )
                {
                    ushort count;
                    ushort[]? regValues;
                    int offset;

                    // Read the loosening parameter block at once.
                    count = (ushort)(Holding.LooseningPartEnd - Holding.LooseningPartStart + 1);
                    regValues = await client.ReadHoldingRegistersAsync(Holding.LooseningPartStart, count, cancellationToken);
                    if (regValues == null)
                    {
                        return false;
                    }
                    Direction = (DirectionKind)regValues[Holding.OffsetDirection];
                    TargetTorque = (float)regValues[Holding.OffsetTargetTorque] / 10f;
                    offset = Holding.OffsetLooseningSteps;
                    foreach (var step in Steps)
                    {
                        step.SetFromRegValues(regValues, offset);
                        offset += Holding.NumRegsPerStep;
                    }

                    return true;
                }

                /// <summary>
                /// Uploads the loosening program to the device.
                /// </summary>
                /// <param name="client">Client used to communicate with the device.</param>
                /// <param name="cancellationToken">Token used to cancel the operation.</param>
                /// <returns><c>true</c> if the upload succeeded; otherwise, <c>false</c>.</returns>
                public override async Task<bool> UploadAsync(
                    ModelPFClient client,
                    CancellationToken cancellationToken = default
                )
                {
                    ushort count;
                    ushort[] regValues;
                    int offset;
                    bool isSuccess;

                    // Write the loosening parameter block at once.
                    count = (ushort)(Holding.LooseningPartEnd - Holding.LooseningPartStart + 1);
                    regValues = new ushort[count];
                    regValues[Holding.OffsetDirection] = (ushort)Direction;
                    regValues[Holding.OffsetTargetTorque] = (ushort)(TargetTorque * 10f);
                    offset = Holding.OffsetLooseningSteps;
                    foreach (var step in Steps)
                    {
                        step.SetToRegValues(regValues, offset);
                        offset += Holding.NumRegsPerStep;
                    }

                    isSuccess = await client.WriteMultipleRegistersAsync(Holding.LooseningPartStart, regValues, cancellationToken);
                    if (!isSuccess)
                    {
                        return false;
                    }

                    return true;
                }

                /// <inheritdoc />
                public override void Adjust(
                    ToolKind toolType
                )
                {
                    // The adjuster is prepared by the owning Program before this call.
                    _torqueAdjuster?.Adjust(ref TargetTorque);
                }
            }

            /// <summary>
            /// Driver free run program.
            /// </summary>
            public class FreeRunProgram : Base
            {
                /// <summary>
                /// Gets or sets the rotating direction.
                /// </summary>
                public DirectionKind Direction = DirectionKind.CW;

                /// <summary>
                /// Gets or sets the target torque. Not used by the user; it is derived from the tool type.
                /// </summary>
                public float TargetTorque;

                /// <summary>
                /// Gets or sets the rotating steps. The number of turns is not used in free run.
                /// </summary>
                public List<Step> Steps = [.. Enumerable.Range(0, 1).Select(_ => new Step())];

                /// <summary>
                /// Internal target torque applied in free run, per tool type.
                /// </summary>
                private static readonly Dictionary<ToolKind, float> _freeRunInnerTargetTorques = new()
                {
                    [ToolKind.S] = 15f,
                    [ToolKind.L] = 50f,
                    [ToolKind.XL] = 100f,
                    [ToolKind.GM] = 400f,
                    [ToolKind.GX] = 1300f,
                };

                /// <summary>
                /// Gets the internal target torque applied in free run for the specified tool type.
                /// </summary>
                /// <param name="toolType">Tool type to look up.</param>
                /// <returns>
                /// The internal target torque. The value for <see cref="ToolKind.S"/> is returned
                /// when the tool type is unknown.
                /// </returns>
                public static float GetInnerTargetTorque(
                    ToolKind toolType
                )
                {
                    if (_freeRunInnerTargetTorques.TryGetValue(toolType, out var value))
                    {
                        return value;
                    }
                    else
                    {
                        return _freeRunInnerTargetTorques[ToolKind.S];
                    }
                }

                /// <summary>
                /// Creates a deep copy of this instance.
                /// </summary>
                /// <returns>A new <see cref="FreeRunProgram"/> instance with the same content.</returns>
                public FreeRunProgram Clone()
                {
                    FreeRunProgram clone = new()
                    {
                        Direction = Direction,
                        TargetTorque = TargetTorque,
                    };

                    // The list is pre-allocated in the field initializer, so copy it element by element.
                    for (var i = 0; i < Steps.Count; i++)
                    {
                        clone.Steps[i] = Steps[i].Clone();
                    }

                    return clone;
                }

                /// <summary>
                /// Determines whether the specified instance has the same content as this instance.
                /// </summary>
                /// <param name="other">Instance to compare with.</param>
                /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
                public bool Equals(
                    FreeRunProgram other
                )
                {
                    // Only the speed matters in free run because the number of turns is not used.
                    for (var i = 0; i < Steps.Count; i++)
                    {
                        if (other.Steps[i].Speed != Steps[i].Speed)
                        {
                            return false;
                        }
                        else if (other.Steps[i].NumTurns == 0)
                        {
                            break;
                        }
                        else if (!other.Steps[i].Equals(Steps[i]))
                        {
                            return false;
                        }
                    }

                    return (
                        other.Direction == Direction
                        && other.TargetTorque == TargetTorque
                    );
                }

                /// <inheritdoc />
                public override void ReadFromFile(
                    BinaryReader reader,
                    int phase = 0
                )
                {
                    Direction = (DirectionKind)reader.ReadInt16();
                    TargetTorque = (float)reader.ReadUInt16() / 10f;
                    foreach (var step in Steps)
                    {
                        step.ReadFromFile(reader);
                    }
                }

                /// <inheritdoc />
                public override void WriteToFile(
                    BinaryWriter writer,
                    int phase = 0
                )
                {
                    writer.Write((ushort)Direction);
                    writer.Write((ushort)(TargetTorque * 10f));
                    foreach (var step in Steps)
                    {
                        step.WriteToFile(writer);
                    }
                }

                /// <summary>
                /// Downloads the free run program from the device.
                /// </summary>
                /// <param name="client">Client used to communicate with the device.</param>
                /// <param name="cancellationToken">Token used to cancel the operation.</param>
                /// <returns><c>true</c> if the download succeeded; otherwise, <c>false</c>.</returns>
                public override async Task<bool> DownloadAsync(
                    ModelPFClient client,
                    CancellationToken cancellationToken = default
                )
                {
                    ushort count;
                    ushort[]? regValues;
                    int offset;

                    // Read the free run parameter block at once.
                    count = (ushort)(Holding.FreeRunPartEnd - Holding.FreeRunPartStart + 1);
                    regValues = await client.ReadHoldingRegistersAsync(Holding.FreeRunPartStart, count, cancellationToken);
                    if (regValues == null)
                    {
                        return false;
                    }
                    Direction = (DirectionKind)regValues[Holding.OffsetDirection];
                    TargetTorque = (float)regValues[Holding.OffsetTargetTorque] / 10f;
                    offset = Holding.OffsetFreeRunSteps;
                    foreach (var step in Steps)
                    {
                        step.SetFromRegValues(regValues, offset);
                        offset += Holding.NumRegsPerStep;
                    }

                    return true;
                }

                /// <summary>
                /// Uploads the free run program to the device.
                /// </summary>
                /// <param name="client">Client used to communicate with the device.</param>
                /// <param name="cancellationToken">Token used to cancel the operation.</param>
                /// <returns><c>true</c> if the upload succeeded; otherwise, <c>false</c>.</returns>
                public override async Task<bool> UploadAsync(
                    ModelPFClient client,
                    CancellationToken cancellationToken = default
                )
                {
                    ushort count;
                    ushort[] regValues;
                    int offset;
                    bool isSuccess;

                    // Write the free run parameter block at once.
                    count = (ushort)(Holding.FreeRunPartEnd - Holding.FreeRunPartStart + 1);
                    regValues = new ushort[count];
                    regValues[Holding.OffsetDirection] = (ushort)Direction;
                    regValues[Holding.OffsetTargetTorque] = (ushort)(TargetTorque * 10f);
                    offset = Holding.OffsetFreeRunSteps;
                    foreach (var step in Steps)
                    {
                        step.SetToRegValues(regValues, offset);
                        offset += Holding.NumRegsPerStep;
                    }

                    isSuccess = await client.WriteMultipleRegistersAsync(Holding.FreeRunPartStart, regValues, cancellationToken);
                    if (!isSuccess)
                    {
                        return false;
                    }

                    return true;
                }

                /// <inheritdoc />
                public override void Adjust(
                    ToolKind toolType
                )
                {
                    // The target torque is fixed per tool type and is not edited by the user.
                    TargetTorque = GetInnerTargetTorque(toolType);
                }
            }

            /// <summary>
            /// Zero-based number of this program.
            /// </summary>
            public int ProgramNo;

            /// <summary>
            /// Gets or sets the program name.
            /// </summary>
            public string Name = string.Empty;

            /// <summary>
            /// Gets or sets the screw tightening program.
            /// </summary>
            public TighteningProgram Tightening = new();

            /// <summary>
            /// Gets or sets the screw loosening program.
            /// </summary>
            public LooseningProgram Loosening = new();

            /// <summary>
            /// Gets or sets the driver free run program.
            /// </summary>
            public FreeRunProgram FreeRun = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="Program"/> class.
            /// </summary>
            /// <param name="programNo">Zero-based number of the program.</param>
            public Program(
                int programNo
            )
            {
                ProgramNo = programNo;
            }

            /// <summary>
            /// Creates a deep copy of this instance.
            /// </summary>
            /// <returns>A new <see cref="Program"/> instance with the same content.</returns>
            public Program Clone()
            {
                return new(ProgramNo)
                {
                    Name = Name,
                    Tightening = Tightening.Clone(),
                    Loosening = Loosening.Clone(),
                    FreeRun = FreeRun.Clone(),
                };
            }

            /// <summary>
            /// Determines whether the specified instance has the same content as this instance.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if all the values match; otherwise, <c>false</c>.</returns>
            public bool Equals(
                Program other
            )
            {
                return (
                    string.Equals(other.Name, Name)
                    && other.Tightening.Equals(Tightening)
                    && other.Loosening.Equals(Loosening)
                    && other.FreeRun.Equals(FreeRun)
                );
            }

            /// <inheritdoc />
            public override void ReadFromFile(
                BinaryReader reader,
                int phase = 0
            )
            {
                // The tightening program is split into two phases in the file layout.
                Name = StringExtension.ReadFromFile(reader, MaxProgramNameLength);
                Tightening.ReadFromFile(reader, 0);
                Loosening.ReadFromFile(reader);
                FreeRun.ReadFromFile(reader);
                Tightening.ReadFromFile(reader, 1);
            }

            /// <inheritdoc />
            public override void WriteToFile(
                BinaryWriter writer,
                int phase = 0
            )
            {
                // The tightening program is split into two phases in the file layout.
                Name.WriteToFile(writer, MaxProgramNameLength);
                Tightening.WriteToFile(writer, 0);
                Loosening.WriteToFile(writer);
                FreeRun.WriteToFile(writer);
                Tightening.WriteToFile(writer, 1);
            }

            /// <summary>
            /// Downloads this program from the device.
            /// </summary>
            /// <param name="client">Client used to communicate with the device.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> if the download succeeded; otherwise, <c>false</c>.</returns>
            public override async Task<bool> DownloadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                bool isSuccess;

                // Select the program to be loaded into the register area.
                isSuccess = await client.WriteSingleRegisterAsync(Holding.SelectProgram, (ushort)ProgramNo, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                // Wait until the device finishes loading the selected program.
                if (!await client.WaitReadyAsync(RequestCode.DOWNLOAD_PROGRAM, cancellationToken))
                {
                    return false;
                }

                isSuccess = await Tightening.DownloadAsync(client, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                isSuccess = await Loosening.DownloadAsync(client, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                isSuccess = await FreeRun.DownloadAsync(client, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Uploads this program to the device.
            /// </summary>
            /// <param name="client">Client used to communicate with the device.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> if the upload succeeded; otherwise, <c>false</c>.</returns>
            public override async Task<bool> UploadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                bool isSuccess;

                // Fill the register area first, then let the device store it as the selected program.
                isSuccess = await Tightening.UploadAsync(client, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                isSuccess = await Loosening.UploadAsync(client, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                isSuccess = await FreeRun.UploadAsync(client, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                isSuccess = await client.WriteSingleRegisterAsync(Holding.SelectProgram, (ushort)ProgramNo, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                // Wait until the device finishes storing the program.
                if (!await client.WaitReadyAsync(RequestCode.UPLOAD_PROGRAM, cancellationToken))
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Torque adjuster shared with the nested programs while <see cref="Adjust"/> is running.
            /// </summary>
            private static TorqueAdjuster? _torqueAdjuster;

            /// <inheritdoc />
            public override void Adjust(
                ToolKind toolType
            )
            {
                // The valid torque range depends on both the tool type and the tightening mode.
                _torqueAdjuster = new(toolType, Tightening.Mode);

                Tightening.Adjust(toolType);
                Loosening.Adjust(toolType);
                FreeRun.Adjust(toolType);

                _torqueAdjuster = null;
            }
        }
    }
}
