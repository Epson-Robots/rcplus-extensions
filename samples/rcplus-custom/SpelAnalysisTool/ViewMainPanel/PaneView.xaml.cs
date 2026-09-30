// -----------------------------------------------------------------------
// <copyright file="ViewPane.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Common;
using SpelAnalysisTool.Model;
using SpelAnalysisTool.View3D;
using SpelAnalysisTool.ViewControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for ViewPane.xaml
    /// </summary>
    public partial class PaneView : UserControl, IMainPanelPane
    {
        /// <inheritdoc/>
        public string PaneName => "View";

        private RC3DView _rc3dview = new RC3DView() { Width = 600, Height = 600 };
        private const double _floorSize = 2000.0;

        private MouseFor3D? _mouseFor3D;
        private MouseForGrid? _mouseForTriSlider;

        private Canvas _canvasFloorScale = new Canvas()
        {
            Opacity = 0.5,
            Background = new SolidColorBrush(Color.FromArgb(0, 3, 3, 3)),
            Width = _floorSize,
            Height = _floorSize,
        };

        private List<double> _reenactSpeeds = new List<double>()
        {
            0.1, 0.25, 0.5, 1, 2, 4, 10,
        };

        private double GetReenActSpeed()
        {
            var idx = 0;

            if (_comboBoxReenactSpeed.SelectedIndex >= 0)
            {
                idx = _comboBoxReenactSpeed.SelectedIndex;
            }

            return _reenactSpeeds[idx];
        }

        private const double TIMER_INTERVAL = 0.050;
        private DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL * 1000) };

        private class ReenactManager
        {
            public bool Reenacting => _reenacting;

            private bool _reenacting;
            private double _timerInterval;
            private Func<double> _getSpeedFunc;
            private bool _waiting;
            private int _waitCounter;

            public ReenactManager(double timerInterval, Func<double> getSpeedFunc)
            {
                _timerInterval = timerInterval;
                _getSpeedFunc = getSpeedFunc;
            }

            public void Resume()
            {
                _waiting = false;
                _waitCounter = 0;

                _reenacting = true;
            }

            public void Pause()
            {
                _reenacting = false;
            }

            public void Tick(DataGrid dataGrid)
            {
                if (!_reenacting)
                {
                    return;
                }

                if (_waitCounter > 0)
                {
                    _waitCounter--;
                    return;
                }

                if (_waiting)
                {
                    _reenacting = dataGrid.IncSelect(1);
                    _waiting = false;
                }

                var idxCurrent = dataGrid.SelectedIndex;

                if (idxCurrent < 0)
                {
                    Pause();
                    return;
                }

                if (idxCurrent >= dataGrid.Items.Count - 1)
                {
                    Pause();
                    return;
                }

                if (dataGrid.ItemsSource is not List<LogDataMotion.Record> records)
                {
                    Pause();
                    return;
                }

                var recCurrent = records[idxCurrent];
                var recNext = records[idxCurrent + 1];
                var timeDiff = (recNext.RealElapsedTime - recCurrent.RealElapsedTime) / _getSpeedFunc();

                if (timeDiff > _timerInterval)
                {
                    _waiting = true;
                    _waitCounter = Math.Max(1, (int)(timeDiff / _timerInterval));
                }
                else
                {
                    _waiting = false;
                    _waitCounter = 0;
                    _reenacting = dataGrid.IncSelect(Math.Max(1, (int)(_timerInterval / timeDiff)));
                }
            }
        }

        private ReenactManager _reenactManager;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaneView()
        {
            _reenactManager = new ReenactManager(TIMER_INTERVAL, GetReenActSpeed);

            InitializeComponent();
        }

        private void CreateFloorScale()
        {
            double canvasSize = _floorSize;
            double canvasSizeHalf = canvasSize / 2;
            double strokeThickness = 4;
            double strokeThicknessThin = 1;
            double fontSize = 100;

            {
                var brushScale = new SolidColorBrush(Color.FromArgb(200, 3, 3, 3));
                var brushAxisX = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                var brushAxisY = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                var brushAxisZ = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                var scaleCount = 20;
                var scaleSize = canvasSize / scaleCount;

                for (int i = 0; i <= scaleCount; i++)
                {
                    _canvasFloorScale.Children.Add(new Line() { X1 = 0, Y1 = i * scaleSize, X2 = canvasSize, Y2 = i * scaleSize, StrokeThickness = strokeThicknessThin, Stroke = brushScale });
                    _canvasFloorScale.Children.Add(new Line() { X1 = i * scaleSize, Y1 = 0, X2 = i * scaleSize, Y2 = canvasSize, StrokeThickness = strokeThicknessThin, Stroke = brushScale });
                }

                Func<string, Brush, double, double, Grid> createTB = (txt, brush, x, y) =>
                {
                    var grid = new Grid()
                    {
                        Margin = new Thickness(x - fontSize / 2, y - fontSize / 2, 0, 0),
                        Width = fontSize,
                        Height = fontSize,
                    };

                    grid.Children.Add(new TextBlock()
                    {
                        Text = txt,
                        Foreground = brush,
                        FontSize = fontSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    });

                    return grid;
                };

                _canvasFloorScale.Children.Add(createTB("X", brushAxisX, canvasSize - fontSize / 2, canvasSize / 2 + fontSize / 2));
                _canvasFloorScale.Children.Add(createTB("Y", brushAxisY, canvasSize / 2 - fontSize / 2, fontSize / 2));

                _canvasFloorScale.Children.Add(new Line() { X1 = 0, Y1 = canvasSize / 2, X2 = canvasSize, Y2 = canvasSize / 2, StrokeThickness = strokeThickness, Stroke = brushAxisX });
                _canvasFloorScale.Children.Add(new Line() { X1 = canvasSize / 2, Y1 = 0, X2 = canvasSize / 2, Y2 = canvasSize, StrokeThickness = strokeThickness, Stroke = brushAxisY });
            }
        }

        /// <inheritdoc/>
        public void Init(MainPanel mainPanel)
        {
            _mouseFor3D = new MouseFor3D(_rc3dview, _rc3dFront, _viewDirectionCtrl);

            Action showHide3DViewItem = () =>
            {
                _txtInfoHead.Visibility = _chkBoxShowInfo.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
                _rc3dview.FloorScale.Enabled = _chkBoxShowFloor.IsChecked == true;
                _rc3dview.Robot.Enabled = _chkBoxShowRobot.IsChecked == true;
                _rc3dview.Duct.Enabled = _chkBoxShowRobot.IsChecked == true;
                _rc3dview.Bellows.Enabled = _chkBoxShowRobot.IsChecked == true;
                _rc3dview.CurPoint.Enabled = _chkBoxShowCurPoint.IsChecked == true;
                _rc3dview.RobotPoints.Enabled = _chkBoxShowPoints.IsChecked == true;
                _rc3dview.RobotLocus.Enabled = _chkBoxShowLocus.IsChecked == true;
                _rc3dview.Refresh();
            };
            new List<CheckBox>()
            {
                _chkBoxShowInfo, _chkBoxShowFloor, _chkBoxShowRobot, _chkBoxShowCurPoint, _chkBoxShowPoints, _chkBoxShowLocus,
            }.ForEach(chk =>
            {
                chk.Checked += (s, e) => showHide3DViewItem();
                chk.Unchecked += (s, e) => showHide3DViewItem();
            });
            showHide3DViewItem();

            foreach (var speed in _reenactSpeeds)
            {
                _comboBoxReenactSpeed.Items.Add($"x{speed}");
            }
            _comboBoxReenactSpeed.SelectedIndex = 3;

            _btnReenactTop.Click += (s, e) => mainPanel.DG_Motion?.SelectIndex(0);
            _btnReenactPause.Click += (s, e) => _reenactManager.Pause();
            _btnReenactResume.Click += (s, e) => _reenactManager.Resume();

            _mouseForTriSlider = new MouseForGrid(_triSlider._grid);
            _mouseForTriSlider.MouseDragAction = mainPanel.OnMouseDrag;

            CreateFloorScale();
            _rc3dview.FloorScale.Draw(_canvasFloorScale, 2);
            _rc3dview.Refresh();
            _rc3dContent.Content = _rc3dview.Control;

            mainPanel.OnControllerConnected += async (s, e) =>
            {
                if (_radioLive.IsChecked == true)
                {
                    await UpdateRobot();
                }
            };

            mainPanel.OnStartFunctionPrePre += (s, e) =>
            {
                _reenactManager.Pause();
                _radioLive.IsChecked = true;
            };

            mainPanel.OnStartFunctionPre += (s, e) =>
            {
                _rc3dview.UpdateRobotPoints(new List<LogDataMotion.Record>());
                _txtCycle.Text = "";
                _txtReenactTime.Text = "";
                _triSlider.SetRate(0);
                _gridBar.Children.Clear();
                UpdateDisplay();
            };

            mainPanel.OnLoadCSVPre += (s, e) =>
            {
                _reenactManager.Pause();
                _radioReenact.IsChecked = true;
            };

            mainPanel.OnLoadCSVPost += async (s, e) =>
            {
                await UpdateRobot();
            };

            mainPanel.OnCycleSelectionChanged += (s, e) =>
            {
                var sammary = GeneralManager.Instance.LogDatSection.SammaryRecords;
                var records = GeneralManager.Instance.LogDatSection.SectionRecords;
                var cycle = MainPanel.Instance!.GetCycleData();

                if (cycle == null)
                {
                    return;
                }

                {
                    _gridBar.Children.Clear();

                    var total = cycle.ElapsedTime;
                    var idx = 0;

                    foreach (var sec in records.Where(rec => rec.CycleNo == cycle.CycleNo))
                    {
                        var grid = new Grid();

                        var startTime = sec.StartTime - cycle.StartTime;
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(0.0, startTime), GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(0.0, sec.ElapsedTime), GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(0.0, total - (startTime + sec.ElapsedTime)), GridUnitType.Star) });

                        var pct = 0.0;

                        if (total > 0)
                        {
                            pct = sec.ElapsedTime * 100.0 / total;
                        }

                        var pctInfo = new PercentageInfoControl()
                        {
                            TxtName = sec.SectionName,
                            TxtDescription = $"{sec.ElapsedTime:0.0} sec",
                            ValPct = pct,
                            Background = CommonBrush.GetSequentialBrush(idx++),
                        };

                        grid.Children.Add(pctInfo);
                        Grid.SetColumn(pctInfo, 1);

                        _gridBar.Children.Add(grid);
                    }
                }

                var motCycle = GeneralManager.Instance.LogDatMotion.GetCycleData(cycle.CycleNo);

                if (motCycle != null)
                {
                    _rc3dview.UpdateRobotPoints(motCycle.Records);
                }

                UpdateDisplay();
            };

            mainPanel.OnMotionSelectionChanged += (s, rec) =>
            {
                UpdateRobotDisplay(rec);
            };

            mainPanel.OnCloseWin += (s, e) =>
            {
                _rc3dview.UpdateCurPos(0, 0, 0, 0, 0, 0);
                _motionInfoCtl.ClearAll();
            };

            _radioReenact.Checked += async (s, e) =>
            {
                await UpdateRobot();
                var rec = mainPanel.GetSelectedMotionRecord();

                if (rec != null)
                {
                    UpdateRobotDisplay(rec);
                }

            };
            _radioReenact.Unchecked += async (s, e) => await UpdateRobot();

            _viewDirectionCtrl.Init(_rc3dview);
            _viewDirectionCtrl._btnClose.Click += (s, e) =>
            {
                _viewDirectionCtrl.Visibility = Visibility.Collapsed;
                _btnViewDirection.Visibility = Visibility.Visible;
            };
            _viewDirectionCtrl.Visibility = Visibility.Collapsed;
            _btnViewDirection.Click += (s, e) =>
            {
                _btnViewDirection.Visibility = Visibility.Collapsed;
                _viewDirectionCtrl.Visibility = Visibility.Visible;
            };

            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private async Task UpdateRobot()
        {
            if (_radioReenact.IsChecked == true)
            {
                GeneralManager.Instance.SetRobotInfo(
                    GeneralManager.Instance.LogDatInfo.GetValue("RobotModelName"),
                    GeneralManager.Instance.LogDatInfo.GetValueInt("RobotType"));
            }
            else
            {
                await GeneralManager.Instance.SetRobotInfo();
            }

            if (GeneralManager.Instance.RobotInfo == null || string.IsNullOrEmpty(GeneralManager.Instance.RobotInfo.RoboDesc.Id))
            {
                _rc3dview.ClearRobot();
                return;
            }

            _rc3dview.SetRobotInfo(GeneralManager.Instance.RobotInfo);
            UpdateDisplay();
        }

        private void UpdateRobotDisplay(LogDataMotion.Record rec)
        {
            var motionCycleData = GeneralManager.Instance.LogDatMotion.GetCycleData(rec.CycleNo);
            if (motionCycleData == null) return;

            if (motionCycleData.ElapsedTime > 0)
            {
                _triSlider.SetRate(rec.ElapsedTime / motionCycleData.ElapsedTime);
                _triSlider.Visibility = Visibility.Visible;
            }

            var cnt = GeneralManager.Instance.LogDatSection.GetCycleCnt();
            _txtCycle.Text = cnt > 0 ? $"{rec.CycleNo,3}/{cnt,3}" : "";
            var tm = $"{rec.ElapsedTime:0.000}";
            _txtReenactTime.Text = $"{tm,8} {rec.SectionName}";

            if (_radioReenact.IsChecked != true) return;
            GeneralManager.Instance.SetRobotPos(rec);
            _rc3dview.DrawRobot();
            _motionInfoCtl.UpdateDisplay(rec);
            _rc3dview.UpdateCurPos(
                rec.X * 0.001,
                rec.Y * 0.001,
                rec.Z * 0.001,
                rec.U, rec.V, rec.W);
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            if (GeneralManager.Instance.RobotInfo == null)
            {
                return;
            }

            if (_radioLive.IsChecked == true)
            {
                _reenactManager.Pause();
                GeneralManager.Instance.GetRobotPos();
                _rc3dview.DrawRobot();
                _motionInfoCtl.UpdateDisplayJointOnly(GeneralManager.Instance.RobotInfo.Joints.Select(j => j.CurrentPos).ToList());

                if (GeneralManager.Instance.RobotInfo != null)
                {
                    _rc3dview.UpdateCurPos(
                        GeneralManager.Instance.RobotInfo.CurrentPosX,
                        GeneralManager.Instance.RobotInfo.CurrentPosY,
                        GeneralManager.Instance.RobotInfo.CurrentPosZ,
                        GeneralManager.Instance.RobotInfo.CurrentPosU,
                        GeneralManager.Instance.RobotInfo.CurrentPosV,
                        GeneralManager.Instance.RobotInfo.CurrentPosW);
                }
            }

            if (_radioReenact.IsChecked == true)
            {
                if (MainPanel.Instance?.DG_Motion is DataGrid dataGrid)
                {
                    _reenactManager.Tick(dataGrid);
                }
            }
        }

        private void UpdateDisplay()
        {
            _chkParamsInfoCtl.UpdateDisplay();
            _txtInfoHead.Text = GeneralManager.Instance.LogDatInfo.GetSammary();
        }
    }
}
