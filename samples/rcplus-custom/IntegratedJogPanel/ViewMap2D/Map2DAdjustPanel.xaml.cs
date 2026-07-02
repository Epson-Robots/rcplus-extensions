// -----------------------------------------------------------------------
// <copyright file="Map2DAdjustPanel.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;

namespace IntegratedJogPanel.ViewMap2D
{
    /// <summary>
    /// Interaction logic for Map2DAdjustPanel.xaml
    /// </summary>
    public partial class Map2DAdjustPanel : UserControl
    {
        private Map2DView _map2dView;
        private RobotPoint? Pt { get; set; }

        private class ReqCmdMng
        {
            private List<string> _reqCmds = new List<string>()
            {
                "Update", "Insert Before", "Insert After",
            };
            private int _selectedCmdIdx;

            public string Cmd => _reqCmds[_selectedCmdIdx];

            public string Chanage()
            {
                _selectedCmdIdx++;

                if (_selectedCmdIdx >= _reqCmds.Count)
                {
                    _selectedCmdIdx = 0;
                }

                return Cmd;
            }
        }

        private ReqCmdMng _reqCmdMng = new ReqCmdMng();

        private bool _dragOn;
        private double _szHalf;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map2dView">Map2DView</param>
        public Map2DAdjustPanel(Map2DView map2dView)
        {
            _map2dView = map2dView;

            InitializeComponent();

            _szHalf = _grid.Width / 2;
            _line.Visibility = Visibility.Collapsed;

            _txtBlock.Text = _reqCmdMng.Cmd;
            _btnChanage.PreviewMouseDown += (s, e) =>
            {
                _txtBlock.Text = _reqCmdMng.Chanage();
                e.Handled = true;
            };

            PreviewMouseDown += (s, e) =>
            {
                var pos = e.GetPosition(this);

                if (!GeometricalFunctions.IsPtInsideCircle(_szHalf, _szHalf, pos.X, pos.Y, 20.0))
                {
                    return;
                }

                _dragOn = true;
                _line.X1 = _szHalf;
                _line.Y1 = _szHalf;
                _line.X2 = _szHalf;
                _line.Y2 = _szHalf;
                _line.Visibility = Visibility.Visible;

                e.Handled = true;
                CaptureMouse();
            };

            PreviewMouseMove += (s, e) =>
            {
                var pos = e.GetPosition(this);

                if (!_dragOn)
                {
                    return;
                }

                _line.Visibility = Visibility.Visible;
                _line.X1 = _szHalf;
                _line.Y1 = _szHalf;
                _line.X2 = pos.X;
                _line.Y2 = pos.Y;

                e.Handled = true;
            };

            PreviewMouseUp += async (s, e) =>
            {
                ReleaseMouseCapture();

                if (!_dragOn)
                {
                    return;
                }

                var pos = e.GetPosition(this);

                _dragOn = false;

                await _map2dView.ReqAdust(Pt, _txtBlock.Text, pos.X - _szHalf, pos.Y - _szHalf);
                _line.Visibility = Visibility.Collapsed;

                e.Handled = true;
            };
        }

        /// <summary>
        /// Start adjust.
        /// </summary>
        /// <param name="robotPoint">RobotPoint</param>
        public void StartAdjust(RobotPoint robotPoint)
        {
            _dragOn = false;
            _line.Visibility = Visibility.Collapsed;

            Pt = robotPoint;
            var pt2D = _map2dView.Conv3Dto2D(Pt.X, Pt.Y, Pt.Z);
            var ptView = _map2dView.CalcViewPos(pt2D.X, pt2D.Y);

            Margin = new Thickness(ptView.X - _grid.Width / 2, ptView.Y - _grid.Height / 2, 0, 0);
        }
    }
}
