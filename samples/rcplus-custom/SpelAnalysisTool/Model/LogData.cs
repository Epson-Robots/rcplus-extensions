// -----------------------------------------------------------------------
// <copyright file="LogData.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Log data base class.
    /// </summary>
    public abstract class LogData
    {
        /// <summary>
        /// CSV file name suffix.
        /// </summary>
        public abstract string SuffixName { get; }

        /// <summary>
        /// Reset data.
        /// </summary>
        public virtual void ResetData()
        {
        }

        /// <summary>
        /// Load data.
        /// </summary>
        /// <param name="lines">Lines.</param>
        /// <param name="limitCycleNo">Limit cycle no for loading. if its zero, no limit.</param>
        public virtual void LoadData(List<string[]> lines, int limitCycleNo = 0)
        {
        }

        /// <summary>
        /// Load CSV file.
        /// </summary>
        /// <param name="folder">Folder path.</param>
        /// <param name="logname">Log name.</param>
        /// <param name="limitCycleNo">Limit cycle no for loading. if its zero, no limit.</param>
        public void LoadCSV(string folder, string logname, int limitCycleNo = 0)
        {
            ResetData();

            var filepath = $"{folder}\\{logname}_{SuffixName}.csv";
            if (!System.IO.File.Exists(filepath)) return;

            using var fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
            using var reader = new System.IO.StreamReader(fs);
            
            var lines = new List<string[]>();
            var first = true;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (first)
                {
                    first = false;
                    continue;
                }

                if (line == null) continue;

                lines.Add(line.Split(","));
            }

            LoadData(lines, limitCycleNo);
        }
    }
}
