// -----------------------------------------------------------------------
// <copyright file="SectionPane.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using SpelAnalysisTool.ViewControls;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for SectionPane.xaml
    /// </summary>
    public partial class PaneSection : UserControl, IMainPanelPane
    {
        /// <inheritdoc/>
        public string PaneName => "Section";

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaneSection()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Init(MainPanel mainPanel)
        {
            mainPanel.DG_Section = _dataGrid;

            _headerSection.GetDataFunc = GeneralManager.Instance.LogDatSection.GetGridFormatSammaryData;

            _dataGrid.SelectionChanged += (s, e) =>
            {
                mainPanel.SelectMotionItem();
            };

            Action updateDisplay = () =>
            {
                _headerSection.Count = GeneralManager.Instance.LogDatSection.GetSectionCnt();
                UpdateDisplay(mainPanel.GetCycleData);
                UpdateCurrentLine();
            };

            mainPanel.OnStartFunctionPre += (s, e) =>
            {
                updateDisplay();
            };

            mainPanel.OnLoadCSVPost += (s, e) =>
            {
                updateDisplay();
            };

            mainPanel.OnRuntimeLoadCSVPost += (s, e) =>
            {
                updateDisplay();
            };

            mainPanel.OnCycleSelectionChanged += (s, e) =>
            {
                UpdateCurrentLine();
            };
        }

        private void UpdateDisplay(Func<LogDataSection.CycleRecord?> getCycleFunc)
        {
            var rec = GeneralManager.Instance.LogDatSection.SammaryRecords;

            rec.ForEach(r =>
            {
                r.StatGraph = new SectionGraphControl(r, getCycleFunc);
                r.TimelineBar = new TimelineBarControl(r, getCycleFunc);
            });

            _dataGrid.ItemsSource = null;
            _dataGrid.ItemsSource = rec;

            if (rec.Count > 0)
            {
                _dataGrid.SelectedIndex = 0;
            }
        }

        private void UpdateCurrentLine()
        {
            GeneralManager.Instance.LogDatSection.SammaryRecords.ForEach(r =>
            {
                if (r.StatGraph is SectionGraphControl sectionGraphCtl)
                {
                    sectionGraphCtl.UpdateCurrentLine();
                }

                if (r.TimelineBar is TimelineBarControl timelineBarCtl)
                {
                    try
                    {
                        timelineBarCtl.UpdateCurrentLine();
                    }
                    catch
                    {

                    }
                }
            });
        }
    }
}
