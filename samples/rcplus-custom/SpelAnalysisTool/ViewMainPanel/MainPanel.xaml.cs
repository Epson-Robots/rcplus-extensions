// -----------------------------------------------------------------------
// <copyright file="MainPanel.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Common;
using SpelAnalysisTool.Model;
using SpelAnalysisTool.ViewWindows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        private static MainPanel? _instance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static MainPanel? Instance => _instance;

        /// <summary>
        /// Create instance.
        /// </summary>
        /// <returns>Instance.</returns>
        public static MainPanel CreateInstance()
        {
            if (_instance == null) _instance = new MainPanel();
            return _instance;
        }

        /// <summary>
        /// DataGrid Cycle.
        /// </summary>
        public DataGrid? DG_Cycle { get; set; }

        /// <summary>
        /// DataGrid Section.
        /// </summary>
        public DataGrid? DG_Section { get; set; }

        /// <summary>
        /// DataGrid Motion.
        /// </summary>
        public DataGrid? DG_Motion { get; set; }

        /// <summary>
        /// Event on show window.
        /// </summary>
        public event EventHandler? OnShowWin;

        /// <summary>
        /// Event on close window.
        /// </summary>
        public event EventHandler? OnCloseWin;

        /// <summary>
        /// Event on controller connected.
        /// </summary>
        public event EventHandler? OnControllerConnected;

        /// <summary>
        /// Event on start function pre pre.
        /// </summary>
        public event EventHandler? OnStartFunctionPrePre;

        /// <summary>
        /// Event on start function pre.
        /// </summary>
        public event EventHandler? OnStartFunctionPre;

        /// <summary>
        /// Event on satrt function post.
        /// </summary>
        public event EventHandler? OnStartFunctionPost;

        /// <summary>
        /// Event on load csv file pre.
        /// </summary>
        public event EventHandler? OnLoadCSVPre;

        /// <summary>
        /// Event on load csv file post.
        /// </summary>
        public event EventHandler? OnLoadCSVPost;

        /// <summary>
        /// Event on load csv file in runtime post
        /// </summary>
        public event EventHandler? OnRuntimeLoadCSVPost;

        /// <summary>
        /// Event on cycle selection changed.
        /// </summary>
        public event EventHandler? OnCycleSelectionChanged;

        /// <summary>
        /// Event on motion selection changed.
        /// </summary>
        public event EventHandler<LogDataMotion.Record>? OnMotionSelectionChanged;

        private List<IMainPanelPane> _panes;

        private class RuntimeObserver
        {
            public Action<int>? ReqLoadCSVAction { get; set; }

            private bool _running;
            private bool _logUpdated;
            private int _cycleNo;
            private DateTime _startDateTime;

            public void Start()
            {
                _startDateTime = DateTime.Now;
                _cycleNo = 0;
                _logUpdated = false;
                _running = true;
            }

            public void End()
            {
                _running = false;
            }

            public void Tick()
            {
                if (!_running) return;

                if (!_logUpdated)
                {
                    _logUpdated = GeneralManager.Instance.CheckSectionLogFileUpdate(_startDateTime);
                }

                if (!_logUpdated) return;

                var ret = GeneralManager.Instance.CheckEndCycleNo();

                if (ret > _cycleNo)
                {
                    _cycleNo = ret;
                    ReqLoadCSVAction?.Invoke(_cycleNo);
                }
            }
        }

        private RuntimeObserver _runtimeObserver = new RuntimeObserver();
        private DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPanel()
        {
            InitializeComponent();

            DataContext = GeneralManager.Instance;

            _panes = new List<IMainPanelPane>()
            {
                new PaneChkParams(),
                new PaneAdjParams(),
                new PaneView(),
                new PaneCycle(),
                new PaneSection(),
                new PaneMotion(),
            };
            _panes.ForEach(pane => pane.Init(this));

            _layout.UpdateLayoutAction = LayoutPanes;
            LayoutPanes(false);

            _btnStartFunction.Click += async (s, e) =>
            {
                OnStartFunctionPrePre?.Invoke(this, EventArgs.Empty);

                GeneralManager.Instance.Conf.Save();
                await GeneralManager.Instance.WriteAdjParamsInc();

                var buildOK = await GeneralManager.Instance.BuildProject();
                if (!buildOK) return;

                var window = new StartFunctionWindow() { Owner = Window.GetWindow(this) };
                window.ShowDialog();
                if (window.DialogResult != true) return;

                GeneralManager.Instance.ResetLogDat();
                
                OnStartFunctionPre?.Invoke(this, EventArgs.Empty);
                await GeneralManager.Instance.StartFunction(window.SelectedFuncName, window.EnableRuntimeObservation);
                OnStartFunctionPost?.Invoke(this, EventArgs.Empty);

                if (window.EnableRuntimeObservation)
                {
                    _runtimeObserver.Start();
                }
            };

            _runtimeObserver.ReqLoadCSVAction = cycleNo =>
            {
                GeneralManager.Instance.LoadCSV(cycleNo);
                OnRuntimeLoadCSVPost?.Invoke(this, EventArgs.Empty);
                DG_Cycle?.SelectLast();
            };

            _btnLoadCSV.Click += (s, e) =>
            {
                OnLoadCSVPre?.Invoke(this, EventArgs.Empty);
                GeneralManager.Instance.ResetLogDat();
                GeneralManager.Instance.LoadCSV();
                OnLoadCSVPost?.Invoke(this, EventArgs.Empty);
            };

            _btnAddSpelLogger.Click += async (s, e) =>
            {
                var window = new AddSpelLoggerWindow() { Owner = Window.GetWindow(this) };
                window.ShowDialog();
                if (window.DialogResult != true) return;

                await GeneralManager.Instance.AddSpelLogger(window.IsSpelLoggerTypeFile, window.AddSample);
            };

            _btnLayout.Click += (s, e) =>
            {
                _layout.Visibility = (_layout.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            };

            GeneralManager.Instance.AllTaskFinishedAcition = () =>
            {
                _runtimeObserver.End();
                OnLoadCSVPre?.Invoke(this, EventArgs.Empty);
                GeneralManager.Instance.LoadCSV();
                OnLoadCSVPost?.Invoke(this, EventArgs.Empty);
            };

            Action<DataGrid?, bool> selChangeDataGrid = (dataGrid, plus) =>
            {
                if (dataGrid == null || dataGrid.Items.Count <= 0) return;

                var idxNew = 0;

                if (plus)
                {
                    if (dataGrid.SelectedIndex >= (dataGrid.Items.Count - 1)) return;
                    idxNew = dataGrid.SelectedIndex + 1;
                }
                else
                {
                    if (dataGrid.SelectedIndex <= 0) return;
                    idxNew = dataGrid.SelectedIndex - 1;
                }

                dataGrid.SelectIndex(idxNew);
            };

            _btnCycleLeft.Click += (s, e) => selChangeDataGrid(DG_Cycle, false);
            _btnCycleRight.Click += (s, e) => selChangeDataGrid(DG_Cycle, true);

            _btnSectionLeft.Click += (s, e) => selChangeDataGrid(DG_Section, false);
            _btnSectionRight.Click += (s, e) => selChangeDataGrid(DG_Section, true);

            _btnMotionLeft.Click += (s, e) => selChangeDataGrid(DG_Motion, false);
            _btnMotionRight.Click += (s, e) => selChangeDataGrid(DG_Motion, true);

            Func<Task> connectionStateChangedFunc = async () =>
            {
                if (!GeneralManager.Instance.Controller.IsConnected) return;
                OnControllerConnected?.Invoke(this, EventArgs.Empty);
            };

            GeneralManager.Instance.Controller.ConnectionStateChangedAction = async () => await connectionStateChangedFunc();
            connectionStateChangedFunc();

            _timer.Start();
            _timer.Tick += (s, e) => _runtimeObserver.Tick();
        }

        /// <summary>
        /// Layout panes.
        /// </summary>
        /// <param name="saveLayoutSize">Save layout size or not.</param>
        private void LayoutPanes(bool saveLayoutSize)
        {
            if (saveLayoutSize)
            {
                SaveLayoutSize();
            }

            foreach (var c in _grid.Children)
            {
                if (c is Grid grid)
                {
                    grid.Children.Clear();
                }
            }

            _grid.Children.Clear();
            _grid.ColumnDefinitions.Clear();
            _grid.RowDefinitions.Clear();

            Func<GridSplitter> createSplitterV = () =>
            {
                return new GridSplitter()
                {
                    Width = 4,
                    Background = Brushes.Transparent,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = Cursors.SizeWE,
                };
            };

            Func<GridSplitter> createSplitterH = () =>
            {
                return new GridSplitter()
                {
                    Height = 8,
                    Background = Brushes.Transparent,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Cursor = Cursors.SizeNS,
                };
            };

            var layout = GeneralManager.Instance.Conf.Set.MainPanelLayout;
            var colsSet = GeneralManager.Instance.Conf.Set.MainPanelLayout.SizeRateColumns;

            for (var col = 0; col < MainPanelLayout.COL_NUM; col++)
            {
                var names = layout.Items
                    .Where(item => !item.IsHidden && item.Column == col)
                    .OrderBy(item => item.Row)
                    .Select(item => item.Name).ToList();

                if (names.Count == 0)
                {
                    continue;
                }

                if (_grid.ColumnDefinitions.Count != 0)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), });
                    var splitter = createSplitterV();
                    _grid.Children.Add(splitter);
                    Grid.SetColumn(splitter, _grid.ColumnDefinitions.Count - 1);
                }

                MainPanelLayout.SizeRateColumn? sizeCol = (col < colsSet.Count) ? colsSet[col] : null;

                _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(sizeCol != null ? sizeCol.SizeRate : 1, GridUnitType.Star), });
                var grid = new Grid();
                _grid.Children.Add(grid);
                Grid.SetColumn(grid, _grid.ColumnDefinitions.Count - 1);

                var row = 0;

                foreach (var name in names)
                {
                    var pane = _panes.FirstOrDefault(p => p.PaneName == name);

                    if (pane is not UIElement element)
                    {
                        continue;
                    }

                    if (grid.RowDefinitions.Count != 0)
                    {
                        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
                        var splitter = createSplitterH();
                        grid.Children.Add(splitter);
                        Grid.SetRow(splitter, grid.RowDefinitions.Count - 1);
                    }

                    grid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(
                        sizeCol != null && row < sizeCol.SizeRateRows.Count ? sizeCol.SizeRateRows[row] : 1,
                        GridUnitType.Star),
                    });
                    grid.Children.Add(element);
                    Grid.SetRow(element, grid.RowDefinitions.Count - 1);

                    row++;
                }
            }
        }

        /// <summary>
        /// Save layout size.
        /// </summary>
        private void SaveLayoutSize()
        {
            var sizeRateColumns = new List<MainPanelLayout.SizeRateColumn>();

            foreach (var col in _grid.Children)
            {
                if (col is Grid grid)
                {
                    var sizeRateColumn = new MainPanelLayout.SizeRateColumn();
                    sizeRateColumn.SizeRate = Math.Max(0, grid.ActualWidth);

                    foreach (var row in grid.Children)
                    {
                        if (row is IMainPanelPane pane && row is FrameworkElement element)
                        {
                            sizeRateColumn.SizeRateRows.Add(Math.Max(0, element.ActualHeight));
                        }
                    }

                    sizeRateColumns.Add(sizeRateColumn);
                }
            }

            var colsSet = GeneralManager.Instance.Conf.Set.MainPanelLayout.SizeRateColumns;
            colsSet.Clear();
            var colSum = sizeRateColumns.Sum(c => c.SizeRate);

            foreach (var colTmp in sizeRateColumns)
            {
                var col = new MainPanelLayout.SizeRateColumn();
                col.SizeRate = (colSum > 0) ? colTmp.SizeRate * sizeRateColumns.Count / colSum : 1.0;

                var rowSum = colTmp.SizeRateRows.Sum();

                foreach (var rowTmp in colTmp.SizeRateRows)
                {
                    col.SizeRateRows.Add((rowSum > 0) ? rowTmp * colTmp.SizeRateRows.Count / rowSum : 1.0);
                }

                colsSet.Add(col);
            }
        }

        /// <summary>
        /// On show window.
        /// </summary>
        public void OnShowWindow()
        {
            GeneralManager.Instance.Conf.Load();
            LayoutPanes(false);
            OnShowWin?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// On close window.
        /// </summary>
        public void OnCloseWindow()
        {
            SaveLayoutSize();
            GeneralManager.Instance.Conf.Save();
            OnLoadCSVPre?.Invoke(this, EventArgs.Empty);
            GeneralManager.Instance.ResetLogDat();
            OnLoadCSVPost?.Invoke(this, EventArgs.Empty);
            OnStartFunctionPre?.Invoke(this, EventArgs.Empty);
            OnCloseWin?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            OnLoadCSVPost?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change cycle selection.
        /// </summary>
        public void ChangeCycleSelection()
        {
            OnCycleSelectionChanged?.Invoke(this, EventArgs.Empty);
            SelectMotionItem();
        }

        /// <summary>
        /// Change motion selection.
        /// </summary>
        /// <param name="rec"></param>
        public void ChangeMotionSelection(LogDataMotion.Record rec)
        {
            OnMotionSelectionChanged?.Invoke(this, rec);
        }

        /// <summary>
        /// Get selected motion record.
        /// </summary>
        /// <returns>Motion record.</returns>
        public LogDataMotion.Record? GetSelectedMotionRecord()
        {
            if (DG_Motion == null) return null;
            if (DG_Motion.SelectedItem is not LogDataMotion.Record rec) return null;

            return rec;
        }

        /// <summary>
        /// On mouse drag.
        /// </summary>
        /// <param name="rate">Rate</param>
        public void OnMouseDrag(double rate)
        {
            var cycle = GetCycleData();
            if (cycle == null) return;

            var rec = GeneralManager.Instance.LogDatMotion.GetSelectedRecords();
            if (rec == null) return;

            var tm = cycle.ElapsedTime * rate;
            var idx = -1;

            for (var i = 0; i < rec.Count; i++)
            {
                if (rec[i].CycleNo == cycle.CycleNo && rec[i].ElapsedTime >= tm)
                {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
            {
                DG_Motion?.SelectIndex(idx);
            }
        }

        /// <summary>
        /// Get cycle data.
        /// </summary>
        /// <returns>Cycle data.</returns>
        public LogDataSection.CycleRecord? GetCycleData()
        {
            var idx = 0;

            if (DG_Cycle != null && DG_Cycle.SelectedIndex >= 0)
            {
                idx = DG_Cycle.SelectedIndex;
            }

            var cycles = GeneralManager.Instance.LogDatSection.CycleRecords;

            return idx < cycles.Count ? cycles[idx] : null;
        }

        /// <summary>
        /// Select motion item.
        /// </summary>
        public void SelectMotionItem()
        {
            if (DG_Section == null || DG_Motion == null) return;

            var cycle = GetCycleData();
            if (cycle == null) return;

            if (DG_Motion.SelectedItem is not LogDataMotion.Record motionItem) return;
            if (DG_Section.SelectedItem is not LogDataSection.SammaryRecord sam) return;
            if (motionItem.CycleNo == cycle.CycleNo && motionItem.SectionName == sam.SectionName) return;

            if (DG_Motion.ItemsSource is not List<LogDataMotion.Record> list) return;

            if (sam.SectionName.StartsWith("*"))
            {
                DG_Motion.SelectedItem = list.FirstOrDefault(item => item.CycleNo == cycle.CycleNo);
            }
            else
            {
                DG_Motion.SelectedItem = list.FirstOrDefault(item => item.CycleNo == cycle.CycleNo && item.SectionName == sam.SectionName);
            }

            if (DG_Motion.SelectedItem == null) return;

            DG_Motion.ScrollIntoView(list.Last());
            DG_Motion.ScrollIntoView(DG_Motion.SelectedItem);
        }
    }
}
