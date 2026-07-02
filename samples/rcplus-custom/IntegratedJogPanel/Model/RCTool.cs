// -----------------------------------------------------------------------
// <copyright file="RCTool.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// RC+ Tool
    /// </summary>
    public class RCTool
    {
        /// <summary>Number</summary>
        public int Number { get; set; }

        /// <summary>X</summary>
        public double X { get; set; }

        /// <summary>Y</summary>
        public double Y { get; set; }

        /// <summary>Z</summary>
        public double Z { get; set; }

        /// <summary>U</summary>
        public double U { get; set; }

        /// <summary>V</summary>
        public double V { get; set; }

        /// <summary>W</summary>
        public double W { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="no">Number</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="u">U</param>
        /// <param name="v">V</param>
        /// <param name="w">W</param>
        public RCTool(int number, double x, double y, double z, double u, double v, double w)
        {
            Number = number;
            X = x;
            Y = y;
            Z = z;
            U = u;
            V = v;
            W = w;
        }
    }
}

