// -----------------------------------------------------------------------
// <copyright file="Map2DPointAdv.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;

namespace IntegratedJogPanel.ViewMap2D
{
    /// <summary>
    /// Interaction logic for Map2DPointAdv.xaml
    /// </summary>
    public partial class Map2DPointAdv : UserControl, IMap2DObject
    {
        private Map2DView _map2dView;
        private RobotPoint _pt;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map2dView">Map2DView</param>
        /// <param name="pt">RobotPoint</param>
        public Map2DPointAdv(Map2DView map2dView, RobotPoint pt)
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
            var agl = _map2dView.Conv3Dto2DAngle(_pt.U, _pt.V, _pt.W);
            _rot.Angle = -agl.Angle;
            _gridRot.Visibility = agl.Determinate ? Visibility.Visible : Visibility.Hidden;

            var sz = Map2DCurrentPos.CalcSizeByDepth(pt2D.Depth);

            _grid.Width = sz;
            _grid.Height = sz;

            Margin = new Thickness(ptView.X - _grid.Width / 2, ptView.Y - _grid.Height / 2, 0, 0);
        }
    }
}
