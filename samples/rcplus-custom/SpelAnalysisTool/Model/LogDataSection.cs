// -----------------------------------------------------------------------
// <copyright file="LogDataSection.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Log data - Section.
    /// </summary>
    public class LogDataSection : LogData
    {
        /// <inheritdoc/>
        public override string SuffixName => "Section";

        /// <summary>
        /// Cycle record.
        /// </summary>
        public class CycleRecord
        {
            public int CycleNo { get; set; }

            public double StartTime { get; set; }

            public double EndTime { get; set; }

            public double ElapsedTime => EndTime - StartTime;
        }

        /// <summary>
        /// Section record.
        /// </summary>
        public class SectionRecord
        {
            public int CycleNo { get; set; }

            public string SectionName { get; set; } = "";

            public double StartTime { get; set; }

            public double EndTime { get; set; }

            public double ElapsedTime => EndTime - StartTime;
        }

        /// <summary>
        /// Sammary record.
        /// </summary>
        public class SammaryRecord
        {
            public string SectionName { get; set; } = "";

            public int Count { get; set; }

            public double Total { get; set; }

            public double Mean => Total / Count;

            public double StdDev { get; set; }

            public double Range => Max - Min;

            public double Min { get; set; }

            public double Max { get; set; }

            public double Percentage { get; set; }

            public double StartTime { get; set; }

            public double EndTime { get; set; }

            public List<double> Datas = new List<double>();

            public UserControl? StatGraph { get; set; }

            public UserControl? TimelineBar { get; set; }
        }

        /// <summary>
        /// Cycle records.
        /// </summary>
        public List<CycleRecord> CycleRecords { get; } = new List<CycleRecord>();

        /// <summary>
        /// Section records.
        /// </summary>
        public List<SectionRecord> SectionRecords { get; } = new List<SectionRecord>();

        /// <summary>
        /// Sammary records.
        /// </summary>
        public List<SammaryRecord> SammaryRecords { get; } = new List<SammaryRecord>();

        /// <inheritdoc/>
        public override void ResetData()
        {
            CycleRecords.Clear();
            SectionRecords.Clear();
            SammaryRecords.Clear();
        }

        /// <summary>
        /// Get cycle count.
        /// </summary>
        /// <returns>Cycle count.</returns>
        public int GetCycleCnt()
        {
            return CycleRecords.Count;
        }

        /// <summary>
        /// Get section count.
        /// </summary>
        /// <returns>Section count.</returns>
        public int GetSectionCnt()
        {
            return SammaryRecords.Count(s => !s.SectionName.StartsWith("*"));
        }

        /// <summary>
        /// Get grid format cycle data.
        /// </summary>
        /// <returns>Grid format cycle data.</returns>
        public List<List<string>> GetGridFormatCycleData()
        {
            var list = new List<List<string>>();

            var header = "Cycle";
            var headerList = header.Split(",").ToList();

            SammaryRecords.ForEach(sam =>
            {
                headerList.Add(sam.SectionName);
            });
            
            list.Add(headerList);

            var count = GetCycleCnt();

            for (var i = 1; i <= count; i++)
            {
                var l = new List<string>();

                l.Add($"{i}");
                var total = SectionRecords.Where(rec => rec.CycleNo == i).Sum(rec => rec.ElapsedTime);
                l.Add($"{total:0.000}");

                if (total > 0)
                {
                    SammaryRecords.ForEach(sam =>
                    {
                        var target = SectionRecords.FirstOrDefault(rec => rec.CycleNo == i && rec.SectionName == sam.SectionName);

                        if (target != null)
                        {
                            l.Add($"{target.ElapsedTime:0.000}");
                        }
                    });
                }

                list.Add(l);
            }

            return list;
        }

        /// <summary>
        /// Get grid format sammary data.
        /// </summary>
        /// <returns>Grid format sammary data.</returns>
        public List<List<string>> GetGridFormatSammaryData()
        {
            var list = new List<List<string>>();

            var header = "SectionName,pct.,StdDev,Mean,Min,Max,Range";
            list.Add(header.Split(",").ToList());

            SammaryRecords.ForEach(sam =>
            {
                var l = new List<string>();
                l.Add(sam.SectionName);
                l.Add($"{sam.Percentage:0.0}");
                l.Add($"{sam.StdDev:0.000}");
                l.Add($"{sam.Mean:0.000}");
                l.Add($"{sam.Min:0.000}");
                l.Add($"{sam.Max:0.000}");
                l.Add($"{sam.Range:0.000}");

                list.Add(l);
            });

            return list;
        }

        /// <inheritdoc/>
        public override void LoadData(List<string[]> lines, int limitCycleNo = 0)
        {
            foreach (var words in lines)
            {
                // DateTime,ElapsedTime,CycleNo,SectionName,Flag
                if (words.Length < 5) continue;
                var elapsedTime = double.Parse(words[1]);
                var cycleNo = int.Parse(words[2]);
                var sectionName = words[3];
                var flag = words[4];

                if (limitCycleNo > 0 && cycleNo > limitCycleNo)
                {
                    break;
                }

                if (flag == "CycleStart")
                {
                    CycleRecords.Add(new CycleRecord()
                    {
                        CycleNo = cycleNo,
                        StartTime = elapsedTime,
                    });
                }
                else if (flag == "CycleEnd")
                {
                    var target = CycleRecords.FirstOrDefault(rec => rec.CycleNo == cycleNo);

                    if (target != null)
                    {
                        target.EndTime = elapsedTime;
                    }
                }
                else if (flag == "SectionStart")
                {
                    SectionRecords.Add(new SectionRecord()
                    {
                        CycleNo = cycleNo,
                        SectionName = sectionName,
                        StartTime = elapsedTime,
                    });
                }
                else if (flag == "SectionEnd")
                {
                    var target = SectionRecords.FirstOrDefault(rec => rec.CycleNo == cycleNo && rec.SectionName == sectionName);

                    if (target != null)
                    {
                        target.EndTime = elapsedTime;

                        var sam = SammaryRecords.FirstOrDefault(sam => sam.SectionName == sectionName);

                        if (sam == null)
                        {
                            SammaryRecords.Add(new SammaryRecord()
                            {
                                SectionName = sectionName,
                                Count = 1,
                                Total = target.ElapsedTime,
                                Min = target.ElapsedTime,
                                Max = target.ElapsedTime,
                            });
                        }
                        else
                        {
                            sam.Count += 1;
                            sam.Total += target.ElapsedTime;
                            sam.Min = Math.Min(sam.Min, target.ElapsedTime);
                            sam.Max = Math.Max(sam.Max, target.ElapsedTime);
                        }
                    }
                }
            }

            // Calcurate starndard deviation.
            foreach (var sam in SammaryRecords)
            {
                var tmpVal = 0.0;

                foreach (var rec in SectionRecords)
                {
                    if (rec.SectionName == sam.SectionName)
                    {
                        sam.Datas.Add(rec.ElapsedTime);
                        tmpVal += Math.Pow(rec.ElapsedTime - sam.Mean, 2);
                    }
                }

                sam.StdDev = Math.Sqrt(tmpVal / sam.Count);
            }

            var cycleCnt = GetCycleCnt();
            if (cycleCnt <= 0)
            {
                return;
            }

            // Calcurate percentage.
            var sum = CycleRecords.Average(cycle => cycle.ElapsedTime);
            if (sum > 0)
            {
                foreach (var sam in SammaryRecords)
                {
                    sam.Percentage = sam.Mean * 100 / sum;
                    var targetRecords = SectionRecords.Where(rec => rec.SectionName == sam.SectionName).ToList();

                    var cnt = 0;
                    var sumStart = 0.0;
                    var sumEnd = 0.0;

                    foreach (var sec in targetRecords)
                    {
                        var cycle = CycleRecords.FirstOrDefault(c => c.CycleNo == sec.CycleNo);

                        if (cycle == null)
                        {
                            continue;
                        }

                        sumStart += sec.StartTime - cycle.StartTime;
                        sumEnd += sec.EndTime - cycle.StartTime;
                        cnt++;
                    }

                    if (cnt > 0)
                    {
                        sam.StartTime = sumStart / cnt;
                        sam.EndTime = sumEnd / cnt;
                    }
                }
            }

            // Create total sammary.
            {
                var total = new SammaryRecord();
                total.SectionName = "*Total";
                total.Datas = CycleRecords.Select(cycle => cycle.ElapsedTime).ToList();
                total.Count = total.Datas.Count;
                total.Total = total.Datas.Sum();
                total.Min = total.Datas.Min();
                total.Max = total.Datas.Max();
                total.StdDev = Math.Sqrt(total.Datas.Select(c => Math.Pow(c - total.Mean, 2)).Sum() / total.Datas.Count);
                total.Percentage = 100.0;
                total.StartTime = 0.0;
                total.EndTime = total.Mean;

                SammaryRecords.Insert(0, total);
            }
        }

        /// <summary>
        /// Check last cycle no.
        /// </summary>
        /// <param name="filepath">File path.</param>
        /// <returns>Last cycle no.</returns>
        public int CheckEndCycleNo(string filepath)
        {
            var ret = 0;

            if (!System.IO.File.Exists(filepath))
            {
                return ret;
            }

            using (var fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                var first = true;

                using (var reader = new System.IO.StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (first)
                        {
                            first = false;
                            continue;
                        }

                        if (line == null) continue;

                        // DateTime,ElapsedTime,CycleNo,SectionName,Flag
                        var words = line.Split(",");
                        if (words.Length < 5) continue;
                        var elapsedTime = double.Parse(words[1]);
                        var cycleNo = int.Parse(words[2]);
                        var sectionName = words[3];
                        var flag = words[4];

                        if (flag == "CycleEnd")
                        {
                            if (cycleNo > ret)
                            {
                                ret = cycleNo;
                            }
                        }
                    }
                }
            }

            return ret;
        }
    }
}
