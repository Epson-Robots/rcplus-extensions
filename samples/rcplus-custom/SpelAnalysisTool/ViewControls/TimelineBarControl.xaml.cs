// -----------------------------------------------------------------------
// <copyright file="TimelineBarControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for TimelineBarControl.xaml
    /// </summary>
    public partial class TimelineBarControl : UserControl
    {
        private LogDataSection.SammaryRecord _sammary;
        private Func<LogDataSection.CycleRecord?> _getCycleFunc;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sammary">Sammary record.</param>
        /// <param name="getCycleFunc">Get cycle Func.</param>
        public TimelineBarControl(LogDataSection.SammaryRecord sammary, Func<LogDataSection.CycleRecord?> getCycleFunc)
        {
            _sammary = sammary;
            _getCycleFunc = getCycleFunc;

            InitializeComponent();
        }

        /// <summary>
        /// Update data.
        /// </summary>
        /// <param name="total">Total size.</param>
        /// <param name="line1Start">Line1 start.</param>
        /// <param name="line1End">Line1 end.</param>
        /// <param name="line2Start">Line2 start.</param>
        /// <param name="line2End">Line2 end.</param>
        public void UpdateData(double total, double line1Start, double line1End, double line2Start, double line2End)
        {
            if (total <= 0)
            {
                _colLine1A.Width = new GridLength(1.0, GridUnitType.Star);
                _colLine1B.Width = new GridLength(0.0, GridUnitType.Star);
                _colLine1C.Width = new GridLength(0.0, GridUnitType.Star);
                _txt1.Text = "";

                _colLine2A.Width = new GridLength(1.0, GridUnitType.Star);
                _colLine2B.Width = new GridLength(0.0, GridUnitType.Star);
                _colLine2C.Width = new GridLength(0.0, GridUnitType.Star);
                _txt2.Text = "";
            }
            else
            {
                _colLine1A.Width = new GridLength(line1Start, GridUnitType.Star);
                _colLine1B.Width = new GridLength(line1End - line1Start, GridUnitType.Star);
                _colLine1C.Width = new GridLength(total - line1End, GridUnitType.Star);
                _txt1.Text = $"{line1End - line1Start:0.000}";

                _colLine2A.Width = new GridLength(line2Start, GridUnitType.Star);
                _colLine2B.Width = new GridLength(line2End - line2Start, GridUnitType.Star);
                _colLine2C.Width = new GridLength(total - line2End, GridUnitType.Star);
                _txt2.Text = $"{line2End - line2Start:0.000}";
            }
        }

        /// <summary>
        /// Update current line.
        /// </summary>
        public void UpdateCurrentLine()
        {
            var motionCycle = _getCycleFunc();
            if (motionCycle == null) return;

            var sams = GeneralManager.Instance.LogDatSection.SammaryRecords;
            if (sams.Count <= 0) return;

            var cycles = GeneralManager.Instance.LogDatSection.CycleRecords;
            var cycle = cycles.FirstOrDefault(c => c.CycleNo == motionCycle.CycleNo);
            if (cycle == null) return;

            var total = Math.Max(sams[0].EndTime, cycle.ElapsedTime);

            var startTime = 0.0;
            var endTime = 0.0;

            if (_sammary.SectionName == "*Total")
            {
                startTime = 0.0;
                endTime = cycle.ElapsedTime;
            }
            else
            {
                var sections = GeneralManager.Instance.LogDatSection.SectionRecords;
                var section = sections.FirstOrDefault(s => s.CycleNo == motionCycle.CycleNo && s.SectionName == _sammary.SectionName);

                if (section != null)
                {
                    startTime = section.StartTime - cycle.StartTime;
                    endTime = section.EndTime - cycle.StartTime;
                }
            }

            UpdateData(total, _sammary.StartTime, _sammary.EndTime, startTime, endTime);
        }
    }
}
