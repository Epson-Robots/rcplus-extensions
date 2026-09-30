// -----------------------------------------------------------------------
// <copyright file="KeyStatCommon.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Input;

namespace SpelAnalysisTool.Common
{
    /// <summary>
    /// Key status.
    /// </summary>
    public static class KeyStatCommon
    {
        /// <summary>
        /// Is Ctrl key down or not.
        /// </summary>
        public static bool IsCtrlKeyDown =>
            (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
            (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down;
    }
}
