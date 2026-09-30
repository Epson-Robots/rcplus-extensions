// -----------------------------------------------------------------------
// <copyright file="TighteningSpecialParameter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections;
using System.ComponentModel;
using System.Numerics;
using System.Reactive.Disposables;
using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;

namespace VanguardModelPFScrewdriver.Dialogs.EditProgram
{
    /// <summary>
    /// Base class of the special parameters of a tightening program.
    /// It holds the value, its availability, its enabled state and the display information
    /// used by the edit program dialog.
    /// </summary>
    public class TighteningSpecialParameter : IDisposable
    {
        /// <summary>
        /// Wraps a <see cref="ReactiveProperty{T}"/> so that it can be used through
        /// the <see cref="IProperty{T}"/> abstraction and reports validation errors.
        /// </summary>
        /// <typeparam name="T">Numeric type of the wrapped value.</typeparam>
        public class ReactivePropertyWrapper<T> : IProperty<T>, INotifyDataErrorInfo, IDisposable where T : INumber<T>
        {
            /// <summary>The wrapped reactive property that actually stores the value.</summary>
            private readonly ReactiveProperty<T> _property = new();

            /// <summary>Forwards the property change notification of the wrapped property.</summary>
            public event PropertyChangedEventHandler? PropertyChanged
            {
                add => _property.PropertyChanged += value;
                remove => _property.PropertyChanged -= value;
            }

            /// <summary>Gets or sets the value displayed on the UI.</summary>
            public T Value
            {
                get => _property.Value;
                set => _property.Value = value;
            }

            /// <summary>
            /// Gets or sets the raw value stored in the tightening program.
            /// For this wrapper it is identical to <see cref="Value"/>.
            /// </summary>
            public T InnerValue
            {
                get => Value;
                set => Value = value;
            }

            /// <summary>Holds the subscriptions to be released on disposal.</summary>
            private readonly CompositeDisposable _disposables = [];

            /// <summary>
            /// Registers the validator that checks the value every time it is updated.
            /// </summary>
            /// <param name="validator">Validator applied to the current value.</param>
            public void SetValidator(
                IValidator validator
            )
            {
                _property.SetValidateNotifyError((value) => validator.Validate(_property, float.CreateChecked(Value))).AddTo(_disposables);
            }

            /// <summary>Releases the validation subscription.</summary>
            public void Dispose()
            {
                GC.SuppressFinalize(this);
                _disposables.Dispose();
            }

            /// <summary>Raised when the validation errors of the value have changed.</summary>
            public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

            /// <summary>Gets a value indicating whether the current value is invalid.</summary>
            public bool HasErrors => _property.HasErrors;

            /// <summary>Gets the validation errors of the specified property.</summary>
            /// <param name="propertyName">Name of the property to be inspected.</param>
            /// <returns>The current validation errors.</returns>
            public IEnumerable GetErrors(string? propertyName) => _property.GetErrors(propertyName);

            /// <summary>
            /// Initializes a new instance of the <see cref="ReactivePropertyWrapper{T}"/> class
            /// and relays the error notification of the wrapped property.
            /// </summary>
            public ReactivePropertyWrapper()
            {
                _property.ErrorsChanged += (_, ev) => ErrorsChanged?.Invoke(this, ev);
            }
        }

        /// <summary>Current tightening mode shared by all the special parameters.</summary>
        public static ReactivePropertySlim<ModeKind> Mode { get; } = new(ModeKind.Normal);

        /// <summary>Current torque unit shared by all the special parameters.</summary>
        public static ReactivePropertySlim<TorqueUnitKind> TorqueUnit { get; } = new(TorqueUnitKind.MNM);

        /// <summary>Indicates whether this parameter can be used in the current mode.</summary>
        public ReactivePropertySlim<bool> Available { get; } = new(true);

        /// <summary>Indicates whether this parameter is enabled in the tightening program.</summary>
        public ReactivePropertySlim<bool> Effective { get; } = new(false);

