// -----------------------------------------------------------------------
// <copyright file="ChkParams.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Check parameters.
    /// </summary>
    internal class ChkParams
    {
        public ChkParam CycleTime { get; set; } = new ChkParam("sec", false, 0, true, 30);
        public ChkParam Velocity { get; set; } = new ChkParam("mm/sec", false, 0, true, 10);
        public ChkParam TCPSpeed { get; set; } = new ChkParam("mm/sec", false, 0, true, 10);

        public ChkParam X { get; set; } = new ChkParam("mm", false, -100, false, 100);
        public ChkParam Y { get; set; } = new ChkParam("mm", false, -100, false, 100);
        public ChkParam Z { get; set; } = new ChkParam("mm", false, -100, false, 100);
        public ChkParam U { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam V { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam W { get; set; } = new ChkParam("deg", false, -10, false, 10);

        public ChkParam J1 { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam J2 { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam J3 { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam J4 { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam J5 { get; set; } = new ChkParam("deg", false, -10, false, 10);
        public ChkParam J6 { get; set; } = new ChkParam("deg", false, -10, false, 10);

        public ChkParam Torque1 { get; set; } = new ChkParam("", false, -0.5, false, 0.5);
        public ChkParam Torque2 { get; set; } = new ChkParam("", false, -0.5, false, 0.5);
        public ChkParam Torque3 { get; set; } = new ChkParam("", false, -0.5, false, 0.5);
        public ChkParam Torque4 { get; set; } = new ChkParam("", false, -0.5, false, 0.5);
        public ChkParam Torque5 { get; set; } = new ChkParam("", false, -0.5, false, 0.5);
        public ChkParam Torque6 { get; set; } = new ChkParam("", false, -0.5, false, 0.5);

        [JsonIgnore]
        public List<ChkParam> Params { get; } = new List<ChkParam>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ChkParams()
        {
            SetParams();
        }

        /// <summary>
        /// Set properties to Params.
        /// </summary>
        public void SetParams()
        {
            Params.Clear();

            foreach (var prop in GetType().GetProperties().Where(x => x.PropertyType == typeof(ChkParam)))
            {
                var item = (ChkParam?)prop.GetValue(this);
                if (item == null) continue;
                item.Name = prop.Name;
                Params.Add(item);
            }
        }

        /// <summary>
        /// Update status.
        /// </summary>
        public void UpdateStat()
        {
            Params.ForEach(p => p.ClearStat());

            var sec = GeneralManager.Instance.LogDatSection;
            var motion = GeneralManager.Instance.LogDatMotion;
            var total = sec.SammaryRecords.FirstOrDefault(sam => sam.SectionName.StartsWith("*"));
            var recM = motion.GetSelectedRecords();

            if (total != null)
            {
                CycleTime.UpdateStat(total.Datas);
            }

            var sigmas = new List<double>();

            sec.SammaryRecords.ForEach(sam =>
            {
                sam.Datas.ForEach(d =>
                {
                    if (sam.StdDev > 0)
                    {
                        sigmas.Add(Math.Abs((d - sam.Mean) / sam.StdDev));
                    }
                });
            });

            Action<ChkParam, Func<LogDataMotion.Record, double>> chk = (c, f) =>
            {
                c.UpdateStat(recM.Select(rec => f(rec)).ToList());
            };

            chk(Velocity, rec => rec.Velocity);
            chk(TCPSpeed, rec => rec.TCPSpeed);

            chk(X, rec => rec.X);
            chk(Y, rec => rec.Y);
            chk(Z, rec => rec.Z);
            chk(U, rec => rec.U);
            chk(V, rec => rec.V);
            chk(W, rec => rec.W);

            chk(J1, rec => rec.J1);
            chk(J2, rec => rec.J2);
            chk(J3, rec => rec.J3);
            chk(J4, rec => rec.J4);
            chk(J5, rec => rec.J5);
            chk(J6, rec => rec.J6);
        }
    }
}
