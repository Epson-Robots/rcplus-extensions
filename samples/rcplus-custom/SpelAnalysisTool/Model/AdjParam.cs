// -----------------------------------------------------------------------
// <copyright file="AdjParam.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Adjustment parameter.
    /// </summary>
    internal class AdjParam
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Comment.
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Value.
        /// </summary>
        public int Value { get; set; }
    }
}
