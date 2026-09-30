// -----------------------------------------------------------------------
// <copyright file="MouseForGrid.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace SpelAnalysisTool.Common
{
    /// <summary>
    /// Mouse operations for Grid.
    /// </summary>
    internal class MouseForGrid
    {
        /// <summary>
        /// Mouse drag Action.
        /// </summary>
        public Action<double>? MouseDragAction { get; set; }

        private Grid _grid;
        private bool _running;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="grid">Target Grid.</param>
        public MouseForGrid(Grid grid)
        {
            _grid = grid;

            Action<Point> mouseDragAction = pt =>
            {
                if (_grid.ActualWidth < 0) return;

                MouseDragAction?.Invoke(pt.X / _grid.ActualWidth);
            };

            _grid.PreviewMouseLeftButtonDown += (s, e) =>
            {
                _running = true;
                _grid.CaptureMouse();
            };

            _grid.PreviewMouseLeftButtonUp += (s, e) =>
            {
                _grid.ReleaseMouseCapture();

                if (!_running) return;

                mouseDragAction(e.GetPosition(_grid));
                _running = false;
            };

            _grid.PreviewMouseMove += (s, e) =>
            {
                if (!_running) return;

                mouseDragAction(e.GetPosition(_grid));
            };
        }
    }
}
