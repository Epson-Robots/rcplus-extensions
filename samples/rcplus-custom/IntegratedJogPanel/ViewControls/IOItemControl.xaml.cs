// -----------------------------------------------------------------------
// <copyright file="IOItemControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Epson.RoboticsShared.ExtensionsAPI;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for IOItemControl.xaml
    /// </summary>
    public partial class IOItemControl : UserControl
    {
        /// <summary>
        /// Is this start item
        /// </summary>
        public bool IsStartItem
        {
            set
            {
                if (value)
                {
                    _border.BorderThickness = new Thickness(1);
                    Width = 35;
                }
            }
        }

        /// <summary>
        /// Is this item selected
        /// </summary>
        public bool IsSelected
        {
            get => _border.Background == SolidBrushes.CtrlBlue;
            set
            {
                _border.Background = value ? SolidBrushes.CtrlBlue : Brushes.White;
                _txtBlockLabel.Foreground = value ? Brushes.White : Brushes.Black;
                _txtBlockBitNo.Foreground = value ? Brushes.White : Brushes.Black;
            }
        }

        /// <summary>
        /// Is dummy (Use in IOBitSelectWindow)
        /// </summary>
        public bool IsDummy
        {
            get => _isDummy;
            set
            {
                _isDummy = value;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Cliced Action
        /// </summary>
        public Action<IOItemControl>? ClickedAction { get; set; }

        private bool _isDummy;
        private IOItem _ioItem;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ioItem">IOItem</param>
        /// <param name="isStartItem">Is this start item</param>
        public IOItemControl(IOItem ioItem, bool isStartItem = false)
        {
            _ioItem = ioItem;

            InitializeComponent();

            IsStartItem = isStartItem;
            UpdateDisplay();

            PreviewMouseDown += (s, e) => ClickedAction?.Invoke(this);

            _ioItem.StatChangedAction = () =>
            {
                _markGreen.Fill = _ioItem.IsOn ? Brushes.Green : Brushes.LightGray;
            };

            _markGreen.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (IsDummy || _ioItem.BitNo < 0 || _ioItem.IOKind != IRCXIOAPI.RCXIOKind.Output)
                {
                    return;
                }

                if (e.ClickCount != 2)
                {
                    return;
                }

                _ioItem.SetIOStat(_markGreen.Fill == Brushes.LightGray);
            };
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            _txtBlockBitNo.Text = _ioItem.BitNo < 0 ? "" : $"{_ioItem.BitNo}";
            _txtBlockLabel.Text = _ioItem.Label;
            _markGreen.Visibility = (IsDummy || _ioItem.BitNo < 0) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
