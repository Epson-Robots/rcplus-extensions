// -----------------------------------------------------------------------
// <copyright file="EditProgramWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Text.RegularExpressions;
using VanguardModelPFScrewdriver.Dialogs.EditWindow;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using VanguardModelPFScrewdriver.Utils;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

// Tuple alias describing the editing capabilities of a step editor row.
using StepEditorType = (bool CanDrag, bool NumTrunsEnabled, bool CanDelete);

namespace VanguardModelPFScrewdriver.Dialogs.EditProgram
{
    /// <summary>
    /// View model for the "Edit Program" dialog.
    /// Provides editing of a single ModelPF program (tightening / loosening / free-run
    /// settings, steps and special parameters) and writes the result back to the
    /// <see cref="ModelPFData"/> instance when the user confirms the dialog.
    /// </summary>
    internal partial class EditProgramWindowViewModel : DialogViewModel, IValidator
    {
        /// <summary>
        /// Regular expression that accepts only alphanumeric characters (used for the program name).
        /// </summary>
        [GeneratedRegex(@"^[a-zA-Z0-9]*$")]
        private static partial Regex AlnumPattern();

        /// <summary>Gets the program number currently being edited.</summary>
        public ReactivePropertySlim<int> ProgramNo { get; } = new();

        /// <summary>Gets the tool type the program is targeted for.</summary>
        public ReactivePropertySlim<ToolKind> ToolType { get; } = new();

        /// <summary>Gets the torque unit used for display and input.</summary>
        public ReactivePropertySlim<TorqueUnitKind> TorqueUnit { get; } = new();

        /// <summary>Gets the number of decimal places used to display torque values.</summary>
        public ReactivePropertySlim<int> TorqueDecimalPlaces { get; } = new();

        /// <summary>Gets the program name. Validated against length and alphanumeric rules.</summary>
        public ReactiveProperty<string> Name { get; } = new();

        /// <summary>Gets the hint text shown for the focused input control.</summary>
        public ReactivePropertySlim<string> Hint { get; } = new();

        /// <summary>Gets or sets the valid value range of the focused input control.</summary>
        public FloatRange HintRange { get; set; } = new();

        /// <summary>Gets the selectable rotation directions.</summary>
        public ReactiveCollection<DirectionKind> Directions { get; } =
        [
            DirectionKind.CW,
            DirectionKind.CCW,
        ];

        /// <summary>Gets the rotation direction used for tightening.</summary>
        public ReactivePropertySlim<DirectionKind> SelectedTighteningDirection { get; } = new(DirectionKind.CW);

        /// <summary>Gets the rotation direction used for loosening.</summary>
        public ReactivePropertySlim<DirectionKind> SelectedLooseningDirection { get; } = new(DirectionKind.CCW);

        /// <summary>Gets the rotation direction used for free run.</summary>
        public ReactivePropertySlim<DirectionKind> SelectedFreeRunDirection { get; } = new(DirectionKind.CW);

        /// <summary>Gets the target torque for tightening.</summary>
        public TorqueProperty TighteningTargetTorque { get; } = new();

        /// <summary>Gets the target torque for loosening.</summary>
        public TorqueProperty LooseningTargetTorque { get; } = new();

        /// <summary>Gets the command that appends a tightening step.</summary>
        public ReactiveCommand TighteningAddStepCommand { get; }

        /// <summary>Gets a value indicating whether another tightening step can be added.</summary>
        public ReactivePropertySlim<bool> TighteningCanAddStep { get; } = new(true);

        /// <summary>Gets the command that appends a loosening step.</summary>
        public ReactiveCommand LooseningAddStepCommand { get; }

        /// <summary>Gets a value indicating whether another loosening step can be added.</summary>
        public ReactivePropertySlim<bool> LooseningCanAddStep { get; } = new(true);

        /// <summary>Gets the command that appends a free-run step.</summary>
        public ReactiveCommand FreeRunAddStepCommand { get; }

        /// <summary>Gets a value indicating whether another free-run step can be added.</summary>
        public ReactivePropertySlim<bool> FreeRunCanAddStep { get; } = new(true);

