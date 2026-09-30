// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditor.xaml.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using VanguardModelPFScrewdriver.Dialogs;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Code Behind of ProjectFileEditor Control
    /// </summary>
    public partial class ProjectFileEditor : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFileEditor"/> class
        /// and wires up the UI behaviors of the editor.
        /// </summary>
        public ProjectFileEditor()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                // Remember the hosting window so that dialogs can be shown as its child.
                DialogViewModel.DockingWindow = Window.GetWindow(this);
            };

            VerticalSplitter.DragCompleted += (_, ev) =>
            {
                if (ev.Canceled)
                {
                    return;
                }

                var newWidth = SettingsColumn.ActualWidth + ev.HorizontalChange;

                // Snap the settings column to either the collapsed or the expanded state.
                if (newWidth < SettingsColumn.MaxWidth / 2)
                {
                    CollapseSettingsColumn();
                }
                else
                {
                    ExpandSettingsColumn();
                }
            };

            // Editing a scale value implies that the corresponding axis is fixed.
            YScale.GotKeyboardFocus += (_, _) =>
            {
                if (YFixedScale.IsChecked == false)
                {
                    YFixedScale.IsChecked = true;
                }
            };
            XScale.GotKeyboardFocus += (_, _) =>
            {
                if (XFixedScale.IsChecked == false)
                {
                    XFixedScale.IsChecked = true;
                }
            };
        }

        /// <summary>
        /// Shrinks the settings column to its minimum width.
        /// </summary>
        private void CollapseSettingsColumn()
        {
            const double _minWidth = 10;

            SettingsColumn.Width = new GridLength(_minWidth);
        }

        /// <summary>
        /// Restores the settings column to its maximum width.
        /// </summary>
        public void ExpandSettingsColumn()
        {
            SettingsColumn.Width = new GridLength(SettingsColumn.MaxWidth);
        }
    }
}
