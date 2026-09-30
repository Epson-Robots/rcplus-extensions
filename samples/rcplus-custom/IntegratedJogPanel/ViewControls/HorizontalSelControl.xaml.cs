// -----------------------------------------------------------------------
// <copyright file="HorizontalSelControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using IntegratedJogPanel.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntegratedJogPanel.View
{
    /// <summary>
    /// Interaction logic for HorizontalSelControl.xaml
    /// </summary>
    public partial class HorizontalSelControl : UserControl
    {
        /// <summary>
        /// Selected word
        /// </summary>
        public string SelectedWord
        {
            get => _selectedWord;
            set
            {
                _selectedWord = value;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Item selected Func for async
        /// </summary>
        public Func<string, Task>? SelectedFuncAsync { get; set; }

        private List<string> _words { get; } = new List<string>();
        private string _selectedWord = "";

        /// <summary>
        /// Constructor.
        /// </summary>
        public HorizontalSelControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="words">Selection words array</param>
        public void Init(string[] words)
        {
            _words.Clear();
            _words.AddRange(words);

            _stackPanel.Children.Clear();

            _words.ForEach(word =>
            {
                var btn = new TextBlock() { Text = word };
                btn.MouseDown += async (s, e) =>
                {
                    _selectedWord = _words[_stackPanel.Children.IndexOf((System.Windows.UIElement)s)];

                    if (SelectedFuncAsync != null)
                    {
                        await SelectedFuncAsync(_selectedWord);
                    }
                    
                    UpdateDisplay();
                };

                _stackPanel.Children.Add(btn);
            });
        }

        private void UpdateDisplay()
        {
            for (var i = 0; i < _words.Count; i++)
            {
                if (_stackPanel.Children[i] is TextBlock btn)
                {
                    var on = _words[i] == _selectedWord;

                    btn.Background = on ? SolidBrushes.CtrlBlue : Brushes.Gray;
                    btn.Foreground = on ? Brushes.White : Brushes.LightGray;
                    btn.FontWeight = on ? FontWeights.Bold : FontWeights.Normal;
                }
            }
        }
    }
}
