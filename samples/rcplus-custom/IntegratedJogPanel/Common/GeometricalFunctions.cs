// -----------------------------------------------------------------------
// <copyright file="GeometricalFunctions.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IntegratedJogPanel.Common
{
    /// <summary>
    /// Provides geometrical functions.
    /// </summary>
    internal class GeometricalFunctions
    {
        /// <summary>
        /// Gets whether a point is inside circle or not.
        /// </summary>
        /// <param name="cx">Circle center X</param>
        /// <param name="cy">Circle center Y</param>
        /// <param name="x">Point X</param>
        /// <param name="y">Point Y</param>
        /// <param name="radius">Circls radius</param>
        /// <returns>Whether a point is inside circle.</returns>
        public static bool IsPtInsideCircle(double cx, double cy, double x, double y, double radius)
        {
            var distance = Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2));
            return distance < radius;
        }
    }
}
