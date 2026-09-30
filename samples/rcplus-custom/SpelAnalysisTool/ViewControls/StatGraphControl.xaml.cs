// -----------------------------------------------------------------------
// <copyright file="StatGraphControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for StatGraphControl.xaml
    /// </summary>
    public partial class StatGraphControl : UserControl
    {
        /// <summary>
        /// X Min.
        /// </summary>
        public double XMin { get; set; } = -5.0;
        
        /// <summary>
        /// X Max.
        /// </summary>
        public double XMax { get; set; } = 5.0;
        
        /// <summary>
        /// Bar graph num.
        /// </summary>
        public int BarGraphNum { get; set; } = 41;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatGraphControl()
        {
            InitializeComponent();
        }

        private double CalcFx(double x)
        {
            var a = 1.0 / Math.Sqrt(2.0 * Math.PI);
            var b = Math.Exp(-1.0 * Math.Pow(x, 2) / 2.0);
            return a * b;
        }

        private double _mean;
        private double _sigma;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="mean">Mean.</param>
        /// <param name="sigma">Sigma.</param>
        /// <param name="datas">Datas.</param>
        public void Init(double mean, double sigma, List<double> datas)
        {
            _mean = mean;
            _sigma = sigma;

            // Draw line graph.
            {
                var points = new PointCollection();
                _pathLineGraph.Data = new PathGeometry()
                {
                    Figures = {
                        new PathFigure()
                        {
                            StartPoint = new Point(XMin, -CalcFx(XMin)),
                            Segments = {
                                new PolyLineSegment()
                                {
                                    Points = points,
                                }
                            }
                        }
                    },
                };

                var range = XMax - XMin;
                var num = 100;
                var diff = range / num;

                for (var i = 0; i < num; i++)
                {
                    var x = XMin + i * diff;
                    points.Add(new Point(x, -CalcFx(x)));
                }
            }

            // Draw sigma line.
            {
                var num = (int)(XMax - XMin) - 1;

                var points = new PointCollection();
                _pathLineSigma.Data = new PathGeometry()
                {
                    Figures = {
                    new PathFigure()
                    {
                        StartPoint = new Point(0.0, 0.0),
                        Segments = {
                            new PolyLineSegment()
                            {
                                Points = points,
                            }
                        }
                    }
                },
                };

                for (var i = 1; i <= num; i++)
                {

                    points.Add(new Point(i, 0.0));
                    points.Add(new Point(i, 1.0));
                    points.Add(new Point(i, 0.0));
                }

                points.Add(new Point(num + 1, 0.0));
            }

            // Draw bar graph.
            {
                var path = new PathGeometry();
                var xmin = XMin * sigma;
                var xmax = XMax * sigma;
                var range = xmax - xmin;
                var diff = range / BarGraphNum;

                for (var i = 0; i < BarGraphNum; i++)
                {
                    var x = xmin + i * diff;
                    var count = datas.Where(d => (x) <= (d - mean) && (d - mean) < (x + diff)).Count();

                    var points = new PointCollection();
                    points.Add(new Point(x + diff, 0));
                    points.Add(new Point(x + diff, -count));
                    points.Add(new Point(x, -count));

                    path.Figures.Add(new PathFigure()
                    {
                        StartPoint = new Point(x, 0),
                        Segments = {
                            new PolyLineSegment()
                            {
                                Points = points,
                            }
                        },
                        IsClosed = true
                    });
                }

                _pathBarGraph.Data = path;
            }
        }

        /// <summary>
        /// Clear graph.
        /// </summary>
        public void Clear()
        {
            _txtYMax.Text = $"";
            _txtYMin.Text = $"";

            _pathBarGraph.Data = null;
            _pathLineGraph.Data = null;
        }

        /// <summary>
        /// Draw current value line.
        /// </summary>
        /// <param name="val">Current value.</param>
        public void DrawCurrentLine(double val)
        {
            var xmin = XMin * _sigma;
            var xmax = XMax * _sigma;

            var points = new PointCollection();
            _pathLineCurrent.Data = new PathGeometry()
            {
                Figures = {
                    new PathFigure()
                    {
                        StartPoint = new Point(xmin, 0.0),
                        Segments = {
                            new PolyLineSegment()
                            {
                                Points = points,
                            }
                        }
                    }
                },
            };

            var cur = val - _mean;
            points.Add(new Point(cur, 0.0));
            points.Add(new Point(cur, 1.0));
            points.Add(new Point(cur, 0.0));
            points.Add(new Point(xmax, 0.0));
        }
    }
}
