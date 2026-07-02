// -----------------------------------------------------------------------
// <copyright file="DockingWindowContent.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SMCElectricGripper.DockingWindow
{
    /// <summary>
    /// Code Behind of DockingWindowContent Control
    /// </summary>
    public partial class DockingWindowContent : UserControl
    {
        private static readonly Regex _numericRegex = new(@"^\d*\.?\d*$");

        public DockingWindowContent()
        {
            InitializeComponent();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                e.Handled = true;
                return;
            }

            var newText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength).Insert(textBox.SelectionStart, e.Text);
            e.Handled = !_numericRegex.IsMatch(newText);
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var pasteText = (string)e.DataObject.GetData(typeof(string))!;
            if (!_numericRegex.IsMatch(pasteText))
            {
                e.CancelCommand();
            }
        }

        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                CommitTextBox(textBox);
            }
        }

        private void NumericTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                CommitTextBox(textBox);
                e.Handled = true;
            }
        }

        private static void CommitTextBox(TextBox textBox)
        {
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
            if (double.TryParse(
                    textBox.Text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value))
            {
                textBox.Text = value.ToString(CultureInfo.InvariantCulture);
            }

            if (textBox.Tag is ICommand command && command.CanExecute(null))
            {
                command.Execute(null);
            }
        }
    }
}