        /// <summary>Gets the tightening step slots. A <c>null</c> element means the slot is empty.</summary>
        public ReactiveCollection<StepEditorViewModel?> TighteningSteps { get; } = [null, null, null, null, null, null, null, null, null];

        /// <summary>Gets the loosening step slots. A <c>null</c> element means the slot is empty.</summary>
        public ReactiveCollection<StepEditorViewModel?> LooseningSteps { get; } = [null, null];

        /// <summary>Gets the free-run step slots. A <c>null</c> element means the slot is empty.</summary>
        public ReactiveCollection<StepEditorViewModel?> FreeRunSteps { get; } = [null];

        /// <summary>Editor capabilities applied to tightening steps.</summary>
        private readonly StepEditorType TighteningStepEditorType = (true, true, true);

        /// <summary>Editor capabilities applied to loosening steps.</summary>
        private readonly StepEditorType LooseningStepEditorType = (true, true, true);

        /// <summary>Editor capabilities applied to free-run steps (fixed single step, speed only).</summary>
        private readonly StepEditorType FreeRunStepEditorType = (false, false, false);

        /// <summary>Gets the sampling angle used during tightening.</summary>
        public ReactiveProperty<int> SamplingAngle { get; } = new(1);

        /// <summary>Gets the torque adjustment rate applied to the measured torque.</summary>
        public ReactiveProperty<int> TorqueAdjustRate { get; } = new(0);

        /// <summary>Gets the command that opens the judgement window editor dialog.</summary>
        public ReactiveCommand EditWindowCommand { get; } = new();

        /// <summary>Gets the selectable tightening modes.</summary>
        public ReactiveCollection<ModeKind> Modes { get; } =
        [
            ModeKind.Normal,
            ModeKind.Tapping,
            ModeKind.Fast,
        ];

        /// <summary>Gets the currently selected tightening mode.</summary>
        public ReactivePropertySlim<ModeKind> SelectedMode { get; } = new(ModeKind.Normal);

        /// <summary>Gets the mode dependent special parameters shown in the tightening tab.</summary>
        public ReactiveCollection<TighteningSpecialParameter> TighteningSpecialParameters { get; } =
        [
            new StartDetectionAmountParameter(),
            new InitialTappingTorqueParameter(),
            new TorqueUpDetectionTimeParameter(),
            new PermissibleTurnsBelowParameter(),
            new PermssibleTurnsAboveParameter(),
            new FurtherTighteningAngleParameter(),
            new FurtherTighteningAngleForProtrudingParameter(),
            new TorqueThresholdToChangeSpeedParameter(),
            new SpeedAfterChangedParameter(),
        ];

        /// <summary>Gets a value indicating whether the program is uploaded to the tool on OK.</summary>
        public ReactivePropertySlim<bool> UploadThisProgram { get; } = new(false);

        /// <summary>Gets a value indicating whether uploading is available (i.e. a client is connected).</summary>
        public ReactivePropertySlim<bool> CanUploadThisProgram { get; } = new(false);

        /// <summary>
        /// Creates a step editor view model, initializes it with default values and
        /// stores it into the specified slot of <paramref name="steps"/>.
        /// </summary>
        /// <param name="steps">Step slot collection to store the created view model in.</param>
        /// <param name="canAdd">Flag updated when the step count changes.</param>
        /// <param name="editorType">Editing capabilities to apply to the step.</param>
        /// <param name="index">Slot index to store the created view model at.</param>
        /// <returns>The created step editor view model.</returns>
        private StepEditorViewModel SetStep(
            ReactiveCollection<StepEditorViewModel?> steps,
            ReactivePropertySlim<bool> canAdd,
            StepEditorType editorType,
            int index
        )
        {
            const float _numTurnsMin = 0.1f;
            const int _speedMin = 10;

            StepEditorViewModel viewModel = new(this);

            viewModel.CanDrag.Value = editorType.CanDrag;
            viewModel.NumTurnsEnabled.Value = editorType.NumTrunsEnabled;
            if (editorType.NumTrunsEnabled)
            {
                viewModel.NumTurns.Value = _numTurnsMin;
            }
            viewModel.SpeedEnabled.Value = true;
            viewModel.Speed.Value = _speedMin;

            viewModel.CanDelete.Value = editorType.CanDelete;
            viewModel.DeleteCommand = viewModel.CanDelete
                .ToReactiveCommand()
                .WithSubscribe(() => OnDeleteStep(steps, canAdd, viewModel))
                .AddTo(_disposables);

            // Dragging is only meaningful when more than one step exists.
            viewModel.QueryCanDrag = () =>
            {
                return steps.Count(x => x != null) > 1;
            };

            // Reorder the step according to the drop position.
            viewModel.Dropped = (dropPositionIndex) =>
            {
                var oldIndex = steps.IndexOf(viewModel);
                var newIndex = (oldIndex < dropPositionIndex) ? dropPositionIndex - 1 : dropPositionIndex;
                if (oldIndex != newIndex)
                {
                    steps.Move(oldIndex, newIndex);
                }
            };

            steps[index] = viewModel;

            return viewModel;
        }

