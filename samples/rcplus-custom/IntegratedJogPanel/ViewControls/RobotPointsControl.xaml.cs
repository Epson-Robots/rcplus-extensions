// -----------------------------------------------------------------------
// <copyright file="RobotPointsControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Model;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for RobotPointsControl.xaml
    /// </summary>
    public partial class RobotPointsControl : UserControl
    {
        /// <summary>
        /// Selection chagend Action
        /// </summary>
        public Action? SelectionChangedAction { get; set; }

        /// <summary>
        /// Edited Action
        /// </summary>
        public Action? EditedAction { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RobotPointsControl()
        {
            InitializeComponent();

            _dataGrid.ItemsSource = GeneralManager.Instance.RobotPoints;

            _dataGrid.SelectionChanged += (s, e) => SelectionChangedAction?.Invoke();

            _dataGrid.SourceUpdated += (s, e) =>
            {
                EditedAction?.Invoke();
            };

            _dataGrid.PreparingCellForEdit += (s, e) =>
            {
                if (e.EditingElement is TextBox textBox)
                {
                    var currentColumn = e.Column;

                    if (currentColumn.Header.ToString() == "Label*")
                    {
                        textBox.PreviewTextInput -= TextBox_PreviewTextInputLabel;
                        textBox.PreviewTextInput += TextBox_PreviewTextInputLabel;
                    }
                    else if (currentColumn.Header.ToString() == "Description*")
                    {
                        // Do nothing.
                    }
                    else
                    {
                        textBox.PreviewTextInput -= TextBox_PreviewTextInputNum;
                        textBox.PreviewTextInput += TextBox_PreviewTextInputNum;
                    }
                }
            };
        }

        private string GetNewText(TextBox tb, string nextText)
        {
            return tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, nextText);
        }

        private void TextBox_PreviewTextInputLabel(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            var regex = new Regex(@"^\D.*");
            e.Handled = !regex.IsMatch(GetNewText(textBox, e.Text));
        }

        private void TextBox_PreviewTextInputNum(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            e.Handled = !double.TryParse(GetNewText(textBox, e.Text), out _);
        }

        /// <summary>
        /// Reset items.
        /// </summary>
        public void ResetItems()
        {
            _dataGrid.ItemsSource = null;
            _dataGrid.ItemsSource = GeneralManager.Instance.RobotPoints;
        }

        /// <summary>
        /// Select last item.
        /// </summary>
        public void SelectLastItem()
        {
            SelectIndex(_dataGrid.Items.Count - 1);
        }

        /// <summary>
        /// Select index.
        /// </summary>
        /// <param name="idx">Index</param>
        public void SelectIndex(int idx)
        {
            if (idx < 0)
            {
                return;
            }

            if (idx >=  _dataGrid.Items.Count)
            {
                return;
            }

            _dataGrid.SelectedIndex = idx;
            _dataGrid.ScrollIntoView(_dataGrid.SelectedItem);
        }

        /// <summary>
        /// Get selected RobotPoint.
        /// </summary>
        /// <returns>RobotPoint</returns>
        public RobotPoint? GetSelectedRobotPoint()
        {
            if (_dataGrid.SelectedItem is RobotPoint point)
            {
                return point;
            }

            return null;
        }

        /// <summary>
        /// Get selected index.
        /// </summary>
        /// <returns>Selected index.</returns>
        public int GetSelectedIndex()
        {
            return _dataGrid.SelectedIndex;
        }
    }
}
