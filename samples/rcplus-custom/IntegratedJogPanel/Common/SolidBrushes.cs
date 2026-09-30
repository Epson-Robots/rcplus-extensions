// -----------------------------------------------------------------------
// <copyright file="SolidBrushes.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows.Media;

namespace IntegratedJogPanel.Common
{
    /// <summary>
    /// Provides special solid color brushes.
    /// </summary>
    internal static class SolidBrushes
    {
        /// <summary>Blue</summary>
        public static SolidColorBrush Blue { get; set; } = new SolidColorBrush(Color.FromRgb(0, 120, 215));

        /// <summary>Yellow</summary>
        public static SolidColorBrush Yellow { get; set; } = new SolidColorBrush(Color.FromRgb(220, 220, 0));

        /// <summary>Blue for control</summary>
        public static SolidColorBrush CtrlBlue { get; set; } = new SolidColorBrush(Color.FromRgb(0, 120, 215));

        /// <summary>Gray A (Light)</summary>
        public static SolidColorBrush GrayA { get; set; } = new SolidColorBrush(Color.FromRgb(190, 190, 190));

        /// <summary>Gray B (Middle)</summary>
        public static SolidColorBrush GrayB { get; set; } = new SolidColorBrush(Color.FromRgb(220, 220, 220));

        /// <summary>Gray C (Dark)</summary>
        public static SolidColorBrush GrayC { get; set; } = new SolidColorBrush(Color.FromRgb(240, 240, 240));

        /// <summary>Transparent White</summary>
        public static SolidColorBrush TranspWhite = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

        /// <summary>Transparent Blue</summary>
        public static SolidColorBrush TranspBlue = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255));
    }
}
