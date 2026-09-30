// -----------------------------------------------------------------------
// <copyright file="GeneralConf.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.Text.Json;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// General configuration.
    /// </summary>
    internal class GeneralConf
    {
        /// <summary>
        /// Settings.
        /// </summary>
        public class Settings
        {
            public string LogName { get; set; } = "RuntimeLog";

            public ChkParams ChkParams { get; set; } = new ChkParams();

            public List<AdjParam> AdjParams { get; set; } = new List<AdjParam>();

            public string StartFunctionName { get; set; } = "main";

            public MainPanelLayout MainPanelLayout { get; set; } = new MainPanelLayout();

            public List<string> SelectedMotionGraphs { get; set; } = new List<string>()
            {
                "Velocity", "TCPSpeed",
            };

            public List<string> SelectedMotionColumns { get; set; } = new List<string>()
            {
                "Cycle", "Time", "SectionName", "Velocity", "TCPSpeed", "XYZ",
            };
        }

        /// <summary>
        /// Settings.
        /// </summary>
        public Settings Set => _settings;

        private Settings _settings = new Settings();

        private IRCXConfiguration? _configuration;
        private IRCXGeneralAPI _generalAPI;
        private IRCXProjectAPI _projectAPI;

        private string _filenameConf = "SpelAnalysisToolConf.json";

        /// <summary>
        /// Constructor.
        /// </summary>
        public GeneralConf()
        {
            _configuration = Main.Settings;
            _generalAPI = Main.GetAPI<IRCXGeneralAPI>();
            _projectAPI = Main.GetAPI<IRCXProjectAPI>();
            
            Load();

            _generalAPI.AddDataToCollect($"Conf/Layout/{string.Join(':', Set.MainPanelLayout.Items.Where(item => !item.IsHidden).Select(item => item.Name).ToList())}");
            _generalAPI.AddDataToCollect($"Conf/MotionGraph/{string.Join(':', Set.SelectedMotionGraphs)}");
            _generalAPI.AddDataToCollect($"Conf/MotionColumn/{string.Join(':', Set.SelectedMotionColumns)}");
        }

        /// <summary>
        /// Load settings from JSON string.
        /// </summary>
        /// <param name="json">JSON string.</param>
        public void FromJson(string json)
        {
            try
            {
                var ret = JsonSerializer.Deserialize<Settings>(json);

                if (ret != null)
                {
                    _settings = ret;
                }
            }
            catch (JsonException)
            {
            }
        }

        /// <summary>
        /// Settings to JSON string.
        /// </summary>
        /// <returns>JSON string.</returns>
        public string? ToJason()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                return json;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Load settings.
        /// </summary>
        public void Load()
        {
            if (_configuration == null) return;

            var buf = _configuration.Read();
            if (buf == null) return;
            if (buf.Length <= 0) return;

            FromJson(System.Text.Encoding.UTF8.GetString(buf));
            Set.ChkParams.SetParams();
            Set.MainPanelLayout.SetItems();

            // Save conf to project folder.
            var folder = _projectAPI.ProjectFolder;

            if (folder == null)
            {
                return;
            }

            var filepath = $"{folder}\\{_filenameConf}";

            if (!System.IO.File.Exists(filepath))
            {
                return;
            }

            var json = System.IO.File.ReadAllText(filepath);
            FromJson(json);
            Set.ChkParams.SetParams();
            Set.MainPanelLayout.SetItems();
        }

        /// <summary>
        /// Save settings.
        /// </summary>
        public void Save()
        {
            if (_configuration == null) return;

            var json = ToJason();
            if (string.IsNullOrEmpty(json)) return;

            _configuration.Write(System.Text.Encoding.UTF8.GetBytes(json));

            // Save conf to project folder.
            var folder = _projectAPI.ProjectFolder;

            if (folder == null)
            {
                return;
            }

            var filepath = $"{folder}\\{_filenameConf}";

            System.IO.File.WriteAllText(filepath, json);
        }
    }
}
