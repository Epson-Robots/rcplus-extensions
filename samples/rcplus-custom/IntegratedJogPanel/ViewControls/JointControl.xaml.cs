// -----------------------------------------------------------------------
// <copyright file="JointControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Epson.RoboticsShared.ExtensionsAPI;
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for JointControl.xaml
    /// </summary>
    public partial class JointControl : UserControl
    {
        /// <summary>
        /// Joint index
        /// </summary>
        public int JointIndex
        {
            set
            {
                _jointIndex = value;
                _txtBlockLabel.Text = $"J{_jointIndex + 1}";
            }
        }

        /// <summary>
        /// Jogging Action
        /// </summary>
        public Action? JoggingAction { get; set; }

        /// <summary>
        /// Jogged Action
        /// </summary>
        public Action? JoggedAction { get; set; }

        private int _jointIndex;

        /// <summary>
        /// Constructor.
        /// </summary>
        public JointControl()
        {
            InitializeComponent();

            _pathNeedle.Visibility = Visibility.Hidden;

            _btnMinus.Click += async (s, e) => await ExecJogProc(_btnMinus, false);
            _btnPlus.Click += async (s, e) => await ExecJogProc(_btnPlus, true);
            _btnMinus.PreviewMouseLeftButtonDown += async (s, e) => await ExecContJogProc(_btnMinus, false);
            _btnPlus.PreviewMouseLeftButtonDown += async (s, e) => await ExecContJogProc(_btnPlus, true);
        }

        private async Task ExecJogProc(Button btn, bool plus)
        {
            if (GeneralManager.Instance.JogDistanceJoint.SelectedType == IRCXRobotManagerAPI.RCXJogDistance.Continuous)
            {
                return;
            }

            btn.IsEnabled = false;
            JoggingAction?.Invoke();
            await RobotController.Instance.StartJointJog(_jointIndex, plus, false);
            JoggedAction?.Invoke();
            btn.IsEnabled = true;
        }

        private async Task ExecContJogProc(Button btn, bool plus)
        {
            if (GeneralManager.Instance.JogDistanceJoint.SelectedType != IRCXRobotManagerAPI.RCXJogDistance.Continuous)
            {
                return;
            }

            JoggingAction?.Invoke();
            await RobotController.Instance.StartJointJog(_jointIndex, plus, true);
            JoggedAction?.Invoke();
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            if (!RobotController.Instance.IsConnected || _jointIndex >= RobotController.Instance.Joints.Count)
            {
                IsEnabled = false;
                _pathNeedle.Visibility = Visibility.Hidden;
                _txtBlockMin.Text = "";
                _txtBlockMax.Text = "";
                _txtBlockCurrent.Text = "";
                _txtJogDistance.Val = 0;
                return;
            }

            IsEnabled = true;

            _pathNeedle.Visibility = Visibility.Visible;

            var joint = RobotController.Instance.Joints[_jointIndex];

            if (_gridLine.ActualWidth > 0)
            {
                _pathNeedle.Margin = new Thickness((_gridLine.ActualWidth - 2) * joint.GetCurrentPosRate(), 0, 0, 0);
            }

            _txtBlockMin.Text = $"{joint.MinMotionRange:0}";
            _txtBlockMax.Text = $"{joint.MaxMotionRange:0}";
            _txtBlockCurrent.Text = $"{joint.CurrentPos:0}";
            _txtJogDistance.Val = GeneralManager.Instance.JogDistanceJoint.GetDistance(_jointIndex);
        }
    }
}
