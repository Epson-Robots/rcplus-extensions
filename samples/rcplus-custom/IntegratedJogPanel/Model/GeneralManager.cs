// -----------------------------------------------------------------------
// <copyright file="GeneralManager.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Epson.RoboticsShared.ExtensionsAPI;
using System.Collections.ObjectModel;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXIOAPI.IRCXIOGroup;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXPointAPI;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXRobotManagerAPI;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static IntegratedJogPanel.Constants;

namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// General mangaer.
    /// </summary>
    internal class GeneralManager
    {
        /// <summary>
        /// GeneralManager singleton instance.
        /// </summary>
        public static GeneralManager Instance { get; } = new GeneralManager();

        /// <summary>
        /// Cofiguration.
        /// </summary>
        public GeneralConf Conf { get; } = new GeneralConf();

        /// <summary>
        /// RC+ Tool list.
        /// </summary>
        public List<RCTool> RCTools { get; } = new List<RCTool>();

        /// <summary>
        /// Robot points.
        /// </summary>
        public ObservableCollection<RobotPoint> RobotPoints { get; } = new ObservableCollection<RobotPoint>();

        /// <summary>
        /// Input I/O list.
        /// </summary>
        public List<IOItem> IOItemsIn { get; } = new List<IOItem>();

        /// <summary>
        /// Output I/O list.
        /// </summary>
        public List<IOItem> IOItemsOut { get; } = new List<IOItem>();

        /// <summary>
        /// Joint jog disantce.
        /// </summary>
        public JogDistance JogDistanceJoint { get; } = new JogDistance(RCXJogMode.Joint, "Joint", "J1,J2,J3,J4,J5,J6", "deg,deg,deg,deg,deg,deg");
        
        /// <summary>
        /// World jog distance.
        /// </summary>
        public JogDistance JogDistanceWorld { get; } = new JogDistance(RCXJogMode.World, "World", "X,Y,Z,U,V,W", "mm,mm,mm,deg,deg,deg");
        
        /// <summary>
        /// Tool jog distance.
        /// </summary>
        public JogDistance JogDistanceTool { get; } = new JogDistance(RCXJogMode.Tool, "Tool", "X,Y,Z,U,V,W", "mm,mm,mm,deg,deg,deg");

        /// <summary>
        /// Is IntegratedJogPanel shown.
        /// </summary>
        public bool IsIntegratedJogPanelShown { get; set; }

        public IRCXWindowAPI? WindowAPI { get; }

        private IRCXGeneralAPI _generalAPI;
        private IRCXProjectAPI _projectAPI;
        private IRCXPointAPI _pointAPI;
        private IRCXIOAPI _ioAPI;
        private List<JogDistance> _jogDistances;

        private GeneralManager()
        {
            WindowAPI = Main.GetAPI<IRCXWindowAPI>();

            _generalAPI = Main.GetAPI<IRCXGeneralAPI>();
            _projectAPI = Main.GetAPI<IRCXProjectAPI>();
            _pointAPI = Main.GetAPI<IRCXPointAPI>();
            _ioAPI = Main.GetAPI<IRCXIOAPI>();

            for (var i = 0; i < 8; i++)
            {
                IOItemsIn.Add(new IOItem(IRCXIOAPI.RCXIOKind.Input, Conf.IOBitsIn.Values[i], true));
                IOItemsOut.Add(new IOItem(IRCXIOAPI.RCXIOKind.Output, Conf.IOBitsOut.Values[i], true));
            }

            _jogDistances = new List<JogDistance>()
            {
                JogDistanceJoint,
                JogDistanceWorld,
                JogDistanceTool,
            };

            SetRCTools(new List<RCTool>());
        }

        /// <summary>
        /// Add data to collect log.
        /// </summary>
        /// <param name="txt">Text</param>
        public void AddDataToCollect(string txt)
        {
            _generalAPI.AddDataToCollect(txt);
        }

        /// <summary>
        /// Show message.
        /// </summary>
        /// <param name="msg">Message</param>
        public void ShowMsg(string msg)
        {
            GeneralManager.Instance.WindowAPI?.ShowMessageBox(
                new(Main.CommonId, Caption.ExtensionName), new(msg),
                IRCXWindowAPI.ButtonType.OK, IRCXWindowAPI.IconType.None);
        }

        /// <summary>
        /// Show error message.
        /// </summary>
        /// <param name="error">Error Id</param>
        public void ShowErrorMsg(int error)
        {
            if (error == 0)
            {
                return;
            }

            ShowMsg(RobotController.Instance.GetErrMsg(error));
        }

        /// <summary>
        /// Set RC+ Tool list.
        /// </summary>
        /// <param name="rctools">RC Tool list</param>
        public void SetRCTools(List<RCTool> rctools)
        {
            RCTools.Clear();
            RCTools.Add(new RCTool(0, 0, 0, 0, 0, 0, 0));
            RCTools.AddRange(rctools);
        }

        /// <summary>
        /// Get I/O list.
        /// </summary>
        /// <param name="ioKind">I/O kind</param>
        /// <returns>I/O list</returns>
        public List<IOItem> GetIOList(IRCXIOAPI.RCXIOKind ioKind)
        {
            var list = new List<IOItem>();
            var (result, groups) = _ioAPI.GetIOGroups();

            if (result == RCXResult.Success)
            {
                foreach (var group in groups)
                {
                    PortRange? portRange = null;

                    if (ioKind == IRCXIOAPI.RCXIOKind.Input)
                    {
                        portRange = group.GetInputPortRange<bool>();
                    }
                    else if (ioKind == IRCXIOAPI.RCXIOKind.Output)
                    {
                        portRange = group.GetOutputPortRange<bool>();
                    }

                    if (portRange == null)
                    {
                        continue;
                    }

                    for (var i = portRange.startPort; i <= portRange.endPort; i++)
                    {
                        list.Add(new IOItem(ioKind, i, true));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Start I/O state watch.
        /// </summary>
        public void StartIOWatch()
        {
            IOItemsIn.ForEach(item => item.StartWatch());
            IOItemsOut.ForEach(item => item.StartWatch());
        }

        /// <summary>
        /// End I/O state watch.
        /// </summary>
        public void EndIOWatch()
        {
            IOItemsIn.ForEach(item => item.EndWatch());
            IOItemsOut.ForEach(item => item.EndWatch());
        }

        /// <summary>
        /// Reindex robot points number.
        /// </summary>
        public void ReindexRobotPointsNo()
        {
            var idx = 0;

            foreach (var robotPoint in RobotPoints)
            {
                robotPoint.Number = idx;
                idx++;
            }
        }

        /// <summary>
        /// Get RC+ point filenames list.
        /// </summary>
        /// <returns>RC+ point filenames list</returns>
        public List<string> GetRCPlusPtsFiles()
        {
            if (_projectAPI.ProjectFiles == null)
            {
                return new List<string>();
            }

            return _projectAPI.ProjectFiles.Where(f => f.Name.ToLower().EndsWith(".pts")).Select(f => f.Name).ToList();
        }

        /// <summary>
        /// Load robot points from RC+ robot point file.
        /// </summary>
        /// <param name="filename">Robot point file name</param>
        public void LoadRobotPointsFromRCPlusPts(string filename)
        {
            var (ret, points) = _pointAPI.GetPoints(filename);

            if (ret != RCXResult.Success)
            {
                return;
            }

            if (points == null)
            {
                return;
            }

            var tmpPoints = new List<RobotPoint>();
            var strError = "";

            foreach (var pt in points)
            {
                var local = (int)pt["Local"].Value;

                if (local != 0)
                {
                    strError = Main.Captions?[Caption.PointsArea_PtsLoadErrorLocal];
                    break;
                }

                var no = (int)pt["Number"].Value;

                if (no != tmpPoints.Count)
                {
                    strError = Main.Captions?[Caption.PointsArea_PtsLoadErrorNotSequential];
                    break;
                }

                var point = new RobotPoint()
                {
                    Number = no,
                    Label = (string)pt["Label"].Value,
                    X = (double)pt["X"].Value,
                    Y = (double)pt["Y"].Value,
                    Z = (double)pt["Z"].Value,
                    U = (double)pt["U"].Value,
                    V = (double)pt["V"].Value,
                    W = (double)pt["W"].Value,
                    R = (double)pt["R"].Value,
                    S = (double)pt["S"].Value,
                    T = (double)pt["T"].Value,
                    Hand = (int)pt["Hand"].Value,
                    Elbow = (int)pt["Elbow"].Value,
                    Wrist = (int)pt["Wrist"].Value,
                    J1Flag = (int)pt["J1Flag"].Value,
                    J2Flag = (int)pt["J2Flag"].Value,
                    J4Flag = (int)pt["J4Flag"].Value,
                    J6Flag = (int)pt["J6Flag"].Value,
                    J1Angle = (double)pt["J1Angle"].Value,
                    J4Angle = (double)pt["J4Angle"].Value,
                    Description = (string)pt["Description"].Value,
                };

                tmpPoints.Add(point);
            }

            if (!string.IsNullOrEmpty(strError))
            {
                ShowMsg(strError);
                return;
            }

            if (RobotPoints.Count > 0)
            {
                var resConfirm = GeneralManager.Instance.WindowAPI?.ShowMessageBox(
                    new(Main.CommonId, Caption.ExtensionName), new(Main.Captions?[Caption.PointsArea_PtsLoadConfirmOverwrite] ?? ""),
                    IRCXWindowAPI.ButtonType.Yes_No_Cancel, IRCXWindowAPI.IconType.Question);

                if (resConfirm == IRCXWindowAPI.ResponseType.Cancel)
                {
                    return;
                }
                else if (resConfirm == IRCXWindowAPI.ResponseType.Yes)
                {
                    RobotPoints.Clear();
                }
            }

            tmpPoints.ForEach(pt => RobotPoints.Add(pt));
            ReindexRobotPointsNo();
        }

        /// <summary>
        /// Create RC+ robot point file.
        /// </summary>
        /// <returns>RC+ Robot point filename</returns>
        public async Task<string?> CreateRCPlusFile()
        {
            var result = await _projectAPI.CreateFileAsync(".pts", Main.CommonId);
            return result;
        }

        /// <summary>
        /// Save robot points to RC+ robot point file. 
        /// </summary>
        /// <param name="filename">RC+ Robot point filename</param>
        public void SaveRobotPointsToRCPlusPts(string filename)
        {
            foreach (var pt in RobotPoints)
            {
                Dictionary<string, RCXPointElement> point = new()
                {
                    ["Number"] = new(typeof(int), pt.Number),
                    ["Label"] = new(typeof(string), pt.Label),
                    ["X"] = new(typeof(double), pt.X),
                    ["Y"] = new(typeof(double), pt.Y),
                    ["Z"] = new(typeof(double), pt.Z),
                    ["U"] = new(typeof(double), pt.U),
                    ["V"] = new(typeof(double), pt.V),
                    ["W"] = new(typeof(double), pt.W),
                    ["R"] = new(typeof(double), pt.R),
                    ["S"] = new(typeof(double), pt.S),
                    ["T"] = new(typeof(double), pt.T),
                    ["Local"] = new(typeof(int), 0),
                    ["Hand"] = new(typeof(int), pt.Hand),
                    ["Elbow"] = new(typeof(int), pt.Elbow),
                    ["Wrist"] = new(typeof(int), pt.Wrist),
                    ["J1Flag"] = new(typeof(int), pt.J1Flag),
                    ["J2Flag"] = new(typeof(int), pt.J2Flag),
                    ["J4Flag"] = new(typeof(int), pt.J4Flag),
                    ["J6Flag"] = new(typeof(int), pt.J6Flag),
                    ["J1Angle"] = new(typeof(double), pt.J1Angle),
                    ["J4Angle"] = new(typeof(double), pt.J4Angle),
                    ["Description"] = new(typeof(string), pt.Description),
                };
                var result = _pointAPI.AddPoint(filename, point);

                if (result != RCXResult.Success)
                {
                    ShowMsg($"{string.Format(Main.Captions?[Caption.PointsArea_PtsSaveError] ?? "", pt.Number)}\n{result}");
                    break;
                }
            }
        }
    }
}
