// -----------------------------------------------------------------------
// <copyright file="StepEditorViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using VanguardModelPFScrewdriver.Utils;

namespace VanguardModelPFScrewdriver.Dialogs.EditProgram
{
    /// <summary>
    /// View model for a single step row in the program editor.
    /// Holds the editable parameters of the step and the state used for drag and drop reordering.
    /// </summary>
    public class StepEditorViewModel : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this step can currently be dragged in the step list.
        /// </summary>
        public ReactivePropertySlim<bool> CanDrag { get; } = new(true);

        /// <summary>
        /// Gets the number of turns configured for this step. The value is validated by <see cref="IValidator"/>.
        /// </summary>
        public ReactiveProperty<float?> NumTurns { get; } = new();

        /// <summary>
        /// Gets a value indicating whether the number of turns editor is enabled.
        /// </summary>
        public ReactivePropertySlim<bool> NumTurnsEnabled { get; } = new(true);

        /// <summary>
        /// Gets the rotation speed configured for this step. The value is validated by <see cref="IValidator"/>.
        /// </summary>
        public ReactiveProperty<int> Speed { get; } = new(0);

        /// <summary>
        /// Gets a value indicating whether the speed editor is enabled.
        /// </summary>
        public ReactivePropertySlim<bool> SpeedEnabled { get; } = new(true);

        /// <summary>
        /// Gets a value indicating whether this step can be deleted.
        /// </summary>
        public ReactivePropertySlim<bool> CanDelete { get; } = new(true);

        /// <summary>
        /// Gets or sets the command executed when the user deletes this step.
        /// </summary>
        public ReactiveCommand? DeleteCommand { get; set; }

        /// <summary>
        /// Gets or sets the callback used by the view to ask the owner whether dragging is allowed right now.
        /// </summary>
        public Func<bool>? QueryCanDrag { get; set; }

        /// <summary>
        /// Gets or sets the callback raised when this step is dropped, passing the target index.
        /// </summary>
        public Action<int>? Dropped { get; set; }

        /// <summary>
        /// Validator used to check the user input of the step parameters.
        /// </summary>
        private readonly IValidator _validator;

        /// <summary>
        /// Holds the subscriptions created by this instance so that they are released on dispose.
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="StepEditorViewModel"/> class
        /// and hooks up input validation for the editable properties.
        /// </summary>
        /// <param name="validator">Validator used to verify the entered values.</param>
        public StepEditorViewModel(
            IValidator validator
        )
        {
            _validator = validator;

            // Validate the number of turns; a null input is validated as the default value.
            NumTurns.SetValidateNotifyError((value) => _validator.Validate(NumTurns, value ?? default)).AddTo(_disposables);

            // Validate the speed value.
            Speed.SetValidateNotifyError((value) => _validator.Validate(Speed, value)).AddTo(_disposables);
        }

        /// <summary>
        /// Releases all subscriptions held by this view model.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _disposables.Dispose();
        }
    }
}
