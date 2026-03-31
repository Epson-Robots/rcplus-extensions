// -----------------------------------------------------------------------
// <copyright file="StickProperties.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using System.Windows;

    /// <summary>
    /// Stick.xaml dependency properties
    /// </summary>
    public partial class Stick
    {
        /// <summary>
        /// Normalized position
        /// </summary>
        public Vector Position
        {
            get => (Vector)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        /// <summary>
        /// Field of the "Position"
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(
                nameof(Position),
                typeof(Vector),
                typeof(Stick),
                new FrameworkPropertyMetadata(
                    default(Vector),
                    (FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                     | FrameworkPropertyMetadataOptions.AffectsRender),
                    OnPositionChanged,
                    CoercePositionNormalized
                )
            );

        /// <summary>
        /// Position changed event handler
        /// </summary>
        /// <param name="d">The object</param>
        /// <param name="ev">The event</param>
        private static void OnPositionChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            if (d is Stick stick)
            {
                stick.UpdateRawPosition();
            }
        }

        /// <summary>
        /// Coerce value of the "Position"
        /// </summary>
        /// <param name="d">The object</param>
        /// <param name="value">The value</param>
        /// <returns>Corrected value</returns>
        private static object CoercePositionNormalized(
            DependencyObject d,
            object value
        )
        {
            var vector = (Vector)value;

            vector.X = Math.Clamp(vector.X, -1.0, 1.0);
            vector.Y = Math.Clamp(vector.Y, -1.0, 1.0);

            return vector;
        }

        /// <summary>
        /// Field key of the "RawPosition"
        /// </summary>
        private static readonly DependencyPropertyKey RawPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RawPosition),
                typeof(Vector),
                typeof(Stick),
                new PropertyMetadata(default(Vector))
            );

        /// <summary>
        /// Field of the "RawPosition"
        /// </summary>
        public static readonly DependencyProperty RawPositionProperty =
            RawPositionPropertyKey.DependencyProperty;

        /// <summary>
        /// Raw (pixel) position
        /// </summary>
        public Vector RawPosition => (Vector)GetValue(RawPositionProperty);

        /// <summary>
        /// Set raw position
        /// </summary>
        private void UpdateRawPosition()
        {
            var rawPosition = new Vector(Position.X * _radius, -(Position.Y * _radius));

            SetValue(RawPositionPropertyKey, rawPosition);
        }
    }
}
