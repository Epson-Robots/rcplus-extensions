// -----------------------------------------------------------------------
// <copyright file="IProperty.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// A bindable property that notifies its changes and can be validated.
    /// </summary>
    /// <typeparam name="T">The type of the value held by the property.</typeparam>
    public interface IProperty<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the value exposed to the UI, expressed in the unit currently displayed.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the value in its internal representation, which is independent of
        /// the displayed unit. It is identical to <see cref="Value"/> when no conversion applies.
        /// </summary>
        public T InnerValue { get; set; }

        /// <summary>
        /// Sets the validator used to check the value when it is updated.
        /// </summary>
        /// <param name="validator">The validator to use.</param>
        public void SetValidator(
            IValidator validator
        );
    }
}
