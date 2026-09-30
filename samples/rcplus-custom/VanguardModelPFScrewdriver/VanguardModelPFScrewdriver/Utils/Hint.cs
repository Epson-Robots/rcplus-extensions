// -----------------------------------------------------------------------
// <copyright file="Hint.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Attached properties that provide the context sensitive hint (help) information for UI elements.
    /// The hint is resolved from <see cref="IdProperty"/>, <see cref="ToolTypeProperty"/> and
    /// <see cref="ModeProperty"/>, and the results are exposed through the read-only
    /// <see cref="TextProperty"/> and <see cref="RangeProperty"/>.
    /// </summary>
    public static class Hint
    {
        /// <summary>
        /// Key of the read-only Text attached property.
        /// </summary>
        private static readonly DependencyPropertyKey TextKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "Text",
                typeof(string),
                typeof(Hint),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits)
            );

        /// <summary>
        /// Identifies the read-only Text attached property that holds the resolved hint text.
        /// The value is inherited by the child elements.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            TextKey.DependencyProperty;

        /// <summary>
        /// Gets the resolved hint text of the specified object.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns>The hint text, or an empty string when no hint was found.</returns>
        public static string GetText(DependencyObject d) => (string)d.GetValue(TextProperty);

        /// <summary>
        /// Key of the read-only Range attached property.
        /// </summary>
        private static readonly DependencyPropertyKey RangeKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "Range",
                typeof(FloatRange),
                typeof(Hint),
                new FrameworkPropertyMetadata(new FloatRange(), FrameworkPropertyMetadataOptions.Inherits)
            );

        /// <summary>
        /// Identifies the read-only Range attached property that holds the valid input range
        /// associated with the resolved hint. The value is inherited by the child elements.
        /// </summary>
        public static readonly DependencyProperty RangeProperty =
            RangeKey.DependencyProperty;

        /// <summary>
        /// Gets the valid input range of the specified object.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns>The value range associated with the hint.</returns>
        public static FloatRange GetRange(DependencyObject d) => (FloatRange)d.GetValue(RangeProperty);

        /// <summary>
        /// Identifies the Id attached property that selects the hint entry.
        /// </summary>
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.RegisterAttached(
                "Id",
                typeof(HintId),
                typeof(Hint),
                new PropertyMetadata(HintId.None, OnInputChanged)
            );

        /// <summary>
        /// Sets the hint identifier of the specified object.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <param name="id">The hint identifier.</param>
        public static void SetId(DependencyObject d, HintId id) => d.SetValue(IdProperty, id);

        /// <summary>
        /// Gets the hint identifier of the specified object.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns>The hint identifier.</returns>
        public static HintId GetId(DependencyObject d) => (HintId)d.GetValue(IdProperty);

        /// <summary>
        /// Identifies the ToolType attached property.
        /// The value is inherited so that it can be set once on a parent container.
        /// </summary>
        public static readonly DependencyProperty ToolTypeProperty =
            DependencyProperty.RegisterAttached(
                "ToolType",
                typeof(ToolKind),
                typeof(Hint),
                new FrameworkPropertyMetadata(ToolKind.Unknown, FrameworkPropertyMetadataOptions.Inherits, OnInputChanged)
            );

        /// <summary>
        /// Sets the tool type used to resolve the hint.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <param name="toolType">The tool type.</param>
        public static void SetToolType(DependencyObject d, ToolKind toolType) => d.SetValue(ToolTypeProperty, toolType);

        /// <summary>
        /// Gets the tool type used to resolve the hint.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns>The tool type.</returns>
        public static ToolKind GetToolType(DependencyObject d) => (ToolKind)d.GetValue(ToolTypeProperty);

        /// <summary>
        /// Identifies the Mode attached property.
        /// The value is inherited so that it can be set once on a parent container.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached(
                "Mode",
                typeof(ModeKind),
                typeof(Hint),
                new FrameworkPropertyMetadata(ModeKind.Normal, FrameworkPropertyMetadataOptions.Inherits, OnInputChanged)
            );

        /// <summary>
        /// Sets the tightening mode used to resolve the hint.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <param name="mode">The tightening mode.</param>
        public static void SetMode(DependencyObject d, ModeKind mode) => d.SetValue(ModeProperty, mode);

        /// <summary>
        /// Gets the tightening mode used to resolve the hint.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns>The tightening mode.</returns>
        public static ModeKind GetMode(DependencyObject d) => (ModeKind)d.GetValue(ModeProperty);

        /// <summary>
        /// Called when one of the input properties (Id, ToolType or Mode) changes.
        /// Resolves the matching hint and stores the results in the read-only
        /// Text and Range properties.
        /// </summary>
        /// <param name="d">The dependency object the properties are attached to.</param>
        /// <param name="ev">The property change event data.</param>
        private static void OnInputChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            string text = string.Empty;
            FloatRange range = new();

            // Look up the entry matching the current id / tool type / mode combination.
            var entry = HintData.Instance.Find(GetId(d), GetToolType(d), GetMode(d));
            if (entry != null)
            {
                // The entry fills in the valid range while building the hint text.
                text = entry.GetHint(ref range);
            }

            d.SetValue(TextKey, text);
            d.SetValue(RangeKey, range);
        }
    }
}
