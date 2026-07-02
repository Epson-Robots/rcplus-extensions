// -----------------------------------------------------------------------
// <copyright file="JogItemPanel.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Jog item panel.
    /// </summary>
    public class JogItemPanel : Panel
    {
        /// <summary>
        /// Header text
        /// </summary>
        public string Header
        {
            set
            {
                HeaderCtrl._txtTitle.Text = value;
            }
            get => HeaderCtrl._txtTitle.Text;
        }

        /// <summary>
        /// Is expanded
        /// </summary>
        public bool IsExpanded
        {
            set
            {
                _isExpanded = value;
                UpdateDisplay();
            }
            get => _isExpanded;
        }

        /// <summary>
        /// Is content enabled
        /// </summary>
        public bool IsContentEnabled
        {
            set
            {
                _isContentEnabled = value;
                UpdateDisplay();
            }
            get => _isContentEnabled;
        }

        /// <summary>
        /// Show horizontal selection or not
        /// </summary>
        public bool ShowHSel
        {
            set
            {
                HeaderCtrl._hSel.Visibility = value? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Header Control
        /// </summary>
        public JogItemPanelHeader HeaderCtrl { get; } = new JogItemPanelHeader();
        
        private bool _isExpanded = true;
        private bool _isContentEnabled = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public JogItemPanel()
        {
            InternalChildren.Add(HeaderCtrl);

            UpdateDisplay();

            HeaderCtrl._hSel.Init(new string[] { "Cont", "Long", "Mid", "Short" });
            HeaderCtrl._hSel.SelectedWord = "Cont";
            HeaderCtrl._gridExpander.MouseDown += (s, e) =>
            {
                _isExpanded = !_isExpanded;
                UpdateDisplay();
            };
        }

        private void UpdateDisplay()
        {
            HeaderCtrl._markToClose.Visibility = _isExpanded ? Visibility.Visible : Visibility.Hidden;
            HeaderCtrl._markToOpen.Visibility = !_isExpanded ? Visibility.Visible : Visibility.Hidden;
            HeaderCtrl._btnEdit.IsEnabled = _isContentEnabled;

            foreach (UIElement child in InternalChildren)
            {
                if (child != HeaderCtrl)
                {
                    child.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
                    child.IsEnabled = _isContentEnabled;
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                resultSize.Width = Math.Max(resultSize.Width, child.DesiredSize.Width);
                resultSize.Height += child.DesiredSize.Height;
            }

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double y = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(0, y, finalSize.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
            }
            return finalSize;
        }
    }
}
