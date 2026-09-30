// -----------------------------------------------------------------------
// <copyright file="StepEditor.xaml.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using VanguardModelPFScrewdriver.Utils;

namespace VanguardModelPFScrewdriver.Dialogs.EditProgram
{
    /// <summary>
    /// Interaction logic for StepEditor.xaml.
    /// </summary>
    public partial class StepEditor : UserControl
    {
        /// <summary>
        /// Gets or sets the callback raised when a drag operation on an editor starts.
        /// </summary>
        public static Action<UIElement, StepEditorViewModel>? EditorDragStarted { get; set; }

        /// <summary>
        /// Gets or sets the callback raised when a drag operation on an editor ends.
        /// </summary>
        public static Action<UIElement, StepEditorViewModel>? EditorDragEnded { get; set; }

        /// <summary>
        /// Occurs while a dragged item is moved over any editor instance.
        /// </summary>
        public static event EventHandler<DragEventArgs>? EditorDragOver;

        /// <summary>
        /// Occurs when a dragged item is dropped onto any editor instance.
        /// </summary>
        public static event EventHandler<DragEventArgs>? EditorDrop;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepEditor"/> class.
        /// </summary>
        public StepEditor()
        {
            InitializeComponent();

            // Forward instance-level drag/drop events to the static events so that
            // the owning list can handle them in one place.
            DragOver += (sender, ev) => EditorDragOver?.Invoke(sender, ev);

            Drop += (sender, ev) => EditorDrop?.Invoke(sender, ev);

            StepEditorThumb.DragStarted += (sender, ev) =>
            {
                // Start dragging only when the view model allows it.
                if (DataContext is StepEditorViewModel viewModel && viewModel.QueryCanDrag?.Invoke() == true)
                {
                    // Use the hosting UserControl as the drag source element.
                    var editor = StepEditorThumb.Ancestor<UserControl>();
                    if (editor == null)
                    {
                        return;
                    }

                    // Notify listeners before the blocking drag-and-drop loop begins.
                    EditorDragStarted?.Invoke(editor, viewModel);

                    // This call blocks until the drag-and-drop operation completes.
                    DragDrop.DoDragDrop(
                        editor,
                        viewModel,
                        DragDropEffects.Move
                    );

                    // Notify listeners after the drag-and-drop operation has finished.
                    EditorDragEnded?.Invoke(editor, viewModel);
                }
            };
        }
    }
}
