// -----------------------------------------------------------------------
// <copyright file="JogDistanceWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Epson.RoboticsShared.ExtensionsAPI;
using IntegratedJogPanel.Model;
using IntegratedJogPanel.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static IntegratedJogPanel.Constants;

namespace IntegratedJogPanel
{
    /// <summary>
    /// Interaction logic for JogDistanceWindow.xaml
    /// </summary>
    public partial class JogDistanceWindow : Window
    {
        private JogDistance _jogDistance;

        private List<RealNumberEditControl> _numEditL = new List<RealNumberEditControl>();
        private List<RealNumberEditControl> _numEditM = new List<RealNumberEditControl>();
        private List<RealNumberEditControl> _numEditS = new List<RealNumberEditControl>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jogDistance">Jog distance</param>
        public JogDistanceWindow(JogDistance jogDistance)
        {
            _jogDistance = jogDistance;

            InitializeComponent();

            Title = $"{_jogDistance.Name} {Main.Captions?[Caption.JogDistanceWin_Title]}";

            {
                var idx = 0;
                new List<TextBlock>()
                { _txtLabel1, _txtLabel2, _txtLabel3, _txtLabel4, _txtLabel5, _txtLabel6 }.ForEach(txt =>
                {
                    txt.Text = _jogDistance.Labels[idx++];
                });
            }

            var numItems = 6;

            if (RobotController.Instance.IsSCARA)
            {
                numItems = 4;

                for (var i = 11; i < _grid.RowDefinitions.Count; i++)
                {
                    _grid.RowDefinitions[i].Height = new GridLength(0);
                }
            }

            Action<int, int, string, double, List<RealNumberEditControl>> addItem = (row, col, unitLabel, val, list) =>
            {
                var item = new RealNumberEditControl();
                item.Max = col switch { 0 => 180.0, 1 => 30.0, _ => 10.0, };
                item.UnitLabel = unitLabel;
                item.GetSpinDiffFunc = () =>
                {
                    return _onOffSpinInteger.SwitchOn ? 1.0 : 0.001;
                };
                item.ChangedAction = src =>
                {
                    if (_onOffAccordDistanceMM.SwitchOn && src.UnitLabel == "mm"
                    || _onOffAccordDistanceDeg.SwitchOn && src.UnitLabel == "deg")
                    {
                        list.ForEach(x =>
                        {
                            if (x != src && x.UnitLabel == src.UnitLabel)
                            {
                                x.Val = src.Val;
                            }
                        });
                    }
                };

                _grid.Children.Add(item);
                Grid.SetRow(item, 3 + row * 2);
                Grid.SetColumn(item, 3 + col * 2);

                list.Add(item);
            };

            for (var idx = 0; idx < numItems; idx++)
            {
                var unitLabel = _jogDistance.Units[idx];

                if (RobotController.Instance.IsSCARA && idx == 2)
                {
                    unitLabel = "mm";
                }

                addItem(idx, 0, unitLabel, _jogDistance.GetDistance(IRCXRobotManagerAPI.RCXJogDistance.Long, idx), _numEditL);
                addItem(idx, 1, unitLabel, _jogDistance.GetDistance(IRCXRobotManagerAPI.RCXJogDistance.Middle, idx), _numEditM);
                addItem(idx, 2, unitLabel, _jogDistance.GetDistance(IRCXRobotManagerAPI.RCXJogDistance.Short, idx), _numEditS);
            }

            Loaded += async (s, e) =>
            {
                IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    await _jogDistance.Load();

                    for (var idx = 0; idx < numItems; idx++)
                    {
                        _numEditL[idx].Val = _jogDistance.DistanceL[idx];
                        _numEditM[idx].Val = _jogDistance.DistanceM[idx];
                        _numEditS[idx].Val = _jogDistance.DistanceS[idx];
                    }

                    IsEnabled = true;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            };

            _btnOK.Click += async (s, e) =>
            {
                IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    for (var idx = 0; idx < numItems; idx++)
                    {
                        _jogDistance.DistanceL[idx] = _numEditL[idx].Val;
                        _jogDistance.DistanceM[idx] = _numEditM[idx].Val;
                        _jogDistance.DistanceS[idx] = _numEditS[idx].Val;
                    }

                    await _jogDistance.Save();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                Close();
            };

            _btnCancel.Click += (s, e) =>
            {
                Close();
            };
        }
    }
}
