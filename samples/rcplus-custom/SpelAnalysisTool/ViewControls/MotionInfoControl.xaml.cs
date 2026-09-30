// -----------------------------------------------------------------------
// <copyright file="MotionInfoControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for MotionInfoControl.xaml
    /// </summary>
    public partial class MotionInfoControl : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MotionInfoControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clear all.
        /// </summary>
        public void ClearAll()
        {
            var txts = new List<TextBlock>()
            {
                _txtCycle, _txtTime, _txtSectionName,
                _txtX, _txtY, _txtZ, _txtU, _txtV, _txtW,
                _txtJ1, _txtJ2, _txtJ3, _txtJ4, _txtJ5, _txtJ6,
                _txtTQ1, _txtTQ2, _txtTQ3, _txtTQ4, _txtTQ5, _txtTQ6,
                _txtVelocity, _txtTCPSpeed,
            };

            txts.ForEach(txt => txt.Text = "");

            _colWidthVelocity.Width = new GridLength(0, GridUnitType.Pixel);
            _colWidthTCPSpeed.Width = new GridLength(0, GridUnitType.Pixel);
        }

        /// <summary>
        /// Update display.
        /// </summary>
        /// <param name="rec">Motion record.</param>
        public void UpdateDisplay(LogDataMotion.Record rec)
        {
            _txtCycle.Text = $"Cycle {rec.CycleNo} / {GeneralManager.Instance.LogDatSection.GetCycleCnt()}";
            VALTXT(_txtTime, rec.ElapsedTime);
            _txtSectionName.Text = rec.SectionName;

            VALTXT(_txtX, rec.X, rec.IsXNG);
            VALTXT(_txtY, rec.Y, rec.IsYNG);
            VALTXT(_txtZ, rec.Z, rec.IsZNG);
            VALTXT(_txtU, rec.U, rec.IsUNG);
            VALTXT(_txtV, rec.V, rec.IsVNG);
            VALTXT(_txtW, rec.W, rec.IsWNG);

            VALTXT(_txtJ1, rec.J1, rec.IsJ1NG);
            VALTXT(_txtJ2, rec.J2, rec.IsJ2NG);
            VALTXT(_txtJ3, rec.J3, rec.IsJ3NG);
            VALTXT(_txtJ4, rec.J4, rec.IsJ4NG);
            VALTXT(_txtJ5, rec.J5, rec.IsJ5NG);
            VALTXT(_txtJ6, rec.J6, rec.IsJ6NG);

            VALTXT(_txtTQ1, rec.Torque1, rec.IsTorque1NG);
            VALTXT(_txtTQ2, rec.Torque2, rec.IsTorque2NG);
            VALTXT(_txtTQ3, rec.Torque3, rec.IsTorque3NG);
            VALTXT(_txtTQ4, rec.Torque4, rec.IsTorque4NG);
            VALTXT(_txtTQ5, rec.Torque5, rec.IsTorque5NG);
            VALTXT(_txtTQ6, rec.Torque6, rec.IsTorque6NG);

            VALTXT(_txtVelocity, rec.Velocity, rec.IsVelocityNG);
            VALTXT(_txtTCPSpeed, rec.TCPSpeed, rec.IsTCPSpeedNG);

            _colWidthVelocity.Width = new GridLength(Math.Max(0.0, rec.Velocity / 10.0), GridUnitType.Pixel);
            _colWidthTCPSpeed.Width = new GridLength(Math.Max(0.0, rec.TCPSpeed / 10.0), GridUnitType.Pixel);
        }

        private void VALTXT(TextBlock txt, double val, bool isNG = false)
        {
            txt.Text = $"{val:0.000}";
            txt.Foreground = isNG ? Brushes.Red : Brushes.Black;
        }

        /// <summary>
        /// Update display joint only.
        /// </summary>
        /// <param name="joints">Joint positions.</param>
        public void UpdateDisplayJointOnly(List<double> joints)
        {
            ClearAll();

            var txts = new List<TextBlock>()
            {
                _txtJ1, _txtJ2, _txtJ3, _txtJ4, _txtJ5, _txtJ6,
            };

            for (var i = 0; i < Math.Min(joints.Count, txts.Count); i++)
            {
                VALTXT(txts[i], joints[i]);
            }
        }
    }
}
