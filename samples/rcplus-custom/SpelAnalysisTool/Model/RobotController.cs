// -----------------------------------------------------------------------
// <copyright file="RobotController.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings.Extensions;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXRobotManagerAPI;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Robot controller.
    /// </summary>
    internal class RobotController
    {
        /// <summary>
        /// Is connected or not.
        /// </summary>
        public bool IsConnected => _controllerConnectionAPI.IsOnline == true;

        public double PosX;
        public double PosY;
        public double PosZ;
        public double PosU;
        public double PosV;
        public double PosW;

        public double[] Joints = new double[6];

        /// <summary>
        /// Connection state changed Action.
        /// </summary>
        public Func<Task>? ConnectionStateChangedAction { get; set; }

        private IRCXControllerConnectionAPI _controllerConnectionAPI;
        private IRCXProgramExecutionAPI _programExecutionAPI;
        private IRCXRobotManagerAPI _robotManagerAPI;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RobotController()
        {
            _controllerConnectionAPI = Main.GetAPI<IRCXControllerConnectionAPI>();
            _programExecutionAPI = Main.GetAPI<IRCXProgramExecutionAPI>();
            _robotManagerAPI = Main.GetAPI<IRCXRobotManagerAPI>();

            _robotManagerAPI.ObserveProperty(x => x.CurrentRobotNumber).Subscribe(async (value) =>
            {
                if (value < 1) return;
                await ExecConnectedProc();
            });
        }

        private async Task ExecConnectedProc()
        {
            if (ConnectionStateChangedAction != null)
            {
                await ConnectionStateChangedAction();
            }
        }

        /// <summary>
        /// Exec SPEL command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>errNo:Error Number, reply:Reply</returns>
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
        /// Get current robot position.
        /// </summary>
        public void GetCurrPos()
        {
            if (!IsConnected)
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
                        for (var i = 0; i < Joints.Length && i < position.Count; i++)
                        {
                            Joints[i] = position[(RCXJogJointAxis)i];
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
