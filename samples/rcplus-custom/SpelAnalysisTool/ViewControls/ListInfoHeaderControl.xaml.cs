// -----------------------------------------------------------------------
// <copyright file="ListInfoHeaderControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace SpelAnalysisTool.ViewControls
{
    /// <summary>
    /// Interaction logic for ListInfoHeaderControl.xaml
    /// </summary>
    public partial class ListInfoHeaderControl : UserControl
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string TxtName
        {
            set
            {
                _name = value;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Count of list items.
        /// </summary>
        public int Count
        {
            set
            {
                _count = value;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Get data Func.
        /// </summary>
        public Func<List<List<string>>>? GetDataFunc { get; set; }

        private string _name = "";
        private int _count;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ListInfoHeaderControl()
        {
            InitializeComponent();

            Action<bool> toClip = isCSV =>
            {
                if (GetDataFunc == null)
                {
                    return;
                }

                var dat = GetDataFunc();
                var txt = "";

                foreach (var rec in dat)
                {
                    var first = true;

                    foreach (var str in rec)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            txt += isCSV ? "," : "\t";
                        }

                        txt += str;
                    }

                    txt += "\n";
                }

                Clipboard.SetText(txt);
            };

            _btnCSV.Click += (s, e) => toClip(true);
            _btnTSV.Click += (s, e) => toClip(false);
        }

        /// <summary>
        /// Update display.
        /// </summary>
        public void UpdateDisplay()
        {
            if (_count > 0)
            {
                _txtName.Text = $"{_name} ({_count})";
            }
            else
            {
                _txtName.Text = _name;
            }
        }
    }
}
