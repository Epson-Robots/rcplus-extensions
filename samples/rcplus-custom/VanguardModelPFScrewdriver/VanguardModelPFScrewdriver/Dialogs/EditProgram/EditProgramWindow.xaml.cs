// -----------------------------------------------------------------------
// <copyright file="EditProgramWindow.xaml.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.Utils;

namespace VanguardModelPFScrewdriver.Dialogs.EditProgram
{
    /// <summary>
    /// Interaction logic for EditProgramWindow.xaml.
    /// Hosts the step editors and provides drag and drop reordering of steps.
    /// </summary>
    public partial class EditProgramWindow : Window
    {
        /// <summary>
        /// Adorner that draws the insertion indicator while dragging.
        /// </summary>
        private DropIndicatorAdorner? _adorner;

        /// <summary>
        /// Vertical positions (in <c>MainGrid</c> coordinates) where a step can be dropped.
        /// </summary>
        private double[]? _dropPositions;

        /// <summary>
        /// Index of the drop position currently highlighted by the indicator.
        /// </summary>
        private int _dropIndex;

        /// <summary>
        /// Rows that contain the step editors, keyed by their grid column.
        /// </summary>
        private readonly Dictionary<int, Grid> _stepsRow = [];

        /// <summary>
        /// Row that accepts drop operations during the current drag.
        /// </summary>
        private Grid? _currentStepsRow;

        /// <summary>
        /// Gets the grid cell (row and column) that owns the specified element.
        /// </summary>
        /// <param name="startElement">Element to start the visual tree walk from.</param>
        /// <returns>The row and column of the owning cell, or (-1, -1) when not found.</returns>
        private static (int Row, int Column) GetCell(
            DependencyObject startElement
        )
        {
            var presenter = startElement.Ancestor<ContentPresenter>();

            if (presenter == null)
            {
                return (-1, -1);
            }
            else
            {
                return (Grid.GetRow(presenter), Grid.GetColumn(presenter));
            }
        }

        /// <summary>
        /// Collects all <see cref="StepEditor"/> instances placed in the specified column.
        /// </summary>
        /// <param name="topElement">Root element of the visual subtree to search.</param>
        /// <param name="column">Target grid column.</param>
        /// <returns>The step editors found in the column, in visual order.</returns>
        private static List<StepEditor> GetStepEditors(
            DependencyObject topElement,
            int column
        )
        {
            List<StepEditor> editors = [];

            var count = VisualTreeHelper.GetChildrenCount(topElement);
            for (int index = 0; index < count; index++)
            {
                var child = VisualTreeHelper.GetChild(topElement, index);
                if (child is StepEditor editor)
                {
                    editors.Add(editor);
                }
                else
                {
                    // Descend only into elements that belong to the target column.
                    if (child is UIElement element && Grid.GetColumn(element) == column)
                    {
                        editors.AddRange(GetStepEditors(element, column));
                    }
                }
            }

            return editors;
        }

        /// <summary>
        /// Calculates the candidate drop positions from the given editors.
        /// </summary>
        /// <param name="editors">Step editors laid out in a single column.</param>
        /// <returns>Y coordinates of each editor top edge plus the bottom edge of the last editor.</returns>
        private List<double> CalcDropPositions(
            List<StepEditor> editors
        )
        {
            List<double> positions = [];

            if (editors.Count > 0)
            {
                foreach (var editor in editors)
                {
                    // Top edge of each editor becomes an insertion point.
                    var y = editor.TranslatePoint(new Point(0, 0), MainGrid).Y;
                    positions.Add(y);
                }

                // Append the bottom edge of the last editor as the trailing insertion point.
                var lastEditor = editors.Last();
                var lastY = lastEditor.TranslatePoint(new Point(0, lastEditor.ActualHeight), MainGrid).Y;
                positions.Add(lastY);
            }

            return positions;
        }

        /// <summary>
        /// Determines the insertion index closest to the specified vertical position.
        /// </summary>
        /// <param name="y">Pointer position in <c>MainGrid</c> coordinates.</param>
        /// <returns>The insertion index, or -1 when it cannot be determined.</returns>
        private int DecideDropIndex(
            double y
        )
        {
            if (_dropPositions == null)
            {
                return -1;
            }

            if (y < _dropPositions[0])
            {
                // Above the first editor: insert at the head.
                return 0;
            }
            else if (y > _dropPositions[^1])
            {
                // Below the last editor: insert at the tail.
                return _dropPositions.Length - 1;
            }
            else
            {
                for (int index = 1; index < _dropPositions.Length; index++)
                {
                    var maxY = _dropPositions[index];
                    if (y > maxY)
                    {
                        continue;
                    }
                    // Snap to the nearer boundary of the interval that contains y.
                    var minY = _dropPositions[index - 1];
                    var midY = (minY + maxY) / 2;
                    return (y < midY) ? index - 1 : index;
                }

                return -1;
            }
        }

