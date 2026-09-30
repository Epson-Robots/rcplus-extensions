// -----------------------------------------------------------------------
// <copyright file="ProgressWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using V2 = Epson.RoboticsShared.ExtensionsAPI.V2;

namespace VanguardModelPFScrewdriver.Dialogs.Progress
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressWindow"/> class.
        /// </summary>
        public ProgressWindow()
        {
            InitializeComponent();

            // The window handle only exists after loading, so tweak the styles here.
            Loaded += (_, _) =>
            {
                HideTitltBarButtons();
            };
        }

        /// <summary>
        /// Returns the currently active application window, falling back to the main window.
        /// </summary>
        /// <returns>The window to be used as the owner of the progress dialog.</returns>
        private static Window GetActiveWindow()
        {
            return (Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(x => x.IsActive)
                ?? Application.Current.MainWindow
            );
        }

        /// <summary>
        /// Runs the specified operation on a background thread while showing a modal progress dialog.
        /// </summary>
        /// <param name="action">The operation to execute; it receives a reporter used to update the dialog.</param>
        /// <param name="delayMSec">
        /// Delay in milliseconds before the dialog is shown. If the operation completes within this
        /// period, the dialog is never displayed, which avoids flickering for short operations.
        /// </param>
        /// <returns>A task that completes once the operation has finished.</returns>
        public static async Task OpenAsync(
            Func<IProgressReporter, Task> action,
            int delayMSec = 0
        )
        {
            ProgressWindow window = new()
            {
                Owner = GetActiveWindow(),
            };

            ProgressReporter reporter = new(window);

            CancellationTokenSource tokenSource = new();

            var closed = false;

            // Execute the operation off the UI thread and close the dialog when it finishes.
            Task clientTask = Task.Run(async () =>
            {
                await action.Invoke(reporter);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Close();
                    closed = true;
                    tokenSource.Cancel();
                });
            });

            var cursorState = Main.GetAPI<V2.IRCXGeneralAPI>().SetMouseCursorBusy();
            try
            {
                await Task.Delay(delayMSec, tokenSource.Token);
            }
            catch (Exception)
            {
                // It was probably cancelled.
            }
            finally
            {
                cursorState.Dispose();
            }

            // Show the dialog only if the operation is still running.
            if (!closed)
            {
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Removes the minimize, maximize and system menu buttons from the title bar.
        /// </summary>
        private void HideTitltBarButtons()
        {
            var hWnd = new WindowInteropHelper(this).Handle;
            var curLong = NativeMethods.GetWindowLongPtr(hWnd, NativeMethods.GWL_STYLE);
            _ = NativeMethods.SetWindowLongPtr(
                hWnd,
                NativeMethods.GWL_STYLE,
                curLong & ~(
                    NativeMethods.WS_MINIMIZEBOX
                    | NativeMethods.WS_MAXIMIZEBOX
                    | NativeMethods.WS_SYSMENU
                )
            );
        }
    }

    /// <summary>
    /// P/Invoke declarations and window style constants used to customize the progress window.
    /// </summary>
    public static partial class NativeMethods
    {
        /// <summary>
        /// Index of the window style value used with GetWindowLongPtr / SetWindowLongPtr.
        /// </summary>
        internal const int GWL_STYLE = -16;

        /// <summary>
        /// Window style flag for the minimize box.
        /// </summary>
        internal const IntPtr WS_MINIMIZEBOX = 0x00020000;

        /// <summary>
        /// Window style flag for the maximize box.
        /// </summary>
        internal const IntPtr WS_MAXIMIZEBOX = 0x00010000;

        /// <summary>
        /// Window style flag for the system menu (including the close button).
        /// </summary>
        internal const IntPtr WS_SYSMENU = 0x00080000;

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="nIndex">The zero-based offset of the value to retrieve, such as <see cref="GWL_STYLE"/>.</param>
        /// <returns>The requested value, or <see cref="IntPtr.Zero"/> on failure.</returns>
        [LibraryImport("user32", EntryPoint = "GetWindowLongPtrW")]
        internal static partial IntPtr GetWindowLongPtr(
            IntPtr hWnd,
            int nIndex
        );

        /// <summary>
        /// Changes an attribute of the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="nIndex">The zero-based offset of the value to set, such as <see cref="GWL_STYLE"/>.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>The previous value, or <see cref="IntPtr.Zero"/> on failure.</returns>
        [LibraryImport("user32", EntryPoint = "SetWindowLongPtrW")]
        internal static partial IntPtr SetWindowLongPtr(
            IntPtr hWnd,
            int nIndex,
            IntPtr dwNewLong
        );
    }
}
