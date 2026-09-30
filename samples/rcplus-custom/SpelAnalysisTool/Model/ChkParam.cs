// -----------------------------------------------------------------------
// <copyright file="ChkParam.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Check parameter.
    /// </summary>
    internal class ChkParam
    {
        /// <summary>
        /// Name.
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; } = "";

        /// <summary>
        /// Unit string.
        /// </summary>
        public string Unit { get; set; } = "";

        /// <summary>
        /// Is Min check enabled or not.
        /// </summary>
        public bool IsEnabledMin { get; set; }
        
        /// <summary>
        /// Min value.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Is Max check enabled or not.
        /// </summary>
        public bool IsEnabledMax { get; set; }

        /// <summary>
        /// Max value.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Is enabled this or not.
        /// </summary>
        [JsonIgnore]
        public bool IsEnabled => IsEnabledMin || IsEnabledMax;

        /// <summary>
        /// Is NG Min check or not.
        /// </summary>
        [JsonIgnore]
        public bool IsNGMin { get; set; }

        /// <summary>
        /// Is NG Max check or not.
        /// </summary>
        [JsonIgnore]
        public bool IsNGMax { get; set; }

        /// <summary>
        /// Is NG check.
        /// </summary>
        [JsonIgnore]
        public bool IsNG => IsNGMin || IsNGMax;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ChkParam()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="unit">Unit string.</param>
        /// <param name="isEnabledMin">Is Min check enabled or not.</param>
        /// <param name="min">Min value.</param>
        /// <param name="isEnabledMax">Is Max check enabled or not.</param>
        /// <param name="max">Max value.</param>
        public ChkParam(string unit, bool isEnabledMin, double min, bool isEnabledMax, double max)
        {
            Unit = unit;

            IsEnabledMin = isEnabledMin;
            Min = min;

            IsEnabledMax = isEnabledMax;
            Max = max;
        }

        /// <summary>
        /// Clear status.
        /// </summary>
        public void ClearStat()
        {
            IsNGMin = false;
            IsNGMax = false;
        }

        /// <summary>
        /// Check NG.
        /// </summary>
        /// <param name="val">Value.</param>
        /// <returns>true:NG</returns>
        public bool CheckNG(double val)
        {
            return (IsEnabledMin && val < Min) || (IsEnabledMax && val > Max);
        }

        /// <summary>
        /// Update status.
        /// </summary>
        /// <param name="values">Values.</param>
        public void UpdateStat(List<double> values)
        {
            if (IsEnabledMin)
            {
                IsNGMin = values.Any(v => v < Min);
            }

            if (IsEnabledMax)
            {
                IsNGMax = values.Any(v => v > Max);
            }
        }
    }
}