        /// <summary>
        /// Handles the start of a step drag: prepares drop positions and shows the indicator.
        /// </summary>
        /// <param name="editor">Editor element being dragged.</param>
        /// <param name="editorViewModel">View model of the dragged editor.</param>
        private void OnDragStarted(
            UIElement editor,
            StepEditorViewModel editorViewModel
        )
        {
            var (row, column) = GetCell(editor);

            var editors = GetStepEditors(MainGrid, column);
            _dropPositions = [.. CalcDropPositions(editors)];

            // Compute the indicator bounds from the column layout.
            var x = MainGrid.ColumnDefinitions.Take(column).Sum(x => x.ActualWidth);
            var width = MainGrid.ColumnDefinitions[column].ActualWidth;
            var marginRow = row == 0 ? row + 1 : row - 1;
            var height = MainGrid.RowDefinitions[marginRow].ActualHeight;

            _adorner?.Show(new(x, 0, width, height));

            // Enable hit testing only on the row that should receive the drop.
            if (_stepsRow.TryGetValue(column, out _currentStepsRow))
            {
                _currentStepsRow.IsHitTestVisible = true;
            }
        }

        /// <summary>
        /// Handles the end of a step drag: hides the indicator and restores hit testing.
        /// </summary>
        /// <param name="editor">Editor element that was dragged.</param>
        /// <param name="editorViewModel">View model of the dragged editor.</param>
        private void OnDragEnded(
            UIElement editor,
            StepEditorViewModel editorViewModel
        )
        {
            _adorner?.Hide();
            if (_currentStepsRow != null)
            {
                _currentStepsRow.IsHitTestVisible = false;
                _currentStepsRow = null;
            }
        }

        /// <summary>
        /// Updates the insertion indicator while the pointer moves over the grid.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="ev">Drag event data.</param>
        private void OnDragOver(
            object? sender,
            DragEventArgs ev
        )
        {
            ev.Effects = DragDropEffects.Move;
            ev.Handled = true;

            var dropIndex = DecideDropIndex(ev.GetPosition(MainGrid).Y);
            if (dropIndex >= 0)
            {
                _dropIndex = dropIndex;
                _adorner?.MoveTo(_dropPositions![dropIndex]);
            }
        }

        /// <summary>
        /// Completes the drag by notifying the dragged view model of the drop index.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="ev">Drag event data.</param>
        private void OnDrop(
            object? sender,
            DragEventArgs ev
        )
        {
            if (ev.Data.GetData(typeof(StepEditorViewModel)) is StepEditorViewModel editorViewMode)
            {
                editorViewMode.Dropped?.Invoke(_dropIndex);
                ev.Handled = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditProgramWindow"/> class.
        /// </summary>
        public EditProgramWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                // Attach the drop indicator adorner once the visual tree is ready.
                var layer = AdornerLayer.GetAdornerLayer(MainGrid);
                _adorner = new(MainGrid);
                layer.Add(_adorner);

                // Map grid columns to the rows that host their step editors.
                _stepsRow[2] = TighteningStepsRow;
                _stepsRow[4] = LooseningStepsRow;

                StepEditor.EditorDragStarted += OnDragStarted;
                StepEditor.EditorDragEnded += OnDragEnded;
                StepEditor.EditorDragOver += OnDragOver;
                StepEditor.EditorDrop += OnDrop;
            };

            // Handle drag events even when they are already marked as handled.
            MainGrid.AddHandler(
                DragDrop.DragOverEvent,
                new DragEventHandler(OnDragOver),
                true
            );
            MainGrid.AddHandler(
                DragDrop.DropEvent,
                new DragEventHandler(OnDrop),
                true
            );

            GotKeyboardFocus += (_, ev) =>
            {
                // Update the hint area according to the focused element.
                if (ev.NewFocus is DependencyObject d && DataContext is EditProgramWindowViewModel viewModel)
                {
                    viewModel.Hint.Value = Hint.GetText(d);
                    viewModel.HintRange = Hint.GetRange(d);
                }
            };

            Closing += (_, ev) =>
            {
                if (DataContext is EditProgramWindowViewModel viewModel)
                {
                    if (viewModel.CloseResult == null)
                    {
                        ev.Cancel = true;

                        if (Cancel.Command != null && Cancel.Command.CanExecute(null))
                        {
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
