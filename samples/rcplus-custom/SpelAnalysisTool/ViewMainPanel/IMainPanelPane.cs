// -----------------------------------------------------------------------
// <copyright file="IMainPanelPane.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// MainPanelPane interface.
    /// </summary>
    internal interface IMainPanelPane
    {
        /// <summary>
        /// Pane name.
        /// </summary>
        public string PaneName { get; }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="mainPanel">MainPanel</param>
        public void Init(MainPanel mainPanel);
    }
}
