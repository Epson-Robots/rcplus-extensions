// -----------------------------------------------------------------------
// <copyright file="MouseFor3D.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpelAnalysisTool.View3D
{
    /// <summary>
    /// Mouse operations for 3D.
    /// </summary>
    internal class MouseFor3D
    {
        private bool _mouseDownLeft;
        private bool _mouseDownRight;
        private double _mouseDownRightMove;
        private double _mouseDownRightMovedJudgeLength = 3.0;
        private Point _mousePos;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rc3dview">RC3dView.</param>
        /// <param name="rc3dFront">Front Grid.</param>
        /// <param name="viewDirCtrl">ViewDirectionControl.</param>
        public MouseFor3D(RC3DView rc3dview, Grid rc3dFront, ViewDirectionControl? viewDirCtrl)
        {
            rc3dFront.ContextMenu = new ContextMenu();
            Action<string, Action> addCMenu = (name, action) =>
            {
                var menuItem = new MenuItem() { Header = name };
                menuItem.Click += (s, e) =>
                {
                    action();
                    viewDirCtrl?.ReqUpdate();
                };
                rc3dFront.ContextMenu.Items.Add(menuItem);
            };

            addCMenu("Reset", () => rc3dview.VCam.ResetPerspective());
            addCMenu("Front", () => rc3dview.VCam.ChangeViewFront());
            addCMenu("Rear", () => rc3dview.VCam.ChangeViewRear());
            addCMenu("Right Side", () => rc3dview.VCam.ChangeViewRightside());
            addCMenu("Left Side", () => rc3dview.VCam.ChangeViewLeftside());
            rc3dFront.ContextMenu.Items.Add(new Separator());
            addCMenu("Orthographic", () =>
            {
                rc3dview.VCam.ResetOrthoCam();
                rc3dview.VCam.IsOrtho = true;
            });

            rc3dFront.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _mouseDownLeft = true;
                }

                if (!rc3dview.VCam.IsOrtho && e.RightButton == MouseButtonState.Pressed)
                {
                    _mouseDownRightMove = 0.0;
                    _mouseDownRight = true;
                }

                _mousePos = e.GetPosition(rc3dFront);

                if (!(_mouseDownLeft || _mouseDownRight))
                {
                    return;
                }

                rc3dFront.CaptureMouse();
            };

            rc3dFront.MouseUp += (s, e) =>
            {
                rc3dFront.ReleaseMouseCapture();

                if (_mouseDownRight && _mouseDownRightMove > _mouseDownRightMovedJudgeLength)
                {
                    e.Handled = true;
                }

                _mouseDownLeft = false;
                _mouseDownRight = false;
                _mouseDownRightMove = 0.0;
            };

            rc3dFront.MouseMove += (s, e) =>
            {
                if (!(_mouseDownLeft || _mouseDownRight))
                {
                    return;
                }

                var pos = e.GetPosition(rc3dFront);

                if (rc3dview.VCam.IsOrtho)
                {
                    var gridW = rc3dFront.ActualWidth;
                    var orthoW = rc3dview.VCam.GetOrthoWidth();
                    rc3dview.VCam.SetOrthoPosConsiderRot(
                        (pos.X - _mousePos.X) * orthoW / gridW,
                        (pos.Y - _mousePos.Y) * orthoW / gridW);
                }
                else
                {
                    if (_mouseDownRight)
                    {
                        rc3dview.VCam.ViewMoveH -= (pos.X - _mousePos.X) * 0.005;
                        rc3dview.VCam.ViewMoveV += (pos.Y - _mousePos.Y) * 0.005;
                        _mouseDownRightMove += Math.Sqrt(Math.Pow(pos.X - _mousePos.X, 2) + Math.Pow(pos.Y - _mousePos.Y, 2));
                    }
                    else
                    {
                        rc3dview.VCam.ViewAngleH -= (pos.X - _mousePos.X) * 0.01;
                        rc3dview.VCam.ViewAngleV += (pos.Y - _mousePos.Y) * 0.01;
                    }

                    rc3dview.VCam.UpdatePerspective();
                }

                _mousePos = pos;
            };

            rc3dFront.MouseWheel += (s, e) =>
            {
                if (rc3dview.VCam.IsOrtho)
                {
                    var orthoW = rc3dview.VCam.GetOrthoWidth();
                    orthoW += e.Delta * 0.0003;
                    orthoW = Math.Clamp(orthoW, 0.1, 3.0);
                    rc3dview.VCam.SetOrthoWidth(orthoW);
                }
                else
                {
                    if (KeyStatCommon.IsCtrlKeyDown)
                    {
                        rc3dview.VCam.ViewMoveV -= e.Delta * 0.0002;
                    }
                    else
                    {
                        rc3dview.VCam.ViewDistance = Math.Max(0.1, rc3dview.VCam.ViewDistance + e.Delta * 0.001);
                    }

                    rc3dview.VCam.UpdatePerspective();
                }
            };
        }
    }
}
