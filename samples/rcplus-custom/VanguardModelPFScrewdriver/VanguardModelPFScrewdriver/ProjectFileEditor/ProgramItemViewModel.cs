// -----------------------------------------------------------------------
// <copyright file="ProgramItemViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using VanguardModelPFScrewdriver.ModelPF;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// View model of a single program entry shown in the program list.
    /// </summary>
    public class ProgramItemViewModel
    {
        /// <summary>
        /// Gets the text shown in the list, formatted as "&lt;number&gt;: &lt;name&gt;".
        /// </summary>
        public ReactivePropertySlim<string> DisplayName { get; } = new();

        /// <summary>
        /// Gets or sets the program number identifying this entry.
        /// </summary>
        public int ProgramNo { get; set; }

        /// <summary>
        /// Applies the specified program data to this view model.
        /// </summary>
        /// <param name="program">Program data to display.</param>
        public void Set(
            ModelPFData.Program program
        )
        {
            // Build the display text and keep the number for later identification.
            DisplayName.Value = $"{program.ProgramNo}: {program.Name}";
            ProgramNo = program.ProgramNo;
        }
    }
}
