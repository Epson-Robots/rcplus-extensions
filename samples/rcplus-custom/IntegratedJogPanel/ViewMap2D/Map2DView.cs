// -----------------------------------------------------------------------
// <copyright file="RC2DView.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static IntegratedJogPanel.Constants;

namespace IntegratedJogPanel.ViewMap2D
{
    /// <summary>
    /// Map 2D view.
    /// </summary>
    public class Map2DView
    {
        public class TwoDViewConf
        {
            public ViewDirection.Option ViewDir { get; set; }
            public double OffsetX { get; set; } = 0;
            public double OffsetY { get; set; } = 0;
            public double Zoom { get; set; }
            public bool ShowPoints { get; set; } = true;
            public bool ShowLocus { get; set; } = true;
            public bool ShowPtAdv { get; set; }
            public bool ShowPtLabel { get; set; }
        }

        /// <summary>
        /// View configuration
        /// </summary>
        public TwoDViewConf ViewConf { get; } = new TwoDViewConf();

        /// <summary>
        /// Notice select point Action
        /// </summary>
        public Func<RobotPoint, Task>? NoticeSelectPointAction { get; set; }

        /// <summary>
        /// Notice adjust Action
        /// </summary>
        public Func<RobotPoint, string, double, double, double, Task>? NoticeAdjustAction { get; set; }

        /// <summary>
        /// Refresh view Action
        /// </summary>
        public Action<Map2DView>? RefreshAction { get; set; }

        /// <summary>
        /// Whether already Initialized
        /// </summary>
        public bool AlreadyInit { get; private set; }

        private const double DefMMWidth = 2000;
        private const double DefMMHeight = 2000;
        private const double AreaHfSzMM = 1000;

        private Canvas _canv { get; set; }
        private Canvas _canvPts { get; set; }
        private Canvas _canvAdj { get; set; }

        private Map2DLineScale _axisH;
        private Map2DLineScale _axisV;
        private RobotPoint _currentPos { get; } = new RobotPoint();
        private Map2DCurrentPos _currentPosCtrl;
        private Map2DAdjustPanel _adjPanel;
        private bool _mouseDownLeft;
        private Point _mousePos;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canvas">Base canvas</param>
        /// <param name="canvasPts">Canvas for Points</param>
        /// <param name="canvAdj">Canvas for Adjustment</param>
        /// <param name="map2dFront">Canvas for mouse operation</param>
        public Map2DView(Canvas canvas, Canvas canvasPts, Canvas canvAdj, Grid map2dFront)
        {
            _canv = canvas;
            _canvPts = canvasPts;
            _canvAdj = canvAdj;

            _adjPanel = new Map2DAdjustPanel(this);
            _canvAdj.Visibility = Visibility.Hidden;
            _canvAdj.Children.Add(_adjPanel);
            _canvAdj.MouseDown += (s, e) =>
            {
                _canvAdj.Visibility = Visibility.Hidden;
            };

            _axisH = new Map2DLineScale(this, -AreaHfSzMM, 0, +AreaHfSzMM, 0, Brushes.Green);
            _axisV = new Map2DLineScale(this, 0, -AreaHfSzMM, 0, +AreaHfSzMM, Brushes.Green);

            _currentPosCtrl = new Map2DCurrentPos(this, _currentPos);
            _currentPosCtrl.Visibility = Visibility.Hidden;

            map2dFront.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _mouseDownLeft = true;
                }

                _mousePos = e.GetPosition(map2dFront);

                e.Handled = false;
                map2dFront.CaptureMouse();
            };

            map2dFront.MouseUp += (s, e) =>
            {
                map2dFront.ReleaseMouseCapture();
                _mouseDownLeft = false;
            };

            map2dFront.MouseMove += (s, e) =>
            {
                if (!_mouseDownLeft)
                {
                    return;
                }

                var pos = e.GetPosition(map2dFront);

                ViewConf.OffsetX += (pos.X - _mousePos.X);
                ViewConf.OffsetY += (pos.Y - _mousePos.Y);
                Refresh();

                _mousePos = pos;
            };

