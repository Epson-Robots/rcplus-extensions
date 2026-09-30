// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelFile.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Extension : Project File Editor (Specific Part)
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Load content of the file
        /// </summary>
        internal void LoadContent()
        {
            if (FilePath != null)
            {
                WindowCaption = new(FileName);

                try
                {
                    using FileStream fileStream = new(FilePath, FileMode.Open, FileAccess.Read);
                    _proFuseData.ReadFromFile(fileStream);
                    _proFuseData.SetOriginal();

                    SelectedCTContsId.Value = _proFuseData.OtherData.LastCTContsId;
                    SelectedToolType.Value = _proFuseData.OtherData.LastToolType;

                    SetPreferences();
                    SetProgramItems();
                    SetIOLabels();

                    PlotControlHolder.Value = Plot.PlotControl;

                    SetupLogsAndGraph();
                }
                catch (Exception)
                {
                    ErrorReporter.Message(Caption.LoadDefFileFailed);
                }
            }
        }

        /// <summary>
        /// Save current content into the file
        /// </summary>
        internal void SaveContent()
        {
            if (FilePath != null)
            {
                try
                {
                    using FileStream fileStream = new(FilePath, FileMode.Create, FileAccess.Write);
                    _proFuseData.WriteToFile(fileStream);
                }
                catch (Exception)
                {
                    ErrorReporter.Message(Caption.SaveDefFileFailed);
                }
            }
        }
    }
}
