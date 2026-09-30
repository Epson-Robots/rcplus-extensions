// -----------------------------------------------------------------------
// <copyright file="LingGraphControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for LingGraphControl.xaml
    /// </summary>
    public partial class LineGraphControl : UserControl
    {
        private LineGraphData _lineGraphData = new LineGraphData();

        /// <summary>
        /// Constructor.
        /// </summary>
        public LineGraphControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="lineGraphData">Line graph data.</param>
        public void Init(LineGraphData lineGraphData)
        {
            _lineGraphData = lineGraphData;

            var yMin = _lineGraphData.YMin;
            var yMax = _lineGraphData.YMax;

            if (_lineGraphData.ChkMinEnabled)
            {
                yMin = Math.Min(yMin, _lineGraphData.ChkMin);
            }

            if (_lineGraphData.ChkMaxEnabled)
            {
                yMax = Math.Max(yMax, _lineGraphData.ChkMax);
            }

            _txtYMax.Text = $"{yMax:0.000}";
            _txtYMin.Text = $"{yMin:0.000}";

            var rectA = new PathFigure() { StartPoint = new Point(0, -yMax) };
            var rectB = new PathFigure() { StartPoint = new Point(_lineGraphData.TotalTime, -yMax) };
            var rectC = new PathFigure() { StartPoint = new Point(_lineGraphData.TotalTime, -yMin) };
            var rectD = new PathFigure() { StartPoint = new Point(0, -yMin) };

            // Scale
            _pathScale.Data = null;
            if (yMin < 0 && yMax > 0)
            {
                _pathScale.Data = new PathGeometry()
                {
                    Figures = {
                        rectA, rectB, rectC, rectD,
                        new PathFigure()
                        {
                            StartPoint = new Point(0, 0),
                            Segments = {
                                new PolyLineSegment()
                                {
                                    Points = new PointCollection()
                                    {
                                        new Point(_lineGraphData.TotalTime, 0),
                                    },
                                }
                            }
                        },
                    },
                };
            }

            // Chk Min Max
            _pathChkMinMax.Data = null;
            if (_lineGraphData.ChkMinEnabled || _lineGraphData.ChkMaxEnabled)
            {
                var path = new PathGeometry()
                {
                    Figures = {
                        rectA, rectB, rectC, rectD,
                    },
                };

                if (_lineGraphData.ChkMinEnabled)
                {
                    path.Figures.Add(new PathFigure()
                    {
                        IsClosed = true,
                        IsFilled = true,
                        StartPoint = new Point(0, -yMin),
                        Segments = {
                            new PolyLineSegment()
                            {
                                Points = new PointCollection()
                                {
                                    new Point(_lineGraphData.TotalTime, -yMin),
                                    new Point(_lineGraphData.TotalTime, -_lineGraphData.ChkMin),
                                    new Point(0, -_lineGraphData.ChkMin),
                                },
                            }
                        },
                    });
                }

                if (_lineGraphData.ChkMaxEnabled)
                {
                    path.Figures.Add(new PathFigure()
                    {
                        IsClosed = true,
                        IsFilled = true,
                        StartPoint = new Point(0, -yMax),
                        Segments = {
                            new PolyLineSegment()
                            {
                                Points = new PointCollection()
                                {
                                    new Point(_lineGraphData.TotalTime, -yMax),
                                    new Point(_lineGraphData.TotalTime, -_lineGraphData.ChkMax),
                                    new Point(0, -_lineGraphData.ChkMax),
                                },
                            }
                        },
                    });
                }

                _pathChkMinMax.Data = path;
            }

            // Data
            {
                var ptStart = new Point(0, 0);
                if (_lineGraphData.Datas.Count > 0)
                {
                    var pt = _lineGraphData.Datas[0];
                    ptStart = new Point(pt.X, -pt.Y);
                }

                var points = new PointCollection();
                _path.Data = new PathGeometry()
                {
                    Figures = {
                        rectA, rectB, rectC, rectD,
                        new PathFigure()
                        {
                            StartPoint = ptStart,
                            Segments = {
                                new PolyLineSegment()
                                {
                                    Points = points,
                                }
                            }
                        },
                    },
                };

                _lineGraphData.Datas.ForEach(d =>
                {
                    points.Add(new Point(d.X, -d.Y));
                });
            }
        }

        /// <summary>
        /// Clear graph.
        /// </summary>
        public void Clear()
        {
            _txtYMin.Text = $"";
            _txtYMax.Text = $"";

            _pathChkMinMax.Data = null;
            _path.Data = null;
        }
    }
}
