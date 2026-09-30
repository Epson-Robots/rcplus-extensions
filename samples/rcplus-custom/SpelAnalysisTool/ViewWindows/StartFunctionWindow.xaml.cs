// -----------------------------------------------------------------------
// <copyright file="StartFunctionWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows;

namespace SpelAnalysisTool.ViewWindows
{
    /// <summary>
    /// Interaction logic for StartFunctionWindow.xaml
    /// </summary>
    public partial class StartFunctionWindow : Window
    {
        public string SelectedFuncName => (string)_comboBoxFunction.SelectedItem;

        public bool EnableRuntimeObservation => _chkBoxEnableRuntimeObservation.IsChecked == true;

        public StartFunctionWindow()
        {
            InitializeComponent();

            var functions = GeneralManager.Instance.GetFunctionNames();

            if (functions != null)
            {
                functions.ForEach(func => _comboBoxFunction.Items.Add(func));
            }

            var funcDef = "main";

            if (_comboBoxFunction.Items.Count <= 0)
            {
                _comboBoxFunction.Items.Add(funcDef);
            }

            if (_comboBoxFunction.Items.Contains(GeneralManager.Instance.Conf.Set.StartFunctionName))
            {
                _comboBoxFunction.SelectedItem = GeneralManager.Instance.Conf.Set.StartFunctionName;
            }
            else if (_comboBoxFunction.Items.Contains(funcDef))
            {
                _comboBoxFunction.SelectedItem = funcDef;
            }
            else
            {
                _comboBoxFunction.SelectedIndex = 0;
            }

            _btnStartFunction.Click += (s, e) =>
            {
                GeneralManager.Instance.Conf.Set.StartFunctionName = SelectedFuncName;
                DialogResult = true;
                Close();
            };

            _btnCancel.Click += (s, e) => Close();
        }
    }
}
