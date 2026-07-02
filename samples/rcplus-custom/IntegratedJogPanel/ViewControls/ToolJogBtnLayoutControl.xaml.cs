// -----------------------------------------------------------------------
// <copyright file="ToolJogBtnLayoutControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for ToolJogBtnLayoutControl.xaml
    /// </summary>
    public partial class ToolJogBtnLayoutControl : UserControl
    {
        /// <summary>
        /// Is selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                _border.Background = _isSelected ? SolidBrushes.TranspBlue : SolidBrushes.TranspWhite;
            }
        }

        /// <summary>
        /// Tool jog button layout Type
        /// </summary>
        public ToolJogBtnLayout.Option Type
        {
            get => _type;
            set
            {
                _type = value;

                var words = ToolJogBtnLayout.GetAxesLabels(_type);

                _txtBlockA.Text = words[0].Trim();
                _txtBlockB.Text = words[1].Trim();
                _txtBlockC.Text = words[2].Trim();
                _txtBlockD.Text = words[3].Trim();
            }
        }

        private bool _isSelected;
        private ToolJogBtnLayout.Option _type;

        /// <summary>
        /// Clicked Action
        /// </summary>
        public Action<ToolJogBtnLayout.Option>? ClickedAction { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ToolJogBtnLayoutControl()
        {
            InitializeComponent();

            PreviewMouseDown += (s, e) => ClickedAction?.Invoke(_type);
        }
    }
}
