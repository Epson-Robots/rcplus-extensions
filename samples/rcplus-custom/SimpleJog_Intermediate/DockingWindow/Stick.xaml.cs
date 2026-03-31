// -----------------------------------------------------------------------
// <copyright file="Stick.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Stick.xaml interaction logic
    /// </summary>
    public partial class Stick : UserControl
    {
        /// <summary>
        /// Dragging state
        /// </summary>
        private bool _dragging = false;

        /// <summary>
        /// Knob range radius
        /// </summary>
        private double _radius;

        /// <summary>
        /// Dead zone radius
        /// </summary>
        private double _deadZone;

        /// <summary>
        /// Knob center position
        /// </summary>
        private Point _center;

        /// <summary>
        /// Knob offset at first click
        /// </summary>
        private Vector _offset;

        /// <summary>
        /// Smoothed position
        /// </summary>
        private Vector _smoothed;

        /// <summary>
        /// Range limitter
        /// </summary>
        private const double _limitFactor = 0.7;

        /// <summary>
        /// Dead zone size
        /// </summary>
        private const double _deadZoneFactor = 0.05;

        /// <summary>
        /// Smoothing coefficient
        /// </summary>
        private const double _smoothingFactor = 0.2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Stick()
        {
            InitializeComponent();

            Knob.Loaded += (_, _) =>
            {
                _radius = Math.Min(KnobRange.RenderSize.Width, KnobRange.Height) / 2.0 * _limitFactor;
                _deadZone = _radius * _deadZoneFactor;
                _center = new Point(KnobRange.RenderSize.Width / 2.0, KnobRange.RenderSize.Height / 2.0);
            };

            Knob.MouseLeftButtonDown += (_, ev) =>
            {
                Knob.CaptureMouse();

                _dragging = true;

                _offset = ev.GetPosition(KnobRange) - _center;
                _smoothed = new Vector();
            };

            Knob.MouseLeftButtonUp += (_, _) =>
            {
                Knob.ReleaseMouseCapture();

                _dragging = false;

                UpdateKnobPosition(_center);
            };

            Knob.MouseMove += (sender, ev) =>
            {
                if (_dragging && ev.LeftButton == MouseButtonState.Pressed)
                {
                    UpdateKnobPosition(ev.GetPosition(KnobRange) - _offset);
                }
            };

            Knob.LostMouseCapture += (_, _) =>
            {
                _dragging = false;
            };
        }

        /// <summary>
        /// Update knob position
        /// </summary>
        /// <param name="mousePosInRange">Relative mouse position in knob range</param>
        private void UpdateKnobPosition(
            Point mousePosInRange
        )
        {
            var x = mousePosInRange.X - _center.X;
            var y = mousePosInRange.Y - _center.Y;

            var distanceFromCenter = Math.Sqrt(x * x + y * y);
            if (distanceFromCenter < _deadZone)
            {
                Position = _smoothed = new Vector();
            }
            else if (distanceFromCenter < _radius)
            {
                _smoothed = new Vector(
                    _smoothed.X * (1 - _smoothingFactor) + (x / _radius) * _smoothingFactor,
                    _smoothed.Y * (1 - _smoothingFactor) + (y / _radius) * _smoothingFactor
                );
                Position = new Vector(_smoothed.X, -_smoothed.Y);
            }
        }
    }
}