            map2dFront.MouseWheel += (s, e) =>
            {
                var pos = e.GetPosition(map2dFront);
                var zoomBefrore = ViewConf.Zoom;

                ViewConf.Zoom += e.Delta * 0.0004;

                ViewConf.Zoom = Math.Min(ViewConf.Zoom, 8.0);
                ViewConf.Zoom = Math.Max(ViewConf.Zoom, 0.1);

                var zoomAfter = ViewConf.Zoom;

                ViewConf.OffsetX = pos.X - (pos.X - ViewConf.OffsetX) * zoomAfter / zoomBefrore;
                ViewConf.OffsetY = pos.Y - (pos.Y - ViewConf.OffsetY) * zoomAfter / zoomBefrore;
                Refresh();
            };
        }

        /// <summary>
        /// Intititalize view.
        /// </summary>
        public void Init()
        {
            if (AlreadyInit)
            {
                return;
            }

			var w = _canv.ActualWidth;
			var h = _canv.ActualHeight;

            if (w <= 0 || h <= 0)
            {
                return;
            }

            AlreadyInit = true;

            ViewConf.Zoom = Math.Min(w / DefMMWidth, h / DefMMHeight);
            ViewConf.OffsetX = w / 2;
            ViewConf.OffsetY = h / 2;

            _canv.Children.Clear();

            // Draw Scale
            var list10mm = new List<int>();
            var list50mm = new List<int>();
            var list100mm = new List<int>();

            for (int i = -(int)AreaHfSzMM; i <= AreaHfSzMM; i++)
            {
                if (i % 100 == 0)
                {
                    list100mm.Add(i);
                }
                else if (i % 50 == 0)
                {
                    list50mm.Add(i);
                }
                else if (i % 10 == 0)
                {
                    list10mm.Add(i);
                }
            }

            Action<List<int>, SolidColorBrush> drawScale = (list, brush) =>
            {
                foreach (var i in list)
                {
                    _canv.Children.Add(new Map2DLineScale(this, -AreaHfSzMM, i, +AreaHfSzMM, i, brush));
                    _canv.Children.Add(new Map2DLineScale(this, i, -AreaHfSzMM, i, +AreaHfSzMM, brush));
                }
            };

            drawScale(list10mm, SolidBrushes.GrayC);
            drawScale(list50mm, SolidBrushes.GrayB);
            drawScale(list100mm, SolidBrushes.GrayA);

            // Add Axes
            _canv.Children.Add(_axisH);
            _canv.Children.Add(_axisV);

            // Add Current Pos Mark
            _canv.Children.Add(_currentPosCtrl);

            Refresh();
        }

        /// <summary>
        /// Convert 3D to 2D.
        /// </summary>
        /// <param name="x">3D X</param>
        /// <param name="y">3D Y</param>
        /// <param name="z">3D Z</param>
        /// <returns>2D X,Y,Depth</returns>
        public (double X, double Y, double Depth) Conv3Dto2D(double x, double y, double z)
        {
            return ViewDirection.Conv3Dto2D(ViewConf.ViewDir, x, y, z);
        }

        /// <summary>
        /// Convert 3D to 2D angle.
        /// </summary>
        /// <param name="u">U</param>
        /// <param name="v">V</param>
        /// <param name="w">W</param>
        /// <returns>(2D angle, Dterminate)</returns>
        public (double Angle, bool Determinate) Conv3Dto2DAngle(double u, double v, double w)
        {
            return ViewDirection.Conv3Dto2DAngle(ViewConf.ViewDir, u, v, w);
        }

        /// <summary>
        /// Calcrate view position.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>View position XY</returns>
        public (double X, double Y) CalcViewPos(double x, double y)
        {
            var vX = x * ViewConf.Zoom + ViewConf.OffsetX;
            var vY = y * ViewConf.Zoom + ViewConf.OffsetY;

            return (vX, vY);
        }

        /// <summary>
        /// Get current view area.
        /// </summary>
        /// <returns>View area</returns>
        public (double X, double Y, double Width, double Height) GetCurrentViewArea()
        {
            var vX = -ViewConf.OffsetX / ViewConf.Zoom;
            var vY = -ViewConf.OffsetY / ViewConf.Zoom;
            var vWidth = _canv.ActualWidth / ViewConf.Zoom;
            var vHeight = _canv.ActualHeight / ViewConf.Zoom;

            return (vX, vY, vWidth, vHeight);
        }

        /// <summary>
        /// Change view direction.
        /// </summary>
        /// <param name="viewDir">View direction</param>
        public void ChangeViewDir(ViewDirection.Option viewDir)
        {
            Map2DPoint? selectedPoint = null;

            foreach (var item in _canvPts.Children)
            {
                if (item is Map2DPoint obj && obj.IsSelected)
                {
                    selectedPoint = obj;
                    break;
                }
            }

            // If there is a selection point, switch viewpoint without moving that point.
            if (selectedPoint != null)
            {
                var viewPosBefore = selectedPoint.CalcViewPos();
                ViewConf.ViewDir = viewDir;
                var viewPosAfter = selectedPoint.CalcViewPos();

                ViewConf.OffsetX += viewPosBefore.X - viewPosAfter.X;
                ViewConf.OffsetY += viewPosBefore.Y - viewPosAfter.Y;

                Refresh();
            }
            else
            {
                ViewConf.ViewDir = viewDir;
                Refresh();
            }

            // Set the axis color.
            if ((int)ViewConf.ViewDir < 4)
            {
                _axisH._line.Stroke = (int)ViewConf.ViewDir % 2 == 0 ? Brushes.Green : Brushes.Blue;
                _axisV._line.Stroke = (int)ViewConf.ViewDir % 2 == 0 ? Brushes.Blue : Brushes.Green;
            }
            else
            {
                _axisH._line.Stroke = (int)ViewConf.ViewDir % 2 == 0 ? Brushes.Green : Brushes.Blue;
                _axisV._line.Stroke = Brushes.Red;
            }
        }

        /// <summary>
        /// Refresh view.
        /// </summary>
        public void Refresh()
        {
            foreach (var item in _canv.Children)
            {
                if (item is IMap2DObject obj)
                {
                    obj.Draw();
                }
            }

            foreach (var item in _canvPts.Children)
            {
                if (item is IMap2DObject obj)
                {
                    obj.Draw();
                }
            }

            RefreshAction?.Invoke(this);
        }

        private void DrawRobotPoints()
        {
            for (int i = _canvPts.Children.Count - 1; i >= 0; i--)
            {
                if (_canvPts.Children[i] is Map2DPoint obj)
                {
                    _canvPts.Children.Remove(obj);
                }
            }

            if (ViewConf.ShowPoints)
            {
                foreach (var pt in GeneralManager.Instance.RobotPoints)
                {
                    _canvPts.Children.Add(new Map2DPoint(this, pt));
                }
            }

            Refresh();
        }

        private void DrawRobotPointLocus()
        {
            for (int i = _canv.Children.Count - 1; i >= 0; i--)
            {
                if (_canv.Children[i] is Map2DLineLocus obj)
                {
                    _canv.Children.Remove(obj);
                }
            }

            if (ViewConf.ShowLocus)
            {
                RobotPoint? ptA = null;

                foreach (var pt in GeneralManager.Instance.RobotPoints)
                {
                    if (ptA != null)
                    {
                        _canv.Children.Add(new Map2DLineLocus(this, ptA, pt));
                    }

                    ptA = pt;
                }
            }

            Refresh();
        }

        private void DrawRobotPtLabel()
        {
            for (int i = _canvPts.Children.Count - 1; i >= 0; i--)
            {
                if (_canvPts.Children[i] is Map2DPointLabel obj)
                {
                    _canvPts.Children.Remove(obj);
                }
            }

            if (ViewConf.ShowPtLabel)
            {
                foreach (var pt in GeneralManager.Instance.RobotPoints)
                {
                    _canvPts.Children.Add(new Map2DPointLabel(this, pt));
                }
            }

            Refresh();
        }

        private void DrawRobotPtAdv()
        {
            for (int i = _canvPts.Children.Count - 1; i >= 0; i--)
            {
                if (_canvPts.Children[i] is Map2DPointAdv obj)
                {
                    _canvPts.Children.Remove(obj);
                }
            }

            if (ViewConf.ShowPtAdv)
            {
                foreach (var pt in GeneralManager.Instance.RobotPoints)
                {
                    _canvPts.Children.Add(new Map2DPointAdv(this, pt));
                }
            }

            Refresh();
        }

        /// <summary>
        /// Show current robot position display.
        /// </summary>
        /// <param name="show">true:Show / false:Hide</param>
        public void ShowCurrentPos(bool show)
        {
            _currentPosCtrl.Visibility = show ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Update curent robot position display.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="u">U</param>
        /// <param name="v">V</param>
        /// <param name="w">W</param>
        public void UpdateCurrentPos(double x, double y, double z, double u, double v, double w)
        {
            _currentPos.X = x;
            _currentPos.Y = y;
            _currentPos.Z = z;
            _currentPos.U = u;
            _currentPos.V = v;
            _currentPos.W = w;

            _currentPosCtrl.Draw();
        }

        /// <summary>
        /// Select point.
        /// </summary>
        /// <param name="pt">Point</param>
        public void SelectRobotPoint(RobotPoint? pt)
        {
            Map2DPoint? child = null;

            foreach (var item in _canvPts.Children)
            {
                if (item is Map2DPoint obj)
                {
                    obj.IsSelected = obj.Pt == pt;

                    if (obj.IsSelected)
                    {
                        child = obj;
                    }
                }
            }

            if (child != null)
            {
                _canvPts.Children.Remove(child);
                _canvPts.Children.Add(child);
            }
        }

        /// <summary>
        /// Draw points.
        /// </summary>
        /// <param name="pt">Select point</param>
        public void DrawPoints(RobotPoint? pt)
        {
            DrawRobotPointLocus();
            DrawRobotPtAdv();
            DrawRobotPoints();
            DrawRobotPtLabel();
            SelectRobotPoint(pt);
        }

        /// <summary>
        /// Start adjustment. (call from MainPanel)
        /// </summary>
        /// <param name="pt">Target point</param>
        public void StartAdjust(RobotPoint pt)
        {
            Map2DPoint? ptCtrl = null;

            foreach (var item in _canvPts.Children)
            {
                if (item is Map2DPoint obj)
                {
                    if (obj.Pt == pt)
                    {
                        ptCtrl = obj;
                        break;
                    }
                }
            }

            if (ptCtrl == null)
            {
                return;
            }

            _canvAdj.Visibility = Visibility.Visible;
            _adjPanel.StartAdjust(pt);
        }

        /// <summary>
        /// Request adjsutment. (call from Map2DAdustmentPanel)
        /// </summary>
        /// <param name="pt">Target point</param>
        /// <param name="req">Request command phrase</param>
        /// <param name="x2D">X 2D</param>
        /// <param name="y2D">y 2D</param>
        /// <returns></returns>
        public async Task ReqAdust(RobotPoint? pt, string req, double x2D, double y2D)
        {
            var mmx2D = x2D / ViewConf.Zoom;
            var mmy2D = y2D / ViewConf.Zoom;

            var availableDistance = 50.0;

            if (!GeometricalFunctions.IsPtInsideCircle(0,0, mmx2D, mmy2D, availableDistance))
            {
                GeneralManager.Instance.ShowMsg(new(string.Format(Main.Captions?[Caption.ViewArea_AdujustModeAvailabelDistanceMsg] ?? "", availableDistance)));
                return;
            }

            if (NoticeAdjustAction != null && pt != null)
            {
                var d3D = ViewDirection.Conv2Dto3D(ViewConf.ViewDir, mmx2D, mmy2D);
                await NoticeAdjustAction(pt, req, d3D.X, d3D.Y, d3D.Z);
            }

            _canvAdj.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// End adjustment. (call from MainPanel)
        /// </summary>
        public void EndAdjust()
        {
            _canvAdj.Visibility = Visibility.Hidden;
        }
    }
}
