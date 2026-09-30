// -----------------------------------------------------------------------
// <copyright file="TriangleSliderControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for TriangleSliderControl.xaml
    /// </summary>
    public partial class TriangleSliderControl : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TriangleSliderControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set rate.
        /// </summary>
        /// <param name="rate">Rate of position.</param>
        public void SetRate(double rate)
        {
            _colDefA.Width = new GridLength(rate, GridUnitType.Star);
            _colDefB.Width = new GridLength(1.0 - rate, GridUnitType.Star);
        }
    }
}
