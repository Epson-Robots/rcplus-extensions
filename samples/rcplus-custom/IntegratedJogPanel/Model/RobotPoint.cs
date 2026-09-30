// -----------------------------------------------------------------------
// <copyright file="RobotPoint.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// Robot point.
    /// </summary>
    public class RobotPoint
    {
        /// <summary>Number</summary>
        public int Number { get; set; }

        /// <summary>Label</summary>
        public string Label { get; set; } = "";

        /// <summary>Description</summary>
        public string Description { get; set; } = "";

        /// <summary>X</summary>
        public double X { get; set; }

        /// <summary>Y</summary>
        public double Y { get; set; }

        /// <summary>Z</summary>
        public double Z { get; set; }

        /// <summary>U</summary>
        public double U { get; set; }

        /// <summary>V</summary>
        public double V { get; set; }

        /// <summary>W</summary>
        public double W { get; set; }

        /// <summary>R</summary>
        public double R { get; set; }

        /// <summary>S</summary>
        public double S { get; set; }

        /// <summary>T</summary>
        public double T { get; set; }

        /// <summary>Hand</summary>
        public int Hand { get; set; }

        /// <summary>Elbow</summary>
        public int Elbow { get; set; }

        /// <summary>Wrist</summary>
        public int Wrist { get; set; }

        /// <summary>J1Flag</summary>
        public int J1Flag { get; set; }

        /// <summary>J2Flag</summary>
        public int J2Flag { get; set; }

        /// <summary>J4Flag</summary>
        public int J4Flag { get; set; }

        /// <summary>J6Flag</summary>
        public int J6Flag { get; set; }

        /// <summary>J1Angle</summary>
        public double J1Angle { get; set; }

        /// <summary>J4Angle</summary>
        public double J4Angle { get; set; }

        private static Dictionary<int, string> _flgHand = new Dictionary<int, string>()
        {
            { 0, "" },
            { 1, "/R" },
            { 2, "/L" },
        };

        private static Dictionary<int, string> _flgElbow = new Dictionary<int, string>()
        {
            { 0, "" },
            { 1, "/A" },
            { 2, "/B" },
        };

        private static Dictionary<int, string> _flgWrist = new Dictionary<int, string>()
        {
            { 0, "" },
            { 1, "/NF" },
            { 2, "/F" },
        };

        public string PtFlags => $"{_flgHand[Hand]} {_flgElbow[Elbow]} {_flgWrist[Wrist]} /J1F{J1Flag} /J2F{J2Flag} /J4F{J4Flag} /J6F{J6Flag}";

        /// <summary>
        /// Set position data.
        /// </summary>
        /// <param name="controller">RobotController</param>
        /// <returns>Whether set or not</returns>
        public async Task<bool> SetPos(RobotController controller)
        {
            if (!controller.IsConnected)
            {
                return false;
            }

            if (!await controller.GetCurrPos())
            {
                return false;
            }

            X = controller.PosX;
            Y = controller.PosY;
            Z = controller.PosZ;
            U = controller.PosU;
            V = controller.PosV;
            W = controller.PosW;

            Hand = controller.Hand;
            Elbow = controller.Elbow;
            Wrist = controller.Wrist;
            J1Flag = controller.J1Flag;
            J2Flag = controller.J2Flag;
            J4Flag = controller.J4Flag;
            J6Flag = controller.J6Flag;

            return true;
        }

        /// <summary>
        /// Get point text.
        /// </summary>
        /// <returns>Point text</returns>
        public string GetPtTxt()
        {
            return $"XY({X:0.000},{Y:0.000},{Z:0.000},{U:0.000},{V:0.000},{W:0.000}) {PtFlags}";
        }

        /// <summary>
        /// Get clipboard text to paste to RC+ point editor.
        /// </summary>
        /// <returns>Clipboard text to paste to RC+ point editor</returns>
        public string GetRCPlusPtEditorTxt()
        {
            return $"{Number}\t{Label}\t{X:0.000}\t{Y:0.000}\t{Z:0.000}\t{U:0.000}\t{V:0.000}\t{W:0.000}\t{R:0.000}\t{S:0.000}\t{T:0.000}\t0\t{Hand}\t{Elbow}\t{Wrist}\t{J1Flag}\t{J2Flag}\t{J4Flag}\t{J6Flag}\t{J1Angle:0.000}\t{J4Angle:0.000}\t{Description}";
        }

        /// <summary>
        /// Set point by clipboard text data copied at RC+ point editor.
        /// </summary>
        /// <param name="txt">Clipboard text data copied at RC+ point editor</param>
        /// <returns>Whether set or not</returns>
        public bool SetByRCPlusPtEditorTxt(string txt)
        {
            var words = txt.Split('\t');

            if (words.Length < 22)
            {
                return false;
            }

            var ret = false;

            try
            {
                Number = int.Parse(words[0]);
                Label = words[1];
                X = double.Parse(words[2]);
                Y = double.Parse(words[3]);
                Z = double.Parse(words[4]);
                U = double.Parse(words[5]);
                V = double.Parse(words[6]);
                W = double.Parse(words[7]);
                R = double.Parse(words[8]);
                S = double.Parse(words[9]);
                T = double.Parse(words[10]);
                // Local = int.Parse(words[11]);
                Hand = int.Parse(words[12]);
                Elbow = int.Parse(words[13]);
                Wrist = int.Parse(words[14]);
                J1Flag = int.Parse(words[15]);
                J2Flag = int.Parse(words[16]);
                J4Flag = int.Parse(words[17]);
                J6Flag = int.Parse(words[18]);
                J1Angle = double.Parse(words[19]);
                J4Angle = double.Parse(words[20]);
                Description = words[21];
                ret = true;
            }
            catch
            {
            }

            return ret;
        }
    }
}
