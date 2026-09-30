// -----------------------------------------------------------------------
// <copyright file="AddSpelLoggerWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;

namespace SpelAnalysisTool.ViewWindows
{
    /// <summary>
    /// Interaction logic for AddSpelLoggerWindow.xaml
    /// </summary>
    public partial class AddSpelLoggerWindow : Window
    {
        public bool IsSpelLoggerTypeFile => _radioButtonFile.IsChecked == true;

        public bool AddSample => _checkBoxAddSample.IsChecked == true;

        public AddSpelLoggerWindow()
        {
            InitializeComponent();

            _btnOK.Click += (s, e) => { DialogResult = true; Close(); };
            _btnCancel.Click += (s, e) => Close();
        }
    }
}
