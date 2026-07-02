// -----------------------------------------------------------------------
// <copyright file="IOItem.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.Drawing;
using System.Reflection.Emit;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXIOAPI;
using static IntegratedJogPanel.Model.RobotController;

namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// I/O item.
    /// </summary>
    public class IOItem
    {
        /// <summary>
        /// I/O kind
        /// </summary>
        public IRCXIOAPI.RCXIOKind IOKind { get; set; }

        /// <summary>
        /// I/O bit no
        /// </summary>
        public int BitNo { get; set; }

        /// <summary>
        /// I/O label
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// I/O description
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Wheter bit is on
        /// </summary>
        public bool IsOn => _watcher?.IsOn ?? false;

        /// <summary>
        /// I/O bit state changed Action
        /// </summary>
        public Action? StatChangedAction { get; set; }

        private IRCXIOAPI _ioAPI;

        private class IOWatcher
        {
            public Action? ChangedAction { get; set; }

            public bool IsOn => _isOn;
            
            private IRCXIOAPI.IRCXIOWatcher? _watcher;

            private bool _isOn;

            public bool Init(IRCXIOAPI ioAPI, RCXIOKind kind, int bitNo)
            {
                if (kind == RCXIOKind.Input)
                {
                    var curStat = ioAPI.IOReadInput<bool>(bitNo);
                    _isOn = curStat.Item1 == RCXCommon.RCXResult.Success ? curStat.Item2 : false;
                }

                if (kind == RCXIOKind.Output)
                {
                    var curStat = ioAPI.IOReadOutput<bool>(bitNo);
                    _isOn = curStat.Item1 == RCXCommon.RCXResult.Success ? curStat.Item2 : false;
                }

                var (ret, watcher) = ioAPI.CreateWatcher<bool>(kind, bitNo, WatcherDataChanged);
                _watcher = watcher;
                return ret == RCXCommon.RCXResult.Success && _watcher != null;
            }

            public void End()
            {
                ChangedAction = null;
                _watcher?.Dispose();
                _watcher = null;
            }

            private void WatcherDataChanged(
                IRCXIOWatcher watcher,
                int oldData,
                int newData
            )
            {
                _isOn = newData != 0;
                ChangedAction?.Invoke();
            }
        }

        private IOWatcher? _watcher;

        /// <summary>
        /// Constructor of IOItem.
        /// </summary>
        /// <param name="ioKind">I/O kind</param>
        /// <param name="bitNo">I/O bit no</param>
        /// <param name="loadLabel">I/O label</param>
        public IOItem(IRCXIOAPI.RCXIOKind ioKind, int bitNo, bool loadLabel)
        {
            _ioAPI = Main.GetAPI<IRCXIOAPI>();

            IOKind = ioKind;
            BitNo = bitNo;

            if (loadLabel)
            {
                LoadLabel();
            }
        }

        /// <summary>
        /// Load I/O label.
        /// </summary>
        public void LoadLabel()
        {
            Label = "";
            Description = "";

            if (BitNo < 0)
            {
                return;
            }

            var item = _ioAPI.IOLabels.FirstOrDefault(label => label.GetKind() == IOKind && label.GetDataTypeName() == "Bit" && label.Port == BitNo);

            if (item == null)
            {
                return;
            }

            Label = item.Label;
            Description = item.Description;
        }

        /// <summary>
        /// Set IO bit on/off.
        /// </summary>
        /// <param name="on">On/Off</param>
        public void SetIOStat(bool on)
        {
            if (!RobotController.Instance.IsConnected)
            {
                return;
            }

            _ioAPI.IOWriteOutput<bool>(BitNo, on);
        }

        public void StartWatch()
        {
            EndWatch();

            if (BitNo < 0)
            {
                return;
            }

            var watcher = new IOWatcher();

            if (!watcher.Init(_ioAPI, IOKind, BitNo))
            {
                return;
            }

            _watcher = watcher;
            StatChangedAction?.Invoke();
            _watcher.ChangedAction = () => StatChangedAction?.Invoke();
        }

        public void EndWatch()
        {
            _watcher?.End();
            _watcher = null;
            StatChangedAction?.Invoke();
        }
    }
}
