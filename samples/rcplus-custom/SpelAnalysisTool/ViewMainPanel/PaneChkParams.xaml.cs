// -----------------------------------------------------------------------
// <copyright file="PaneChkParams.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using SpelAnalysisTool.ViewControls;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for PaneChkParams.xaml
    /// </summary>
    public partial class PaneChkParams : UserControl, IMainPanelPane
    {
        /// <inheritdoc/>
        public string PaneName => "ChkParams";

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaneChkParams()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Init(MainPanel mainPanel)
        {
            Action refreshChkParams = () =>
            {
                _stackPanelChkParams.Children.Clear();

                GeneralManager.Instance.Conf.Set.ChkParams.Params.ForEach(param =>
                {
                    _stackPanelChkParams.Children.Add(new ChkParamControl() { DataContext = param });
                });
            };
            refreshChkParams();
            mainPanel.OnShowWin += (s, e) => refreshChkParams();

            _btnApplyChkParams.Click += (s, e) =>
            {
                GeneralManager.Instance.Conf.Save();
                mainPanel.UpdateDisplay();
            };
        }
    }
}
