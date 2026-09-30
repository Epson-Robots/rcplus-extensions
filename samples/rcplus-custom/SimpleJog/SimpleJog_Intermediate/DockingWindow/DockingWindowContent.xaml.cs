// -----------------------------------------------------------------------
// <copyright file="DockingWindowContent.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;

namespace SimpleJog.DockingWindow
{
    /// <summary>
    /// Code Behind of DockingWindowContent Control
    /// </summary>
    public partial class DockingWindowContent : UserControl
    {
        public DockingWindowContent()
        {
            InitializeComponent();

            TeachingLog.Loaded += (_, _) =>
            {
                TeachingLog.MaxHeight = TeachingLog.ActualHeight;
            };
        }
    }
}
