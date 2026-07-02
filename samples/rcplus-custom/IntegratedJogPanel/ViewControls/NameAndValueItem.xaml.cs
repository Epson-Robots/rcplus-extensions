// -----------------------------------------------------------------------
// <copyright file="NameAndValueItem.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for NameAndValueItem.xaml
    /// </summary>
    public partial class NameAndValueItem : UserControl
    {
        /// <summary>
        /// Is this start item
        /// </summary>
        public bool IsStartItem
        {
            set => _borderHead.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Name Text
        /// </summary>
        public string TxtName
        {
            set => _txtBlockName.Text = value;
        }

        /// <summary>
        /// Value Text
        /// </summary>
        public string TxtValue
        {
            set => _txtBlockValue.Text = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public NameAndValueItem()
        {
            InitializeComponent();
        }
    }
}
