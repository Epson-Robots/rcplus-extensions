// -----------------------------------------------------------------------
// <copyright file="Map2DPointLabel.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Model;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntegratedJogPanel.ViewMap2D
{
    /// <summary>
    /// Interaction logic for Map2DPointLabel.xaml
    /// </summary>
    public partial class Map2DPointLabel : UserControl, IMap2DObject
    {
        private Map2DView _map2dView;
        private RobotPoint _pt;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map2dView">Map2DView</param>
        /// <param name="pt">RobotPoint</param>
        public Map2DPointLabel(Map2DView map2dView, RobotPoint pt)
        {
            _map2dView = map2dView;
            _pt = pt;

            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Draw()
        {
            var pt2D = _map2dView.Conv3Dto2D(_pt.X, _pt.Y, _pt.Z);
            var ptView = _map2dView.CalcViewPos(pt2D.X, pt2D.Y);
            _txtTitle.Text = $"{_pt.Label}";
            var formattedText = new FormattedText(
                _txtTitle.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(_txtTitle.FontFamily, _txtTitle.FontStyle, _txtTitle.FontWeight, _txtTitle.FontStretch),
                _txtTitle.FontSize,
                _txtTitle.Foreground,
                VisualTreeHelper.GetDpi(_txtTitle).PixelsPerDip);
            Margin = new Thickness(ptView.X - formattedText.Width / 2, ptView.Y - formattedText.Height - 10, 0, 0);
        }
    }
}
