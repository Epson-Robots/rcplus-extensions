// -----------------------------------------------------------------------
// <copyright file="SelectRCPlusPtsFileWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows;

namespace IntegratedJogPanel
{
    /// <summary>
    /// Interaction logic for SelectRCPlusPtsFileWindow.xaml
    /// </summary>
    public partial class SelectRCPlusPtsFileWindow : Window
    {
        /// <summary>
        /// Selected filename
        /// </summary>
        public string SelectedFilename { get; private set; } = "";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filenames">Filename's list</param>
        public SelectRCPlusPtsFileWindow(List<string> filenames)
        {
            InitializeComponent();

            _listBox.ItemsSource = filenames;

            if (_listBox.Items.Count > 0)
            {
                _listBox.SelectedIndex = 0;
            }

            _btnOK.Click += (s, e) =>
            {
                if (_listBox.SelectedItem == null)
                {
                    return;
                }

                SelectedFilename = _listBox.SelectedItem.ToString() ?? "";
                Close();
            };

            _btnCancel.Click += (s, e) =>
            {
                Close();
            };
        }
    }
}
