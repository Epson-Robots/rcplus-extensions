// -----------------------------------------------------------------------
// <copyright file="LogDataMotion.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Log data - Motion.
    /// </summary>
    public class LogDataMotion : LogData
    {
        /// <inheritdoc/>
        public override string SuffixName => "Motion";

        /// <summary>
        /// Motion record.
        /// </summary>
        public class Record
        {
            public string DateTime { get; set; } = "";
            public double RealElapsedTime { get; set; }
            public double ElapsedTime { get; set; }

            public int CycleNo { get; set; }
            public string SectionName { get; set; } = "";

            public double PosX { get; set; }
            public double PosY { get; set; }
            public double PosZ { get; set; }
            public double PosU { get; set; }
            public double PosV { get; set; }
            public double PosW { get; set; }

            public double[] Joint { get; } = new double[6];
            public double[] Torque { get; } = new double[6];

            public double X => PosX;
            public double Y => PosY;
            public double Z => PosZ;
            public double U => PosU;
            public double V => PosV;
            public double W => PosW;

            public double J1 => Joint[0];
            public double J2 => Joint[1];
            public double J3 => Joint[2];
            public double J4 => Joint[3];
            public double J5 => Joint[4];
            public double J6 => Joint[5];

            public double Torque1 => Torque[0];
            public double Torque2 => Torque[1];
            public double Torque3 => Torque[2];
            public double Torque4 => Torque[3];
            public double Torque5 => Torque[4];
            public double Torque6 => Torque[5];

            public double TCPSpeed { get; set; }

            public double Velocity { get; set; }

            private ChkParams chkParams => GeneralManager.Instance.Conf.Set.ChkParams;

            public bool IsVelocityNG => chkParams.Velocity.CheckNG(Velocity);
            public bool IsTCPSpeedNG => chkParams.TCPSpeed.CheckNG(TCPSpeed);
            public bool IsXNG => chkParams.X.CheckNG(X);
            public bool IsYNG => chkParams.Y.CheckNG(Y);
            public bool IsZNG => chkParams.Z.CheckNG(Z);
            public bool IsUNG => chkParams.U.CheckNG(U);
            public bool IsVNG => chkParams.V.CheckNG(V);
            public bool IsWNG => chkParams.W.CheckNG(W);
            public bool IsJ1NG => chkParams.J1.CheckNG(J1);
            public bool IsJ2NG => chkParams.J2.CheckNG(J2);
            public bool IsJ3NG => chkParams.J3.CheckNG(J3);
            public bool IsJ4NG => chkParams.J4.CheckNG(J4);
            public bool IsJ5NG => chkParams.J5.CheckNG(J5);
            public bool IsJ6NG => chkParams.J6.CheckNG(J6);
            public bool IsTorque1NG => chkParams.Torque1.CheckNG(Torque1);
            public bool IsTorque2NG => chkParams.Torque2.CheckNG(Torque2);
            public bool IsTorque3NG => chkParams.Torque3.CheckNG(Torque3);
            public bool IsTorque4NG => chkParams.Torque4.CheckNG(Torque4);
            public bool IsTorque5NG => chkParams.Torque5.CheckNG(Torque5);
            public bool IsTorque6NG => chkParams.Torque6.CheckNG(Torque6);

            public bool IsNG()
            {
                return IsVelocityNG || IsTCPSpeedNG ||
                    IsXNG || IsYNG || IsZNG || IsUNG || IsVNG || IsWNG ||
                    IsJ1NG || IsJ2NG || IsJ3NG || IsJ4NG || IsJ5NG || IsJ6NG ||
                    IsTorque1NG || IsTorque2NG || IsTorque3NG || IsTorque4NG || IsTorque5NG || IsTorque6NG;
            }

            public void SetVelocity(Record recBefore)
            {
                var d = Math.Sqrt(Math.Pow(PosX - recBefore.PosX, 2) + Math.Pow(PosY - recBefore.PosY, 2) + Math.Pow(PosZ - recBefore.PosZ, 2));
                var t = ElapsedTime - recBefore.ElapsedTime;

                if (t > 0)
                {
                    Velocity = d / t;
                }
            }
        }

        /// <summary>
        /// Cycle data.
        /// </summary>
        public class CycleData
        {
            public int CycleNo { get; set; }

            public double StartTime { get; set; }

            public double EndTime { get; set; }

            public double ElapsedTime => EndTime - StartTime;

            public List<Record> Records { get; } = new List<Record>();

            public LineGraphData GetLineGraphData(string propName)
            {
                var data = new LineGraphData();

                data.TotalTime = ElapsedTime;

                {
                    var type = typeof(Record);
                    var prop = type.GetProperty(propName);

                    if (prop != null)
                    {
                        Records.ForEach(rec =>
                        {
                            data.Datas.Add((rec.ElapsedTime, (double)prop.GetValue(rec)!));
                        });
                    }
                }

                {
                    var type = typeof(ChkParams);
                    var prop = type.GetProperty(propName);

                    if (prop != null && prop.GetValue(GeneralManager.Instance.Conf.Set.ChkParams) is ChkParam chkParam)
                    {
                        data.ChkMinEnabled = chkParam.IsEnabledMin;
                        data.ChkMaxEnabled = chkParam.IsEnabledMax;

                        data.ChkMin = chkParam.Min;
                        data.ChkMax = chkParam.Max;

                    }
                }

                return data;
            }
        }

        /// <summary>
        /// Cycles.
        /// </summary>
        public List<CycleData> Cycles { get; } = new List<CycleData>();

        /// <summary>
        /// Get motion records count.
        /// </summary>
        /// <returns></returns>
        public int GetMotionCnt()
        {
            return Cycles.Sum(m => m.Records.Count);
        }

        /// <inheritdoc/>
        public override void ResetData()
        {
            Cycles.Clear();
        }

        /// <summary>
        /// Get cycle data.
        /// </summary>
        /// <param name="cycleNo">Cycle no.</param>
        /// <returns>Cycle data.</returns>
        public CycleData? GetCycleData(int cycleNo)
        {
            return Cycles.FirstOrDefault(c => c.CycleNo == cycleNo);
        }

        /// <summary>
        /// Get records.
        /// </summary>
        /// <param name="cycleNo">Cycle no.</param>
        /// <param name="sectionName">Section name.</param>
        /// <returns>Records.</returns>
        public List<Record> GetSelectedRecords(int cycleNo = 0, string sectionName = "")
        {
            List<Record> records = new List<Record>();

            foreach (var cycle in Cycles)
            {
                if (cycleNo == 0 || cycleNo == cycle.CycleNo)
                {
                    records.AddRange(cycle.Records);
                }
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                return records;
            }

            return records.Where(rec => rec.SectionName == sectionName).ToList();
        }

        /// <summary>
        /// Get grid format motion data.
        /// </summary>
        /// <returns>Grid format motion data.</returns>
        public List<List<string>> GetGridFormatMotionData()
        {
            var list = new List<List<string>>();

            var header = "Cycle,Time,DateTime,SectionName,Velocity,TCPSpeed,X,Y,Z,U,V,W,J1,J2,J3,J4,J5,J6,Torque1,Torque2,Torque3,Torque4,Torque5,Torque6";
            list.Add(header.Split(",").ToList());

            var dat = GetSelectedRecords();

            dat.ForEach(rec =>
            {
                var l = new List<string>();
                l.Add($"{rec.CycleNo}");
                l.Add($"{rec.ElapsedTime:0.000}");
                l.Add(rec.DateTime);
                l.Add(rec.SectionName);
                l.Add($"{rec.Velocity:0.000}");
                l.Add($"{rec.TCPSpeed:0.000}");
                l.Add($"{rec.X:0.000}");
                l.Add($"{rec.Y:0.000}");
                l.Add($"{rec.Z:0.000}");
                l.Add($"{rec.U:0.000}");
                l.Add($"{rec.V:0.000}");
                l.Add($"{rec.W:0.000}");

                for (var i = 0; i < rec.Joint.Length; i++)
                {
                    l.Add($"{rec.Joint[i]:0.000}");
                }

                for (var i = 0; i < rec.Torque.Length; i++)
                {
                    l.Add($"{rec.Torque[i]:0.000}");
                }

                list.Add(l);
            });

            return list;
        }

        /// <inheritdoc/>
        public override void LoadData(List<string[]> lines, int limitCycleNo = 0)
        {
            foreach (var words in lines)
            {
                // 0: DateTime,
                // 1: ElapsedTime,
                // 2: CycleNo,
                // 3: SectionName,
                // 4: X,
                // 5: Y,
                // 6: Z,
                // 7: U,
                // 8: V,
                // 9: W,
                // 10: J1,
                // 11: J2,
                // 12: J3,
                // 13: J4,
                // 14: J5,
                // 15: J6,
                // 16: Torque1,
                // 17: Torque2,
                // 18: Torque3,
                // 19: Torque4,
                // 20: Torque5,
                // 21: Torque6,
                // 22: TCPSpeed
                if (words.Length < 23) continue;
                var elapsedTime = double.Parse(words[1]);
                var cycleNo = int.Parse(words[2]);
                var sectionName = words[3];

                if (limitCycleNo > 0 && cycleNo > limitCycleNo)
                {
                    break;
                }

                var record = new Record()
                {
                    RealElapsedTime = elapsedTime,
                    CycleNo = cycleNo,
                    SectionName = words[3],
                    DateTime = words[0],
                    PosX = double.Parse(words[4]),
                    PosY = double.Parse(words[5]),
                    PosZ = double.Parse(words[6]),
                    PosU = double.Parse(words[7]),
                    PosV = double.Parse(words[8]),
                    PosW = double.Parse(words[9]),
                    TCPSpeed = double.Parse(words[22]),
                };

                for (var i = 0; i < 6; i++)
                {
                    record.Joint[i] = double.Parse(words[10 + i]);
                    record.Torque[i] = double.Parse(words[16 + i]);
                }

                var target = Cycles.FirstOrDefault(rec => rec.CycleNo == cycleNo);

                if (target == null)
                {
                    var startTime = elapsedTime;
                    var endTime = elapsedTime;

                    var secCycle = GeneralManager.Instance.LogDatSection.CycleRecords.FirstOrDefault(c => c.CycleNo == cycleNo);

                    if (secCycle != null)
                    {
                        startTime = secCycle.StartTime;
                        endTime = secCycle.EndTime;
                    }

                    target = new CycleData()
                    {
                        CycleNo = cycleNo,
                        StartTime = startTime,
                        EndTime = endTime,
                    };

                    Cycles.Add(target);
                }

                record.ElapsedTime = elapsedTime - target.StartTime;

                if (target.Records.Count > 0)
                {
                    record.SetVelocity(target.Records.Last());
                }

                if (elapsedTime > target.EndTime)
                {
                    target.EndTime = elapsedTime;
                }

                target.Records.Add(record);
            }
        }
    }
}
