// -----------------------------------------------------------------------
// <copyright file="PaneAdjParams.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using SpelAnalysisTool.ViewControls;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for PaneAdjParams.xaml
    /// </summary>
    public partial class PaneAdjParams : UserControl, IMainPanelPane
    {
        /// <inheritdoc/>
        public string PaneName => "AdjParams";

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaneAdjParams()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Init(MainPanel mainPanel)
        {
            Action refreshAdjParams = () =>
            {
                _listBoxAdjParams.Items.Clear();

                GeneralManager.Instance.Conf.Set.AdjParams.ForEach(param =>
                {
                    _listBoxAdjParams.Items.Add(new AdjParamControl() { DataContext = param });
                });
            };
            refreshAdjParams();
            mainPanel.OnShowWin += (s, e) => refreshAdjParams();

            _btnAdjParamsToInc.Click += async (s, e) =>
            {
                await GeneralManager.Instance.WriteAdjParamsInc();
                GeneralManager.Instance.Conf.Save();
            };

            _btnRemoveAdjParam.Click += (s, e) =>
            {
                var idx = _listBoxAdjParams.SelectedIndex;

                if (idx < 0)
                {
                    return;
                }

                GeneralManager.Instance.Conf.Set.AdjParams.RemoveAt(idx);
                GeneralManager.Instance.Conf.Save();
                refreshAdjParams();
            };

            _btnAddAdjParam.Click += (s, e) =>
            {
                GeneralManager.Instance.Conf.Set.AdjParams.Add(new AdjParam());
                GeneralManager.Instance.Conf.Save();
                refreshAdjParams();
            };
        }
    }
}