        /// <summary>
        /// Adds a new step into the first empty slot of the specified step collection.
        /// </summary>
        /// <param name="steps">Step slot collection to add to.</param>
        /// <param name="canAdd">Flag updated after the step has been added.</param>
        /// <param name="editorType">Editing capabilities to apply to the new step.</param>
        private void OnAddStep(
            ReactiveCollection<StepEditorViewModel?> steps,
            ReactivePropertySlim<bool> canAdd,
            StepEditorType editorType
        )
        {
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index] == null)
                {
                    _ = SetStep(steps, canAdd, editorType, index);
                    break;
                }
            }

            UpdateCanAddStep(steps, canAdd);
        }

        /// <summary>Adds a tightening step.</summary>
        private void OnTighteningAddStep() => OnAddStep(TighteningSteps, TighteningCanAddStep, TighteningStepEditorType);

        /// <summary>Adds a loosening step.</summary>
        private void OnLooseningAddStep() => OnAddStep(LooseningSteps, LooseningCanAddStep, LooseningStepEditorType);

        /// <summary>Adds a free-run step.</summary>
        private void OnFreeRunAddStep() => OnAddStep(FreeRunSteps, FreeRunCanAddStep, FreeRunStepEditorType);

        /// <summary>
        /// Updates the "can add step" flag depending on whether an empty slot is left.
        /// </summary>
        private static void UpdateCanAddStep(
            ReactiveCollection<StepEditorViewModel?> steps,
            ReactivePropertySlim<bool> canAdd
        )
        {
            canAdd.Value = steps.Any(x => x == null);
        }

        /// <summary>Updates the "can add step" flag of the tightening steps.</summary>
        private void UpdateTighteningCanAddStep() => UpdateCanAddStep(TighteningSteps, TighteningCanAddStep);

        /// <summary>Updates the "can add step" flag of the loosening steps.</summary>
        private void UpdateLooseningCanAddStep() => UpdateCanAddStep(LooseningSteps, LooseningCanAddStep);

        /// <summary>Updates the "can add step" flag of the free-run steps.</summary>
        private void UpdateFreeRunCanAddStep() => UpdateCanAddStep(FreeRunSteps, FreeRunCanAddStep);

        /// <summary>
        /// Removes the specified step and shifts the following steps forward so that
        /// the used slots stay contiguous from the beginning of the collection.
        /// </summary>
        /// <param name="steps">Step slot collection to remove from.</param>
        /// <param name="canAdd">Flag updated after the step has been removed.</param>
        /// <param name="viewModel">Step editor view model to remove.</param>
        private static void OnDeleteStep(
            ReactiveCollection<StepEditorViewModel?> steps,
            ReactivePropertySlim<bool> canAdd,
            StepEditorViewModel viewModel
        )
        {
            var index = steps.IndexOf(viewModel);
            steps[index] = null;

            // Shift the remaining steps forward to fill the gap.
            for (index++; index < steps.Count; index++)
            {
                if (steps[index] == null)
                {
                    break;
                }
                steps[index - 1] = steps[index];
                steps[index] = null;
            }

            UpdateCanAddStep(steps, canAdd);
        }

        /// <summary>
        /// Opens the judgement window editor dialog for the current tightening mode.
        /// </summary>
        private void OnEditWindow()
        {
            if (_proFuseData != null)
            {
                if (_parameters != null)
                {
                    _parameters["Mode"] = SelectedMode.Value;
                }

                _ = ShowDialog<EditWindowWindow>(_proFuseData, parameters: _parameters);
            }
        }

        /// <summary>Snapshot of the program taken when the dialog was opened (used to detect changes).</summary>
        private Program? _original;

        /// <summary>
        /// Loads the specified program into the view model properties.
        /// </summary>
        /// <param name="program">Program to load.</param>
        private void Set(
            Program program
        )
        {
            _original = program.Clone();

            Name.Value = program.Name;

            SelectedTighteningDirection.Value = program.Tightening.Direction;
            TighteningTargetTorque.InnerValue = program.Tightening.TargetTorque;

            int stepIndex;

            // Tightening steps: a step with zero turns marks the end of the valid steps.
            stepIndex = 0;
            foreach (var step in program.Tightening.Steps)
            {
                if (step.NumTurns == 0f)
                {
                    break;
                }
                var viewModel = SetStep(TighteningSteps, TighteningCanAddStep, TighteningStepEditorType, stepIndex);
                viewModel.NumTurns.Value = step.NumTurns;
                viewModel.Speed.Value = step.Speed;
                stepIndex++;
            }

            SelectedMode.Value = program.Tightening.Mode;
            foreach (var tighteningSpecialParameter in TighteningSpecialParameters)
            {
                tighteningSpecialParameter.Set(program.Tightening);
            }
            SamplingAngle.Value = program.Tightening.SamplingAngle;
            TorqueAdjustRate.Value = program.Tightening.TorqueAdjustRate;

            SelectedLooseningDirection.Value = program.Loosening.Direction;
            LooseningTargetTorque.InnerValue = program.Loosening.TargetTorque;

            // Loosening steps: same termination rule as the tightening steps.
            stepIndex = 0;
            foreach (var step in program.Loosening.Steps)
            {
                if (step.NumTurns == 0f)
                {
                    break;
                }
                var viewModel = SetStep(LooseningSteps, LooseningCanAddStep, LooseningStepEditorType, stepIndex);
                viewModel.NumTurns.Value = step.NumTurns;
                viewModel.Speed.Value = step.Speed;
                stepIndex++;
            }

            SelectedFreeRunDirection.Value = program.FreeRun.Direction;

            // Free run has speed only, so every step is loaded unconditionally.
            stepIndex = 0;
            foreach (var step in program.FreeRun.Steps)
            {
                var viewModel = SetStep(FreeRunSteps, FreeRunCanAddStep, FreeRunStepEditorType, stepIndex);
                viewModel.Speed.Value = step.Speed;
                stepIndex++;
            }
        }

        /// <summary>
        /// Writes the current view model values back to the specified program.
        /// Empty step slots are reset to their default values.
        /// </summary>
        /// <param name="program">Program to update.</param>
        private void SetBack(
            Program program
        )
        {
            program.Name = Name.Value;

            program.Tightening.Direction = SelectedTighteningDirection.Value;
            program.Tightening.TargetTorque = TighteningTargetTorque.InnerValue;

            foreach (var (step, index) in program.Tightening.Steps.Select((x, index) => (x, index)))
            {
                var viewModel = TighteningSteps[index];
                if (viewModel != null && viewModel.NumTurns.Value != 0f)
                {
                    step.NumTurns = viewModel.NumTurns.Value ?? 0f;
                    step.Speed = viewModel.Speed.Value;
                }
                else
                {
                    // Unused slot: clear the step.
                    step.NumTurns = 0f;
                    step.Speed = 10;
                }
            }

            program.Tightening.Mode = SelectedMode.Value;
            foreach (var tighteningSpecialParameter in TighteningSpecialParameters)
            {
                tighteningSpecialParameter.SetBack(program.Tightening);
            }
            program.Tightening.SamplingAngle = SamplingAngle.Value;
            program.Tightening.TorqueAdjustRate = TorqueAdjustRate.Value;

            // Apply the judgement windows edited in the window editor dialog, if any.
            if (_parameters?.TryGetValue("NewWindows", out var list) == true && list is List<ModelPFData.Program.Window> windows)
            {
                foreach (var (window, index) in windows.Select((x, index) => (x, index)))
                {
                    program.Tightening.Windows[index] = window;
                }
            }

            program.Loosening.Direction = SelectedLooseningDirection.Value;
            program.Loosening.TargetTorque = LooseningTargetTorque.InnerValue; ;

            foreach (var (step, index) in program.Loosening.Steps.Select((x, index) => (x, index)))
            {
                var viewModel = LooseningSteps[index];
                if (viewModel != null && viewModel.NumTurns.Value != 0f)
                {
                    step.NumTurns = viewModel.NumTurns.Value ?? 0f;
                    step.Speed = viewModel.Speed.Value;
                }
                else
                {
                    // Unused slot: clear the step.
                    step.NumTurns = 0f;
                    step.Speed = 10;
                }
            }

            program.FreeRun.Direction = SelectedFreeRunDirection.Value;

            foreach (var (step, index) in program.FreeRun.Steps.Select((x, index) => (x, index)))
            {
                var viewModel = FreeRunSteps[index];
                if (viewModel != null && viewModel.NumTurns.Value != 0f)
                {
                    step.NumTurns = viewModel.NumTurns.Value ?? 0f;
                    step.Speed = viewModel.Speed.Value;
                }
                else
                {
                    // Unused slot: clear the step.
                    step.NumTurns = 0f;
                    step.Speed = 10;
                }
            }
        }

        /// <summary>
        /// Handles the Cancel action. Asks the user for confirmation when the edited
        /// values differ from the values loaded when the dialog was opened.
        /// </summary>
        protected override void OnCancel()
        {
            if (_proFuseData != null && _original != null)
            {
                // Apply the current values to a clone to compare against the original.
                Program program = _original.Clone();
                SetBack(program);

                if (!_original.Equals(program))
                {
                    var response = Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.DiscardChangesWarning),
                        ButtonType.Yes_No,
                        IconType.Warning,
                        _window
                    );
                    if (response == ResponseType.No)
                    {
                        return;
                    }
                }
            }

            base.OnCancel();
        }

        /// <summary>
        /// Handles the OK action. Writes the edited values back to the project data and
        /// optionally uploads the program to the connected tool.
        /// </summary>
        protected override async Task OnOK()
        {
            if (_proFuseData != null && _parameters!["TargetProgram"] is ProgramItemViewModel targetProgramItem)
            {
                Program program = _proFuseData.ProgramData[targetProgramItem.ProgramNo];
                SetBack(program);

                _proFuseData.SetDirty();

                if (_proFuseClient != null && UploadThisProgram.Value)
                {
                    await ProjectFileEditorViewModel.UploadProgram(_proFuseClient, program, reportCompleted: false);
                }
            }

            await base.OnOK();
        }

        /// <summary>
        /// Initializes the dialog contents from the dialog parameters and sets up validation.
        /// </summary>
        protected override void Setup()
        {
            if (_proFuseData != null && _parameters!["TargetProgram"] is ProgramItemViewModel targetProgramItem)
            {
                ProgramNo.Value = targetProgramItem.ProgramNo;
                ToolType.Value = (ToolKind)_parameters!["ToolType"]!;
                TorqueUnit.Value = _proFuseData.PreferenceData.TorqueUnit;
                TorqueDecimalPlaces.Value = TorqueValueConverter.DecimalPlaces(TorqueUnit.Value);

                Set(_proFuseData.ProgramData[targetProgramItem.ProgramNo]);

                TighteningTargetTorque.TorqueUnit = _proFuseData.PreferenceData.TorqueUnit;
                LooseningTargetTorque.TorqueUnit = _proFuseData.PreferenceData.TorqueUnit;
                TighteningSpecialParameter.TorqueUnit.Value = _proFuseData.PreferenceData.TorqueUnit;

                UpdateTighteningCanAddStep();
                UpdateLooseningCanAddStep();
                UpdateFreeRunCanAddStep();

                // Uploading is only possible while a tool client is available.
                UploadThisProgram.Value = (_proFuseClient != null);
                CanUploadThisProgram.Value = (_proFuseClient != null);

                // Resolve the allowed program name length from the hint definition.
                var minLen = 0;
                var maxLen = 16;
                var nameEntry = HintData.Instance.Find(
                    HintId.ProgramName,
                    ToolKind.S,
                    ModeKind.Normal
                );
                if (nameEntry != null)
                {
                    minLen = nameEntry.Min;
                    maxLen = nameEntry.Max;
                }
                Name.SetValidateNotifyError((name) =>
                {
                    if (
                        name != null
                        && (name.Length < minLen || maxLen < name.Length || !AlnumPattern().IsMatch(name))
                    )
                    {
                        return "InvalidName";
                    }

                    return null;
                })
                .AddTo(_disposables);
            }
        }

        // ======================================================================

        /// <summary>Targets (input controls) that currently hold an invalid value.</summary>
        private readonly HashSet<object> _invalidTargets = [];

        /// <summary>Gets a value indicating whether at least one input holds an invalid value.</summary>
        public bool HasInvalidTarget => (_invalidTargets.Count > 0);

        /// <summary>
        /// Registers or unregisters the specified target as holding an invalid value.
        /// </summary>
        /// <param name="target">Target to update.</param>
        /// <param name="isValid"><c>true</c> when the target holds a valid value.</param>
        public void SetValid(
            object target,
            bool isValid
        )
        {
            if (isValid)
            {
                _invalidTargets.Remove(target);
            }
            else
            {
                _invalidTargets.Add(target);
            }
        }

        /// <summary>
        /// Validates the value of the specified target against the current hint range
        /// and updates the enabled state of the OK button.
        /// </summary>
        /// <param name="target">Target to validate.</param>
        /// <param name="value">Value to validate.</param>
        /// <returns>An error key when the value is invalid; otherwise <c>null</c>.</returns>
        public string? Validate(
            object target,
            object value
        )
        {
            try
            {
                if (Convert.ChangeType(value, typeof(float)) is float floatValue)
                {
                    var isValid = HintRange.IsInRange(floatValue);

                    SetValid(target, isValid);
                    CanOK.Value = !HasInvalidTarget;

                    if (!isValid)
                    {
                        return "ValueOutOfRange";
                    }
                }
            }
            catch (Exception)
            {
                // Values that cannot be converted to a number are not validated here.
                // EMPTY
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditProgramWindowViewModel"/> class,
        /// wiring up the commands and the validation of the editable values.
        /// </summary>
        public EditProgramWindowViewModel()
        {
            TighteningAddStepCommand = TighteningCanAddStep
                .ToReactiveCommand()
                .WithSubscribe(OnTighteningAddStep)
                .AddTo(_disposables);
            LooseningAddStepCommand = LooseningCanAddStep
                .ToReactiveCommand()
                .WithSubscribe(OnLooseningAddStep)
                .AddTo(_disposables);
            FreeRunAddStepCommand = FreeRunCanAddStep
                .ToReactiveCommand()
                .WithSubscribe(OnFreeRunAddStep)
                .AddTo(_disposables);

            EditWindowCommand.Subscribe(OnEditWindow).AddTo(_disposables);

            UpdateTighteningCanAddStep();
            UpdateLooseningCanAddStep();
            UpdateFreeRunCanAddStep();

            // Special parameters depend on the selected tightening mode.
            SelectedMode.Subscribe(mode => TighteningSpecialParameter.Mode.Value = mode).AddTo(_disposables);

            TighteningTargetTorque.SetValidator(this);
            LooseningTargetTorque.SetValidator(this);
            SamplingAngle.SetValidateNotifyError((value) => Validate(SamplingAngle, value)).AddTo(_disposables);
            TorqueAdjustRate.SetValidateNotifyError((value) => Validate(TorqueAdjustRate, value)).AddTo(_disposables);
            foreach (var parameter in TighteningSpecialParameters)
            {
                parameter.SetValiator(this);
            }
        }
    }
}
