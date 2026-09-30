// -----------------------------------------------------------------------
// <copyright file="GeneralManager.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings.Extensions;
using SpelAnalysisTool.Common;
using System.IO;
using System.Reflection;
using System.Windows;
using static SpelAnalysisTool.Constants;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// General manager.
    /// </summary>
    internal class GeneralManager
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static GeneralManager Instance { get; } = new GeneralManager();

        /// <summary>
        /// AdjParam inc file name.
        /// </summary>
        public const string AdjParamsIncFileName = "AdjParams.inc";

        /// <summary>
        /// Configuration.
        /// </summary>
        public GeneralConf Conf { get; } = new GeneralConf();

        /// <summary>
        /// Robot controller.
        /// </summary>
        public RobotController Controller { get; } = new RobotController();

        /// <summary>
        /// Robot information.
        /// </summary>
        public RobotInfo RobotInfo { get; set; } = new RobotInfo(new RCXRobotDescription());

        /// <summary>
        /// Log data - Information.
        /// </summary>
        public LogDataInfo LogDatInfo { get; } = new LogDataInfo();

        /// <summary>
        /// Log data - Section.
        /// </summary>
        public LogDataSection LogDatSection { get; } = new LogDataSection();

        /// <summary>
        /// Log data - Motion.
        /// </summary>
        public LogDataMotion LogDatMotion { get; } = new LogDataMotion();

        /// <summary>
        /// All Task finished Action.
        /// </summary>
        public Action? AllTaskFinishedAcition { get; set; }

        private IRCXGeneralAPI _generalAPI;
        private IRCXWindowAPI _windowAPI;
        private IRCXProjectAPI _projectAPI;
        private IRCXProgramExecutionAPI _programExecutionAPI;
        private IRCXRobotDescriptionAPI _robotDescriptionAPI;

        private List<LogData> _logDats;

        /// <summary>
        /// Constructor.
        /// </summary>
        private GeneralManager()
        {
            _logDats = new List<LogData>()
            {
                LogDatInfo, LogDatSection, LogDatMotion,
            };

            _generalAPI = Main.GetAPI<IRCXGeneralAPI>();
            _windowAPI = Main.GetAPI<IRCXWindowAPI>();
            _projectAPI = Main.GetAPI<IRCXProjectAPI>();
            _programExecutionAPI = Main.GetAPI<IRCXProgramExecutionAPI>();
            _robotDescriptionAPI = Main.GetAPI<IRCXRobotDescriptionAPI>();

            _programExecutionAPI.ObserveProperty(x => x.Tasks).Subscribe((tasks) =>
            {
                if (tasks.Count() <= 0) return;
                if(tasks.Any(t => t.State != IRCXProgramExecutionAPI.IRCXTask.RCXTaskState.Finished)) return;
                AllTaskFinishedAcition?.Invoke();
            });
        }

        /// <summary>
        /// Set RobotInfo.
        /// </summary>
        /// <returns></returns>
        public async Task SetRobotInfo()
        {
            if (!Controller.IsConnected) return;

            var robotInfo1 = await Controller.ExecSPELCmd("Print RobotInfo$(1)");
            var robotTypeStr = await Controller.ExecSPELCmd("Print RobotType");

            var robotType = 0;

            if (robotTypeStr.errNo == 0)
            {
                int.TryParse(robotTypeStr.reply.Trim(), out robotType);
            }

            if (robotInfo1.errNo == 0)
            {
                SetRobotInfo(robotInfo1.reply, robotType);
            }
        }

        /// <summary>
        /// Set RobotInfo.
        /// </summary>
        /// <param name="modelName">Model name.</param>
        /// <param name="robotType">Robot type.</param>
        public void SetRobotInfo(string modelName, int robotType)
        {
            if (RobotInfo == null || RobotInfo.RoboDesc.Id != modelName)
            {
                var roboDesc = _robotDescriptionAPI.CreateRobotDescription(modelName);

                if (roboDesc != null)
                {
                    RobotInfo = new RobotInfo(roboDesc);
                }
            }

            if (RobotInfo == null)
            {
                return;
            }

            if (robotType == 3 || robotType == 6)
            {
                RobotInfo.ResetPosition();

                var joint = RobotInfo.Joints[2];

                if (joint != null)
                {
                    RobotInfo.RobotPointOffsetZ = joint.CurrentPosCoord.Y;
                }
            }
        }

        /// <summary>
        /// Get robot position.
        /// </summary>
        /// <returns>true:Success.</returns>
        public bool GetRobotPos()
        {
            if (!Controller.IsConnected) return false;
            if (RobotInfo == null) return false;

            Controller.GetCurrPos();

            // World
            {
                RobotInfo.CurrentPosX = Controller.PosX * 0.001;
                RobotInfo.CurrentPosY = Controller.PosY * 0.001;
                RobotInfo.CurrentPosZ = Controller.PosZ * 0.001;
                RobotInfo.CurrentPosU = Controller.PosU;
                RobotInfo.CurrentPosV = Controller.PosV;
                RobotInfo.CurrentPosW = Controller.PosW;
            }

            // Joint
            {
                for (var i = 0; i < RobotInfo.Joints.Count; i++)
                {
                    var joint = RobotInfo.Joints[i];

                    if (joint.JointDesc.JointType == RCXRobotDescription.JointType.Rotary)
                    {
                        joint.CurrentPos = Controller.Joints[i] * Math.PI / 180;
                    }
                    else
                    {
                        joint.CurrentPos = Controller.Joints[i] * 0.001;
                    }
                }
            }

            RobotInfo.UpdateCurrentPosCoord();

            return true;
        }

        /// <summary>
        /// Set robot posiion.
        /// </summary>
        /// <param name="logData">Motion log data.</param>
        public void SetRobotPos(LogDataMotion.Record logData)
        {
            // World
            {
                RobotInfo.CurrentPosX = logData.PosX * 0.001;
                RobotInfo.CurrentPosY = logData.PosY * 0.001;
                RobotInfo.CurrentPosZ = logData.PosZ * 0.001;
                RobotInfo.CurrentPosU = logData.PosU;
                RobotInfo.CurrentPosV = logData.PosV;
                RobotInfo.CurrentPosW = logData.PosW;
            }

            // Joint
            {
                for (var i = 0; i < RobotInfo.Joints.Count; i++)
                {
                    var joint = RobotInfo.Joints[i];

                    if (joint.JointDesc.JointType == RCXRobotDescription.JointType.Rotary)
                    {
                        joint.CurrentPos = logData.Joint[i] * Math.PI / 180;
                    }
                    else
                    {
                        joint.CurrentPos = logData.Joint[i] * 0.001;
                    }
                }
            }

            RobotInfo.UpdateCurrentPosCoord();
        }

        /// <summary>
        /// Show message.
        /// </summary>
        /// <param name="msg">Message.</param>
        public void ShowMsg(string msg)
        {
            _windowAPI?.ShowMessageBox(
                new(Main.CommonId, Caption.ExtensionName), new(msg),
                IRCXWindowAPI.ButtonType.OK, IRCXWindowAPI.IconType.None);
        }

        /// <summary>
        /// Get SPEL program function names.
        /// </summary>
        /// <returns>Function names.</returns>
        public List<string>? GetFunctionNames()
        {
            return _programExecutionAPI.UserFunctions?.Where(f => !f.StartsWith("SpelLog_") && !f.StartsWith("SpelLogPrv_")).ToList();
        }

        /// <summary>
        /// Build RC project.
        /// </summary>
        /// <returns>true:Success.</returns>
        public async Task<bool> BuildProject()
        {
            var result = await _projectAPI.BuildAsync(true);

            if (result != RCXCommon.RCXResult.Success)
            {
                ShowMsg($"{result}");
            }

            return result == RCXCommon.RCXResult.Success;
        }

        /// <summary>
        /// Start SPEL program function.
        /// </summary>
        /// <param name="functionName">Function name.</param>
        /// <param name="logObserve">Log ovserve or not.</param>
        /// <returns>Task.</returns>
        public async Task StartFunction(string functionName, bool logObserve)
        {
            var result = await _programExecutionAPI.StartFunctionAsync(functionName, false, false, Main.CommonId);

            if (result != RCXCommon.RCXResult.Success)
            {
                ShowMsg($"{result}");
            }

            _generalAPI.AddDataToCollect($"StartFunction/{(logObserve ? "LogObserve" : "NotLogObserve")}");
        }

        /// <summary>
        /// Reset log data.
        /// </summary>
        public void ResetLogDat()
        {
            _logDats.ForEach(dat => dat.ResetData());
        }

        /// <summary>
        /// Load CSV files.
        /// </summary>
        /// <param name="limitCycleNo">Limit cycle no for loading. if its zero, no limit.</param>
        public void LoadCSV(int limitCycleNo = 0)
        {
            var folder = _projectAPI.ProjectFolder;
            if (folder == null) return;

            _logDats.ForEach(dat => dat.LoadCSV(folder, Conf.Set.LogName, limitCycleNo));

            _generalAPI.AddDataToCollect($"LoadLogCSV/CycleCnt{LogDatSection.GetCycleCnt()}/SectionCnt{LogDatSection.GetSectionCnt()}/MotionCnt{LogDatMotion.GetMotionCnt()}");
        }

        /// <summary>
        /// Check if section log file has been updated.
        /// </summary>
        /// <param name="dateTime">Check if it's newer than this time.</param>
        /// <returns>true:File has been updated.</returns>
        public bool CheckSectionLogFileUpdate(DateTime dateTime)
        {
            var folder = _projectAPI.ProjectFolder;
            if (folder == null) return false;

            var filename = Conf.Set.LogName;
            var filepath = $"{folder}\\{filename}_Section.csv";

            var lastWriteTime = File.GetLastWriteTime(filepath);

            return lastWriteTime > dateTime;
        }

        /// <summary>
        /// Find the last cycle number of section log.
        /// </summary>
        /// <returns>Last cycle number.</returns>
        public int CheckEndCycleNo()
        {
            var folder = _projectAPI.ProjectFolder;
            if (folder == null) return 0;

            var filename = Conf.Set.LogName;
            return LogDatSection.CheckEndCycleNo($"{folder}\\{filename}_Section.csv");
        }

        /// <summary>
        /// Add resource file to RC project.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Task.</returns>
        public async Task AddResourceFileToProject(string filename)
        {
            var folder = _projectAPI.ProjectFolder;
            if (folder == null) return;

            var filepath = folder + "\\" + filename;
            var txt = "";

            var resourceUri = new Uri($"pack://application:,,,/{Main.CommonId};component/Resources/{filename}");
            var streamInfo = Application.GetResourceStream(resourceUri);

            if (streamInfo != null)
            {
                using (var reader = new StreamReader(streamInfo.Stream))
                {
                    txt = reader.ReadToEnd();
                }
            }

            System.IO.File.WriteAllText(filepath, txt);

            await _projectAPI.AddSpecificFileToProjectAsync(folder + "\\" + filename, Main.CommonId);
        }

        /// <summary>
        /// Add SPEL Library to RC project.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task AddLibraryToProject()
        {
            var libZipPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    "Libraries",
                    "SpelLoggerLib.zip"
                );

            var libraryAPI = Main.GetAPI<IRCXLibraryAPI>();
            var result = await libraryAPI.ImportAsync(libZipPath, true).ConfigureAwait(true);
            if (result != RCXCommon.RCXResult.Success)
            {
                Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.Msg_LibraryImportFailed),
                    IRCXWindowAPI.ButtonType.OK,
                    IRCXWindowAPI.IconType.Error);
                return;
            }

            if (libraryAPI.ProjectLibraries != null &&
                libraryAPI.ProjectLibraries.Any(x => string.Equals(x, "SpelLoggerLib.lib", StringComparison.OrdinalIgnoreCase)) != true)
            {
                result = await libraryAPI.AddToProjectAsync("SpelLoggerLib.lib", Main.CommonId).ConfigureAwait(true);
                if (result != RCXCommon.RCXResult.Success)
                {
                    Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.Msg_LibraryProjectAddFailed),
                        IRCXWindowAPI.ButtonType.OK,
                        IRCXWindowAPI.IconType.Error);
                    return;
                }
            }
        }

        /// <summary>
        /// Add AdjParams.inc to RC project.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task AddAdjParamsIncToProject()
        {
            var folder = _projectAPI.ProjectFolder;
            if (folder == null) return;

            await WriteAdjParamsInc();
            await _projectAPI.AddSpecificFileToProjectAsync($"{folder}\\{AdjParamsIncFileName}", Main.CommonId);
        }

        /// <summary>
        /// Write AdjParams.inc.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task WriteAdjParamsInc()
        {
            var folder = _projectAPI.ProjectFolder;
            if (folder == null) return;

            
            var filepath = $"{folder}\\{AdjParamsIncFileName}";
            var txt = "' SpelAnalysisTool AdjParams\r\n";

            await _projectAPI.CloseProjectFileAsync(AdjParamsIncFileName);

            foreach (var param in Conf.Set.AdjParams)
            {
                if (string.IsNullOrEmpty(param.Name)) continue;

                txt += $"#define {param.Name} {param.Value}";

                if (!string.IsNullOrEmpty(param.Comment))
                {
                    txt += $" ' {param.Comment}";
                }

                txt += "\r\n";
            }

            System.IO.File.WriteAllText(filepath, txt);
        }

        /// <summary>
        /// Add SpelLogger.
        /// </summary>
        /// <param name="isSpelLoggerTypeFile">true:File,false:Library</param>
        /// <param name="addSample">Add sample program or not.</param>
        /// <returns>Task.</returns>
        public async Task AddSpelLogger(bool isSpelLoggerTypeFile, bool addSample)
        {
            if (isSpelLoggerTypeFile)
            {
                await GeneralManager.Instance.AddResourceFileToProject("SpelLogger.prg");
            }
            else
            {
                await GeneralManager.Instance.AddLibraryToProject();
            }

            await GeneralManager.Instance.AddAdjParamsIncToProject();

            if (addSample)
            {
                await GeneralManager.Instance.AddResourceFileToProject("SmpUsingSpelLogger.prg");
            }

            _generalAPI.AddDataToCollect($"AddLogger/{(isSpelLoggerTypeFile ? "File" : "Library")}/{(addSample ? "AddSample" : "NotAddSample")}");
        }
    }
}
