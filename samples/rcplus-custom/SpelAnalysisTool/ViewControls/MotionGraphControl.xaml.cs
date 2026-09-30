// -----------------------------------------------------------------------
// <copyright file="MotionGraphControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for MotionGraphControl.xaml
    /// </summary>
    public partial class MotionGraphControl : UserControl
    {
        private string[] _targets = {
            "Velocity", "TCPSpeed",
            "X", "Y", "Z", "U", "V", "W",
            "J1", "J2", "J3", "J4", "J5", "J6",
            "Torque1", "Torque2", "Torque3", "Torque4", "Torque5", "Torque6",
        };

        /// <summary>
        /// Is shown or not.
        /// </summary>
        public bool IsShown { get; set; }

        /// <summary>
        /// Get cycle Func.
        /// </summary>
        public Func<LogDataSection.CycleRecord?>? GetCycleFunc { get; set; }
        
        /// <summary>
        /// Tareget selection changed Action.
        /// </summary>
        public Action? TargetSelectionChagendAction { get; set; }

        private int _graphIdx;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphIdx">Graph index.</param>
        public MotionGraphControl(int graphIdx)
        {
            _graphIdx = graphIdx;

            InitializeComponent();

            foreach (var target in _targets) _comboBoxTarget.Items.Add(target);

            LoadConf();

            _comboBoxTarget.SelectionChanged += (s, e) =>
            {
                UpdateDisplay();
                TargetSelectionChagendAction?.Invoke();
            };

            UpdateDisplay();
        }

        /// <summary>
        /// Load configuration.
        /// </summary>
        public void LoadConf()
        {
            var selectedItem = "Velocity";

            if (_graphIdx < GeneralManager.Instance.Conf.Set.SelectedMotionGraphs.Count)
            {
                selectedItem = GeneralManager.Instance.Conf.Set.SelectedMotionGraphs[_graphIdx];
            }

            _comboBoxTarget.SelectedItem = selectedItem;
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            if (GetCycleFunc == null) return;
            if (!IsShown) return;

            var cycle = GetCycleFunc();
            if (cycle == null) return;

            var motCycle = GeneralManager.Instance.LogDatMotion.GetCycleData(cycle.CycleNo);
            if (motCycle == null) return;

            _graph.Init(motCycle.GetLineGraphData((string)_comboBoxTarget.SelectedItem));
        }

        /// <summary>
        /// Clear graph.
        /// </summary>
        public void Clear()
        {
            _graph.Clear();
        }
    }
}
