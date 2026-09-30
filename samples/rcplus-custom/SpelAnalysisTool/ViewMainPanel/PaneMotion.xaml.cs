// -----------------------------------------------------------------------
// <copyright file="PaneMotion.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Common;
using SpelAnalysisTool.Model;
using SpelAnalysisTool.ViewControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for PaneMotion.xaml
    /// </summary>
    public partial class PaneMotion : UserControl, IMainPanelPane
    {
        /// <inheritdoc/>
        public string PaneName => "Motion";

        private MouseForGrid? _mouseForGraph;
        private List<MotionGraphControl> _graphControls = new List<MotionGraphControl>();

        private class ColCheck
        {
            public string Name { get; }
            public CheckBox ChkBox { get; } = new CheckBox();
            public List<DataGridColumn> Columns { get; } = new List<DataGridColumn>();

            public Action? CheckChangedAction { get; set; }

            private void InitCommon()
            {
                ChkBox.Content = Name;
                LoadConf();

                ChkBox.Checked += (s, e) => UpdateColShowHide();
                ChkBox.Unchecked += (s, e) => UpdateColShowHide();

                UpdateColShowHide();
            }

            public void LoadConf()
            {
                ChkBox.IsChecked = GeneralManager.Instance.Conf.Set.SelectedMotionColumns.Contains(Name);
            }

            public ColCheck(string name, DataGridColumn column)
            {
                Name = name;
                Columns.Add(column);
                InitCommon();
            }

            public ColCheck(string name, List<DataGridColumn> columns)
            {
                Name = name;
                Columns.AddRange(columns);
                InitCommon();
            }

            public void UpdateColShowHide()
            {
                Columns.ForEach(col =>
                {
                    col.Visibility = ChkBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                });

                CheckChangedAction?.Invoke();
            }
        }

        private List<ColCheck> _colChecks = new List<ColCheck>();

        private int GraphNum
        {
            get
            {
                var ret = 1;

                if (_comboBoxGraphNum.SelectedItem is int item)
                {
                    ret = item;
                }

                return ret;
            }
        }

        private bool _confLoading;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaneMotion()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Init(MainPanel mainPanel)
        {
            mainPanel.DG_Motion = _dataGrid;

            _headerMotion.GetDataFunc = GeneralManager.Instance.LogDatMotion.GetGridFormatMotionData;

            Action saveMotionGraphSelection = () =>
            {
                if (_confLoading)
                {
                    return;
                }

                GeneralManager.Instance.Conf.Set.SelectedMotionGraphs.Clear();

                for (var i = 0; i < (int)_comboBoxGraphNum.SelectedItem; i++)
                {
                    if (_graphControls[i] is MotionGraphControl ctl)
                    {
                        GeneralManager.Instance.Conf.Set.SelectedMotionGraphs.Add((string)ctl._comboBoxTarget.SelectedItem);
                    }
                }

                GeneralManager.Instance.Conf.Save();
            };

            for (var i = 0; i < 10; i++)
            {
                _graphControls.Add(new MotionGraphControl(i)
                {
                    GetCycleFunc = mainPanel.GetCycleData,
                    TargetSelectionChagendAction = saveMotionGraphSelection,
                });
                _comboBoxGraphNum.Items.Add(i + 1);
            }

            Action graphNumChangedProc = () =>
            {
                _gridGraph.Children.Clear();
                _gridGraph.RowDefinitions.Clear();
                _graphControls.ForEach(ctl => ctl.IsShown = false);

                for (var i = 0; i < GraphNum; i++)
                {
                    _gridGraph.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    var ctl = _graphControls[i];
                    ctl.IsShown = true;
                    ctl.UpdateDisplay();
                    _gridGraph.Children.Add(ctl);
                    Grid.SetRow(ctl, i);
                }

                saveMotionGraphSelection();
            };
            _comboBoxGraphNum.SelectedItem = GeneralManager.Instance.Conf.Set.SelectedMotionGraphs.Count;
            _comboBoxGraphNum.SelectionChanged += (s, e) => graphNumChangedProc();
            graphNumChangedProc();

            _mouseForGraph = new MouseForGrid(_gridGraphCursor);
            _mouseForGraph.MouseDragAction = mainPanel.OnMouseDrag;
            _colChecks.Add(new ColCheck("DateTime", _colDateTime));
            _colChecks.Add(new ColCheck("SectionName", _colSectionName));
            _colChecks.Add(new ColCheck("Velocity", _colVelocity));
            _colChecks.Add(new ColCheck("TCPSpeed", _colTCPSpeed));
            _colChecks.Add(new ColCheck("XYZ", new List<DataGridColumn>() { _colX, _colY, _colZ, }));
            _colChecks.Add(new ColCheck("UVW", new List<DataGridColumn>() { _colU, _colV, _colW, }));
            _colChecks.Add(new ColCheck("J1", _colJ1));
            _colChecks.Add(new ColCheck("J2", _colJ2));
            _colChecks.Add(new ColCheck("J3", _colJ3));
            _colChecks.Add(new ColCheck("J4", _colJ4));
            _colChecks.Add(new ColCheck("J5", _colJ5));
            _colChecks.Add(new ColCheck("J6", _colJ6));
            _colChecks.Add(new ColCheck("TQ1", _colTQ1));
            _colChecks.Add(new ColCheck("TQ2", _colTQ2));
            _colChecks.Add(new ColCheck("TQ3", _colTQ3));
            _colChecks.Add(new ColCheck("TQ4", _colTQ4));
            _colChecks.Add(new ColCheck("TQ5", _colTQ5));
            _colChecks.Add(new ColCheck("TQ6", _colTQ6));

            _wrapPanelColShow.Children.Clear();
            _colChecks.ForEach(c => _wrapPanelColShow.Children.Add(c.ChkBox));

            List<(DataGridColumn txtCol, string name)> chkCols = new List<(DataGridColumn, string)>()
            {
                (_colVelocity, "IsVelocityNG"),
                (_colTCPSpeed, "IsTCPSpeedNG"),
                (_colX, "IsXNG"),
                (_colY, "IsYNG"),
                (_colZ, "IsZNG"),
                (_colU, "IsUNG"),
                (_colV, "IsVNG"),
                (_colW, "IsWNG"),
                (_colJ1, "IsJ1NG"),
                (_colJ2, "IsJ2NG"),
                (_colJ3, "IsJ3NG"),
                (_colJ4, "IsJ4NG"),
                (_colJ5, "IsJ5NG"),
                (_colJ6, "IsJ6NG"),
                (_colTQ1, "IsTorque1NG"),
                (_colTQ2, "IsTorque2NG"),
                (_colTQ3, "IsTorque3NG"),
                (_colTQ4, "IsTorque4NG"),
                (_colTQ5, "IsTorque5NG"),
                (_colTQ6, "IsTorque6NG"),
            };

            chkCols.ForEach(col =>
            {
                Style cellStyle = new Style(typeof(DataGridCell));
                {
                    Binding binding = new Binding(col.name);
                    DataTrigger trigger = new DataTrigger
                    {
                        Binding = binding,
                        Value = "True",
                    };
                    trigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Red));
                    trigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
                    cellStyle.Triggers.Add(trigger);
                }
                col.txtCol.CellStyle = cellStyle;
            });

            _colChecks.ForEach(c => c.CheckChangedAction = SaveColumnSelection);

            _dataGrid.SelectionChanged += (s, e) =>
            {
                var items = _dataGrid.SelectedItems;
                if (items.Count <= 0 || items[items.Count - 1] is not LogDataMotion.Record rec) return;

                var motionCycleData = GeneralManager.Instance.LogDatMotion.GetCycleData(rec.CycleNo);
                if (motionCycleData == null) return;
                {
                    _pathLineMotionCurrent.Data = new PathGeometry()
                    {
                        Figures = {
                            new PathFigure() { StartPoint = new Point(0, 0) },
                            new PathFigure() { StartPoint = new Point(motionCycleData.ElapsedTime, 0) },
                            new PathFigure()
                            {
                                StartPoint = new Point(rec.ElapsedTime, 0.0),
                                Segments = {
                                    new PolyLineSegment()
                                    {
                                        Points = new PointCollection() { new Point(rec.ElapsedTime, 1.0) },
                                    }
                                }
                            }
                        },
                    };
                    _pathLineMotionCurrent.Visibility = Visibility.Visible;
                }

                mainPanel.ChangeMotionSelection(rec);
            };

            mainPanel.OnShowWin += (s, e) =>
            {
                _confLoading = true;
                _graphControls.ForEach(ctl => ctl.LoadConf());
                _comboBoxGraphNum.SelectedItem = GeneralManager.Instance.Conf.Set.SelectedMotionGraphs.Count;
                LoadColumnSelection();
                _confLoading = false;
            };

            mainPanel.OnStartFunctionPre += (s, e) =>
            {
                ClearMotionGraph();
                UpdateDisplay();
            };

            mainPanel.OnLoadCSVPre += (s, e) =>
            {
                ClearMotionGraph();
            };

            mainPanel.OnLoadCSVPost += (s, e) =>
            {
                UpdateDisplay();
            };

            mainPanel.OnRuntimeLoadCSVPost += (s, e) =>
            {
                UpdateDisplay();
            };

            mainPanel.OnCycleSelectionChanged += (s, e) =>
            {
                _gridGraphBar.Children.Clear();

                var sammary = GeneralManager.Instance.LogDatSection.SammaryRecords;
                var records = GeneralManager.Instance.LogDatSection.SectionRecords;
                var cycle = MainPanel.Instance!.GetCycleData();

                if (cycle == null)
                {
                    return;
                }

                {
                    var total = cycle.ElapsedTime;
                    var idx = 0;

                    foreach (var sec in records.Where(rec => rec.CycleNo == cycle.CycleNo))
                    {
                        var grid = new Grid();

                        var startTime = sec.StartTime - cycle.StartTime;
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(0.0, startTime), GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(0.0, sec.ElapsedTime), GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(0.0, total - (startTime + sec.ElapsedTime)), GridUnitType.Star) });

                        var motionSec = new MotionSectionControl()
                        {
                            TxtTitle = sec.SectionName,
                            HeaderBrush = CommonBrush.GetSequentialBrush(idx++),
                        };

                        grid.Children.Add(motionSec);
                        Grid.SetColumn(motionSec, 1);

                        _gridGraphBar.Children.Add(grid);
                    }
                }

                _graphControls.ForEach(graph => graph.UpdateDisplay());
            };
        }

        private void ClearMotionGraph()
        {
            _pathLineMotionCurrent.Visibility = Visibility.Hidden;
            _gridGraphBar.Children.Clear();
            _graphControls.ForEach(ctl => ctl.Clear());
        }

        private void LoadColumnSelection()
        {
            _colChecks.ForEach(c => c.LoadConf());
        }

        private void SaveColumnSelection()
        {
            GeneralManager.Instance.Conf.Set.SelectedMotionColumns.Clear();
            _colChecks.ForEach(c =>
            {
                if (c.ChkBox.IsChecked == true)
                {
                    GeneralManager.Instance.Conf.Set.SelectedMotionColumns.Add(c.Name);
                }
            });
            GeneralManager.Instance.Conf.Save();
        }

        private void UpdateDisplay()
        {
            _headerMotion.Count = GeneralManager.Instance.LogDatMotion.GetMotionCnt();

            _dataGrid.ItemsSource = null;
            var records = GeneralManager.Instance.LogDatMotion.GetSelectedRecords();
            _dataGrid.ItemsSource = records;

            if (records != null && records.Count > 0)
            {
                _dataGrid.SelectedIndex = 0;
            }
        }
    }
}
