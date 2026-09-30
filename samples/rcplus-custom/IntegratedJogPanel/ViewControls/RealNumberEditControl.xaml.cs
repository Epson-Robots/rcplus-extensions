// -----------------------------------------------------------------------
// <copyright file="RealNumberEditControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for RealNumberEditControl.xaml
    /// </summary>
    public partial class RealNumberEditControl : UserControl
    {
        /// <summary>
        /// Value
        /// </summary>
        public double Val
        {
            set => _txtVal.Text = $"{value:0.000}";
            get => double.Parse(_txtVal.Text);
        }

        /// <summary>
        /// Minimum
        /// </summary>
        public double Min { get; set; } = 0.0;

        /// <summary>
        /// Maximum
        /// </summary>
        public double Max { get; set; } = 10.0;

        /// <summary>
        /// Get differece value in spin Func
        /// </summary>
        public Func<double>? GetSpinDiffFunc { get; set; }

        /// <summary>
        /// Changed Action
        /// </summary>
        public Action<RealNumberEditControl>? ChangedAction { get; set; }

        /// <summary>
        /// Unit label
        /// </summary>
        public string UnitLabel
        {
            set => _txtUnitLabel.Text = value;
            get => _txtUnitLabel.Text;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RealNumberEditControl()
        {
            InitializeComponent();

            Func<double> getSpinDiff = () =>
            {
                var ret = 0.001;

                if (GetSpinDiffFunc != null)
                {
                    ret = GetSpinDiffFunc();
                }

                return ret;
            };

            Action changedProc = () =>
            {
                ChangedAction?.Invoke(this);
            };

            _btnUp.Click += (s, e) =>
            {
                Val = Math.Clamp(Val + getSpinDiff(), Min, Max);
                changedProc();
            };

            _btnDown.Click += (s, e) =>
            {
                Val = Math.Clamp(Val - getSpinDiff(), Min, Max);
                changedProc();
            };

            _txtVal.PreviewTextInput += (s, e) =>
            {
                var textBox = (TextBox)s;
                var isNumber = System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[0-9]");
                var isDot = e.Text == "." && !textBox.Text.Contains(".");

                e.Handled = !(isNumber || isDot);
            };

            _txtVal.LostFocus += (s, e) =>
            {
                var textBox = (TextBox)s;

                if (double.TryParse(textBox.Text, out var res))
                {
                    Val = Math.Clamp(res, Min, Max);
                    changedProc();
                }
                else
                {
                    Val = Val;
                }
            };
        }
    }
}
