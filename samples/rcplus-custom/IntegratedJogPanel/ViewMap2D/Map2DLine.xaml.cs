// -----------------------------------------------------------------------
// <copyright file="Map2DLine.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Model;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntegratedJogPanel.ViewMap2D
{
    /// <summary>
    /// Interaction logic for Map2DLine.xaml
    /// </summary>
    public partial class Map2DLine : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Map2DLine()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Scale line.
    /// </summary>
    public class Map2DLineScale : Map2DLine, IMap2DObject
    {
        private Map2DView _map2dview;
        private double _x1;
        private double _y1;
        private double _x2;
        private double _y2;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map2dview">Map2DView</param>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <param name="brush">Brush</param>
        public Map2DLineScale(Map2DView map2dview, double x1, double y1, double x2, double y2, SolidColorBrush brush)
        {
            _map2dview = map2dview;
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
            _line.Stroke = brush;
        }

        /// <inheritdoc/>
        public void Draw()
        {
            var p1 = _map2dview.CalcViewPos(_x1, _y1);
            var p2 = _map2dview.CalcViewPos(_x2, _y2);

            _line.X1 = p1.X;
            _line.Y1 = p1.Y;

            _line.X2 = p2.X;
            _line.Y2 = p2.Y;
        }
    }

    /// <summary>
    /// Locus line.
    /// </summary>
    public class Map2DLineLocus : Map2DLine, IMap2DObject
    {
        private Map2DView _map2dview;
        private RobotPoint _ptA;
        private RobotPoint _ptB;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map2dview">Map2DView</param>
        /// <param name="ptA">RobotPoint A</param>
        /// <param name="ptB">RobotPoint B</param>
        public Map2DLineLocus(Map2DView map2dview, RobotPoint ptA, RobotPoint ptB)
        {
            _map2dview = map2dview;
            _ptA = ptA;
            _ptB = ptB;
            _line.Stroke = Brushes.Red;
        }

        /// <inheritdoc/>
        public void Draw()
        {
            {
                var pt2D = _map2dview.Conv3Dto2D(_ptA.X, _ptA.Y, _ptA.Z);
                var ptView = _map2dview.CalcViewPos(pt2D.X, pt2D.Y);

                _line.X1 = ptView.X;
                _line.Y1 = ptView.Y;
            }

            {
                var pt2D = _map2dview.Conv3Dto2D(_ptB.X, _ptB.Y, _ptB.Z);
                var ptView = _map2dview.CalcViewPos(pt2D.X, pt2D.Y);

                _line.X2 = ptView.X;
                _line.Y2 = ptView.Y;
            }
        }
    }
}
