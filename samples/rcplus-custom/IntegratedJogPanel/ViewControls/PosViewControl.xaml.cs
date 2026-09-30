// -----------------------------------------------------------------------
// <copyright file="PosViewControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using IntegratedJogPanel.ViewMap2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for PosViewControl.xaml
    /// </summary>
    public partial class PosViewControl : UserControl
    {
        public ViewDirection.Option ViewDir
        {
            get => _viewDir;
            set
            {
                _viewDir = value;

                Grid[] grids = new Grid[] {
                    _gridMark0,
                    _gridMark1,
                    _gridMark2,
                    _gridMark3,
                    _gridMark4,
                    _gridMark5,
                    _gridMark6,
                    _gridMark7,
                };

                for (int i = 0; i < grids.Length; i++)
                {
                    grids[i].Visibility = i == (int)_viewDir ? Visibility.Visible : Visibility.Hidden;
                }

                var words = ViewDirection.GetAxesLabels(_viewDir);

                _txtBlockA.Text = words[0];
                _txtBlockB.Text = words[1];
                _txtBlockC.Text = words[2];
                _txtBlockD.Text = words[3];

                UpdateView();
            }
        }

        public RobotPointsControl? PtsCtrl { get; set; }

        private ViewDirection.Option _viewDir;
        private Rectangle _rectViewArea = new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)), };

        private int _idxRoboOpacity;
        private double[] _roboOpacities = { 1.0, 0.2, 0.0 };

        /// <summary>
        /// Constructor.
        /// </summary>
        public PosViewControl()
        {
            InitializeComponent();

            _canvasViewArea.Children.Add(_rectViewArea);

            _gridRoboMark.PreviewMouseDown += (s, e) =>
            {
                _idxRoboOpacity = (_idxRoboOpacity + 1) % _roboOpacities.Length;
                _gridRoboMark.Opacity = _roboOpacities[_idxRoboOpacity];
            };
        }

        /// <summary>
        /// Update view.
        /// </summary>
        public void UpdateView()
        {
            var w = _canvasPts.ActualWidth;
            var h = _canvasPts.ActualHeight;

            var w_h = w / 2;
            var h_h = h / 2;

            if (w < 0 || h < 0)
            {
                return;
            }

            _canvasPts.Children.Clear();
            var ellipseSz = 4.0;

            var selectedPt = PtsCtrl?.GetSelectedRobotPoint();

            foreach (var pt in GeneralManager.Instance.RobotPoints)
            {
                var pt2D = ViewDirection.Conv3Dto2D(_viewDir, pt.X, pt.Y, pt.Z);

                var x = pt2D.X * w_h / 1000 + w_h - ellipseSz / 2;
                var y = pt2D.Y * h_h / 1000 + h_h - ellipseSz / 2;

                var ellipse = new Ellipse()
                {
                    Margin = new Thickness(x, y, 0, 0),
                    Width = ellipseSz,
                    Height = ellipseSz,
                    Fill = selectedPt == pt ? SolidBrushes.Blue : Brushes.Red,
                };

                _canvasPts.Children.Add(ellipse);
            }
        }

        /// <summary>
        /// Refresh Map2DView area display.
        /// </summary>
        /// <param name="view">Map2DView</param>
        public void Refresh(Map2DView view)
        {
            if (!view.AlreadyInit)
            {
                return;
            }

            var w = _canvasViewArea.ActualWidth;
            var h = _canvasViewArea.ActualHeight;

            if (w <= 0 || h <= 0)
            {
                return;
            }

            var w_h = w / 2;
            var h_h = h / 2;

            var rate = w_h / 1000;

            var area = view.GetCurrentViewArea();

            _rectViewArea.Margin = new Thickness(area.X * rate + w_h, area.Y * rate + h_h, 0, 0);
            _rectViewArea.Width = area.Width * rate;
            _rectViewArea.Height = area.Height * rate;
        }
    }
}
