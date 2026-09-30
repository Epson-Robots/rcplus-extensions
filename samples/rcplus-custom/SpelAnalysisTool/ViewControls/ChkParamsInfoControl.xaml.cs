// -----------------------------------------------------------------------
// <copyright file="ChkParamsInfoControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for ChkParamsInfoControl.xaml
    /// </summary>
    public partial class ChkParamsInfoControl : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ChkParamsInfoControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clear display.
        /// </summary>
        public void ClearDisplay()
        {
            _stackPanel.Children.Clear();
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            _stackPanel.Children.Clear();
            if (GeneralManager.Instance.LogDatSection.CycleRecords.Count <= 0) return;

            var chkParams = GeneralManager.Instance.Conf.Set.ChkParams;
            chkParams.UpdateStat();

            foreach (var param in chkParams.Params)
            {
                if (!param.IsEnabled)
                {
                    continue;
                }

                var stack = new StackPanel() { Orientation = Orientation.Horizontal, };
                stack.Children.Add(new Ellipse(){ Fill = param.IsNG ? Brushes.Red : Brushes.LightGreen, });
                stack.Children.Add(new TextBlock() { Text = param.Name });
                _stackPanel.Children.Add(stack);
            }
        }
    }
}
