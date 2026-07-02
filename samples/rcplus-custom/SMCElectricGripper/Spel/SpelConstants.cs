// -----------------------------------------------------------------------
// <copyright file="SpelConstants.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Spel
{
    /// <summary>
    /// Defines the public interface of the SMC LEHR SPEL library.
    /// This class provides SPEL library function call templates and
    /// SPEL library global variable names that are consumed by the
    /// Extension layer to interact with the SPEL library.
    /// </summary>
    internal static class SpelConstants
    {
        /// <summary>
        /// File name of the SPEL control library for the SMC LEHR gripper.
        /// </summary>
        public const string LEHR_LIBRARY_NAME = "SMC_LEHR.lib";

        // =========================================================
        // SPEL library functions for extensions
        // =========================================================

        /// <summary>
        /// Starts communication with the gripper.
        /// {0}: COM port number (1–5)
        /// </summary>
        public const string FUNC_LEHR_CONNECT = "SMC_LEHR_Connect_({0})";

        /// <summary>
        /// Disconnects communication with the gripper.
        /// </summary>
        public const string FUNC_LEHR_DISCONNECT = "SMC_LEHR_Disconnect_()";

        /// <summary>
        /// Resets alarms on the gripper.
        /// </summary>
        public const string FUNC_LEHR_RESET = "SMC_LEHR_Reset_()";

        /// <summary>
        /// Sets the operation mode for the specified recipe.
        /// {0}: Recipe number (1–16)
        /// {1}: Mode value (0 = Positioning, 2 = Gripping)
        /// </summary>
        public const string FUNC_LEHR_SET_MODE = "SMC_LEHR_SetMode_({0}, {1})";

        /// <summary>
        /// Sets the target position.
        /// {0}: Recipe number (1–16)
        /// {1}: Position (mm)
        /// </summary>
        public const string FUNC_LEHR_SET_POSITION = "SMC_LEHR_SetPos_({0}, {1})";

        /// <summary>
        /// Sets the target speed.
        /// {0}: Recipe number (1–16)
        /// {1}: Speed (mm/s)
        /// </summary>
        public const string FUNC_LEHR_SET_SPEED = "SMC_LEHR_SetSpeed_({0}, {1})";

        /// <summary>
        /// Sets the gripping force.
        /// {0}: Recipe number (1–16)
        /// {1}: Force (N)
        /// </summary>
        public const string FUNC_LEHR_SET_FORCE = "SMC_LEHR_SetForce_({0}, {1})";

        /// <summary>
        /// Turns the gripper servo ON or OFF.
        /// {0}: Servo state (0 = OFF, 1 = ON)
        /// </summary>
        public const string FUNC_LEHR_SERVO = "SMC_LEHR_Servo_({0})";

        /// <summary>
        /// Executes the motion defined by the specified recipe.
        /// {0}: Recipe number (1–16)
        /// {1}: Wait flag (0 = wait for completion, otherwise no wait)
        /// </summary>
        public const string FUNC_LEHR_EXECUTE = "SMC_LEHR_Execute_({0}, {1})";

        /// <summary>
        /// Aborts the currently running operation.
        /// </summary>
        public const string FUNC_LEHR_ABORT = "SMC_LEHR_Abort_()";

        /// <summary>
        /// Gets the operation mode for the specified recipe.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_MODE"/>.
        /// {0}: Recipe number (1–16)
        /// </summary>
        public const string FUNC_LEHR_GET_MODE = "SMC_LEHR_GetMode_({0})";

        /// <summary>
        /// Gets the target position setting for the specified recipe.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_POSITION"/>.
        /// {0}: Recipe number (1–16)
        /// </summary>
        public const string FUNC_LEHR_GET_POSITION = "SMC_LEHR_GetPos_({0})";

        /// <summary>
        /// Gets the target speed setting for the specified recipe.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_SPEED"/>.
        /// {0}: Recipe number (1–16)
        /// </summary>
        public const string FUNC_LEHR_GET_SPEED = "SMC_LEHR_GetSpeed_({0})";

        /// <summary>
        /// Gets the target force setting for the specified recipe.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_FORCE"/>.
        /// {0}: Recipe number (1–16)
        /// </summary>
        public const string FUNC_LEHR_GET_FORCE = "SMC_LEHR_GetForce_({0})";

        /// <summary>
        /// Gets the current gripper status.
        /// </summary>
        public const string FUNC_LEHR_STATUS = "SMC_LEHR_Status_()";

        /// <summary>
        /// Gets the current position of the gripper.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_CURRENT_POSITION"/>.
        /// </summary>
        public const string FUNC_LEHR_GET_CURRENT_POSITION = "SMC_LEHR_GetCurPos_()";

        /// <summary>
        /// Gets the current speed of the gripper.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_CURRENT_SPEED"/>.
        /// </summary>
        public const string FUNC_LEHR_GET_CURRENT_SPEED = "SMC_LEHR_GetCurSpeed_()";

        /// <summary>
        /// Gets the current gripping force of the gripper.
        /// The retrieved value is stored in the SPEL global variable <see cref="GVar_LEHR_CURRENT_FORCE"/>.
        /// </summary>
        public const string FUNC_LEHR_GET_CURRENT_FORCE = "SMC_LEHR_GetCurForce_()";

        // =========================================================
        // SPEL library global variables
        // =========================================================

        /// <summary>
        /// Return value of the last executed library function
        /// </summary>
        public const string GVar_LEHR_RETURN = "SMC_LEHR_return_";

        /// <summary>
        /// Operation mode value (0 = Positioning, 2 = Gripping)
        /// </summary>
        public const string GVar_LEHR_MODE = "SMC_LEHR_mode_";

        /// <summary>
        /// Target position setting value (mm)
        /// </summary>
        public const string GVar_LEHR_POSITION = "SMC_LEHR_pos_";

        /// <summary>
        /// Target speed setting value (mm/s)
        /// </summary>
        public const string GVar_LEHR_SPEED = "SMC_LEHR_spd_";

        /// <summary>
        /// Target force setting value (N)
        /// </summary>
        public const string GVar_LEHR_FORCE = "SMC_LEHR_frc_";

        /// <summary>
        /// Alarm status
        /// </summary>
        public const string GVar_LEHR_STATUS = "SMC_LEHR_sts_";

        /// <summary>
        /// Current position value (mm).
        /// </summary>
        public const string GVar_LEHR_CURRENT_POSITION = "SMC_LEHR_curpos_";

        /// <summary>
        /// Current speed value (mm/s).
        /// </summary>
        public const string GVar_LEHR_CURRENT_SPEED = "SMC_LEHR_curspd_";

        /// <summary>
        /// Current force value (N).
        /// </summary>
        public const string GVar_LEHR_CURRENT_FORCE = "SMC_LEHR_curfrc_";

        /// <summary>
        /// Communication task number used by the gripper.
        /// </summary>
        public const string GVar_LEHR_COMM_TASK_NO = "SMC_LEHR_commtaskno_";

        /// <summary>
        /// Servo on/off state.
        /// 0 = Off, 1 = On.
        /// </summary>
        public const string GVar_LEHR_SERVO_ON_OFF = "SMC_LEHR_servo_onoff_";

        /// <summary>Jog state.
        /// 1 = closing, 2 = opening, 0 = idle.
        /// </summary>
        public const string GVar_LEHR_JOG = "SMC_LEHR_jog_";
    }

    /// <summary>
    /// Represents the jog operation state of the gripper.
    /// </summary>
    internal enum JogState
    {
        /// <summary>No jog operation (idle state).</summary>
        Idle = 0,

        /// <summary>Jogging in the closing direction.</summary>
        Closing = 1,

        /// <summary>Jogging in the opening direction.</summary>
        Opening = 2,
    }
}