        /// <summary>Gets the localized caption of this parameter.</summary>
        public string DisplayName => Main.Captions![_captionId];

        /// <summary>Holds the property that stores the value of this parameter.</summary>
        public ReactivePropertySlim<IProperty<float>> ValueHolder { get; } = new();

        /// <summary>Gets the format string used to display the value.</summary>
        public string Format { get; init; } = string.Empty;

        /// <summary>Gets or sets the number of decimal places of the value.</summary>
        public int DecimalPlaces { get; set; } = 0;

        /// <summary>Gets the identifier of the hint text of this parameter.</summary>
        public HintId HintId { get; init; } = HintId.None;

        /// <summary>Identifier of the caption resource of this parameter.</summary>
        protected int _captionId;

        /// <summary>Holds the subscriptions to be released on disposal.</summary>
        protected readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="TighteningSpecialParameter"/> class
        /// with a plain floating point value holder.
        /// </summary>
        public TighteningSpecialParameter()
        {
            ValueHolder.Value = new ReactivePropertyWrapper<float>();
        }

        /// <summary>Releases the subscriptions held by this parameter.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _disposables.Dispose();
        }

        /// <summary>
        /// Reflects the specified special flag of the tightening program to <see cref="Effective"/>.
        /// </summary>
        /// <param name="tighteningProgram">Tightening program to be read.</param>
        /// <param name="flag">Special flag assigned to this parameter.</param>
        protected void SetEffective(
            TighteningProgram tighteningProgram,
            Special flag
        )
        {
            Effective.Value = (tighteningProgram.SpecialFlags & flag) != 0;
        }

        /// <summary>
        /// Writes <see cref="Effective"/> back to the specified special flag of the tightening program.
        /// </summary>
        /// <param name="tighteningProgram">Tightening program to be updated.</param>
        /// <param name="flag">Special flag assigned to this parameter.</param>
        protected void SetBackEffective(
            TighteningProgram tighteningProgram,
            Special flag
        )
        {
            if (Effective.Value)
            {
                tighteningProgram.SpecialFlags |= flag;
            }
            else
            {
                tighteningProgram.SpecialFlags &= ~flag;
            }
        }

        /// <summary>Loads the value of this parameter from the tightening program.</summary>
        /// <param name="tighteningProgram">Tightening program to be read.</param>
        public virtual void Set(
            TighteningProgram tighteningProgram
        )
        {
        }

        /// <summary>Stores the value of this parameter into the tightening program.</summary>
        /// <param name="tighteningProgram">Tightening program to be updated.</param>
        public virtual void SetBack(
            TighteningProgram tighteningProgram
        )
        {
        }

