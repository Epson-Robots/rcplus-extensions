// -----------------------------------------------------------------------
// <copyright file="ViewDirectionSelectControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for ViewDirectionSelectControl.xaml
    /// </summary>
    public partial class ViewDirectionSelectControl : UserControl
    {
        /// <summary>
        /// Selected Action
        /// </summary>
        public Action<ViewDirection.Option>? SelectedAction { get; set; }

        private List<ViewDirectionControl> _optControls = new List<ViewDirectionControl>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewDirectionSelectControl()
        {
            InitializeComponent();

            _optControls.Add(_optCtrl0);
            _optControls.Add(_optCtrl1);
            _optControls.Add(_optCtrl2);
            _optControls.Add(_optCtrl3);
            _optControls.Add(_optCtrl4);
            _optControls.Add(_optCtrl5);
            _optControls.Add(_optCtrl6);
            _optControls.Add(_optCtrl7);

            _optControls.ForEach(ctrl =>
            {
                ctrl.ClickedAction = t => SelectType(t);
            });
        }

        /// <summary>
        /// Select type.
        /// </summary>
        /// <param name="t">Type</param>
        public void SelectType(ViewDirection.Option t)
        {
            foreach (var c in _optControls)
            {
                c.IsSelected = t == c.Type;
            }

            SelectedAction?.Invoke(t);
        }

        /// <summary>
        /// Get selected type.
        /// </summary>
        /// <returns>Selected type</returns>
        public ViewDirection.Option GetSelectedType()
        {
            return _optControls.FirstOrDefault(c => c.IsSelected)?.Type ?? 0;
        }
    }
}
