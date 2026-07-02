// -----------------------------------------------------------------------
// <copyright file="Map2DPoint.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntegratedJogPanel.ViewMap2D
{
    /// <summary>
    /// Interaction logic for Map2DPoint.xaml
    /// </summary>
    public partial class Map2DPoint : UserControl, IMap2DObject
    {
        /// <summary>
        /// RobotPoint
        /// </summary>
        public RobotPoint Pt { get; }

        /// <summary>
        /// Is selected
        /// </summary>
        public bool IsSelected
        {
            set
            {
                _circle.Fill = value ? SolidBrushes.Blue : Brushes.Red;
            }
            get => _circle.Fill == SolidBrushes.Blue;
        }

        private Map2DView _map2dView;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map2dView">Map2DView</param>
        /// <param name="pt">RobotPoint</param>
        public Map2DPoint(Map2DView map2dView, RobotPoint pt)
        {
            _map2dView = map2dView;
            Pt = pt;

            InitializeComponent();

            _txtBlock.Text = $"{Pt.Number}";
            _circle.Fill = Brushes.Red;

            MouseDown += async (s, e) =>
            {
                if (_map2dView.NoticeSelectPointAction != null)
                {
                    await _map2dView.NoticeSelectPointAction.Invoke(Pt);
                }
            };
        }

        /// <inheritdoc/>
        public void Draw()
        {
            var ptView = CalcViewPos();
            Margin = new Thickness(ptView.X - _circle.Width / 2, ptView.Y - _circle.Height / 2, 0, 0);
        }

        /// <summary>
        /// Calcurate view position.
        /// </summary>
        /// <returns>XY</returns>
        public (double X, double Y) CalcViewPos()
        {
            var pt2D = _map2dView.Conv3Dto2D(Pt.X, Pt.Y, Pt.Z);
            return _map2dView.CalcViewPos(pt2D.X, pt2D.Y);
        }
    }
}
