// -----------------------------------------------------------------------
// <copyright file="DataGridCommon.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.Windows.Controls;

namespace SpelAnalysisTool.Common
{
    /// <summary>
    /// DataGrid extension methods.
    /// </summary>
    internal static class DataGridCommon
    {
        /// <summary>
        /// Select index.
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="idx">Index</param>
        public static void SelectIndex(this DataGrid dataGrid, int idx)
        {
            if (idx < 0 || idx >= dataGrid.Items.Count) return;

            Main.GetAPI<IRCXGeneralAPI>().DataCollectionEnabled = false;
            dataGrid.SelectedIndex = idx;
            Main.GetAPI<IRCXGeneralAPI>().DataCollectionEnabled = true;

            dataGrid.ScrollIntoView(dataGrid.SelectedItem);
        }

        /// <summary>
        /// Select last item.
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        public static void SelectLast(this DataGrid dataGrid)
        {
            if (dataGrid.Items.Count <= 0) return;
            dataGrid.SelectIndex(dataGrid.Items.Count - 1);
        }

        /// <summary>
        /// Increment selected index.
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="inc">Increment number.</param>
        /// <returns>true:Success. / false:Out of range.</returns>
        public static bool IncSelect(this DataGrid dataGrid, int inc = 1)
        {
            if (dataGrid.Items.Count <= 0) return false;

            var idx = dataGrid.SelectedIndex;
            if (idx < 0 || idx >= (dataGrid.Items.Count - 1)) return false;

            var idxNext = Math.Min(idx + inc, dataGrid.Items.Count - 1);
            dataGrid.SelectIndex(idxNext);

            return idxNext < (dataGrid.Items.Count - 1);
        }
    }
}
