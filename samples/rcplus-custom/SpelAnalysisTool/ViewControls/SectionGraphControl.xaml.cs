// -----------------------------------------------------------------------
// <copyright file="SectionGraphControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for SectionGraphControl.xaml
    /// </summary>
    public partial class SectionGraphControl : UserControl
    {
        private LogDataSection.SammaryRecord _sammary;
        private Func<LogDataSection.CycleRecord?> _getCycleFunc;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sammary">Sammary record.</param>
        /// <param name="getCycleFunc">Get cycle Func.</param>
        public SectionGraphControl(LogDataSection.SammaryRecord sammary, Func<LogDataSection.CycleRecord?> getCycleFunc)
        {
            _sammary = sammary;
            _getCycleFunc = getCycleFunc;

            InitializeComponent();

            _graph.Init(sammary.Mean, sammary.StdDev, sammary.Datas);
            UpdateCurrentLine();
        }

        /// <summary>
        /// Update current cylcle line display.
        /// </summary>
        public void UpdateCurrentLine()
        {
            var cycle = _getCycleFunc();
            if (cycle == null) return;

            var records = GeneralManager.Instance.LogDatSection.SectionRecords;
            if (!records.Any(rec => rec.CycleNo == cycle.CycleNo)) return;

            if (_sammary.SectionName == "*Total")
            {
                _graph.DrawCurrentLine(records.Where(rec => rec.CycleNo == cycle.CycleNo).Sum(rec => rec.ElapsedTime));
            }
            else
            {
                var target = records.FirstOrDefault(rec => rec.CycleNo == cycle.CycleNo && rec.SectionName == _sammary.SectionName);
                if (target == null) return;

                _graph.DrawCurrentLine(target.ElapsedTime);
            }
        }
    }
}
