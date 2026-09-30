// -----------------------------------------------------------------------
// <copyright file="PercentageInfoControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for PercentageInfoControl.xaml
    /// </summary>
    public partial class PercentageInfoControl : UserControl
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string TxtName
        {
            set => _txtName.Text = value;
        }

        /// <summary>
        /// Description.
        /// </summary>
        public string TxtDescription
        {
            set => _txtDescription.Text = value;
        }

        /// <summary>
        /// Percentage.
        /// </summary>
        public double ValPct
        {
            set => _txtPct.Text = $"{value:0.0}";
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PercentageInfoControl()
        {
            InitializeComponent();
        }
    }
}
