// -----------------------------------------------------------------------
// <copyright file="OnOffButton.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for OnOffButton.xaml
    /// </summary>
    public partial class OnOffButton : UserControl
    {
		public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(OnOffButton), new PropertyMetadata(""));

		public string LabelText
		{
			get => (string)GetValue(LabelTextProperty);
			set => SetValue(LabelTextProperty, value);
		}

        /// <summary>
        /// Is swtich on
        /// </summary>
        public bool SwitchOn
        {
            set
            {
                _switchOn = value;
                UpdateView();
            }
            get => _switchOn;
        }

        /// <summary>
        /// On/Off changed Func
        /// </summary>
        public Func<bool, bool>? ChangedFunc;

        /// <summary>
        /// On/Off changed Func for async
        /// </summary>
        public Func<bool, Task<bool>>? ChangedFuncAsync;

        bool _switchOn;

        /// <summary>
        /// Constructor.
        /// </summary>
        public OnOffButton()
        {
            InitializeComponent();
            UpdateView();

            _gridMark.MouseDown += async (s, e) =>
            {
                var checkOK = false;
                var soNew = !_switchOn;

                if (ChangedFuncAsync != null)
                {
                    checkOK = await ChangedFuncAsync.Invoke(!_switchOn);
                }
                else if (ChangedFunc != null)
                {
                    checkOK = ChangedFunc.Invoke(!_switchOn);
                }
                else
                {
                    checkOK = true;
                }

                if (!checkOK)
                {
                    return;
                }

                _switchOn = soNew;
                GeneralManager.Instance.AddDataToCollect($"OnOffButton/{this.Name}/" + (_switchOn ? "On" : "Off"));

                UpdateView();
            };
        }

        private void UpdateView()
        {
            var brush = _switchOn ? SolidBrushes.CtrlBlue : Brushes.Gray;
            _ellipseL.Fill = brush;
            _ellipseR.Fill = brush;
            _rectangle.Fill = brush;
            _textBlockOnOff.Text = _switchOn ? "On" : "Off";
            _ellipseOn.Visibility = _switchOn ? Visibility.Visible : Visibility.Hidden;
            _ellipseOff.Visibility = _switchOn ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Switch on/off with registered Func.
        /// </summary>
        /// <param name="on"></param>
        public void SwitchOnWithFunc(bool on)
        {
            var checkOK = false;

            if (ChangedFunc != null)
            {
                checkOK = ChangedFunc.Invoke(on);
            }
            else
            {
                checkOK = true;
            }

            if (!checkOK)
            {
                return;
            }

            _switchOn = on;
            UpdateView();
        }
    }
}
