// -----------------------------------------------------------------------
// <copyright file="TorqueProperty.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.ComponentModel;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Bindable torque value that is always stored in mN&#183;m internally and exposed
    /// to the UI in the currently selected torque unit. The value is validated on every
    /// update and the result is reported through <see cref="INotifyDataErrorInfo"/>.
    /// </summary>
    public class TorqueProperty : IProperty<float>, INotifyDataErrorInfo
    {
        /// <summary>
        /// Occurs when the displayed value or the torque unit changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when the validation result of the value changes.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// The torque unit used to present the value.
        /// </summary>
        private TorqueUnitKind _torqueUnit = TorqueUnitKind.MNM;

        /// <summary>
        /// The value in mN&#183;m, which is the internal representation.
        /// </summary>
        private float _valueMNM;

        /// <summary>
        /// The validator invoked whenever the value is updated.
        /// </summary>
        private IValidator? _validator;

        /// <summary>
        /// The current validation error message, or <c>null</c> when the value is valid.
        /// </summary>
        private string? _validationError;

        /// <summary>
        /// Gets or sets the torque unit used to present the value.
        /// Changing the unit does not modify the internal value.
        /// </summary>
        public TorqueUnitKind TorqueUnit
        {
            get => _torqueUnit;
            set
            {
                _torqueUnit = value;

                // The displayed value depends on the unit, so refresh the binding.
                PropertyChanged?.Invoke(this, new(nameof(Value)));
            }
        }

        /// <summary>
        /// Gets or sets the value in mN&#183;m without any unit conversion or validation.
        /// </summary>
        public float InnerValue
        {
            get => _valueMNM;
            set => _valueMNM = value;
        }

        /// <summary>
        /// Gets or sets the value expressed in <see cref="TorqueUnit"/>.
        /// Setting the value validates it and raises the change notifications.
        /// </summary>
        public float Value
        {
            get
            {
                // Convert the internal value into the displayed unit.
                return _torqueUnit switch
                {
                    TorqueUnitKind.KGFCM => TorqueValueConverter.MNM2KGFCM(_valueMNM),
                    _ => _valueMNM,
                };
            }
            set
            {
                // Convert the input back into the internal unit.
                _valueMNM = _torqueUnit switch
                {
                    TorqueUnitKind.KGFCM => TorqueValueConverter.KGFCM2MNM(value),
                    _ => value
                };

                PropertyChanged?.Invoke(this, new(nameof(Value)));

                // Notify the binding only when the validation result actually changed.
                var error = _validator?.Validate(this, Value);
                if (_validationError != error)
                {
                    _validationError = error;
                    ErrorsChanged?.Invoke(this, new(nameof(Value)));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current value is invalid.
        /// </summary>
        public bool HasErrors => (_validationError != null);

        /// <summary>
        /// Gets the validation errors of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property. This type reports a single error only.</param>
        /// <returns>The error messages, or an empty sequence when the value is valid.</returns>
        public IEnumerable GetErrors(
            string? propertyName
        )
        {
            return _validationError != null ? [_validationError] : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Sets the validator used to check the value when it is updated.
        /// </summary>
        /// <param name="validator">The validator to use.</param>
        public void SetValidator(
            IValidator validator
        )
        {
            _validator = validator;
        }
    }
}
