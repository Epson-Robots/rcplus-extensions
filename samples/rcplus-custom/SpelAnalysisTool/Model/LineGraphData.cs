// -----------------------------------------------------------------------
// <copyright file="LineGraphData.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Line graph data.
    /// </summary>
    public class LineGraphData
    {
        public double TotalTime { get; set; }

        public double XMin => Datas.Select(d => d.X).Min();
        public double XMax => Datas.Select(d => d.X).Max();

        public double YMin => Datas.Select(d => d.Y).Min();
        public double YMax => Datas.Select(d => d.Y).Max();
        public bool YIncludeZero => YMin < 0 && YMax > 0;

        public List<(double X, double Y)> Datas { get; set; } = new List<(double X, double Y)>();

        public bool ChkMinEnabled { get; set; }
        public bool ChkMaxEnabled { get; set; }

        public double ChkMin { get; set; }
        public double ChkMax { get; set; }
    }
}
