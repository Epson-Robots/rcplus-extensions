namespace SpelAnalysisTool.DockingWindow
{
    /// <summary>
    /// Extension : Docking Window (Specific Part)
    /// </summary>
    internal partial class DockingWindowContentViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DockingWindowContentViewModel()
        {
        }

        /// <inheritdoc />
        public Task WindowCreated()
        {
            return Task.CompletedTask;
        }
    }
}
