// -----------------------------------------------------------------------
// <copyright file="PlusMinusIntControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for PlusMinusIntControl.xaml
    /// </summary>
    public partial class PlusMinusIntControl : UserControl
    {
        /// <summary>
        /// Value
        /// </summary>
        public double Val
        {
            set
            {
                var txt = $"{value:0}";

                if (value == 0)
                {
                    // Zero is 0
                }
                else if (value < 0.01)
                {
                    txt = $"{value:0.000}";
                }
                else if (value < 0.1)
                {
                    txt = $"{value:0.00}";
                }
                else if (value < 1)
                {
                    txt = $"{value:0.0}";
                }

                _txtInt.Text = txt;
                var visibility = value == 0 ? Visibility.Hidden : Visibility.Visible;
                _mark.Visibility = visibility;
                _txtInt.Visibility = visibility;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlusMinusIntControl()
        {
            InitializeComponent();
        }
    }
}
