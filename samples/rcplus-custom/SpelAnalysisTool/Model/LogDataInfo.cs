// -----------------------------------------------------------------------
// <copyright file="LogDataInfo.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Log data - Information.
    /// </summary>
    public class LogDataInfo : LogData
    {
        /// <inheritdoc/>
        public override string SuffixName => "Info";

        /// <summary>
        /// Name-value pair datas.
        /// </summary>
        public List<(string Name, string Value)> Datas = new List<(string Name, string Value)> ();

        /// <inheritdoc/>
        public override void ResetData()
        {
            Datas.Clear();
        }

        /// <inheritdoc/>
        public override void LoadData(List<string[]> lines, int limitCycleNo = 0)
        {
            foreach (var words in lines)
            {
                // 0: Name,
                // 1: Value,
                if (words.Length < 2) continue;

                Datas.Add((words[0], words[1]));
            }
        }

        /// <summary>
        /// Get value.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Value.</returns>
        public string GetValue(string name)
        {
            return Datas.Any(d => d.Name == name) ? Datas.FirstOrDefault(d => d.Name == name).Value : "";
        }

        /// <summary>
        /// Get value as integer.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Value.</returns>
        public int GetValueInt(string name)
        {
            var ret = 0;
            var str = GetValue(name);
            int.TryParse(str, out ret);

            return ret;
        }

        /// <summary>
        /// Get sammary.
        /// </summary>
        /// <returns>Sammary.</returns>
        public string GetSammary()
        {
            var ret = "";

            Datas.ForEach(d =>
            {
                if (d.Name.StartsWith("TL"))
                {
                    ret += $"{d.Name,-5} {d.Value.Trim()}\n";
                }
                else
                {
                    ret += $"{d.Name,-14} {d.Value.Trim()}\n";
                }
            });

            return ret;
        }
    }
}
