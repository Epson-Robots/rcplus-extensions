// -----------------------------------------------------------------------
// <copyright file="TwoOptionSelButton.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for TwoOptionSelButton.xaml
    /// </summary>
    public partial class TwoOptionSelButton : UserControl
    {
        /// <summary>
        /// Loabel text
        /// </summary>
        public string LabelTxt
        {
            set => _textBlockLabel.Text = value;
        }

        /// <summary>
        /// Text selection A
        /// </summary>
        public string TxtA
        {
            set => _txtA.Text = value;
        }

        /// <summary>
        /// Text selection B
        /// </summary>
        public string TxtB
        {
            set
            {
                _txtB.Text = value;
            }
        }

        /// <summary>
        /// Is selected B or not
        /// </summary>
        public bool IsSelectedB
        {
            set
            {
                _isSelectedB = value;
                UpdateView();
            }
            get => _isSelectedB;
        }

        /// <summary>
        /// Selection changed Func
        /// </summary>
        public Func<bool, bool>? ChangedFunc;

        /// <summary>
        /// Selection changed Func for async
        /// </summary>
        public Func<bool, Task<bool>>? ChangedFuncAsync;

        bool _isSelectedB;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TwoOptionSelButton()
        {
            InitializeComponent();
            UpdateView();

            _gridMark.MouseDown += async (s, e) =>
            {
                var checkOK = false;

                if (ChangedFuncAsync != null)
                {
                    checkOK = await ChangedFuncAsync.Invoke(!_isSelectedB);
                }
                else if (ChangedFunc != null)
                {
                    checkOK = ChangedFunc.Invoke(!_isSelectedB);
                }
                else
                {
                    checkOK = true;
                }

                if (!checkOK)
                {
                    return;
                }

                _isSelectedB = !_isSelectedB;
                UpdateView();
            };
        }

        private void UpdateView()
        {
            _gridA.Visibility = !_isSelectedB ? Visibility.Visible : Visibility.Hidden;
            _gridB.Visibility = _isSelectedB ? Visibility.Visible : Visibility.Hidden;
            _txtA.Foreground = !_isSelectedB ? Brushes.White : Brushes.LightGray;
            _txtB.Foreground = _isSelectedB ? Brushes.White : Brushes.LightGray;
        }
    }
}
