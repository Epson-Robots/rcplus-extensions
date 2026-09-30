// -----------------------------------------------------------------------
// <copyright file="IOBitSelectWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Model;
using IntegratedJogPanel.View;
using System.Windows;
using static IntegratedJogPanel.Constants;
using Epson.RoboticsShared.ExtensionsAPI;

namespace IntegratedJogPanel
{
    /// <summary>
    /// Interaction logic for IOBitSelectWindow.xaml
    /// </summary>
    public partial class IOBitSelectWindow : Window
    {
        private IRCXIOAPI.RCXIOKind _ioKind;
        private List<IOItem> _orgItems = new List<IOItem>();
        private List<IOItem> _setItems = new List<IOItem>();
        private GeneralConf.ConfItem _confItem;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ioKind">I/O kind</param>
        public IOBitSelectWindow(IRCXIOAPI.RCXIOKind ioKind)
        {
            InitializeComponent();
            
            _ioKind = ioKind;

            Title = $"I/O {_ioKind} {Main.Captions?[Caption.IOBitSelectWin_Title]}";
            _dataGrid.ItemsSource = GeneralManager.Instance.GetIOList(_ioKind);

            if (_dataGrid.Items.Count > 0)
            {
                _dataGrid.SelectedIndex = 0;
            }

            if (_ioKind == IRCXIOAPI.RCXIOKind.Input)
            {
                _orgItems = GeneralManager.Instance.IOItemsIn;
                _confItem = GeneralManager.Instance.Conf.IOBitsIn;
            }
            else // IOItem.EIOType.Out
            {
                _orgItems = GeneralManager.Instance.IOItemsOut;
                _confItem = GeneralManager.Instance.Conf.IOBitsOut;
            }

            _orgItems.ForEach(org =>
            {
                var item = new IOItem(org.IOKind, org.BitNo, true);
                _setItems.Add(item);
                var ctrl = new IOItemControl(item, _stackPanel.Children.Count == 0) { IsDummy = true, };
                ctrl.UpdateDisplay();

                ctrl.ClickedAction = s =>
                {
                    foreach (var c in _stackPanel.Children)
                    {
                        if (c is  IOItemControl ic)
                        {
                            ic.IsSelected = s == ic;
                        }
                    }
                };

                _stackPanel.Children.Add(ctrl);
            });

            SetSelectedIdx(0);

            Action addProc = () =>
            {
                if (_dataGrid.SelectedItem is not IOItem srcItem)
                {
                    return;
                }

                var idx = 0;
                var find = false;

                for (var i = _setItems.Count - 1; i >= 0; i--)
                {
                    var item = _setItems[i];

                    if (item.BitNo >= 0)
                    {
                        break;
                    }

                    find = true;
                    idx = i;
                }

                if (!find)
                {
                    return;
                }

                _setItems[idx].BitNo = srcItem.BitNo;
                _setItems[idx].Label = srcItem.Label;
                SetSelectedIdx(idx);
                UpdateItems();
            };

            _dataGrid.MouseDoubleClick += (s, e) => addProc();
            _btnAdd.Click += (s, e) => addProc();

            _btnSet.Click += (s, e) =>
            {
                if (_dataGrid.SelectedItem is not IOItem srcItem)
                {
                    return;
                }

                var idx = GetSelectedIdx();

                if (idx < 0)
                {
                    return;
                }

                _setItems[idx].BitNo = srcItem.BitNo;
                _setItems[idx].Label = srcItem.Label;

                UpdateItems();
            };

            _btnClr.Click += (s, e) =>
            {
                var idx = GetSelectedIdx();

                if (idx < 0)
                {
                    return;
                }

                _setItems[idx].BitNo = -1;
                _setItems[idx].LoadLabel();

                UpdateItems();
            };

            _btnClrAll.Click += (s, e) =>
            {
                for (var i = 0; i < _setItems.Count; i++)
                {
                    _setItems[i].BitNo = -1;
                    _setItems[i].LoadLabel();
                }

                UpdateItems();
            };

            _btnLeft.Click += (s, e) =>
            {
                var idx = GetSelectedIdx();

                if (idx < 0)
                {
                    return;
                }

                idx--;

                if (idx < 0)
                {
                    return;
                }

                var bitNo = _setItems[idx].BitNo;
                var label = _setItems[idx].Label;
                _setItems[idx].BitNo = _setItems[idx + 1].BitNo;
                _setItems[idx].Label = _setItems[idx + 1].Label;
                _setItems[idx + 1].BitNo = bitNo;
                _setItems[idx + 1].Label = label;

                SetSelectedIdx(idx);

                UpdateItems();
            };

            _btnRight.Click += (s, e) =>
            {
                var idx = GetSelectedIdx();

                if (idx < 0)
                {
                    return;
                }

                idx++;

                if (idx >= _setItems.Count)
                {
                    return;
                }

                var bitNo = _setItems[idx].BitNo;
                var label = _setItems[idx].Label;
                _setItems[idx].BitNo = _setItems[idx - 1].BitNo;
                _setItems[idx].Label = _setItems[idx - 1].Label;
                _setItems[idx - 1].BitNo = bitNo;
                _setItems[idx - 1].Label = label;

                SetSelectedIdx(idx);

                UpdateItems();
            };

            _btnReset.Click += (s, e) =>
            {
                for (var i = 0; i< _setItems.Count; i++)
                {
                    _setItems[i].BitNo = i;
                    _setItems[i].LoadLabel();
                }

                UpdateItems();
            };

            _btnOK.Click += (s, e) =>
            {
                for (var i = 0; i < _orgItems.Count; i++)
                {
                    _orgItems[i].BitNo = _setItems[i].BitNo;
                    _orgItems[i].LoadLabel();
                    _confItem.Values[i] = _orgItems[i].BitNo;
                }

                Close();
            };

            _btnCancel.Click += (s, e) =>
            {
                Close();
            };
        }

        private void UpdateItems()
        {
            foreach (var ctrl in _stackPanel.Children)
            {
                if (ctrl is IOItemControl ioCtrl)
                {
                    ioCtrl.UpdateDisplay();
                }
            }
        }

        private int GetSelectedIdx()
        {
            var ret = -1;

            for (var i = 0; i < _stackPanel.Children.Count; i++)
            {
                if (_stackPanel.Children[i] is IOItemControl ctrl)
                {
                    if (ctrl.IsSelected)
                    {
                        ret = i;
                        break;
                    }
                }
            }

            return ret;
        }

        private void SetSelectedIdx(int idx)
        {
            for (var i = 0; i < _stackPanel.Children.Count; i++)
            {
                if (_stackPanel.Children[i] is IOItemControl ctrl)
                {
                    ctrl.IsSelected = i == idx;
                }
            }
        }
    }
}
