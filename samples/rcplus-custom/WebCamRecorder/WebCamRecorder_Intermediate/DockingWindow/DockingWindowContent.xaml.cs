// -----------------------------------------------------------------------
// <copyright file="DockingWindowContent.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;

namespace WebCamRecorder.DockingWindow
{
    /// <summary>
    /// Code Behind of DockingWindowContent Control
    /// </summary>
    public partial class DockingWindowContent : UserControl
    {
        public DockingWindowContent()
        {
            InitializeComponent();

            if (DataContext is DockingWindowContentViewModel viewModel)
            {
                viewModel.SetPreviewImage(PreviewImage);
            }
        }
    }
}
