// -----------------------------------------------------------------------
// <copyright file="ViewDirection.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IntegratedJogPanel.Common
{
    public static class ViewDirection
    {
        /// <summary>
        /// View direction option
        /// </summary>
        public enum Option
        {
            /// <summary>Front view</summary>
            FrontView = 0,

            /// <summary>Right side view</summary>
            RightSideView = 1,

            /// <summary>Rear view</summary>
            RearView = 2,

            /// <summary>Left side view</summary>
            LeftSideView = 3,

            /// <summary>Top view, X is left</summary>
            TopViewXLeft = 4,

            /// <summary>Top view, X is bottom</summary>
            TopViewXBottom = 5,

            /// <summary>Top view, X is right</summary>
            TopViewXRight = 6,

            /// <summary>Top view, X is top</summary>
            TopViewXTop = 7,
        }

        /// <summary>
        /// Get axes labels.
        /// </summary>
        /// <param name="viewDir">View direction</param>
        /// <returns>Button label array</returns>
        public static string[] GetAxesLabels(Option viewDir)
        {
            //  3
            // 0 2
            //  1
            return (viewDir switch
            {
                Option.FrontView =>
                                        "X,Y, , ",
                Option.RightSideView =>
                                        " ,X,Y, ",
                Option.RearView =>
                                        " , ,X,Y",
                Option.LeftSideView =>
                                        "Y, , ,X",
                Option.TopViewXLeft =>
                                        "X, , ,Z",
                Option.TopViewXBottom =>
                                        " , ,Y,Z",
                Option.TopViewXRight =>
                                        " , ,X,Z",
                Option.TopViewXTop =>
                                        "Y, , ,Z",
                _ => "X,Y, , ",
            }).Split(",");
        }

        /// <summary>
        /// Get jog button labels.
        /// </summary>
        /// <param name="viewDir">View direction</param>
        /// <returns>Button 6 labels array</returns>
        public static string[] GetJogBtnLabels(Option viewDir)
        {
            //  1  4
            // 0 3 
            //  2  5
            return (viewDir switch
            {
                Option.FrontView =>
                                        "X+,Y-,Y+,X-,Z+,Z-",
                Option.RightSideView =>
                                        "Y-,X-,X+,Y+,Z+,Z-",
                Option.RearView =>
                                        "X-,Y+,Y-,X+,Z+,Z-",
                Option.LeftSideView =>
                                        "Y+,X+,X-,Y-,Z+,Z-",
                Option.TopViewXLeft =>
                                        "X+,Z+,Z-,X-,Y-,Y+",
                Option.TopViewXBottom =>
                                        "Y-,Z+,Z-,Y+,X-,X+",
                Option.TopViewXRight =>
                                        "X-,Z+,Z-,X+,Y+,Y-",
                Option.TopViewXTop =>
                                        "Y+,Z+,Z-,Y-,X+,X-",
                _ => "X+,Y-,Y+,X-,Z+,Z-",
            }).Split(",");
        }

        /// <summary>
        /// Convert 2D point to 3D point.
        /// </summary>
        /// <param name="viewDir">View direction</param>
        /// <param name="x">2D point X</param>
        /// <param name="y">2D point Y</param>
        /// <returns>3D point X, Y, Z</returns>
        public static (double X, double Y, double Z) Conv2Dto3D(Option viewDir, double x, double y)
        {
            return viewDir switch
            {
                Option.FrontView => (-x, y, 0),
                Option.RightSideView => (y, x, 0),
                Option.RearView => (x, -y, 0),
                Option.LeftSideView => (-y, -x, 0),
                Option.TopViewXLeft => (-x, 0, -y),
                Option.TopViewXBottom => (0, x, -y),
                Option.TopViewXRight => (x, 0, -y),
                Option.TopViewXTop => (0, -x, -y),
                _ => (-x, y, 0),
            };
        }

        /// <summary>
        /// Convert 3D point to 2D point.
        /// </summary>
        /// <param name="viewDir">View direction</param>
        /// <param name="x">3D point X</param>
        /// <param name="y">3D point Y</param>
        /// <param name="z">3D point Z</param>
        /// <returns>2D point X, Y, Depth</returns>
        public static (double X, double Y, double Depth) Conv3Dto2D(Option viewDir, double x, double y, double z)
        {
            return viewDir switch
            {
                Option.FrontView => (-x, y, z),
                Option.RightSideView => (y, x, z),
                Option.RearView => (x, -y, z),
                Option.LeftSideView => (-y, -x, z),
                Option.TopViewXLeft => (-x, -z, y),
                Option.TopViewXBottom => (y, -z, x),
                Option.TopViewXRight => (x, -z, -y),
                Option.TopViewXTop => (-y, -z, -x),
                _ => (-x, y, z),
            };
        }

        /// <summary>
        /// Convert 3D angle to 2D angle.
        /// </summary>
        /// <param name="viewDir">View direction</param>
        /// <param name="u">3D angle U</param>
        /// <param name="v">3D angle V</param>
        /// <param name="w">3D angle W</param>
        /// <returns>(2D angle, Dterminate)</returns>
        public static (double Angle, bool Determinate) Conv3Dto2DAngle(Option viewDir, double u, double v, double w)
        {
            var thetaU = u * Math.PI / 180;
            var thetaV = v * Math.PI / 180;
            var thetaW = w * Math.PI / 180;

            var dx = 1.0;
            var dy = 0.0;
            var dz = 0.0;

            {
                var tmpy = dy * Math.Cos(thetaW) - dz * Math.Sin(thetaW);
                var tmpz = dy * Math.Sin(thetaW) + dz * Math.Cos(thetaW);
                dy = tmpy;
                dz = tmpz;
            }

            {
                var tmpz = dz * Math.Cos(thetaV) - dx * Math.Sin(thetaV);
                var tmpx = dz * Math.Sin(thetaV) + dx * Math.Cos(thetaV);
                dz = tmpz;
                dx = tmpx;
            }

            {
                var tmpx = dx * Math.Cos(thetaU) - dy * Math.Sin(thetaU);
                var tmpy = dx * Math.Sin(thetaU) + dy * Math.Cos(thetaU);
                dx = tmpx;
                dy = tmpy;
            }

            var uPrime = u;
            var vPrime = v;
            var wPrime = w;

            var uDeterminate = false;
            var vDeterminate = false;
            var wDeterminate = false;

            Func<double, bool> isNotZero = val =>
            {
                return Math.Abs(val) > 0.00001;
            };

            if (isNotZero(dx) || isNotZero(dy))
            {
                uPrime = Math.Atan2(dy, dx) * 180 / Math.PI;
                uDeterminate = true;
            }

            if (isNotZero(dz) || isNotZero(dx))
            {
                vPrime = Math.Atan2(dx, dz) * 180 / Math.PI;
                vDeterminate = true;
            }

            if (isNotZero(dy) || isNotZero(dz))
            {
                wPrime = Math.Atan2(dz, dy) * 180 / Math.PI;
                wDeterminate = true;
            }

            return viewDir switch
            {
                Option.FrontView => (uPrime, uDeterminate),
                Option.RightSideView => (uPrime + 90, uDeterminate),
                Option.RearView => (uPrime + 180, uDeterminate),
                Option.LeftSideView => (uPrime + 270, uDeterminate),
                Option.TopViewXLeft => (vPrime - 90, vDeterminate),
                Option.TopViewXBottom => (wPrime - 180, wDeterminate),
                Option.TopViewXRight => (-vPrime - 90, vDeterminate),
                Option.TopViewXTop => (-wPrime, wDeterminate),
                _ => (uPrime, uDeterminate),
            };
        }
    }
}
