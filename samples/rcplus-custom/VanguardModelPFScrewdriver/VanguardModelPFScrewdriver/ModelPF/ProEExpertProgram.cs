// -----------------------------------------------------------------------
// <copyright file="ProEExpertProgram.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Represents a judgement window of the torque waveform in a ProE-Expert program.
    /// </summary>
    public class PEWindow
    {
        /// <summary>
        /// Gets or sets a value indicating whether this window is used for judgement.
        /// </summary>
        [XmlElement("WindowEnable")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of turns where the window starts.
        /// </summary>
        public float StartTurn { get; set; }

        /// <summary>
        /// Gets or sets the number of turns where the window ends.
        /// </summary>
        public float EndTurn { get; set; }

        /// <summary>
        /// Gets or sets the torque value at the start of the window.
        /// </summary>
        [XmlElement("TorqueUnder")]
        public float TorqueAtStart { get; set; }

        /// <summary>
        /// Gets or sets the torque value at the end of the window.
        /// </summary>
        [XmlElement("TorqueUpper")]
        public float TorqueAtEnd { get; set; }

        /// <summary>
        /// Gets or sets the torque width of the window.
        /// </summary>
        public float TorqueWidth { get; set; }

        /// <summary>
        /// Gets or sets the color used to draw the window frame.
        /// </summary>
        public string LineColor { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a single step of a driving operation.
    /// </summary>
    public class PEStep
    {
        /// <summary>
        /// Gets or sets the rotation speed in rpm.
        /// </summary>
        [XmlElement("SpeedRpm")]
        public int Speed { get; set; }

        /// <summary>
        /// Gets or sets the number of turns performed in this step.
        /// </summary>
        [XmlElement("TurnCount")]
        public float NumTurns { get; set; }
    }

    /// <summary>
    /// Represents the detailed parameters of a driving operation.
    /// </summary>
    public class PEParamDetail
    {
        /// <summary>
        /// Gets or sets the rotation direction.
        /// </summary>
        [XmlElement("TurnDirection")]
        public int Direction { get; set; }

        /// <summary>
        /// Gets or sets the target torque value.
        /// </summary>
        [XmlElement("TargetTorqueValue")]
        public float TargetTorque { get; set; }

        /// <summary>
        /// Gets or sets the screw attribute (driving mode).
        /// </summary>
        [XmlElement("ScrewAttr")]
        public int Mode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="StartDetectionAmount"/> is used.
        /// </summary>
        [XmlElement("StartFastEnable")]
        public bool StartDetectionAmountIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the amount used to detect the start of tightening.
        /// </summary>
        [XmlElement("StartFastDetect")]
        public float StartDetectionAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="InitialTappingTorque"/> is used.
        /// </summary>
        [XmlElement("InitialTapEnable")]
        public bool InitialTappingTorqueIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the torque applied for the initial tapping.
        /// </summary>
        [XmlElement("InitialTapTorque")]
        public float InitialTappingTorque { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TorqueUpDetectionTime"/> is used.
        /// </summary>
        [XmlElement("TorqueDetectEnable")]
        public bool TorqueUpDetectionTimeIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the time used to detect the torque-up.
        /// </summary>
        [XmlElement("TorqueDetect")]
        public float TorqueUpDetectionTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="PermissibleTurnsBelow"/> is used.
        /// </summary>
        [XmlElement("LimitTurnMinusEnable")]
        public bool PermissibleTurnsBelowIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the permissible number of turns below the target.
        /// </summary>
        [XmlElement("LimitTurnMinusCount")]
        public float PermissibleTurnsBelow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="PermissibleTurnsAbove"/> is used.
        /// </summary>
        [XmlElement("LimitTurnPlusEnable")]
        public bool PermissibleTurnsAboveIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the permissible number of turns above the target.
        /// </summary>
        [XmlElement("LimitTurnPlusCount")]
        public float PermissibleTurnsAbove { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="FurtherTighteningAngle"/> is used.
        /// </summary>
        [XmlElement("ReverseTurnAngleEnable")]
        public bool FurtherTighteningAngleIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the angle of the additional tightening.
        /// </summary>
        [XmlElement("ReverseTurnAngle")]
        public int FurtherTighteningAngle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="FurtherTighteningAngleForProtruding"/> is used.
        /// </summary>
        [XmlElement("Reserved01Enable")]
        public bool FurtherTighteningAngleForProtrudingIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the angle of the additional tightening for a protruding screw.
        /// </summary>
        [XmlElement("Reserved01")]
        public int FurtherTighteningAngleForProtruding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TorqueThresholdToChangeSpeed"/> is used.
        /// </summary>
        [XmlElement("Reserved02Enable")]
        public bool TorqueThresholdToChangeSpeedIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the torque threshold at which the rotation speed is switched.
        /// </summary>
        [XmlElement("Reserved02")]
        public float TorqueThresholdToChangeSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="SpeedAfterChanged"/> is used.
        /// </summary>
        [XmlElement("Reserved03Enable")]
        public bool SpeedAfterChangedIsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the rotation speed applied after the speed change.
        /// </summary>
        [XmlElement("Reserved03")]
        public int SpeedAfterChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Extended1"/> is used.
        /// </summary>
        [XmlElement("Reserved04Enable")]
        public bool Extended1IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the reserved extended parameter.
        /// </summary>
        [XmlElement("Reserved04")]
        public int Extended1 { get; set; }
    }

    /// <summary>
    /// Represents the parameter set of a driving operation (steps and detailed parameters).
    /// </summary>
    public class PEParam
    {
        /// <summary>
        /// Gets or sets the driving steps executed in order.
        /// </summary>
        [XmlArray("StepParam")]
        [XmlArrayItem("DeviceParamProgramS")]
        public List<PEStep> Steps { get; set; } = [];

        /// <summary>
        /// Gets or sets the detailed parameters of the operation.
        /// </summary>
        [XmlElement("OtherParam")]
        public PEParamDetail Details { get; set; } = new();
    }

    /// <summary>
    /// Represents a single ProE Expert program.
    /// </summary>
    public class PEProgram
    {
        /// <summary>
        /// Gets or sets the program name.
        /// </summary>
        [XmlElement("ProgramName")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of judgement windows.
        /// </summary>
        [XmlElement("WindowStepCount")]
        public int NumWindowSteps { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of tightening steps.
        /// </summary>
        [XmlElement("ScrewFastStepCount")]
        public int NumTighteningSteps { get; set; } = 9;

        /// <summary>
        /// Gets or sets the number of loosening steps.
        /// </summary>
        [XmlElement("ScrewLotnStepCount")]
        public int NumLooseningSteps { get; set; } = 2;

        /// <summary>
        /// Gets or sets the number of free run steps.
        /// </summary>
        [XmlElement("DriverTurnStepCount")]
        public int NumFreeRunSteps { get; set; } = 1;

        /// <summary>
        /// Gets or sets the sampling angle interval of the torque waveform.
        /// </summary>
        [XmlElement("DataSamplingTick")]
        public int SamplingAngle { get; set; }

        /// <summary>
        /// Gets or sets the torque adjustment rate.
        /// </summary>
        [XmlElement("TorqueModyfyParam")]
        public int TorqueAdjustRate { get; set; }

        /// <summary>
        /// Gets or sets the judgement windows of the program.
        /// </summary>
        [XmlArray("WindowRangeSets")]
        [XmlArrayItem("WindowRangeProgram")]
        public List<PEWindow> Windows { get; set; } = [];

        /// <summary>
        /// Gets or sets the parameters of the tightening operation.
        /// </summary>
        [XmlElement("ScrewFastening")]
        public PEParam Tightening { get; set; } = new();

        /// <summary>
        /// Gets or sets the parameters of the loosening operation.
        /// </summary>
        [XmlElement("ScrewLottening")]
        public PEParam Loosening { get; set; } = new();

        /// <summary>
        /// Gets or sets the parameters of the free run operation.
        /// </summary>
        [XmlElement("DriverTurn")]
        public PEParam FreeRun { get; set; } = new();
    }

    /// <summary>
    /// Represents the whole ProE Expert program file and provides XML serialization.
    /// </summary>
    [XmlRoot("ArrayOfProgramSetInfo")]
    public class ProEExpertProgram
    {
        /// <summary>
        /// Maximum number of programs a device can hold.
        /// </summary>
        private const int _numPrograms = 16;

        /// <summary>
        /// Gets or sets the programs contained in the file.
        /// </summary>
        [XmlElement("ProgramSetInfo")]
        public List<PEProgram> Programs { get; set; } = [];

        /// <summary>
        /// Serializes this instance into the specified XML file.
        /// </summary>
        /// <param name="path">The destination file path.</param>
        /// <returns>true when the file was written successfully; otherwise, false.</returns>
        public bool Save(
            string path
        )
        {
            try
            {
                using StreamWriter writer = new(path);
                XmlSerializer serializer = new(typeof(ProEExpertProgram));
                serializer.Serialize(writer, this);

                return true;
            }
            catch (Exception)
            {
                // IGNORE: report the failure with the return value.
            }

            return false;
        }

        /// <summary>
        /// Deserializes a program file from the specified XML file.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <returns>The loaded programs, or null when the file is invalid or cannot be read.</returns>
        public static ProEExpertProgram? Load(
            string path
        )
        {
            try
            {
                using StreamReader reader = new(path);
                XmlSerializer serializer = new(typeof(ProEExpertProgram));

                // Unknown attributes/elements are only traced; they do not abort the deserialization.
                serializer.UnknownAttribute += (sender, ev) =>
                {
                    Debug.Print($"{ev.Attr.Name}");
                };
                serializer.UnknownElement += (sender, ev) =>
                {
                    Debug.Print($"{ev.Element.Name}");
                };
                if (serializer.Deserialize(reader) is ProEExpertProgram programData)
                {
                    return programData;
                }
            }
            catch (Exception)
            {
                // IGNORE: return null when the file cannot be deserialized.
            }

            return null;
        }
    }
}
