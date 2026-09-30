// -----------------------------------------------------------------------
// <copyright file="SelectSrcToDstControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for SelectSrcToDstControl.xaml
    /// </summary>
    public partial class SelectSrcToDstControl : UserControl
    {
        /// <summary>
        /// Update layout action.
        /// </summary>
        public Action<bool>? UpdateLayoutAction { get; set; }

        private List<CellControl> _dstCells = new List<CellControl>();
        private List<MainPanelLayout.LayoutItem> _items => GeneralManager.Instance.Conf.Set.MainPanelLayout.Items;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectSrcToDstControl()
        {
            InitializeComponent();

            _btnClose.Click += (s, e) =>
            {
                Visibility = Visibility.Collapsed;
            };

            foreach (var ctl in _gridDst.Children)
            {
                if (ctl is CellControl cell)
                {
                    cell.Txt = "";
                    cell.ClickedAction = SelectDstIdx;
                    _dstCells.Add(cell);
                }
            }

            UpdateDisplay(false);
            SelectDstIdx(1);

            _btnToVisible.Click += (s, e) =>
            {
                var srcItem = GetSelectedSrcItem();
                if (srcItem == null) return;

                var dstIdx = GetSelectedCellIdx();
                if (dstIdx == 0) return;

                var dstItem = GetSelectedDstItem();

                if (dstItem != null)
                {
                    dstItem.LayoutLoc = 0;
                }

                srcItem.LayoutLoc = dstIdx;
                UpdateDisplay();
            };

            _btnToHidden.Click += (s, e) =>
            {
                var dstItem = GetSelectedDstItem();
                if (dstItem == null) return;

                dstItem.LayoutLoc = 0;
                UpdateDisplay();
            };

            _btnReset.Click += (s, e) =>
            {
                _items.ForEach(item => item.Reset());
                UpdateDisplay();
            };

            Action<bool, bool> movItem = (changeRow, toNegative) =>
            {
                var dstItem = GetSelectedDstItem();
                if (dstItem == null) return;

                var adder = 0;

                if (changeRow)
                {
                    if (toNegative)
                    {
                        if (dstItem.Row <= 0) return;
                        adder = -1;
                    }
                    else
                    {
                        if (dstItem.Row >= MainPanelLayout.ROW_NUM - 1) return;
                        adder = +1;
                    }
                }
                else
                {
                    if (toNegative)
                    {
                        if (dstItem.Column <= 0) return;
                        adder = -MainPanelLayout.ROW_NUM;
                    }
                    else
                    {
                        if (dstItem.Column >= MainPanelLayout.COL_NUM - 1) return;
                        adder = +MainPanelLayout.ROW_NUM;
                    }
                }

                var idx = dstItem.LayoutLoc + adder;
                var compItem = GetDstItem(idx);

                if (compItem != null)
                {
                    compItem.LayoutLoc = dstItem.LayoutLoc;
                }

                dstItem.LayoutLoc = idx;
                SelectDstIdx(idx);
                UpdateDisplay();
            };

            _btnUp.Click += (s, e) => movItem(true, true);
            _btnDown.Click += (s, e) => movItem(true, false);

            _btnLeft.Click += (s, e) => movItem(false, true);
            _btnRight.Click += (s, e) => movItem(false, false);
        }

        private MainPanelLayout.LayoutItem? GetSelectedSrcItem()
        {
            if (_listBoxSrc.SelectedItem is not MainPanelLayout.LayoutItem item)
            {
                return null;
            }

            return item;
        }

        private MainPanelLayout.LayoutItem? GetSelectedDstItem()
        {
            var cell = _dstCells.FirstOrDefault(c => c.IsSelected);
            if (cell == null) return null;

            return _items.FirstOrDefault(i => i.LayoutLoc == cell.Idx);
        }

        private MainPanelLayout.LayoutItem? GetDstItem(int layoutLoc)
        {
            return _items.FirstOrDefault(i => i.LayoutLoc == layoutLoc);
        }

        private int GetSelectedCellIdx()
        {
            var cell = _dstCells.FirstOrDefault(c => c.IsSelected);
            if (cell == null) return 0;

            return cell.Idx;
        }

        private void SelectDstIdx(int idx)
        {
            _dstCells.ForEach(cell => cell.IsSelected = cell.Idx == idx);
        }

        private void UpdateDisplay(bool updateLayout = true)
        {
            _listBoxSrc.Items.Clear();
            _dstCells.ForEach(cell => cell.Txt = "");

            _items.ForEach(item =>
            {
                if (item.LayoutLoc == 0)
                {
                    _listBoxSrc.Items.Add(item);
                }
                else
                {
                    var target = _dstCells.FirstOrDefault(c => c.Idx == item.LayoutLoc);
                    if (target != null)
                    {
                        target.Txt = item.Name;
                    }
                }
            });

            if (updateLayout)
            {
                UpdateLayoutAction?.Invoke(true);
                GeneralManager.Instance.Conf.Save();
            }
        }
    }
}
