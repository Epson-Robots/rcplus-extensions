// -----------------------------------------------------------------------
// <copyright file="HintId.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Identifies the hint (help text) shown for an input field of the screwdriver settings.
    /// Each value is resolved into its hint definition by <see cref="HintData"/> and is
    /// referenced from XAML through the <see cref="Hint"/> attached properties.
    /// The member names must match the keys used in <c>Resources/Hints.json</c>.
    /// </summary>
    public enum HintId
    {
        None,

        // Common program settings.
        ProgramName,
        Torque,

        // Step settings.
        StepTurns,
        StepSpeed,

        // Tightening settings.
        TighteningStartDetectionAmount,
        TigtheningInitialTappingTorque,
        TighteningTorqueUpDetectionTime,
        TighteningPermissibleTurnsBelow,
        TighteningPermissibleTurnsAbove,
        TightenningFurtherTighteningAngle,
        TightentingFurtherTighteningAngleForProtruding,
        TighteningTorqueThresholdToChangeSpeed,
        TightentingSpeedAfterChanged,

        // Torque adjustment settings.
        SamplingAngle,
        TorqueAdjustRate,

        // Judgement window settings.
        WindowStartTurn,
        WindowEndTurn,
        WindowTorqueAtStart,
        WindowTorqueAtEnd,
        WindowTorqueWidth,
    }
}
