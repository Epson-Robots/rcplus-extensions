// -----------------------------------------------------------------------
// <copyright file="SelectSrcToDstControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Model;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for SelectSrcToDstControl.xaml
    /// </summary>
    public partial class SelectSrcToDstControl : UserControl
    {
        /// <summary>
        /// Update layout Action
        /// </summary>
        public Action? UpdateLayoutAction { get; set; }

        /// <summary>
        /// JogItemPanel's dictionary
        /// </summary>
        public Dictionary<int, JogItemPanel> DicPanels { get; set; } = new Dictionary<int, JogItemPanel>();

        /// <summary>
        /// Hidden items
        /// </summary>
        public ObservableCollection<JogItemPanel> HiddenItems { get; } = new ObservableCollection<JogItemPanel>();

        /// <summary>
        /// Visible items
        /// </summary>
        public ObservableCollection<JogItemPanel> VisibleItems { get; } = new ObservableCollection<JogItemPanel>();

        /// <summary>
        /// Constructor
        /// </summary>
        public SelectSrcToDstControl()
        {
            InitializeComponent();

            _listBoxSrc.ItemsSource = HiddenItems;
            _listBoxDst.ItemsSource = VisibleItems;

            Action updateProc = () =>
            {
                GeneralManager.Instance.Conf.JogLayout.SetAll(0);

                var idx = 0;

                foreach (var item in VisibleItems)
                {
                    foreach (var p in DicPanels)
                    {
                        if (item == p.Value)
                        {
                            GeneralManager.Instance.Conf.JogLayout.Values[idx] = p.Key;
                            break;
                        }
                    }
                    idx++;
                }

                UpdateLayoutAction?.Invoke();
            };

            _btnRight.Click += (s, e) =>
            {
                if (_listBoxSrc.SelectedItem is JogItemPanel panel)
                {
                    HiddenItems.Remove(panel);
                    VisibleItems.Add(panel);
                    _listBoxDst.SelectedIndex = _listBoxDst.Items.Count - 1;
                    updateProc();
                }   
            };

            _btnLeft.Click += (s, e) =>
            {
                if (_listBoxDst.SelectedItem is JogItemPanel panel)
                {
                    VisibleItems.Remove(panel);
                    HiddenItems.Add(panel);
                    updateProc();
                }
            };

            _btnUp.Click += (s, e) =>
            {
                if (_listBoxDst.SelectedItem is JogItemPanel panel)
                {
                    var idx = VisibleItems.IndexOf(panel) - 1;

                    if (idx >= 0)
                    {
                        VisibleItems.Remove(panel);
                        VisibleItems.Insert(idx, panel);
                        _listBoxDst.SelectedIndex = idx;
                        updateProc();
                    }
                }
            };

            _btnDown.Click += (s, e) =>
            {
                if (_listBoxDst.SelectedItem is JogItemPanel panel)
                {
                    var idx = VisibleItems.IndexOf(panel) + 1;

                    if (idx < VisibleItems.Count)
                    {
                        VisibleItems.Remove(panel);
                        VisibleItems.Insert(idx, panel);
                        _listBoxDst.SelectedIndex = idx;
                        updateProc();
                    }
                }
            };

            _btnReset.Click += (s, e) =>
            {
                GeneralManager.Instance.Conf.JogLayout.SetValues(GeneralConf.DefJogLayout);
                UpdateFromConf();
                updateProc();
            };
        }

        /// <summary>
        /// Update from configuration.
        /// </summary>
        public void UpdateFromConf()
        {
            HiddenItems.Clear();
            VisibleItems.Clear();

            foreach (var no in GeneralManager.Instance.Conf.JogLayout.Values)
            {
                if (DicPanels.ContainsKey(no))
                {
                    VisibleItems.Add(DicPanels[no]);
                }
            }

            foreach (var p in DicPanels)
            {
                if (!VisibleItems.Contains(p.Value))
                {
                    HiddenItems.Add(p.Value);
                }
            }
        }
    }
}
