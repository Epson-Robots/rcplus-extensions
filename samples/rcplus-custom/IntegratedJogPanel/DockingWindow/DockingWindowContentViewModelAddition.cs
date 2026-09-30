// -----------------------------------------------------------------------
// <copyright file="DockingWindowContentViewModelAddition.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IntegratedJogPanel.DockingWindow
{
    /// <summary>
    /// Extension : Docking Window (Specific Part)
    /// </summary>
    internal partial class DockingWindowContentViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DockingWindowContentViewModel()
        {
        }

        /// <inheritdoc />
        public Task WindowCreated()
        {
            return Task.CompletedTask;
        }
    }
}
