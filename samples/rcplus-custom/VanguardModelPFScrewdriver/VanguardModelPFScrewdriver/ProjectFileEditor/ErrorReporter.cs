// -----------------------------------------------------------------------
// <copyright file="ErrorReporter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using VanguardModelPFScrewdriver.ModelPF;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Provides a static entry point for reporting errors and displaying error messages
    /// through the currently registered <see cref="ProjectFileEditorViewModel"/>.
    /// </summary>
    internal static class ErrorReporter
    {
        /// <summary>
        /// The view model that receives error reports and message requests.
        /// </summary>
        private static ProjectFileEditorViewModel? _viewModel;

        /// <summary>
        /// Registers the view model used to handle error reporting and starts its error logger.
        /// </summary>
        /// <param name="viewModel">The view model to delegate error handling to.</param>
        public static void SetDelegate(
            ProjectFileEditorViewModel viewModel
        )
        {
            _viewModel = viewModel;

            viewModel.StartErrorLogger();
        }

        /// <summary>
        /// Reports an error operation to the error log.
        /// </summary>
        /// <param name="errorOperation">The error operation to be recorded.</param>
        public static void Report(
            ErrorLogEntry.ErrOp errorOperation
        )
        {
            ProjectFileEditorViewModel.ReportError(errorOperation);
        }

        /// <summary>
        /// Shows an error message identified by the specified caption identifier.
        /// Does nothing when no view model has been registered.
        /// </summary>
        /// <param name="captionId">The identifier of the message caption to display.</param>
        /// <param name="ownerWindow">The optional owner window of the message dialog.</param>
        public static void Message(
            int captionId,
            Window? ownerWindow = null
        )
        {
            _viewModel?.Message(captionId, ownerWindow);
        }
    }
}
