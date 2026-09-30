// -----------------------------------------------------------------------
// <copyright file="CellControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for CellControl.xaml
    /// </summary>
    public partial class CellControl : UserControl
    {
        /// <summary>
        /// Index.
        /// </summary>
        public int Idx
        {
            get => _idx;
            set
            {
                _idx = value;
                _txtIdx.Text = $"{_idx}";
            }
        }

        /// <summary>
        /// Text.
        /// </summary>
        public string Txt
        {
            get => _txtName.Text;
            set => _txtName.Text = value;
        }

        /// <summary>
        /// Clicked Action.
        /// </summary>
        public Action<int>? ClickedAction { get; set; }

        /// <summary>
        /// Is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateDisplay();
            }
        }

        private int _idx;
        private bool _isSelected;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CellControl()
        {
            InitializeComponent();

            _border.PreviewMouseDown += (s, e) =>
            {
                ClickedAction?.Invoke(Idx);
            };
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            _border.Background = _isSelected ? Brushes.DodgerBlue : Brushes.White;
            _txtIdx.Foreground = _isSelected ? Brushes.White : Brushes.Black;
            _txtName.Foreground = _isSelected ? Brushes.White : Brushes.Black;
        }
    }
}
