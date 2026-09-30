// -----------------------------------------------------------------------
// <copyright file="MainPanel.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Epson.RoboticsShared.ExtensionsAPI;
using IntegratedJogPanel.Common;
using IntegratedJogPanel.Model;
using IntegratedJogPanel.View;
using IntegratedJogPanel.ViewMap2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static IntegratedJogPanel.Constants;

namespace IntegratedJogPanel
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        private static MainPanel? _instance;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static MainPanel? Instance => _instance;
        
        /// <summary>
        /// Create instance.
        /// </summary>
        /// <returns>Instance</returns>
        public static MainPanel CreateInstance()
        {
            if (_instance == null)
            {
                _instance = new MainPanel();
            }

            return _instance;
        }

        /// <summary>
        /// Update captions. (for language changed)
        /// </summary>
        public void UpdateCaptions()
        {
            _btnToJogOnly.Content = Main.Captions?[_guiModeJogOnly ? Caption.Gen_WholeMode : Caption.Gen_ToJogOnly];
            _txtBlockGuidance.Text = Main.Captions?[_mode == EMode.Adjust ? Caption.ViewArea_GuideAdjustMode : Caption.ViewArea_GuideNormalMode];
            SetErrorMessage();
        }

        private enum EMode { Normal, Adjust, }
        private EMode _mode = EMode.Normal;
        private bool _guiModeJogOnly
        {
            get => GeneralManager.Instance.Conf.GUIMode.Values[0] == 1;
            set => GeneralManager.Instance.Conf.GUIMode.Values[0] = value ? 1 : 0;
        }
        private DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
        private Map2DView _map2dview;
        private List<NameAndValueItem> _nvJoints;
        private List<JointControl> _jointControls = new List<JointControl>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPanel()
        {
            InitializeComponent();

            // Initialize fields.
            _map2dview = new Map2DView(_map2dCanvas, _map2dCanvasPts, _map2dCanvasAdj, _map2dFront);
            _posViewCtrl.PtsCtrl = _robotPointsControl;
            _map2dview.RefreshAction = view => _posViewCtrl.Refresh(view);
            GeneralManager.Instance.IOItemsIn.ForEach(item => _stackPanelIOIn.Children.Add(new IOItemControl(item, _stackPanelIOIn.Children.Count == 0)));
            GeneralManager.Instance.IOItemsOut.ForEach(item => _stackPanelIOOut.Children.Add(new IOItemControl(item, _stackPanelIOOut.Children.Count == 0)));
            _nvJoints = new List<NameAndValueItem>() { _nvCurPosJ1, _nvCurPosJ2, _nvCurPosJ3, _nvCurPosJ4, _nvCurPosJ5, _nvCurPosJ6, };
            _jointControls = new List<JointControl>() { _jogJ1, _jogJ2, _jogJ3, _jogJ4, _jogJ5, _jogJ6, };
            _jointControls.ForEach(j => j.JoggingAction = () => _onOffBtnAdjustMode.SwitchOnWithFunc(false));

            // Initaizeli areas.
            InitJogArea();
            InitPointsArea();
            InitViewArea();

            // Initialize for controller connection.
            Action toJogCont = () =>
            {
                var word = "Cont";
                _jogItemJoint.HeaderCtrl._hSel.SelectedWord = word;
                _jogItemWorld.HeaderCtrl._hSel.SelectedWord = word;
                _jogItemTool.HeaderCtrl._hSel.SelectedWord = word;
                GeneralManager.Instance.JogDistanceJoint.SelectType(word);
                GeneralManager.Instance.JogDistanceWorld.SelectType(word);
                GeneralManager.Instance.JogDistanceTool.SelectType(word);
            };

            Func<Task> controllerConnStatChangedProc = async () =>
            {
                var con = RobotController.Instance.IsConnected;

                toJogCont();

                if (con)
                {
                    _nvRobotName.TxtValue = await RobotController.Instance.PrintTxt("Print RobotInfo$(0)");
                    _nvRobotModelName.TxtValue = await RobotController.Instance.PrintTxt("Print RobotInfo$(1)");
                    _nvRobotSerialNumber.TxtValue = await RobotController.Instance.PrintTxt("Print RobotInfo$(4)");
                    UpdateControllerStatDisplay();

                    _cmbBoxTool.Items.Clear();

                    if (RobotController.Instance.HasRobot)
                    {
                        var rcTools = await RobotController.Instance.GetRCTools();
                        GeneralManager.Instance.SetRCTools(rcTools);
                        GeneralManager.Instance.RCTools.ForEach(tl => _cmbBoxTool.Items.Add($"Tool {tl.Number}"));
                        _cmbBoxTool.SelectedIndex = await RobotController.Instance.GetRCTool();
                    }

                    GeneralManager.Instance.StartIOWatch();
                }
                else
                {
                    GeneralManager.Instance.EndIOWatch();
                    _onOffBtnAdjustMode.SwitchOnWithFunc(false);
                    _onOffButtonMotor.SwitchOn = false;
                    _btnPower.IsSelectedB = false;
                    _btnSpeed.IsSelectedB = false;
                    _nvRobotName.TxtValue = "";
                    _nvRobotModelName.TxtValue = "";
                    _nvRobotSerialNumber.TxtValue = "";
                    _txtBlockEStop.Foreground = Brushes.LightGray;
                    _txtBlockSafety.Foreground = Brushes.LightGray;
                    _txtBlockError.Foreground = Brushes.LightGray;
                    _txtBlockWarning.Foreground = Brushes.LightGray;
                }

                var isEnableJog = con && RobotController.Instance.HasRobot;

                _borderControllerState.IsEnabled = con;
                _jogItemIOIn.IsContentEnabled = con;
                _jogItemIOOut.IsContentEnabled = con;
                _jogItemJoint.IsContentEnabled = isEnableJog;
                _jogItemWorld.IsContentEnabled = isEnableJog;
                _jogItemTool.IsContentEnabled = isEnableJog;
                _stackPanelAddPoint.IsEnabled = isEnableJog;
                _stackPanelMotionReqMove.IsEnabled = isEnableJog;
                _stackPanelMotionReqGo.IsEnabled = isEnableJog;
                _onOffButtonControllerConnection.SwitchOn = con;
                UpdateCurrentPositioinDisplay();
            };
            controllerConnStatChangedProc();
            RobotController.Instance.ConnectionStateChangedAction = controllerConnStatChangedProc;
            RobotController.Instance.ControllerStateChangedAction = () => Application.Current.Dispatcher.BeginInvoke(() => UpdateControllerStatDisplay());
            RobotController.Instance.IsJogSpeedHigh = () => _btnSpeed.IsSelectedB;

            // Initialize for tool.
            _cmbBoxTool.SelectionChanged += async (s, e) =>
            {
                if (_cmbBoxTool.SelectedIndex < 0)
                {
                    return;
                }

                await RobotController.Instance.ExecCmd($"Tool {_cmbBoxTool.SelectedIndex}");

                var tl = GeneralManager.Instance.RCTools[_cmbBoxTool.SelectedIndex];
                _textBlockTool.Text = $"{tl.X:0} {tl.Y:0} {tl.Z:0} {tl.U:0} {tl.V:0} {tl.W:0}";

                RedrawPoints();
            };

            // Initialize for view direction.
            _viewDirectionSelect.SelectedAction = t =>
            {
                GeneralManager.Instance.AddDataToCollect($"ViewDirectionSelect/{t}");

                _posViewCtrl.ViewDir = t;
                _map2dview.ChangeViewDir(t);

                var words = ViewDirection.GetJogBtnLabels(t);

                _btnJogA.Content = words[0];
                _btnJogB1.Content = words[1];
                _btnJogB2.Content = words[2];
                _btnJogC.Content = words[3];
                _btnJogD1.Content = words[4];
                _btnJogD2.Content = words[5];

                UpdateCurrentPositioinDisplay();
            };
            _viewDirectionSelect.SelectType(0);
            _btnWorldJogLayout.Click += (s, e) =>
            {
                if (_viewDirectionSelectJogWorld.Visibility != Visibility.Visible)
                {
                    _viewDirectionSelectJogWorld.SelectType(_viewDirectionSelect.GetSelectedType());
                    _viewDirectionSelectJogWorld.Visibility = Visibility.Visible;
                }
            };
            _viewDirectionSelectJogWorld.SelectedAction = t =>
            {
                _viewDirectionSelect.SelectType(t);
                _viewDirectionSelectJogWorld.Visibility = Visibility.Collapsed;
            };

            // Intialize for tool jog layout.
            _toolJogBtnLayoutSelect.SelectedAction = t =>
            {
                GeneralManager.Instance.AddDataToCollect($"ToolJogBtnLayoutSelect/{t}");

                var words = ToolJogBtnLayout.GetJogBtnLabels(t);
                _btnJogTLXYa.Content = words[0];
                _btnJogTLXYb1.Content = words[1];
                _btnJogTLXYb2.Content = words[2];
                _btnJogTLXYc.Content = words[3];
            };
            _toolJogBtnLayoutSelect.SelectType(ToolJogBtnLayout.Option.TwdZPlusXTop);
            _btnToolJogLayout.Click += (s, e) =>
            {
                if (_toolJogBtnLayoutSelectJogTool.Visibility != Visibility.Visible)
                {
                    _toolJogBtnLayoutSelectJogTool.SelectType(_toolJogBtnLayoutSelect.GetSelectedType());
                    _toolJogBtnLayoutSelectJogTool.Visibility = Visibility.Visible;
                }
            };
            _toolJogBtnLayoutSelectJogTool.SelectedAction = t =>
            {
                _toolJogBtnLayoutSelect.SelectType(t);
                _toolJogBtnLayoutSelectJogTool.Visibility = Visibility.Collapsed;
            };

            // Initialize for jog only mode.
            {
                Action laytoutByGUIMode = () =>
                {
                    _gridLeftPane.Visibility = _guiModeJogOnly ? Visibility.Collapsed : Visibility.Visible;
                    _stackPanelAddPoint.Visibility = _guiModeJogOnly ? Visibility.Collapsed : Visibility.Visible;
                    _btnWorldJogLayout.Visibility = _guiModeJogOnly ? Visibility.Visible : Visibility.Hidden;
                    _btnToolJogLayout.Visibility = _guiModeJogOnly ? Visibility.Visible : Visibility.Hidden;
                };
                _gridLeftPane.LayoutUpdated += (s, e) => _map2dview.Init();
                laytoutByGUIMode();
                _btnToJogOnly.Click += (s, e) =>
                {
                    _guiModeJogOnly = !_guiModeJogOnly;
                    laytoutByGUIMode();
                    UpdateCaptions();
                };
            }

            // Intitialize for layout jog panel
            {
                _jogLayoutPanel.DicPanels.Add(1, _jogItemIOIn);
                _jogLayoutPanel.DicPanels.Add(2, _jogItemIOOut);
                _jogLayoutPanel.DicPanels.Add(3, _jogItemJoint);
                _jogLayoutPanel.DicPanels.Add(4, _jogItemWorld);
                _jogLayoutPanel.DicPanels.Add(5, _jogItemTool);
                _jogLayoutPanel.UpdateFromConf();
                Action layoutJogPanel = () =>
                {
                    foreach (var item in _jogLayoutPanel.HiddenItems)
                    {
                        Grid.SetRow(item, 4);
                        item.Visibility = Visibility.Collapsed;
                    }

                    var idx = 0;

                    foreach (var item in _jogLayoutPanel.VisibleItems)
                    {
                        Grid.SetRow(item, idx++);
                        item.Visibility = Visibility.Visible;
                    }
                };
                layoutJogPanel();
                _jogLayoutPanel.UpdateLayoutAction = () => layoutJogPanel();
                _btnJogLayout.Click += (s, e) =>
                {
                    var shown = _jogLayoutPanel.Visibility == Visibility.Visible;
                    _jogLayoutPanel.Visibility = shown ? Visibility.Hidden : Visibility.Visible;
                    _jogLayoutMarkToOpen.Visibility = shown ? Visibility.Visible : Visibility.Hidden;
                    _jogLayoutMarkToClose.Visibility = shown ? Visibility.Hidden : Visibility.Visible;
                };
            }

            // Initialize for add / insert point.

            Func<Task> updatePoint = async () =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var idx = GeneralManager.Instance.RobotPoints.IndexOf(point);
                    var pointNew = new RobotPoint();

                    if (await pointNew.SetPos(RobotController.Instance))
                    {
                        pointNew.Label = point.Label;
                        GeneralManager.Instance.RobotPoints.Remove(point);
                        GeneralManager.Instance.RobotPoints.Insert(idx, pointNew);
                        GeneralManager.Instance.ReindexRobotPointsNo();
                        _robotPointsControl.ResetItems();
                        _robotPointsControl.SelectIndex(idx);
                        RedrawPoints();
                    }
                }
            };
            _btnUpdatePoint.Click += async (s, e) =>
            {
                _btnUpdatePoint.IsEnabled = false;
                await updatePoint();
                _btnUpdatePoint.IsEnabled = true;
            };

            Func<Task> insertPointBefore = async () =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var idx = GeneralManager.Instance.RobotPoints.IndexOf(point);

                    if (idx >= 0)
                    {
                        var pointNew = new RobotPoint();

                        if (await pointNew.SetPos(RobotController.Instance))
                        {
                            GeneralManager.Instance.RobotPoints.Insert(idx, pointNew);
                            GeneralManager.Instance.ReindexRobotPointsNo();
                            _robotPointsControl.ResetItems();
                            _robotPointsControl.SelectIndex(idx);
                            RedrawPoints();
                        }
                    }
                }
            };
            _btnInsertPointBefore.Click += async (s, e) =>
            {
                _btnInsertPointBefore.IsEnabled = false;
                await insertPointBefore();
                _btnInsertPointBefore.IsEnabled = true;
            };

            Func<Task> insertPointAfter = async () =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var idx = GeneralManager.Instance.RobotPoints.IndexOf(point) + 1;

                    if (idx >= 0)
                    {
                        var pointNew = new RobotPoint();

                        if (await pointNew.SetPos(RobotController.Instance))
                        {
                            GeneralManager.Instance.RobotPoints.Insert(idx, pointNew);
                            GeneralManager.Instance.ReindexRobotPointsNo();
                            _robotPointsControl.ResetItems();
                            _robotPointsControl.SelectIndex(idx);
                            RedrawPoints();
                        }
                    }
                }
            };
            _btnInsertPointAfter.Click += async (s, e) =>
            {
                _btnInsertPointAfter.IsEnabled = false;
                await insertPointAfter();
                _btnInsertPointAfter.IsEnabled = true;
            };

            Func<Task> addPoint = async () =>
            {
                var point = new RobotPoint();
                point.Number = GeneralManager.Instance.RobotPoints.Count;

                if (await point.SetPos(RobotController.Instance))
                {
                    GeneralManager.Instance.RobotPoints.Add(point);
                    _robotPointsControl.SelectLastItem();
                    RedrawPoints();
                }
            };
            _btnAddPoint.Click += async (s, e) =>
            {
                _btnAddPoint.IsEnabled = false;
                await addPoint();
                _btnAddPoint.IsEnabled = true;
            };

            // Initialize for adjust.

            _map2dview.NoticeAdjustAction = async (pt, req, x, y, z) =>
            {
                if (_mode != EMode.Adjust) return;
                if (!RobotController.Instance.IsConnected) return;

                GeneralManager.Instance.AddDataToCollect($"Adjust/{req}");

                RobotController.Instance.SetJogSpeed(_btnSpeed.IsSelectedB);
                var ret = await RobotController.Instance.ExecSPELCmdEx($"Go Here +X({x:0.000}) +Y({y:0.000}) +Z({z:0.000}) LJM");
                GeneralManager.Instance.ShowErrorMsg(ret.errNo);
                if (ret.errNo != 0) return;

                if (req == "Update") await updatePoint();
                if (req == "Insert After") await insertPointAfter();
                if (req == "Insert Before") await insertPointBefore();
            };

            // Intialize for screen whole.

            UpdateCaptions();

            Loaded += (s, e) => _map2dview.Init();
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                PeriodicProc();
                _timer.Start();
            };
            _timer.Start();
        }

        private void InitJogArea()
        {
            _onOffButtonControllerConnection.ChangedFuncAsync = async (on) =>
            {
                var ret = false;

                if (on)
                {
                    ret = await RobotController.Instance.ConnectController();
                }
                else
                {
                    RobotController.Instance.DisconnectController();
                    ret = true;
                }

                return ret;
            };

            _onOffButtonMotor.ChangedFuncAsync = async (on) =>
            {
                var ret = 0;
                ret = await RobotController.Instance.ExecCmd(on ? "Motor On" : "Motor Off");
                GeneralManager.Instance.ShowErrorMsg(ret);
                return (ret == 0 && on == RobotController.Instance.IsMotorOn);
            };

            _btnReset.Click += async (s, e) => await RobotController.Instance.ExecCmd("Reset");
            _btnPower.ChangedFuncAsync = async (on) => await RobotController.Instance.ExecCmd("Power " + (on ? "High" : "Low")) == 0;
            _btnSpeed.ChangedFunc = on =>
            {
                RobotController.Instance.SetJogSpeed(on);
                return true;
            };

            _jogItemIOIn.HeaderCtrl._btnEdit.Click += (s, e) =>
            {
                new IOBitSelectWindow(IRCXIOAPI.RCXIOKind.Input) { Owner = Window.GetWindow(this) }.ShowDialog();

                foreach (var item in _stackPanelIOIn.Children)
                {
                    if (item is IOItemControl ctrl) ctrl.UpdateDisplay();
                }

                GeneralManager.Instance.StartIOWatch();
            };

            _jogItemIOOut.HeaderCtrl._btnEdit.Click += (s, e) =>
            {
                new IOBitSelectWindow(IRCXIOAPI.RCXIOKind.Output) { Owner = Window.GetWindow(this) }.ShowDialog();

                foreach (var item in _stackPanelIOOut.Children)
                {
                    if (item is IOItemControl ctrl) ctrl.UpdateDisplay();
                }

                GeneralManager.Instance.StartIOWatch();
            };

            Action<JogItemPanelHeader, JogDistance> initJogHeader = (header, jd) =>
            {
                header._btnEdit.Click += (s, e) =>
                {
                    _ = new JogDistanceWindow(jd) { Owner = Window.GetWindow(this) }.ShowDialog();
                    UpdateCurrentPositioinDisplay();
                };

                header._hSel.SelectedFuncAsync = async (word) =>
                {
                    jd.SelectType(word);
                    await jd.Load(jd.SelectedType);
                    UpdateCurrentPositioinDisplay();
                };
            };

            initJogHeader(_jogItemJoint.HeaderCtrl, GeneralManager.Instance.JogDistanceJoint);
            initJogHeader(_jogItemWorld.HeaderCtrl, GeneralManager.Instance.JogDistanceWorld);
            initJogHeader(_jogItemTool.HeaderCtrl, GeneralManager.Instance.JogDistanceTool);

            new List<Button>()
            {
                _btnJogA, _btnJogB1, _btnJogB2, _btnJogC, _btnJogD1, _btnJogD2,
                _btnJogUMinus, _btnJogUPlus,
                _btnJogVMinus, _btnJogVPlus,
                _btnJogWMinus, _btnJogWPlus,
            }.ForEach(btn =>
            {
                btn.Click += async (s, e) =>
                {
                    _onOffBtnAdjustMode.SwitchOnWithFunc(false);
                    if (GeneralManager.Instance.JogDistanceWorld.SelectedType == IRCXRobotManagerAPI.RCXJogDistance.Continuous) return;

                    btn.IsEnabled = false;
                    await RobotController.Instance.StartCartesianJog(btn.Content.ToString(), IRCXRobotManagerAPI.RCXJogMode.World, false);
                    btn.IsEnabled = true;
                };

                btn.PreviewMouseLeftButtonDown += async (s, e) =>
                {
                    _onOffBtnAdjustMode.SwitchOnWithFunc(false);
                    if (GeneralManager.Instance.JogDistanceWorld.SelectedType != IRCXRobotManagerAPI.RCXJogDistance.Continuous) return;

                    await RobotController.Instance.StartCartesianJog(btn.Content.ToString(), IRCXRobotManagerAPI.RCXJogMode.World, true);
                };
            });

            new List<Button>()
            {
                _btnJogTLXYa, _btnJogTLXYb1, _btnJogTLXYb2, _btnJogTLXYc, _btnJogTLZPlus, _btnJogTLZMinus,
                _btnJogTLUMinus, _btnJogTLUPlus,
                _btnJogTLVMinus, _btnJogTLVPlus,
                _btnJogTLWMinus, _btnJogTLWPlus,
            }.ForEach(btn =>
            {
                btn.Click += async (s, e) =>
                {
                    _onOffBtnAdjustMode.SwitchOnWithFunc(false);
                    if (GeneralManager.Instance.JogDistanceTool.SelectedType == IRCXRobotManagerAPI.RCXJogDistance.Continuous) return;

                    btn.IsEnabled = false;
                    await RobotController.Instance.StartCartesianJog(btn.Content.ToString(), IRCXRobotManagerAPI.RCXJogMode.Tool, false);
                    btn.IsEnabled = true;
                };

                btn.PreviewMouseLeftButtonDown += async (s, e) =>
                {
                    _onOffBtnAdjustMode.SwitchOnWithFunc(false);
                    if (GeneralManager.Instance.JogDistanceTool.SelectedType != IRCXRobotManagerAPI.RCXJogDistance.Continuous) return;

                    await RobotController.Instance.StartCartesianJog(btn.Content.ToString(), IRCXRobotManagerAPI.RCXJogMode.Tool, true);
                };
            });
        }

        private void InitPointsArea()
        {
            _robotPointsControl.SelectionChangedAction = () =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint pt)
                {
                    _map2dview.SelectRobotPoint(pt);
                    _posViewCtrl.UpdateView();
                }
            };

            _robotPointsControl.EditedAction = () =>
            {
                _map2dview.Refresh();
                _posViewCtrl.UpdateView();
            };

            _btnRobotPointUp.Click += (s, e) =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var idx = GeneralManager.Instance.RobotPoints.IndexOf(point);

                    if (idx > 0)
                    {
                        GeneralManager.Instance.RobotPoints.Remove(point);
                        GeneralManager.Instance.RobotPoints.Insert(idx - 1, point);
                        GeneralManager.Instance.ReindexRobotPointsNo();
                        _robotPointsControl.ResetItems();
                        _robotPointsControl.SelectIndex(idx - 1);
                        RedrawPoints();
                    }
                }
            };

            _btnRobotPointDown.Click += (s, e) =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var idx = GeneralManager.Instance.RobotPoints.IndexOf(point);

                    if (idx < (GeneralManager.Instance.RobotPoints.Count - 1))
                    {
                        GeneralManager.Instance.RobotPoints.Remove(point);
                        GeneralManager.Instance.RobotPoints.Insert(idx + 1, point);
                        GeneralManager.Instance.ReindexRobotPointsNo();
                        _robotPointsControl.ResetItems();
                        _robotPointsControl.SelectIndex(idx + 1);
                        RedrawPoints();
                    }
                }
            };

            _btnRobotPointRemove.Click += (s, e) =>
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var preSelIdx = _robotPointsControl.GetSelectedIndex();
                    GeneralManager.Instance.RobotPoints.Remove(point);
                    GeneralManager.Instance.ReindexRobotPointsNo();
                    _robotPointsControl.ResetItems();

                    if (preSelIdx < GeneralManager.Instance.RobotPoints.Count)
                    {
                        _robotPointsControl.SelectIndex(preSelIdx);
                    }
                    else
                    {
                        _robotPointsControl.SelectLastItem();
                    }

                    RedrawPoints();
                }
            };

            _btnRobotPointsClear.Click += (s, e) =>
            {
                GeneralManager.Instance.RobotPoints.Clear();
                RedrawPoints();
            };

            _btnRobotPointMove.Click += async (s, e) => await ExecMotion("Move", EMotionType.ToSel, false);
            _btnRobotPointMoveHeadToSel.Click += async (s, e) => await ExecMotion("Move", EMotionType.HeadToSel, false);
            _btnRobotPointMoveAll.Click += async (s, e) => await ExecMotion("Move", EMotionType.HeadToTail, false);
            _btnRobotPointGo.Click += async (s, e) => await ExecMotion("Go", EMotionType.ToSel, false);
            _btnRobotPointGoHeadToSel.Click += async (s, e) => await ExecMotion("Go", EMotionType.HeadToSel, false);
            _btnRobotPointGoAll.Click += async (s, e) => await ExecMotion("Go", EMotionType.HeadToTail, false);
            _btnRobotPointStop.Click += async (s, e) =>
            {
                await RobotController.Instance.AbortSPELCmdEx();
                _stopReq = true;
            };

            _btnAddPointsFromClipboard.Click += (s, e) =>
            {
                var txt = Clipboard.GetText();
                var lines = txt.Split("\r\n");

                var pts = new List<RobotPoint>();
                var no = GeneralManager.Instance.RobotPoints.Count;
                var ok = false;

                foreach (var line in lines)
                {
                    var pt = new RobotPoint();
                    ok = pt.SetByRCPlusPtEditorTxt(line);
                    pt.Number = no++;

                    if (!ok)
                    {
                        break;
                    }

                    pts.Add(pt);
                }

                if (!ok)
                {
                    return;
                }

                pts.ForEach(pt => GeneralManager.Instance.RobotPoints.Add(pt));
                _robotPointsControl.SelectLastItem();
                RedrawPoints();
            };

            _btnPointsToClipboard.Click += (s, e) =>
            {
                var txt = "";

                foreach (var pt in GeneralManager.Instance.RobotPoints)
                {
                    if (!string.IsNullOrEmpty(txt)) txt += "\r\n";
                    txt += pt.GetRCPlusPtEditorTxt();
                }

                Clipboard.SetText(txt);
            };

            _btnLoadFromRCPlusPts.Click += (s, e) =>
            {
                var files = GeneralManager.Instance.GetRCPlusPtsFiles();
                if (files.Count <= 0) return;

                var win = new SelectRCPlusPtsFileWindow(files) { Owner = Window.GetWindow(this) };
                win.ShowDialog();
                if (string.IsNullOrEmpty(win.SelectedFilename)) return;

                GeneralManager.Instance.LoadRobotPointsFromRCPlusPts(win.SelectedFilename);
                _robotPointsControl.SelectLastItem();
                RedrawPoints();
            };

            _btnSaveToRCPlusPts.Click += async (s, e) =>
            {
                var filename = await GeneralManager.Instance.CreateRCPlusFile();
                if (string.IsNullOrEmpty(filename)) return;

                GeneralManager.Instance.SaveRobotPointsToRCPlusPts(filename);
            };
        }

        private void InitViewArea()
        {
            _map2dCanvas.SizeChanged += (s, e) => _map2dview.Refresh();

            _map2dview.NoticeSelectPointAction = async (pt) =>
            {
                _robotPointsControl.SelectIndex(GeneralManager.Instance.RobotPoints.IndexOf(pt));
                _posViewCtrl.UpdateView();

                if (_mode != EMode.Adjust) return;

                var isSuccess = await ExecMotion("Go", EMotionType.ToSel, true);
                if (!isSuccess) return;

                _map2dview.StartAdjust(pt);
            };

            Action<OnOffButton, Action<bool>> setShowOnOffCtrl = (btn, action) =>
            {
                btn.ChangedFunc = on =>
                {
                    action(on);
                    RedrawPoints();
                    return true;
                };
            };

            setShowOnOffCtrl(_onOffBtnShowPoints, on => _map2dview.ViewConf.ShowPoints = on);
            setShowOnOffCtrl(_onOffBtnShowLocus, on => _map2dview.ViewConf.ShowLocus = on);
            setShowOnOffCtrl(_onOffBtnShowPtAdv, on => _map2dview.ViewConf.ShowPtAdv = on);
            setShowOnOffCtrl(_onOffBtnShowPtLabel, on => _map2dview.ViewConf.ShowPtLabel = on);

            _onOffBtnAdjustMode.ChangedFunc = on =>
            {
                if (!on)
                {
                    _map2dview.EndAdjust();
                }

                if (on && !RobotController.Instance.IsConnected)
                {
                    return false;
                }

                _mode = on ? EMode.Adjust : EMode.Normal;
                _txtBlockDanger.Visibility = _mode == EMode.Adjust ? Visibility.Visible : Visibility.Collapsed;
                UpdateCaptions();
                return true;
            };
        }

        private void PeriodicProc()
        {
            if (!GeneralManager.Instance.IsIntegratedJogPanelShown) return;
            if (!RobotController.Instance.IsConnected) return;
            if (RobotController.Instance.IsConnecting) return;

            UpdateCurrentPositioinDisplay();
        }

        private bool _stopReq;
        private enum EMotionType { ToSel, HeadToSel, HeadToTail, }
        private async Task<bool> ExecMotion(string motion, EMotionType motionType, bool ljm)
        {
            var isSccess = false;

            if (!RobotController.Instance.IsConnected) return isSccess;

            RobotController.Instance.SetJogSpeed(_btnSpeed.IsSelectedB);

            _stackPanelMotionReqMove.IsEnabled = false;
            _stackPanelMotionReqGo.IsEnabled = false;
            _stopReq = false;
            _btnRobotPointStop.IsEnabled = true;

            var txtLJM = ljm ? " LJM" : "";

            if (motionType == EMotionType.ToSel)
            {
                if (_robotPointsControl.GetSelectedRobotPoint() is RobotPoint point)
                {
                    var ret = await RobotController.Instance.ExecSPELCmdEx($"{motion} {point.GetPtTxt()}{txtLJM}");
                    GeneralManager.Instance.ShowErrorMsg(ret.errNo);
                    isSccess = ret.errNo == 0;
                }
            }
            else
            {
                var selectedIdx = _robotPointsControl.GetSelectedIndex();

                for (var i = 0; i < GeneralManager.Instance.RobotPoints.Count; i++)
                {
                    var point = GeneralManager.Instance.RobotPoints[i];
                    _robotPointsControl.SelectIndex(i);
                    var ret = await RobotController.Instance.ExecSPELCmdEx($"{motion} {point.GetPtTxt()}{txtLJM}");
                    PeriodicProc();
                    GeneralManager.Instance.ShowErrorMsg(ret.errNo);
                    isSccess = ret.errNo == 0;
                    if (ret.errNo != 0) break;

                    UpdateControllerStatDisplay();
                    
                    if (RobotController.Instance.State.IsEStopOn) break;
                    if (motionType == EMotionType.HeadToSel && selectedIdx == i) break;
                    if (_stopReq) break;
                }
            }

            _btnRobotPointStop.IsEnabled = false;
            _stackPanelMotionReqMove.IsEnabled = true;
            _stackPanelMotionReqGo.IsEnabled = true;

            return isSccess;
        }

        private void RedrawPoints()
        {
            _map2dview.DrawPoints(_robotPointsControl.GetSelectedRobotPoint());
            _posViewCtrl.UpdateView();
        }

        private void SetErrorMessage()
        {
            var errorNo = RobotController.Instance.State.SystemErrorCode;
            _textBlockControllerStatus.Text = errorNo == 0 ? "" : $"{errorNo} : {RobotController.Instance.GetErrMsg(errorNo)}";
        }

        private void UpdateControllerStatDisplay()
        {
            _onOffButtonControllerConnection.SwitchOn = RobotController.Instance.IsConnected;
            if (!RobotController.Instance.IsConnected) return;

            RobotController.Instance.GetControllerState();
            _onOffButtonMotor.SwitchOn = RobotController.Instance.State.IsMotorOn;
            _btnPower.IsSelectedB = RobotController.Instance.State.IsPowerHigh;
            _txtBlockEStop.Foreground = RobotController.Instance.State.IsEStopOn ? Brushes.Red : Brushes.LightGray;
            _txtBlockSafety.Foreground = RobotController.Instance.State.IsSafeguardOn ? Brushes.Red : Brushes.LightGray;
            _txtBlockError.Foreground = RobotController.Instance.State.SystemErrorOccured ? Brushes.Red : Brushes.LightGray;
            _txtBlockWarning.Foreground = RobotController.Instance.State.SystemWarningCode != 0 ? Brushes.Blue : Brushes.LightGray;
            SetErrorMessage();
        }

        private void UpdateCurrentPositioinDisplay()
        {
            RobotController.Instance.GetCurrPos2();

            for (var i = 0; i < _nvJoints.Count; i++)
            {
                var txtName = "";
                var txtVal = "";

                if (RobotController.Instance.IsConnected)
                {
                    if (i < RobotController.Instance.Joints.Count)
                    {
                        var joint = RobotController.Instance.Joints[i];

                        txtName += $"J{i + 1} {joint.JointUnit}";
                        txtVal = $"{joint.CurrentPos:0.000}";
                    }
                }
                else
                {
                    txtName += $"J{i + 1}";
                }

                 _nvJoints[i].TxtName = txtName;
                _nvJoints[i].TxtValue = txtVal;
            }

            _jointControls.ForEach(jc => jc.UpdateDisplay());

            if (RobotController.Instance.IsConnected)
            {
                _nvCurPosX.TxtValue = $"{RobotController.Instance.PosX:0.000}";
                _nvCurPosY.TxtValue = $"{RobotController.Instance.PosY:0.000}";
                _nvCurPosZ.TxtValue = $"{RobotController.Instance.PosZ:0.000}";
                _nvCurPosU.TxtValue = $"{RobotController.Instance.PosU:0.000}";
                _nvCurPosV.TxtValue = $"{RobotController.Instance.PosV:0.000}";
                _nvCurPosW.TxtValue = $"{RobotController.Instance.PosW:0.000}";
            }
            else
            {
                _nvCurPosX.TxtValue = "";
                _nvCurPosY.TxtValue = "";
                _nvCurPosZ.TxtValue = "";
                _nvCurPosU.TxtValue = "";
                _nvCurPosV.TxtValue = "";
                _nvCurPosW.TxtValue = "";
            }

            Func<Button, double> getPos = btn =>
            {
                var txt = btn.Content.ToString() ?? "";
                if (txt.StartsWith("X")) return RobotController.Instance.PosX;
                if (txt.StartsWith("Y")) return RobotController.Instance.PosY;
                if (txt.StartsWith("Z")) return RobotController.Instance.PosZ;
                return 0;
            };

            if (RobotController.Instance.IsConnected)
            {
                _txtBlockJogA.Text = $"{getPos(_btnJogA):0}";
                _txtBlockJogB.Text = $"{getPos(_btnJogB1):0}";
                _txtBlockJogC.Text = $"{getPos(_btnJogC):0}";
                _txtBlockJogD.Text = $"{getPos(_btnJogD1):0}";
                _txtBlockJogU.Text = $"{RobotController.Instance.PosU:0}";
                _txtBlockJogV.Text = $"{RobotController.Instance.PosV:0}";
                _txtBlockJogW.Text = $"{RobotController.Instance.PosW:0}";

                _txtBlockJogV.Visibility = RobotController.Instance.IsSCARA ? Visibility.Hidden : Visibility.Visible;
                _txtBlockJogW.Visibility = RobotController.Instance.IsSCARA ? Visibility.Hidden : Visibility.Visible;
                _btnJogVMinus.IsEnabled = !RobotController.Instance.IsSCARA;
                _btnJogVPlus.IsEnabled = !RobotController.Instance.IsSCARA;
                _btnJogWMinus.IsEnabled = !RobotController.Instance.IsSCARA;
                _btnJogWPlus.IsEnabled = !RobotController.Instance.IsSCARA;
            }
            else
            {
                _txtBlockJogA.Text = "";
                _txtBlockJogB.Text = "";
                _txtBlockJogC.Text = "";
                _txtBlockJogD.Text = "";
                _txtBlockJogU.Text = "";
                _txtBlockJogV.Text = "";
                _txtBlockJogW.Text = "";
            } 

            if (RobotController.Instance.IsConnected)
            {
                Func<Button, double> getJD = btn =>
                {
                    var txt = btn.Content.ToString() ?? "";
                    if (txt.StartsWith("X")) return GeneralManager.Instance.JogDistanceWorld.GetDistance(0);
                    if (txt.StartsWith("Y")) return GeneralManager.Instance.JogDistanceWorld.GetDistance(1);
                    if (txt.StartsWith("Z")) return GeneralManager.Instance.JogDistanceWorld.GetDistance(2);
                    return 0;
                };

                _txtJogDistanceA.Val = getJD(_btnJogA);
                _txtJogDistanceB.Val = getJD(_btnJogB1);
                _txtJogDistanceC.Val = getJD(_btnJogC);
                _txtJogDistanceD.Val = getJD(_btnJogD1);
                _txtJogDistanceU.Val = GeneralManager.Instance.JogDistanceWorld.GetDistance(3);
                _txtJogDistanceV.Val = GeneralManager.Instance.JogDistanceWorld.GetDistance(4);
                _txtJogDistanceW.Val = GeneralManager.Instance.JogDistanceWorld.GetDistance(5);
            }
            else
            {
                _txtJogDistanceA.Val = 0;
                _txtJogDistanceB.Val = 0;
                _txtJogDistanceC.Val = 0;
                _txtJogDistanceD.Val = 0;
                _txtJogDistanceU.Val = 0;
                _txtJogDistanceV.Val = 0;
                _txtJogDistanceW.Val = 0;
            }

            if (RobotController.Instance.IsConnected)
            {
                Func<Button, double> getTLJD = btn =>
                {
                    var txt = btn.Content.ToString() ?? "";
                    if (txt.StartsWith("X")) return GeneralManager.Instance.JogDistanceTool.GetDistance(0);
                    if (txt.StartsWith("Y")) return GeneralManager.Instance.JogDistanceTool.GetDistance(1);
                    if (txt.StartsWith("Z")) return GeneralManager.Instance.JogDistanceTool.GetDistance(2);
                    return 0;
                };

                _txtJogDistanceTLA.Val = getTLJD(_btnJogTLXYa);
                _txtJogDistanceTLB.Val = getTLJD(_btnJogTLXYb1);
                _txtJogDistanceTLC.Val = getTLJD(_btnJogTLXYc);
                _txtJogDistanceTLD.Val = getTLJD(_btnJogTLZPlus);
                _txtJogDistanceTLU.Val = GeneralManager.Instance.JogDistanceTool.GetDistance(3);
                _txtJogDistanceTLV.Val = GeneralManager.Instance.JogDistanceTool.GetDistance(4);
                _txtJogDistanceTLW.Val = GeneralManager.Instance.JogDistanceTool.GetDistance(5);

                _btnJogTLVMinus.IsEnabled = !RobotController.Instance.IsSCARA;
                _btnJogTLVPlus.IsEnabled = !RobotController.Instance.IsSCARA;
                _btnJogTLWMinus.IsEnabled = !RobotController.Instance.IsSCARA;
                _btnJogTLWPlus.IsEnabled = !RobotController.Instance.IsSCARA;
            }
            else
            {
                _txtJogDistanceTLA.Val = 0;
                _txtJogDistanceTLB.Val = 0;
                _txtJogDistanceTLC.Val = 0;
                _txtJogDistanceTLD.Val = 0;
                _txtJogDistanceTLU.Val = 0;
                _txtJogDistanceTLV.Val = 0;
                _txtJogDistanceTLW.Val = 0;
            }
            
            _map2dview?.ShowCurrentPos(RobotController.Instance.IsConnected);

            if (RobotController.Instance.IsConnected)
            {
                _map2dview?.UpdateCurrentPos(
                    RobotController.Instance.PosX,
                    RobotController.Instance.PosY,
                    RobotController.Instance.PosZ,
                    RobotController.Instance.PosU,
                    RobotController.Instance.PosV,
                    RobotController.Instance.PosW);
            }
        }
    }
}