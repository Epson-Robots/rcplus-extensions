// -----------------------------------------------------------------------
// <copyright file="PaneCycle.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Common;
using SpelAnalysisTool.Model;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewMainPanel
{
    /// <summary>
    /// Interaction logic for PaneCycle.xaml
    /// </summary>
    public partial class PaneCycle : UserControl, IMainPanelPane
    {
        /// <inheritdoc/>
        public string PaneName => "Cycle";

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaneCycle()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Init(MainPanel mainPanel)
        {
            mainPanel.DG_Cycle = _dataGrid;

            _headerCycle.GetDataFunc = GeneralManager.Instance.LogDatSection.GetGridFormatCycleData;

            _dataGrid.SelectionChanged += (s, e) =>
            {
                mainPanel.ChangeCycleSelection();
            };

            mainPanel.OnStartFunctionPre += (s, e) =>
            {
                UpdateDisplay();
            };

            mainPanel.OnLoadCSVPost += (s, e) =>
            {
                UpdateDisplay();
            };

            mainPanel.OnRuntimeLoadCSVPost += (s, e) =>
            {
                UpdateDisplay();
            };

            mainPanel.OnMotionSelectionChanged += (s, rec) =>
            {
                var cycle = MainPanel.Instance!.GetCycleData();
                if (cycle == null) return;
                if (cycle.CycleNo != rec.CycleNo)
                {
                    _dataGrid.SelectIndex(rec.CycleNo - 1);
                }
            };
        }

        private void UpdateDisplay()
        {
            _headerCycle.Count = GeneralManager.Instance.LogDatSection.GetCycleCnt();

            _dataGrid.ItemsSource = null;

            var cycles = GeneralManager.Instance.LogDatSection.CycleRecords;
            var records = GeneralManager.Instance.LogDatSection.SectionRecords;
            var sammary = GeneralManager.Instance.LogDatSection.SammaryRecords;

            if (sammary.Count <= 0)
            {
                return;
            }

            var dt = new DataTable();
            _dataGrid.Columns.Clear();

            var styleCenter = new Style(typeof(TextBlock));
            styleCenter.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));

            var styleRight = new Style(typeof(TextBlock));
            styleRight.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));

            Action<string, double, Style, string> addColumn = (title, width, style, ngName) =>
            {
                dt.Columns.Add(title);
                var col = new DataGridTextColumn();
                col.Header = title;
                col.ElementStyle = style;

                if (width == 0)
                {
                    col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                else
                {
                    col.Width = width;
                }

                col.Binding = new System.Windows.Data.Binding(title);


                if (!string.IsNullOrEmpty(ngName))
                {
                    Style cellStyle = new Style(typeof(DataGridCell));
                    {
                        Binding binding = new Binding(ngName);
                        DataTrigger trigger = new DataTrigger
                        {
                            Binding = binding,
                            Value = "True",
                        };
                        trigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Red));
                        trigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
                        cellStyle.Triggers.Add(trigger);
                    }
                    col.CellStyle = cellStyle;
                }

                _dataGrid.Columns.Add(col);
            };

            addColumn("Cycle", 38, styleCenter, "");
            addColumn("*Total", 60, styleRight, "IsTotalNG");
            dt.Columns.Add("IsTotalNG");

            sammary.Where(sam => !sam.SectionName.StartsWith("*")).ToList().ForEach(sam =>
            {
                addColumn(sam.SectionName, 0, styleRight, $"Is{sam.SectionName}NG");
                dt.Columns.Add($"Is{sam.SectionName}NG");
            });

            foreach (var cycle in cycles)
            {
                var row = dt.NewRow();
                row["Cycle"] = cycle.CycleNo;

                var total = cycle.ElapsedTime;
                row["*Total"] = $"{total:0.000}";
                row["IsTotalNG"] = GeneralManager.Instance.Conf.Set.ChkParams.CycleTime.CheckNG(total) ? "True" : "";

                if (total > 0)
                {
                    sammary.ForEach(sam =>
                    {
                        var target = records.FirstOrDefault(rec => rec.CycleNo == cycle.CycleNo && rec.SectionName == sam.SectionName);

                        if (target != null)
                        {
                            var prefix = "";
                            var isNG = "";

                            row[sam.SectionName] = $"{prefix}{target.ElapsedTime:0.000}({target.ElapsedTime * 100 / total,4:0.0}%)";
                            row[$"Is{sam.SectionName}NG"] = isNG;
                        }
                    });
                }

                dt.Rows.Add(row);
            }

            _dataGrid.ItemsSource = dt.DefaultView;

            if (dt.Rows.Count > 0)
            {
                _dataGrid.SelectedIndex = 0;
            }
        }
    }
}
