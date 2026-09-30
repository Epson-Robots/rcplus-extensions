// -----------------------------------------------------------------------
// <copyright file="RobotController.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings.Extensions;
using System.Runtime.InteropServices;
using System.Windows;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXIOAPI;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXRobotManagerAPI;
using static IntegratedJogPanel.Model.RobotController;
using V2 = Epson.RoboticsShared.ExtensionsAPI.V2;

namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// Robot controller.
    /// </summary>
    public class RobotController
    {
        /// <summary>
        /// RobotController singleton instance.
        /// </summary>
        public static RobotController Instance { get; } = new RobotController();

        /// <summary>
        /// Is controller connected.
        /// </summary>
        public bool IsConnected => _controllerConnectionAPI.IsOnline == true;

        /// <summary>
        /// Controller has robot or not.
        /// </summary>
        public bool HasRobot => _robotManagerAPI.CurrentRobotNumber >= 1;

        /// <summary>
        /// Is connecting.
        /// </summary>
        public bool IsConnecting { get; private set; }

        /// <summary>
        /// Controller connection state changed Action.
        /// </summary>
        public Func<Task>? ConnectionStateChangedAction { get; set; }

        public class ControllerState
        {
            public bool IsEStopOn { get; set; }

            public bool IsSafeguardOn { get; set; }

            public int SystemErrorCode { get; set; }

            public bool SystemErrorOccured { get; set; }

            public int SystemWarningCode { get; set; }

            public bool IsMotorOn { get; set; }

            public bool IsPowerHigh { get; set; }
        };

        /// <summary>
        /// Controller state.
        /// </summary>
        public ControllerState State { get; } = new ControllerState();

        /// <summary>
        /// Controller state changed Acition.
        /// </summary>
        public Action? ControllerStateChangedAction { get; set; }

        public class Joint
        {
            public string JointUnit { get; set; } = "deg";
            public double MinMotionRange;
            public double MaxMotionRange;
            public double CurrentPos;

            public double GetCurrentPosRate()
            {
                if ((MaxMotionRange - MinMotionRange) == 0)
                {
                    return 0;
                }

                return (CurrentPos - MinMotionRange) / (MaxMotionRange - MinMotionRange);
            }
        }

        /// <summary>
        /// Robot joints.
        /// </summary>
        public List<Joint> Joints { get; } = new List<Joint>();

        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double PosU { get; set; }
        public double PosV { get; set; }
        public double PosW { get; set; }

        public int Hand { get; set; }
        public int Elbow { get; set; }
        public int Wrist { get; set; }
        public int J1Flag { get; set; }
        public int J2Flag { get; set; }
        public int J4Flag { get; set; }
        public int J6Flag { get; set; }

        /// <summary>
        /// Is jog speed high.
        /// </summary>
        public Func<bool>? IsJogSpeedHigh { get; set; }

        private IRCXGeneralAPI _generalAPI;
        private IRCXControllerConnectionAPI _controllerConnectionAPI;
        private IRCXControllerAPI _controllerAPI;
        private V2.IRCXControllerAPI _controllerAPI_V2;
        private IRCXProgramExecutionAPI _programExecutionAPI;
        private V2.IRCXProgramExecutionAPI _programExecutionAPI_V2;
        private IRCXRobotManagerAPI _robotManagerAPI;
        private IRCXJogger? _jogger;

        // 1: Joint 
        // 2: Cartesian 
        // 3: SCARA 
        // 5: 6-AXIS 
        // 6: RS series 
        // 7: N series 
        private int _robotType;
        public bool IsSCARA => _robotType == 3 || _robotType == 6;

        private RobotController()
        {
            _generalAPI = Main.GetAPI<IRCXGeneralAPI>();
            _controllerConnectionAPI = Main.GetAPI<IRCXControllerConnectionAPI>();
            _controllerAPI = Main.GetAPI<IRCXControllerAPI>();
            _controllerAPI_V2 = Main.GetAPI<V2.IRCXControllerAPI>();
            _programExecutionAPI = Main.GetAPI<IRCXProgramExecutionAPI>();
            _programExecutionAPI_V2 = Main.GetAPI<V2.IRCXProgramExecutionAPI>();
            _robotManagerAPI = Main.GetAPI<IRCXRobotManagerAPI>();

            _robotManagerAPI.ObserveProperty(x => x.CurrentRobotNumber).Subscribe(async (value) =>
            {
                if (HasRobot)
                {
                    var robotModelName = await PrintTxt("Print RobotInfo$(1)");

                    if (!string.IsNullOrEmpty(robotModelName))
                    {
                        _generalAPI.AddDataToCollect($"RobotModelName/{robotModelName}");
                    }
                }

                await ExecConnectedProc();
            });

            _controllerAPI.ObserveProperty(x => x.IsEmergencyStopped).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
            _controllerAPI.ObserveProperty(x => x.IsSafeguardOpened).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
            _controllerAPI.ObserveProperty(x => x.IsSystemErrorOccurred).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
            _controllerAPI.ObserveProperty(x => x.IsMotorOn).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
            _controllerAPI.ObserveProperty(x => x.IsPowerHigh).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
            _controllerAPI_V2.ObserveProperty(x => x.SystemErrorCode).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
            _controllerAPI_V2.ObserveProperty(x => x.SystemWarningCode).Subscribe(isOn => ControllerStateChangedAction?.Invoke());
        }

        /// <summary>
        /// Connect controller.
        /// </summary>
        public async Task<bool> ConnectController()
        {
            await _controllerConnectionAPI.ConnectControllerAsync();
            return IsConnected;
        }

        /// <summary>
        /// Disconnect controller.
        /// </summary>
        public async void DisconnectController()
        {
            await _controllerConnectionAPI.DisconnectControllerAsync();
        }

        /// <summary>
        /// Execute connected procedure.
        /// </summary>
        public async Task ExecConnectedProc()
        {
            if (IsConnected)
            {
                IsConnecting = true;
                await ResetJointList();
                IsConnecting = false;
            }

            if (ConnectionStateChangedAction != null)
            {
                await ConnectionStateChangedAction();
            }
        }

        /// <summary>
        /// Get controller state.
        /// </summary>
        public void GetControllerState()
        {
            State.IsEStopOn = _controllerAPI.IsEmergencyStopped ?? false;
            State.IsSafeguardOn = _controllerAPI.IsSafeguardOpened ?? false;
            State.SystemErrorCode = _controllerAPI_V2.SystemErrorCode ?? 0;
            State.SystemErrorOccured = _controllerAPI.IsSystemErrorOccurred ?? false;
            State.SystemWarningCode = _controllerAPI_V2.SystemWarningCode ?? 0;
            State.IsMotorOn = _controllerAPI.IsMotorOn ?? false;
            State.IsPowerHigh = _controllerAPI.IsPowerHigh ?? false;
        }

        /// <summary>
        /// Is motor on.
        /// </summary>
        public bool IsMotorOn => _controllerAPI.IsMotorOn ?? false;

        /// <summary>
        /// Get RC+ error message.
        /// </summary>
        /// <param name="errNo">Error no</param>
        /// <returns>RC+ error message</returns>
        public string GetErrMsg(int errNo) => _controllerAPI_V2.GetMessage(errNo);

        /// <summary>
        /// Execute SPEL command.
        /// </summary>
        /// <param name="cmd">SPEL command</param>
        /// <returns>Return code, Reply text</returns>
        public async Task<(int errNo, string reply)> ExecSPELCmd(string cmd)
        {
            var retErrNo = -1;
            var retReply = "";

            if (!IsConnected)
            {
                return (retErrNo, retReply);
            }

            try
            {
                var ret = await _programExecutionAPI.ExecuteSpelCommandAsync(cmd);
                retErrNo = ret.Item2;
                retReply = ret.Item3;
            }
            catch (Exception)
            {
            }

            return (retErrNo, retReply);
        }

        /// <summary>
        /// Execute SPEL command.
        /// </summary>
        /// <param name="cmd">SPEL command</param>
        /// <returns>Return code, Reply text</returns>
        public async Task<(int errNo, string reply)> ExecSPELCmdEx(string cmd)
        {
            var retErrNo = -1;
            var retReply = "";

            if (!IsConnected)
            {
                return (retErrNo, retReply);
            }

            try
            {
                var ret = await _programExecutionAPI_V2.ExecuteSpelCommandExAsync(cmd, 0);
                retErrNo = ret.Item2;
                retReply = ret.Item3;
            }
            catch (Exception)
            {
            }

            return (retErrNo, retReply);
        }

        /// <summary>
        /// Abort SPEL Commnand.
        /// </summary>
        /// <returns></returns>
        public async Task AbortSPELCmdEx()
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                await _programExecutionAPI_V2.AbortSpelCommandAsync();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Execute SPEL command.
        /// </summary>
        /// <param name="cmd">SPEL command</param>
        /// <returns>Return code</returns>
        public async Task<int> ExecCmd(string cmd)
        {
            var ret = await ExecSPELCmd(cmd);
            return ret.errNo;
        }

        /// <summary>
        /// Print text.
        /// </summary>
        /// <param name="cmd">SPEL command</param>
        /// <returns>Command output text</returns>
        public async Task<string> PrintTxt(string cmd)
        {
            var ret = await ExecSPELCmd(cmd);
            return ret.reply.Trim();
        }

        /// <summary>
        /// Print integer.
        /// </summary>
        /// <param name="cmd">SPEL command</param>
        /// <returns>(Is success, Command output integer)</returns>
        public async Task<(bool Ret, int Val)> PrintInt(string cmd)
        {
            var ret = await ExecSPELCmd(cmd);
            var val = 0;
            return (int.TryParse(ret.reply.Trim(), out val), val);
        }

        /// <summary>
        /// Print double.
        /// </summary>
        /// <param name="cmd">SPEL command</param>
        /// <returns>(Is success, Command output double)</returns>
        public async Task<(bool Ret, double Val)> PrintDbl(string cmd)
        {
            var ret = await ExecSPELCmd(cmd);
            var val = 0.0;
            return (double.TryParse(ret.reply.Trim(), out val), val);
        }

        /// <summary>
        /// Reset joint list.
        /// </summary>
        public async Task ResetJointList()
        {
            Joints.Clear();

            if (!HasRobot)
            {
                return;
            }

            try
            {
                var txtRange = await PrintTxt("Range");
                var ranges = txtRange.Split(",", StringSplitOptions.RemoveEmptyEntries);

                var txtAglToPlsA = await PrintTxt("Print AglToPls(0,0,0,0,0,0)");
                var aglToPlsA = txtAglToPlsA.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                _robotType = (await PrintInt("Print RobotType")).Val;

                var txtAglToPlsB = await PrintTxt("Print AglToPls(10,10,10,10,10,10)");

                if (IsSCARA)
                {
                    txtAglToPlsB = await PrintTxt("Print AglToPls(10,10,-10,10,10,10)");
                }

                var aglToPlsB = txtAglToPlsB.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < ranges.Length / 2; i++)
                {
                    var plsA = (double)(int.Parse(aglToPlsA[i * 2 + 1]));
                    var plsB = (double)(int.Parse(aglToPlsB[i * 2 + 1]));
                    var plsMin = (double)(int.Parse(ranges[i * 2]));
                    var plsMax = (double)(int.Parse(ranges[i * 2 + 1]));

                    var rate = (plsB - plsA) / 10.0;

                    if (rate == 0)
                    {
                        break;
                    }

                    var joint = new Joint();

                    if (IsSCARA && i == 2)
                    {
                        joint.JointUnit = "mm";
                        joint.MinMotionRange = -plsMin / rate;
                        joint.MaxMotionRange = -plsMax / rate;
                    }
                    else
                    {
                        joint.MinMotionRange = plsMin / rate;
                        joint.MaxMotionRange = plsMax / rate;
                    }

                    Joints.Add(joint);
                }
            }
            catch
            {
                Joints.Clear();
            }
        }

        /// <summary>
        /// Get RC+ Tool list.
        /// </summary>
        /// <returns>RC+ Tool list</returns>
        public async Task<List<RCTool>> GetRCTools()
        {
            var list = new List<RCTool>();
            var txt = await PrintTxt("TLSet");
            
            if (!txt.Contains("Tool"))
            {
                return list;
            }

            try
            {
                var lines = txt.Split("\r\n");

                foreach (var line in lines)
                {
                    var kv = line.Split(":");

                    if (kv.Length != 2)
                    {
                        continue;
                    }

                    var ks = kv[0].Split("Tool");

                    if (ks.Length != 2)
                    {
                        continue;
                    }

                    var vs = kv[1].Split(",");

                    list.Add(new RCTool(
                        int.Parse(ks[1].Trim()),
                        double.Parse(vs[0].Trim()),
                        double.Parse(vs[1].Trim()),
                        double.Parse(vs[2].Trim()),
                        double.Parse(vs[3].Trim()),
                        double.Parse(vs[4].Trim()),
                        double.Parse(vs[5].Trim())
                        ));
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        /// <summary>
        /// Get RC+ Tool.
        /// </summary>
        /// <returns>Tool</returns>
        public async Task<int> GetRCTool()
        {
            var ret = 0;

            try
            {
                ret = (await PrintInt("Tool")).Val;
            }
            catch (Exception)
            {
            }

            return ret;
        }
        
        /// <summary>
        /// Get current robot position.
        /// </summary>
        /// <returns>Is success</returns>
        public async Task<bool> GetCurrPos()
        {
            var ret = false;

            if (!IsConnected)
            {
                return ret;
            }

            if (!HasRobot)
            {
                return ret;
            }

            Func<string, double, bool> getDbl = (cmd, val) =>
            {
                return false;
            };

            try
            {
                do
                {
                    var retPosX = await PrintDbl("Print CX(Here)");
                    var retPosY = await PrintDbl("Print CY(Here)");
                    var retPosZ = await PrintDbl("Print CZ(Here)");
                    var retPosU = await PrintDbl("Print CU(Here)");
                    var retPosV = await PrintDbl("Print CV(Here)");
                    var retPosW = await PrintDbl("Print CW(Here)");
                    var retHand = await PrintInt("Print Hand");
                    var retElbow = await PrintInt("Print Elbow");
                    var retWrist = await PrintInt("Print Wrist");
                    var retJ1Flag = await PrintInt("Print J1Flag");
                    var retJ2Flag = await PrintInt("Print J2Flag");
                    var retJ4Flag = await PrintInt("Print J4Flag");
                    var retJ6Flag = await PrintInt("Print J6Flag");

                    if (!retPosX.Ret || !retPosY.Ret || !retPosZ.Ret || !retPosU.Ret || !retPosV.Ret || !retPosW.Ret ||
                        !retHand.Ret || !retElbow.Ret || !retWrist.Ret ||
                        !retJ1Flag.Ret || !retJ2Flag.Ret || !retJ4Flag.Ret || !retJ6Flag.Ret)
                    {
                        break;
                    }

                    PosX = retPosX.Val;
                    PosY = retPosY.Val;
                    PosZ = retPosZ.Val;
                    PosU = retPosU.Val;
                    PosV = retPosV.Val;
                    PosW = retPosW.Val;
                    Hand = retHand.Val;
                    Elbow = retElbow.Val;
                    Wrist = retWrist.Val;
                    J1Flag = retJ1Flag.Val;
                    J2Flag = retJ2Flag.Val;
                    J4Flag = retJ4Flag.Val;
                    J6Flag = retJ6Flag.Val;

                    ret = true;
                } while (false);
            }
            catch (Exception)
            {
            }

            return ret;
        }

        /// <summary>
        /// Get current robot position.
        /// </summary>
        public void GetCurrPos2()
        {
            if (!IsConnected)
            {
                return;
            }

            if (!HasRobot)
            {
                return;
            }

            try
            {
                {
                    var position = _robotManagerAPI.WorldPosition;

                    if (position != null)
                    {
                        Func<RCXJogCartesianAxis, double> getPos = key =>
                        {
                            return position.ContainsKey(key) ? position[key] : 0.0;
                        };

                        PosX = getPos(RCXJogCartesianAxis.X);
                        PosY = getPos(RCXJogCartesianAxis.Y);
                        PosZ = getPos(RCXJogCartesianAxis.Z);
                        PosU = getPos(RCXJogCartesianAxis.U);
                        PosV = getPos(RCXJogCartesianAxis.V);
                        PosW = getPos(RCXJogCartesianAxis.W);
                    }
                }

                {
                    var position = _robotManagerAPI.JointPosition;

                    if (position != null)
                    {
                        for (var i = 0; i < Joints.Count && i < position.Count; i++)
                        {
                            Joints[i].CurrentPos = position[(RCXJogJointAxis)i];
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        [DllImport("user32")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_LBUTTON = 0x01;

        private static bool IsLeftButtonDownNow()
        {
            return ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0);
        }

        /// <summary>
        /// Set jog speed.
        /// </summary>
        /// <param name="fast">Fast</param>
        public void SetJogSpeed(bool fast)
        {
            _robotManagerAPI.SysJogSpeed = fast ? RCXJogSpeed.Fast : RCXJogSpeed.Slow;
        }

        private async void InitJogger()
        {
            if (_jogger == null || !_jogger.IsValid)
            {
                _jogger = await _robotManagerAPI.CreateJoggerAsync();
                _jogger.ShouldKeepJogging = () =>
                {
                    return IsLeftButtonDownNow();
                };
            }

            if (IsJogSpeedHigh != null)
            {
                SetJogSpeed(IsJogSpeedHigh());
            }
        }

        /// <summary>
        /// Start joint jog
        /// </summary>
        /// <param name="jointIndex">Joint index</param>
        /// <param name="plus">Plus</param>
        /// <param name="isCont">Is continuous</param>
        /// <returns></returns>
        public async Task StartJointJog(int jointIndex, bool plus, bool isCont)
        {
            InitJogger();

            if (_jogger == null)
            {
                return;
            }

            _robotManagerAPI.SysJogMode = RCXJogMode.Joint;

            if (!isCont)
            {
                _robotManagerAPI.SysJogDistance = GeneralManager.Instance.JogDistanceJoint.SelectedType;
            }

            var ret = await _jogger.StartJointJogAsync((RCXJogJointAxis)jointIndex, plus, isCont);

            if (ret.Item1 != RCXCommon.RCXResult.Success && ret.Item1 != RCXCommon.RCXResult.IsJoggingAlready)
            {
                GeneralManager.Instance.ShowMsg(ret.Item2);
            }
        }

        private (bool chkOK, RCXJogCartesianAxis axis, bool plus) TxtToCartesianAxis(string txt)
        {
            var chkOK = false;
            var axis = RCXJogCartesianAxis.X;
            var plus = true;

            var dictAxis = new Dictionary<char, RCXJogCartesianAxis>
            {
                { 'X', RCXJogCartesianAxis.X },
                { 'Y', RCXJogCartesianAxis.Y },
                { 'Z', RCXJogCartesianAxis.Z },
                { 'U', RCXJogCartesianAxis.U },
                { 'V', RCXJogCartesianAxis.V },
                { 'W', RCXJogCartesianAxis.W },
            };

            var dictPlus = new Dictionary<char, bool>
            {
                { '+', true },
                { '-', false },
            };

            do
            {
                if (txt.Length != 2)
                {
                    break;
                }

                if (!dictAxis.ContainsKey(txt[0]))
                {
                    break;
                }

                if (!dictPlus.ContainsKey(txt[1]))
                {
                    break;
                }

                axis = dictAxis[txt[0]];
                plus = dictPlus[txt[1]];
                chkOK = true;
            } while (false);

            return (chkOK, axis, plus);
        }

        /// <summary>
        /// Start cartesian jog.
        /// </summary>
        /// <param name="txt">Text (e.g. X+)</param>
        /// <param name="jogMode">Jog mode</param>
        /// <param name="isCont">Is continuous</param>
        /// <returns></returns>
        public async Task StartCartesianJog(string? txt, RCXJogMode jogMode, bool isCont)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return;
            }

            InitJogger();

            if (_jogger == null)
            {
                return;
            }

            var axis = TxtToCartesianAxis(txt);

            if (!axis.chkOK)
            {
                return;
            }

            _robotManagerAPI.SysJogMode = jogMode;

            if (!isCont)
            {
                if (jogMode == RCXJogMode.World)
                {
                    _robotManagerAPI.SysJogDistance = GeneralManager.Instance.JogDistanceWorld.SelectedType;
                }
                else if (jogMode == RCXJogMode.Tool)
                {
                    _robotManagerAPI.SysJogDistance = GeneralManager.Instance.JogDistanceTool.SelectedType;
                }
                else
                {
                    return;
                }
            }

            var ret = await _jogger.StartCartesianJogAsync(axis.axis, axis.plus, isCont);

            if (ret.Item1 != RCXCommon.RCXResult.Success && ret.Item1 != RCXCommon.RCXResult.IsJoggingAlready)
            {
                GeneralManager.Instance.ShowMsg(ret.Item2);
            }
        }
    }
}
