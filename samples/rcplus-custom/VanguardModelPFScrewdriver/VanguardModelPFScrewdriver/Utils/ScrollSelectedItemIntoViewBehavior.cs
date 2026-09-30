// -----------------------------------------------------------------------
// <copyright file="ScrollSelectedItemIntoViewBehavior.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Behavior that automatically scrolls the currently selected item of a
    /// <see cref="DataGrid"/> into the visible area whenever the selection changes.
    /// </summary>
    public class ScrollSelectedItemIntoViewBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// Called when the behavior is attached to the <see cref="DataGrid"/>.
        /// Subscribes to the selection changed event.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        /// <summary>
        /// Called when the behavior is detached from the <see cref="DataGrid"/>.
        /// Unsubscribes from the selection changed event to avoid leaking handlers.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;

            base.OnDetaching();
        }

        /// <summary>
        /// Scrolls the newly selected item into view.
        /// </summary>
        /// <param name="sender">The <see cref="DataGrid"/> raising the event.</param>
        /// <param name="ev">The selection change event data.</param>
        private void OnSelectionChanged(
            object sender,
            SelectionChangedEventArgs ev
        )
        {
            if (AssociatedObject.SelectedItem != null)
            {
                // Defer the scrolling until the layout/containers have been generated,
                // otherwise ScrollIntoView may not find the target row.
                AssociatedObject.Dispatcher.BeginInvoke(
                    () =>
                    {
                        AssociatedObject.UpdateLayout();
                        AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
                    },
                    DispatcherPriority.Background
                );
            }
        }
    }
}