        /// <summary>Registers the validator applied to the value of this parameter.</summary>
        /// <param name="validator">Validator applied to the value.</param>
        public virtual void SetValiator(
            IValidator validator
        )
        {
            ValueHolder.Value.SetValidator(validator);
        }
    }

    /// <summary>
    /// Special parameter that defines the torque amount used to detect the start of tightening.
    /// </summary>
    public class StartDetectionAmountParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartDetectionAmountParameter"/> class
        /// and keeps the torque property synchronized with the current torque unit.
        /// </summary>
        public StartDetectionAmountParameter()
        {
            TorqueProperty property = new();

            ValueHolder.Value = property;
            TorqueUnit
                .Subscribe(
                    (torqueUnit) =>
                    {
                        property.TorqueUnit = torqueUnit;
                        DecimalPlaces = TorqueValueConverter.DecimalPlaces(torqueUnit);
                    }
                )
                .AddTo(_disposables);

            _captionId = Caption.StartDetectionAmount;

            HintId = HintId.TighteningStartDetectionAmount;

            Format = "_T";
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.StartDetectionAmount);
            ValueHolder.Value.InnerValue = tighteningProgram.StartDetectionAmount;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.StartDetectionAmount);
            tighteningProgram.StartDetectionAmount = ValueHolder.Value.InnerValue;
        }
    }

    /// <summary>
    /// Special parameter that defines the torque used while tapping starts.
    /// It is available only in the tapping mode.
    /// </summary>
    public class InitialTappingTorqueParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitialTappingTorqueParameter"/> class
        /// and updates its availability according to the current mode.
        /// </summary>
        public InitialTappingTorqueParameter()
        {
            TorqueProperty property = new();

            ValueHolder.Value = property;
            TorqueUnit
                .Subscribe(
                    (torqueUnit) =>
                    {
                        property.TorqueUnit = torqueUnit;
                        DecimalPlaces = TorqueValueConverter.DecimalPlaces(torqueUnit);
                    }
                )
                .AddTo(_disposables);

            _captionId = Caption.InitialTappingTorque;

            HintId = HintId.TigtheningInitialTappingTorque;

            Format = "_T";

            Mode.Subscribe(mode =>
            {
                Available.Value = mode switch
                {
                    ModeKind.Tapping => true,
                    _ => false
                };
            })
            .AddTo(_disposables);
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.InitialTappingTorque);
            ValueHolder.Value.InnerValue = tighteningProgram.InitialTappingTorque;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.InitialTappingTorque);
            tighteningProgram.InitialTappingTorque = ValueHolder.Value.InnerValue;
        }
    }

    /// <summary>
    /// Special parameter that defines the time required to detect the torque up.
    /// It is available in the tapping and fast modes.
    /// </summary>
    public class TorqueUpDetectionTimeParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TorqueUpDetectionTimeParameter"/> class
        /// and updates its availability according to the current mode.
        /// </summary>
        public TorqueUpDetectionTimeParameter()
        {
            _captionId = Caption.TorqueUpDetectionTime;

            HintId = HintId.TighteningTorqueUpDetectionTime;

            Format = "f2";
            DecimalPlaces = 2;

            Mode.Subscribe(mode =>
            {
                Available.Value = mode switch
                {
                    ModeKind.Tapping => true,
                    ModeKind.Fast => true,
                    _ => false
                };
            })
            .AddTo(_disposables);
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.TorqueUpDetectionTime);
            ValueHolder.Value.Value = tighteningProgram.TorqueUpDetectionTime;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.TorqueUpDetectionTime);
            tighteningProgram.TorqueUpDetectionTime = ValueHolder.Value.Value;
        }
    }

    /// <summary>
    /// Special parameter that defines the lower limit of the permissible number of turns.
    /// </summary>
    public class PermissibleTurnsBelowParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissibleTurnsBelowParameter"/> class.
        /// </summary>
        public PermissibleTurnsBelowParameter()
        {
            _captionId = Caption.PermissibleTurnsBelow;

            HintId = HintId.TighteningPermissibleTurnsBelow;

            Format = "f1";
            DecimalPlaces = 1;
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.PermissibleTurnsBelow);
            ValueHolder.Value.Value = tighteningProgram.PermissibleTurnsBelow;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.PermissibleTurnsBelow);
            tighteningProgram.PermissibleTurnsBelow = ValueHolder.Value.Value;
        }
    }

    /// <summary>
    /// Special parameter that defines the upper limit of the permissible number of turns.
    /// </summary>
    public class PermssibleTurnsAboveParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermssibleTurnsAboveParameter"/> class.
        /// </summary>
        public PermssibleTurnsAboveParameter()
        {
            _captionId = Caption.PermissibleTurnsAbove;

            HintId = HintId.TighteningPermissibleTurnsAbove;

            Format = "f1";
            DecimalPlaces = 1;
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.PermissibleTurnsAbove);
            ValueHolder.Value.Value = tighteningProgram.PermissibleTurnsAbove;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.PermissibleTurnsAbove);
            tighteningProgram.PermissibleTurnsAbove = ValueHolder.Value.Value;
        }
    }

    /// <summary>
    /// Special parameter that defines the additional angle applied after the torque up.
    /// </summary>
    public class FurtherTighteningAngleParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FurtherTighteningAngleParameter"/> class.
        /// </summary>
        public FurtherTighteningAngleParameter()
        {
            _captionId = Caption.FurtherTighteningAngle;

            HintId = HintId.TightenningFurtherTighteningAngle;

            Format = "f1";
            DecimalPlaces = 1;
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.FurtherTighteningAngle);
            ValueHolder.Value.Value = tighteningProgram.FurtherTighteningAngle;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.FurtherTighteningAngle);
            tighteningProgram.FurtherTighteningAngle = ValueHolder.Value.Value;
        }
    }

    /// <summary>
    /// Special parameter that defines the additional angle applied when the screw is protruding.
    /// </summary>
    public class FurtherTighteningAngleForProtrudingParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FurtherTighteningAngleForProtrudingParameter"/> class.
        /// </summary>
        public FurtherTighteningAngleForProtrudingParameter()
        {
            _captionId = Caption.FurtherTighteningAngleForProtruding;

            HintId = HintId.TightentingFurtherTighteningAngleForProtruding;

            Format = "f1";
            DecimalPlaces = 1;
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.FurtherTighteningAngleForProtruding);
            ValueHolder.Value.Value = tighteningProgram.FurtherTighteningAngleForProtruding;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.FurtherTighteningAngleForProtruding);
            tighteningProgram.FurtherTighteningAngleForProtruding = ValueHolder.Value.Value;
        }
    }

    /// <summary>
    /// Special parameter that defines the torque threshold at which the rotation speed is changed.
    /// It is available in the tapping and fast modes.
    /// </summary>
    public class TorqueThresholdToChangeSpeedParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TorqueThresholdToChangeSpeedParameter"/> class
        /// and updates its availability according to the current mode.
        /// </summary>
        public TorqueThresholdToChangeSpeedParameter()
        {
            _captionId = Caption.TorqueThresholdToChangeSpeed;

            HintId = HintId.TighteningTorqueThresholdToChangeSpeed;

            Format = "f1";
            DecimalPlaces = 1;

            Mode.Subscribe(mode =>
            {
                Available.Value = mode switch
                {
                    ModeKind.Tapping => true,
                    ModeKind.Fast => true,
                    _ => false
                };
            })
            .AddTo(_disposables);
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.TorqueThresholdToChangeSpeed);
            ValueHolder.Value.Value = tighteningProgram.TorqueThresholdToChangeSpeed;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.TorqueThresholdToChangeSpeed);
            tighteningProgram.TorqueThresholdToChangeSpeed = ValueHolder.Value.Value;
        }
    }

    /// <summary>
    /// Special parameter that defines the rotation speed used after the speed has been changed.
    /// It is available in the tapping and fast modes.
    /// </summary>
    public class SpeedAfterChangedParameter : TighteningSpecialParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedAfterChangedParameter"/> class
        /// and updates its availability according to the current mode.
        /// </summary>
        public SpeedAfterChangedParameter()
        {
            _captionId = Caption.SpeedAfterChanged;

            HintId = HintId.TightentingSpeedAfterChanged;

            Mode.Subscribe(mode =>
            {
                Available.Value = mode switch
                {
                    ModeKind.Tapping => true,
                    ModeKind.Fast => true,
                    _ => false
                };
            })
            .AddTo(_disposables);
        }

        /// <inheritdoc/>
        public override void Set(
            TighteningProgram tighteningProgram
        )
        {
            SetEffective(tighteningProgram, Special.SpeedAfterChanged);
            ValueHolder.Value.Value = tighteningProgram.SpeedAfterChanged;
        }

        /// <inheritdoc/>
        public override void SetBack(
            TighteningProgram tighteningProgram
        )
        {
            SetBackEffective(tighteningProgram, Special.SpeedAfterChanged);
            tighteningProgram.SpeedAfterChanged = (int)ValueHolder.Value.Value;
        }
    }
}
