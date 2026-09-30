// -----------------------------------------------------------------------
// <copyright file="ProgramDataBridge.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using static VanguardModelPFScrewdriver.ModelPF.ModelPFData;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Converts program data between the internal <see cref="ModelPFData"/> representation
    /// and the ProE-Expert program file representation.
    /// </summary>
    public static class ProgramDataBridge
    {
        /// <summary>
        /// Line colors assigned to the judgement windows when exporting.
        /// The colors are applied cyclically in the order of the windows.
        /// </summary>
        private static readonly string[] _lineColors =
        [
            "Lime",
            "Red",
            "Blue",
            "Cyan",
            "Yellow",
            "Fuchsia",
            "Gray",
            "DarkBlue",
        ];

        /// <summary>
        /// Imports the tightening program from the external representation.
        /// </summary>
        /// <param name="internalData">Tightening program to be filled.</param>
        /// <param name="externalData">Source program in the external representation.</param>
        private static void ImportTightening(
            TighteningProgram internalData,
            PEProgram externalData
        )
        {
            // Zip stops at the shorter sequence, so surplus windows keep their current values.
            foreach (var (i, e) in Enumerable.Zip(internalData.Windows, externalData.Windows))
            {
                i.IsEnabled = e.IsEnabled;
                i.StartTurn = e.StartTurn;
                i.EndTurn = e.EndTurn;
                i.TorqueAtStart = e.TorqueAtStart;
                i.TorqueAtEnd = e.TorqueAtEnd;
                i.TorqueWidth = e.TorqueWidth;
            }

            internalData.Direction = (DirectionKind)externalData.Tightening.Details.Direction;
            internalData.TargetTorque = externalData.Tightening.Details.TargetTorque;

            // The external data keeps an enabled flag per parameter, while the internal
            // data packs them into a single bit field.
            internalData.Mode = (ModeKind)externalData.Tightening.Details.Mode;
            internalData.SpecialFlags = Special.None;
            if (externalData.Tightening.Details.StartDetectionAmountIsEnabled)
            {
                internalData.SpecialFlags |= Special.StartDetectionAmount;
            }
            internalData.StartDetectionAmount = externalData.Tightening.Details.StartDetectionAmount;
            if (externalData.Tightening.Details.InitialTappingTorqueIsEnabled)
            {
                internalData.SpecialFlags |= Special.InitialTappingTorque;
            }
            internalData.InitialTappingTorque = externalData.Tightening.Details.InitialTappingTorque;
            if (externalData.Tightening.Details.TorqueUpDetectionTimeIsEnabled)
            {
                internalData.SpecialFlags |= Special.TorqueUpDetectionTime;
            }
            internalData.TorqueUpDetectionTime = externalData.Tightening.Details.TorqueUpDetectionTime;
            if (externalData.Tightening.Details.PermissibleTurnsBelowIsEnabled)
            {
                internalData.SpecialFlags |= Special.PermissibleTurnsBelow;
            }
            internalData.PermissibleTurnsBelow = externalData.Tightening.Details.PermissibleTurnsBelow;
            if (externalData.Tightening.Details.PermissibleTurnsAboveIsEnabled)
            {
                internalData.SpecialFlags |= Special.PermissibleTurnsAbove;
            }
            internalData.PermissibleTurnsAbove = externalData.Tightening.Details.PermissibleTurnsAbove;
            if (externalData.Tightening.Details.FurtherTighteningAngleIsEnabled)
            {
                internalData.SpecialFlags |= Special.FurtherTighteningAngle;
            }
            internalData.FurtherTighteningAngle = externalData.Tightening.Details.FurtherTighteningAngle;
            if (externalData.Tightening.Details.FurtherTighteningAngleForProtrudingIsEnabled)
            {
                internalData.SpecialFlags |= Special.FurtherTighteningAngleForProtruding;
            }
            internalData.FurtherTighteningAngleForProtruding = externalData.Tightening.Details.FurtherTighteningAngleForProtruding;
            if (externalData.Tightening.Details.TorqueThresholdToChangeSpeedIsEnabled)
            {
                internalData.SpecialFlags |= Special.TorqueThresholdToChangeSpeed;
            }
            internalData.TorqueThresholdToChangeSpeed = externalData.Tightening.Details.TorqueThresholdToChangeSpeed;
            if (externalData.Tightening.Details.SpeedAfterChangedIsEnabled)
            {
                internalData.SpecialFlags |= Special.SpeedAfterChanged;
            }
            internalData.SpeedAfterChanged = externalData.Tightening.Details.SpeedAfterChanged;

            // Zip stops at the shorter sequence, so surplus steps keep their current values.
            foreach (var (i, e) in Enumerable.Zip(internalData.Steps, externalData.Tightening.Steps))
            {
                i.NumTurns = e.NumTurns;
                i.Speed = e.Speed;
            }

            internalData.SamplingAngle = externalData.SamplingAngle;
            internalData.TorqueAdjustRate = externalData.TorqueAdjustRate;
        }

        /// <summary>
        /// Exports the tightening program into the external representation.
        /// </summary>
        /// <param name="internalData">Source tightening program.</param>
        /// <param name="externalData">Program in the external representation to be filled.</param>
        private static void ExportTightening(
            TighteningProgram internalData,
            PEProgram externalData
        )
        {
            foreach (var (i, lineColorIndex) in internalData.Windows.Select((x, index) => (x, index)))
            {
                externalData.Windows.Add(new()
                {
                    IsEnabled = i.IsEnabled,
                    StartTurn = i.StartTurn,
                    EndTurn = i.EndTurn,
                    TorqueAtStart = i.TorqueAtStart,
                    TorqueAtEnd = i.TorqueAtEnd,
                    TorqueWidth = i.TorqueWidth,

                    // Assign the line colors cyclically so that every window is distinguishable.
                    LineColor = _lineColors[lineColorIndex % _lineColors.Length],
                });

            }

            externalData.Tightening.Details.Direction = (int)internalData.Direction;
            externalData.Tightening.Details.TargetTorque = internalData.TargetTorque;

            // Expand the internal bit field into the enabled flag of each parameter.
            externalData.Tightening.Details.Mode = (int)internalData.Mode;
            externalData.Tightening.Details.StartDetectionAmountIsEnabled = (internalData.SpecialFlags & Special.StartDetectionAmount) != 0;
            externalData.Tightening.Details.StartDetectionAmount = internalData.StartDetectionAmount;
            externalData.Tightening.Details.InitialTappingTorqueIsEnabled = (internalData.SpecialFlags & Special.InitialTappingTorque) != 0;
            externalData.Tightening.Details.InitialTappingTorque = internalData.InitialTappingTorque;
            externalData.Tightening.Details.TorqueUpDetectionTimeIsEnabled = (internalData.SpecialFlags & Special.TorqueUpDetectionTime) != 0;
            externalData.Tightening.Details.TorqueUpDetectionTime = internalData.TorqueUpDetectionTime;
            externalData.Tightening.Details.PermissibleTurnsBelowIsEnabled = (internalData.SpecialFlags & Special.PermissibleTurnsBelow) != 0;
            externalData.Tightening.Details.PermissibleTurnsBelow = internalData.PermissibleTurnsBelow;
            externalData.Tightening.Details.PermissibleTurnsAboveIsEnabled = (internalData.SpecialFlags & Special.PermissibleTurnsAbove) != 0;
            externalData.Tightening.Details.PermissibleTurnsAbove = internalData.PermissibleTurnsAbove;
            externalData.Tightening.Details.FurtherTighteningAngleIsEnabled = (internalData.SpecialFlags & Special.FurtherTighteningAngle) != 0;

            // The external data holds the angles as integers.
            externalData.Tightening.Details.FurtherTighteningAngle = (int)internalData.FurtherTighteningAngle;
            externalData.Tightening.Details.FurtherTighteningAngleForProtrudingIsEnabled = (internalData.SpecialFlags & Special.FurtherTighteningAngleForProtruding) != 0;
            externalData.Tightening.Details.FurtherTighteningAngleForProtruding = (int)internalData.FurtherTighteningAngleForProtruding;
            externalData.Tightening.Details.TorqueThresholdToChangeSpeedIsEnabled = (internalData.SpecialFlags & Special.TorqueThresholdToChangeSpeed) != 0;
            externalData.Tightening.Details.TorqueThresholdToChangeSpeed = internalData.TorqueThresholdToChangeSpeed;
            externalData.Tightening.Details.SpeedAfterChangedIsEnabled = (internalData.SpecialFlags & Special.SpeedAfterChanged) != 0;
            externalData.Tightening.Details.SpeedAfterChanged = internalData.SpeedAfterChanged;

            foreach (var i in internalData.Steps)
            {
                externalData.Tightening.Steps.Add(new()
                {
                    NumTurns = i.NumTurns,
                    Speed = i.Speed,
                });
            }

            externalData.SamplingAngle = internalData.SamplingAngle;
            externalData.TorqueAdjustRate = internalData.TorqueAdjustRate;
        }

        /// <summary>
        /// Imports the loosening program from the external representation.
        /// </summary>
        /// <param name="internalData">Loosening program to be filled.</param>
        /// <param name="externalData">Source program in the external representation.</param>
        private static void ImportLoosening(
            LooseningProgram internalData,
            PEProgram externalData
        )
        {
            internalData.Direction = (DirectionKind)externalData.Loosening.Details.Direction;
            internalData.TargetTorque = externalData.Loosening.Details.TargetTorque;

            // Zip stops at the shorter sequence, so surplus steps keep their current values.
            foreach (var (i, e) in Enumerable.Zip(internalData.Steps, externalData.Loosening.Steps))
            {
                i.NumTurns = e.NumTurns;
                i.Speed = e.Speed;
            }
        }

        /// <summary>
        /// Exports the loosening program into the external representation.
        /// </summary>
        /// <param name="internalData">Source loosening program.</param>
        /// <param name="externalData">Program in the external representation to be filled.</param>
        private static void ExportLoosening(
            LooseningProgram internalData,
            PEProgram externalData
        )
        {
            externalData.Loosening.Details.Direction = (int)internalData.Direction;
            externalData.Loosening.Details.TargetTorque = internalData.TargetTorque;

            foreach (var i in internalData.Steps)
            {
                externalData.Loosening.Steps.Add(new()
                {
                    NumTurns = i.NumTurns,
                    Speed = i.Speed,
                });
            }
        }

        /// <summary>
        /// Imports the free run program from the external representation.
        /// </summary>
        /// <param name="internalData">Free run program to be filled.</param>
        /// <param name="externalData">Source program in the external representation.</param>
        private static void ImportFreeRun(
            FreeRunProgram internalData,
            PEProgram externalData
        )
        {
            // The target torque is derived from the tool type, so it is not imported.
            internalData.Direction = (DirectionKind)externalData.FreeRun.Details.Direction;

            // Only the speed is meaningful in free run.
            foreach (var (i, e) in Enumerable.Zip(internalData.Steps, externalData.FreeRun.Steps))
            {
                i.Speed = e.Speed;
            }
        }

        /// <summary>
        /// Exports the free run program into the external representation.
        /// </summary>
        /// <param name="internalData">Source free run program.</param>
        /// <param name="externalData">Program in the external representation to be filled.</param>
        /// <param name="toolType">Tool type used to determine the internal target torque.</param>
        private static void ExportFreeRun(
            FreeRunProgram internalData,
            PEProgram externalData,
            ToolKind toolType
        )
        {
            externalData.FreeRun.Details.Direction = (int)internalData.Direction;

            // The target torque is fixed per tool type and is not edited by the user.
            externalData.FreeRun.Details.TargetTorque = ModelPFData.Program.FreeRunProgram.GetInnerTargetTorque(toolType);

            foreach (var i in internalData.Steps)
            {
                externalData.FreeRun.Steps.Add(new()
                {
                    // The number of turns is not used in free run.
                    NumTurns = 0,
                    Speed = i.Speed,
                });
            }
        }

        /// <summary>
        /// Imports a single program from the external representation.
        /// </summary>
        /// <param name="internalData">Program to be filled.</param>
        /// <param name="externalData">Source program in the external representation.</param>
        private static void ImportProgram(
            Program internalData,
            PEProgram externalData
        )
        {
            internalData.Name = externalData.Name;

            ImportTightening(internalData.Tightening, externalData);
            ImportLoosening(internalData.Loosening, externalData);
            ImportFreeRun(internalData.FreeRun, externalData);
        }

        /// <summary>
        /// Exports a single program into the external representation.
        /// </summary>
        /// <param name="internalData">Source program.</param>
        /// <param name="externalData">Program in the external representation to be filled.</param>
        /// <param name="toolType">Tool type used to determine the internal target torque.</param>
        /// <param name="programIndex">Zero-based index of the program.</param>
        /// <returns>The filled <paramref name="externalData"/> instance.</returns>
        private static PEProgram ExportProgram(
            Program internalData,
            PEProgram externalData,
            ToolKind toolType,
            int programIndex
        )
        {
            // ProE-Expert 1.0.37 raises exception for any name not consists of numbers.
            //externalData.Name = internalData.Name;
            externalData.Name = $"{programIndex + 1}";

            ExportTightening(internalData.Tightening, externalData);
            ExportLoosening(internalData.Loosening, externalData);
            ExportFreeRun(internalData.FreeRun, externalData, toolType);

            return externalData;
        }

        /// <summary>
        /// Imports all the programs of a ProE-Expert program file into the internal data.
        /// </summary>
        /// <param name="internalData">Internal data to be filled.</param>
        /// <param name="externalData">Source ProE-Expert program file content.</param>
        public static void Import(
            ModelPFData internalData,
            ProEExpertProgram externalData
        )
        {
            // Zip stops at the shorter sequence, so surplus programs keep their current values.
            foreach (var (i, e) in Enumerable.Zip(internalData.ProgramData, externalData.Programs))
            {
                ImportProgram(i, e);
            }
        }

        /// <summary>
        /// Exports all the internal programs into a ProE-Expert program file content.
        /// </summary>
        /// <param name="internalData">Source internal data.</param>
        /// <param name="externalData">ProE-Expert program file content to be filled.</param>
        public static void Export(
            ModelPFData internalData,
            ProEExpertProgram externalData
        )
        {
            foreach (var (i, programIndex) in internalData.ProgramData.Select((x, index) => (x, index)))
            {
                externalData.Programs.Add(ExportProgram(i, new(), internalData.OtherData.LastToolType, programIndex));
            }
        }
    }
}
