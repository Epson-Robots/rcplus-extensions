// -----------------------------------------------------------------------
// <copyright file="F1HelpBehavior.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Behavior that invokes a help command when the F1 key is pressed
    /// on the associated <see cref="UIElement"/>.
    /// </summary>
    public class F1HelpBehavior : Behavior<UIElement>
    {
        /// <summary>
        /// Identifies the <see cref="HelpCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HelpCommandProperty =
            DependencyProperty.Register(
                nameof(HelpCommand),
                typeof(ICommand),
                typeof(F1HelpBehavior),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the command executed when the F1 key is pressed.
        /// </summary>
        public ICommand? HelpCommand
        {
            get => (ICommand)GetValue(HelpCommandProperty);
            set => SetValue(HelpCommandProperty, value);
        }

        /// <summary>
        /// Subscribes to the key events of the associated element.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        /// <summary>
        /// Unsubscribes from the key events of the associated element.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            base.OnDetaching();
        }

        /// <summary>
        /// Executes <see cref="HelpCommand"/> when the F1 key is pressed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ev">The key event data.</param>
        private void OnPreviewKeyDown(
            object sender,
            KeyEventArgs ev
        )
        {
            if (
                ev.Key == Key.F1
                && HelpCommand != null
                && HelpCommand.CanExecute(null)
            )
            {
                // Mark the key as handled so the default help handling is suppressed.
                ev.Handled = true;
                HelpCommand.Execute(null);
            }
        }
    }
}
