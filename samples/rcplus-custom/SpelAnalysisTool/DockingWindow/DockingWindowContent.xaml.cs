using SpelAnalysisTool.ViewMainPanel;
using System.Windows.Controls;

namespace SpelAnalysisTool.DockingWindow
{
    /// <summary>
    /// Code Behind of DockingWindowContent Control
    /// </summary>
    public partial class DockingWindowContent : UserControl
    {
        public DockingWindowContent()
        {
            InitializeComponent();

            _contentPresenter.Content = MainPanel.CreateInstance();
        }
    }
}
