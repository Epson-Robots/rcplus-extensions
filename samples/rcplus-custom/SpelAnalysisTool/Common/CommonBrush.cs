// -----------------------------------------------------------------------
// <copyright file="CommonBrush.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Media;

namespace SpelAnalysisTool.Common
{
    /// <summary>
    /// Common brush definitions.
    /// </summary>
    internal static class CommonBrush
    {
        /// <summary>
        /// Sequential brushes.
        /// </summary>
        public static SolidColorBrush[] SequentialBrushes = { Brushes.Red, Brushes.Orange, Brushes.Gold, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet, };

        /// <summary>
        /// Get sequential brush.
        /// </summary>
        /// <param name="idx">Index.</param>
        /// <returns>Brush.</returns>
        public static Brush GetSequentialBrush(int idx) => SequentialBrushes[idx % SequentialBrushes.Length];
    }
}
