// -----------------------------------------------------------------------
// <copyright file="GeneralConf.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Epson.RoboticsShared.ExtensionsAPI;
using static System.Net.Mime.MediaTypeNames;

namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// Configuration.
    /// </summary>
    internal class GeneralConf
    {
        /// <summary>
        /// Configuration item class.
        /// </summary>
        public class ConfItem
        {
            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// Integer values
            /// </summary>
            public List<int> Values { get; } = new List<int>();

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="strValues">Comma separated values string</param>
            public ConfItem(string strValues)
            {
                var words = strValues.Split(',');

                foreach (var word in words)
                {
                    Values.Add(int.Parse(word));
                }
            }

            /// <summary>
            /// Set values by comma separated values string.
            /// </summary>
            /// <param name="strValues">Comma separated values string</param>
            public void SetValues(string strValues)
            {
                var words = strValues.Split(",");

                for (var i = 0; i < Values.Count; i++)
                {
                    if (i < words.Length && int.TryParse(words[i], out var res))
                    {
                        Values[i] = res;
                    }
                }
            }

            /// <summary>
            /// Get comma separated values string.
            /// </summary>
            /// <returns>Comma separated values string</returns>
            public string GetValuesStr()
            {
                return string.Join(",", Values);
            }

            /// <summary>
            /// Set same value to all values.
            /// </summary>
            /// <param name="val">Value</param>
            public void SetAll(int val)
            {
                for (var i = 0; i < Values.Count; i++)
                {
                    Values[i] = val;
                }
            }
        }

        /// <summary>
        /// Version
        /// </summary>
        public ConfItem Version { get; set; } = new ConfItem("1");

        /// <summary>
        /// GUIMode : GUI display mode.
        /// 0:Whole 1:JogOnly
        /// </summary>
        public ConfItem GUIMode { get; } = new ConfItem("0");

        /// <summary>
        /// Default of JogLayout.
        /// </summary>
        public static string DefJogLayout = "1,2,3,4,5";

        /// <summary>
        /// JogLayout : Jog panel layout.
        /// 0:None 1:IOIn 2:IOOut 3:JointJog 4:WorldJog 5:ToolJog
        /// </summary>
        public ConfItem JogLayout { get; } = new ConfItem(DefJogLayout);

        /// <summary>
        /// IOBitsIn : List of input I/O bits to dispaly.
        /// </summary>
        public ConfItem IOBitsIn { get; } = new ConfItem("0,1,2,3,4,5,6,7");

        /// <summary>
        /// IOBitsOut : List of output I/O bits to dispaly.
        /// </summary>
        public ConfItem IOBitsOut { get; } = new ConfItem("0,1,2,3,4,5,6,7");

        private IRCXGeneralAPI _generalAPI;
        private IRCXConfiguration? _configuration;
        private List<ConfItem> _confItems = new List<ConfItem>();

        /// <summary>
        /// Constructor of GeneralConf.
        /// </summary>
        public GeneralConf()
        {
            _generalAPI = Main.GetAPI<IRCXGeneralAPI>();
            _configuration = Main.Settings;

            foreach (var prop in GetType().GetProperties()
                .Where(x => x.PropertyType == typeof(ConfItem)))
            {
                var item = (ConfItem?)prop.GetValue(this);
                if (item == null) continue;
                item.Name = prop.Name;
                _confItems.Add(item);
            }

            Load();

            _confItems.ForEach(item =>
            {
                _generalAPI.AddDataToCollect($"Conf/{item.Name}/{item.GetValuesStr()}");
            });
        }

        /// <summary>
        /// Load configuration.
        /// </summary>
        public void Load()
        {
            if (_configuration == null) return;

            var buf = _configuration.Read();

            if (buf == null)
            {
                return;
            }

            var str = System.Text.Encoding.UTF8.GetString(buf);
            var lines = str.Split('\n');

            foreach (var line in lines)
            {
                var words = line.Split("=");

                if (words.Length != 2)
                {
                    continue;
                }

                var item = _confItems.FirstOrDefault(x => x.Name == words[0]);

                if (item == null)
                {
                    continue;
                }

                item.SetValues(words[1]);
            }
        }

        /// <summary>
        /// Save configuration.
        /// </summary>
        public void Save()
        {
            if (_configuration == null) return;

            var str = "";

            foreach (var item in _confItems)
            {
                if (str != "")
                {
                    str += "\n";
                }

                str += $"{item.Name}={item.GetValuesStr()}";
            }

            _configuration.Write(System.Text.Encoding.UTF8.GetBytes(str));
        }
    }
}
