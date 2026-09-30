// -----------------------------------------------------------------------
// <copyright file="EditWindowWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.Utils;

namespace VanguardModelPFScrewdriver.Dialogs.EditWindow
{
    /// <summary>
    /// Interaction logic for EditWindowWindow.xaml.
    /// </summary>
    public partial class EditWindowWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditWindowWindow"/> class.
        /// </summary>
        public EditWindowWindow()
        {
            InitializeComponent();

            // Update the hint text/range whenever the keyboard focus moves to another element.
            GotKeyboardFocus += (_, ev) =>
            {
                if (ev.NewFocus is DependencyObject d && DataContext is EditWindowWindowViewModel viewModel)
                {
                    viewModel.Hint.Value = Hint.GetText(d);
                    viewModel.HintRange = Hint.GetRange(d);
                }
            };

            // Forward left button clicks on the plot area to the view model.
            PlotArea.PreviewMouseLeftButtonDown += (_, ev) =>
            {
                if (DataContext is EditWindowWindowViewModel viewModel)
                {
                    viewModel.Clicked(ev);
                }
            };

            Closing += (_, ev) =>
            {
                if (DataContext is EditWindowWindowViewModel viewModel)
                {
                    // The close result is not determined yet, so the window was closed
                    // by the user (title bar close button, Alt+F4, etc.).
                    if (viewModel.CloseResult == null)
                    {
                        // Cancel the default closing and let the Cancel command decide instead.
                        ev.Cancel = true;

                        if (Cancel.Command != null && Cancel.Command.CanExecute(null))
                        {
                            // Execute after the current closing sequence has completed.
                            Application.Current.Dispatcher.BeginInvoke(
                                () =>
                                {
                                    Cancel.Command.Execute(null);
                                },
                                DispatcherPriority.Background
                            );
                        }
                    }
                }
            };
        }
    }
}