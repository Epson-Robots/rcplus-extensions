// -----------------------------------------------------------------------
// <copyright file="AutoScrollBehavior.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using Microsoft.Xaml.Behaviors;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    /// <summary>
    /// Behavior for ListBox, automatic scrolling to reveal last added item
    /// </summary>
    public class AutoScrollBehavior : Behavior<ListBox>
    {
        /// <summary>
        /// Collection
        /// </summary>
        private INotifyCollectionChanged? _collection;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            DetachCollectionChangedHandler();

            base.OnDetaching();
        }

        /// <summary>
        /// Loaded event handler of the AssociatedObject
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="ev">Event arguments</param>
        private void OnLoaded(
            object sender,
            RoutedEventArgs ev
        )
        {
            AttachCollectionChangedHandler();
        }

        /// <summary>
        /// Unloaded event handler of the AssociatedObject
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="ev">Event arguments</param>
        private void OnUnloaded(
            object sender,
            RoutedEventArgs ev
        )
        {
            DetachCollectionChangedHandler();
        }

        /// <summary>
        /// Attach CollectionChanged handler to the _collection
        /// </summary>
        private void AttachCollectionChangedHandler()
        {
            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                _collection = collection;
                _collection.CollectionChanged += OnCollectionChanged;
            }
        }

        /// <summary>
        /// Detach CollectionChanged handler from the _collection
        /// </summary>
        private void DetachCollectionChangedHandler()
        {
            if (_collection != null)
            {
                _collection.CollectionChanged -= OnCollectionChanged;
                _collection = null;
            }
        }

        /// <summary>
        /// CollectionChanged event handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="ev">Event arguments</param>
        private void OnCollectionChanged(
            object? sender,
            NotifyCollectionChangedEventArgs ev
        )
        {
            if (
                ev.Action != NotifyCollectionChangedAction.Add
                || ev.NewItems is not { Count: > 0 }
            )
            {
                return;
            }

            var item = ev.NewItems[^1];

            AssociatedObject.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                () => AssociatedObject.ScrollIntoView(item)
            );
        }
    }
}
