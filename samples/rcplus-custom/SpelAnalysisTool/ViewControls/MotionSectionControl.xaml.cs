// -----------------------------------------------------------------------
// <copyright file="MotionSectionControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for MotionSectionControl.xaml
    /// </summary>
    public partial class MotionSectionControl : UserControl
    {
        /// <summary>
        /// Title.
        /// </summary>
        public string TxtTitle
        {
            set => _txtTitle.Text = value;
        }

        /// <summary>
        /// Brush.
        /// </summary>
        public Brush HeaderBrush
        {
            set => _gridHeader.Background = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MotionSectionControl()
        {
            InitializeComponent();
        }
    }
}
