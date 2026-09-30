using Epson.RoboticsShared.ExtensionsAPI;
using SpelAnalysisTool.ViewMainPanel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows.Media;
using static SpelAnalysisTool.Constants;

namespace SpelAnalysisTool.DockingWindow
{
    /// <summary>
    /// Extension : Docking Window (Typically Common Part)
    /// </summary>
    internal partial class DockingWindowContentViewModel : IRCXUserControlViewModel, IDisposable
    {
        /// <inheritdoc />
        public string Id => Main.CommonId;

        /// <inheritdoc />
        public string ViewModelId => $"{Id}.DockingWindow";

        /// <inheritdoc />
        public bool KeepOpenWhenProjectClosing => false;

        /// <summary>
        /// Captions
        /// </summary>
        public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

        /// <inheritdoc />
        public RCXCaption WindowCaption { get; set; } = new(Main.CommonId, Caption.ExtensionName);

        /// <inheritdoc />
        public ImageSource? WindowIcon { get; set; } = Main.CommonIcon;

        /// <summary>
        /// Related disposables
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <inheritdoc />
        public Task<bool> CloseAsync()
        {
            MainPanel.Instance?.OnCloseWindow();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SaveAsync()
        {
            MainPanel.Instance?.OnCloseWindow();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void Reload()
        {
        }

        /// <inheritdoc />
        public void Copy()
        {
        }

        /// <inheritdoc />
        public void Cut()
        {
        }

        /// <inheritdoc />
        public void Paste()
        {
        }

        /// <inheritdoc />
        public void SelectAll()
        {
        }

        /// <inheritdoc />
        public void Undo()
        {
        }

        /// <inheritdoc />
        public void Redo()
        {
        }

        /// <inheritdoc />
        public void ShowHelp()
        {
            const string helpUrl = "https://github.com/Epson-Robots/rcplus-extensions/tree/8.2.0.0/samples/rcplus-custom/SpelAnalysisTool";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = helpUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// Show docking window
        /// </summary>
        /// <returns>Task</returns>
        public static async Task Show()
        {
            DockingWindowContent control = new();

            if (control.DataContext is DockingWindowContentViewModel controlViewModel)
            {
                MainPanel.Instance?.OnShowWindow();
                await Main.GetAPI<IRCXWindowAPI>().ShowDockingWindowAsync(controlViewModel, control);
            }
        }
    }
}
