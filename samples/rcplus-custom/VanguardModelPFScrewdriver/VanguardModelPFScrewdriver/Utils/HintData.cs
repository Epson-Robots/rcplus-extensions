// -----------------------------------------------------------------------
// <copyright file="HintData.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Hint definitions loaded from the <c>Resources/Hints.json</c> resource file.
    /// A hint is selected by its <see cref="HintId"/> and by a key built from
    /// the tool type and the tightening mode.
    /// </summary>
    public class HintData
    {
        /// <summary>
        /// A single hint definition describing the valid value range of an input field.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Gets or sets the lower limit of the valid range (raw value).
            /// </summary>
            public int Min { get; set; }

            /// <summary>
            /// Gets or sets the upper limit of the valid range (raw value).
            /// </summary>
            public int Max { get; set; }

            /// <summary>
            /// Gets or sets the scaling factor used to convert the raw values into
            /// displayed values. Zero means that no scaling is applied.
            /// </summary>
            public int Factor { get; set; }

            /// <summary>
            /// Gets or sets the caption key of the localized format string.
            /// When empty, <see cref="Format"/> is used instead.
            /// </summary>
            public string Caption { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the identifier of the unit in which the values are expressed
            /// (e.g. "Torque"). It is resolved into the unit text inserted into the hint.
            /// </summary>
            public string Unit { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the literal format string used when no <see cref="Caption"/> is specified.
            /// </summary>
            public string Format {  get; set; } = string.Empty;

            /// <summary>
            /// Holds the current unit settings keyed by the unit identifier (see <see cref="Unit"/>).
            /// The application updates this table so that the hints follow the user preferences.
            /// </summary>
            [JsonIgnore]
            public static readonly Dictionary<string, object> UnitInfo = [];

            /// <summary>
            /// Converter used to express the torque limits in the currently selected torque unit.
            /// </summary>
            private static readonly TorqueValueConverter _torqueValueConverter = new();

            /// <summary>
            /// Converter used to obtain the localized name of the currently selected torque unit.
            /// </summary>
            private static readonly TorqueUnitLocalizer _torqueUnitLocalizer = new();

            /// <summary>
            /// Builds the hint text by formatting the limits with the localized or literal format string.
            /// </summary>
            /// <param name="range">
            /// Receives the valid value range in the same scale as the displayed values.
            /// </param>
            /// <returns>The formatted hint text.</returns>
            public string GetHint(
                ref FloatRange range
            )
            {
                // Prefer the localized caption over the literal format string.
                string format = string.IsNullOrEmpty(Caption) ? Format : Main.Captions![Caption].Value;

                string unitExpr = string.Empty;
                string minExpr;
                string maxExpr;

                if (Factor == 0)
                {
                    // No scaling: show the raw values.
                    minExpr = Min.ToString();
                    maxExpr = Max.ToString();

                    range.Min = Min;
                    range.Max = Max;
                }
                else
                {
                    // Scale the raw limits into the displayed values.
                    float min = (float)Min / (float)Factor;
                    float max = (float)Max / (float)Factor;

                    if (
                        Unit == "Torque"
                        && UnitInfo.TryGetValue(Unit, out var info)
                        && info is TorqueUnitKind torqueUnit
                    )
                    {
                        // Torque limits must be converted into the unit currently selected by the user.
                        float minOrig = min;
                        float maxOrig = max;

                        min = (float)_torqueValueConverter.Convert([minOrig, torqueUnit], typeof(float), null!, null!);
                        max = (float)_torqueValueConverter.Convert([maxOrig, torqueUnit], typeof(float), null!, null!);

                        // The string conversion also applies the number of decimal places of the unit.
                        minExpr = (string)_torqueValueConverter.Convert([minOrig, torqueUnit], typeof(string), null!, null!);
                        maxExpr = (string)_torqueValueConverter.Convert([maxOrig, torqueUnit], typeof(string), null!, null!);

                        unitExpr = (string)_torqueUnitLocalizer.Convert(torqueUnit, typeof(string), null!, null!);
                    }
                    else
                    {
                        // The factor is a power of ten, so it also defines the number of decimal places.
                        var digits = (int)Math.Log10(Factor);
                        minExpr = min.ToString($"F{digits}");
                        maxExpr = max.ToString($"F{digits}");
                    }

                    range.Min = min;
                    range.Max = max;
                }

                // {0}: lower limit, {1}: upper limit, {2}: unit text.
                return string.Format(format, minExpr, maxExpr, unitExpr);
            }
        }

        /// <summary>
        /// Gets or sets the raw hint definitions as deserialized from JSON.
        /// The outer key is a <see cref="HintId"/> name, the inner key is a regular
        /// expression pattern matching the "toolType_mode" key.
        /// </summary>
        public Dictionary<string, Dictionary<string, Entry>> Hints { get; set; } = [];

        /// <summary>
        /// Gets or sets the aliases between hint identifiers.
        /// The key reuses the definitions of the referenced identifier.
        /// </summary>
        public Dictionary<string, string> References { get; set; } = [];

        /// <summary>
        /// Gets or sets the prepared hint definitions with parsed identifiers and compiled patterns.
        /// Built by <see cref="Cook"/> and not serialized.
        /// </summary>
        [JsonIgnore]
        public Dictionary<HintId, Dictionary<Regex, Entry>> CookedHints { get; set; } = [];

        /// <summary>
        /// The singleton instance created when the type is first used.
        /// </summary>
        private static readonly HintData _hintData;

        /// <summary>
        /// Gets the shared hint definitions.
        /// </summary>
        public static HintData Instance => _hintData;

        /// <summary>
        /// Initializes the singleton by loading and preparing the hint definitions.
        /// An empty instance is used when the resource file cannot be loaded.
        /// </summary>
        static HintData()
        {
            _hintData = Load() ?? new HintData();
            _hintData.Cook();
        }

        /// <summary>
        /// Converts the raw JSON data into the lookup structure used by <see cref="Find"/>
        /// and resolves the references between hint identifiers.
        /// </summary>
        public void Cook()
        {
            CookedHints.Clear();

            // Convert the string keys into HintId values and anchored regular expressions.
            foreach (var (idStr, dict) in Hints)
            {
                if (Enum.TryParse<HintId>(idStr, out var id))
                {
                    Dictionary<Regex, Entry> cookedDict = [];

                    foreach (var (pattern, entry) in dict)
                    {
                        // Anchor the pattern so that it matches the whole key.
                        Regex regex = new($"^{pattern}$");
                        cookedDict[regex] = entry;
                    }

                    CookedHints[id] = cookedDict;
                }
            }

            // Let the aliases share the definitions of the referenced identifier.
            foreach (var (idStr, referredIdStr) in References)
            {
                if (
                    Enum.TryParse<HintId>(idStr, out var id)
                    && Enum.TryParse<HintId>(referredIdStr, out var referedId)
                )
                {
                    // An explicit definition always wins over the reference.
                    if (!CookedHints.ContainsKey(id) && CookedHints.TryGetValue(referedId, out Dictionary<Regex, Entry>? value))
                    {
                        CookedHints[id] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the hint entry matching the specified identifier, tool type and mode.
        /// </summary>
        /// <param name="id">The hint identifier.</param>
        /// <param name="toolType">The connected tool type.</param>
        /// <param name="mode">The tightening mode.</param>
        /// <returns>The matching entry, or <c>null</c> when no entry matches.</returns>
        public Entry? Find(
            HintId id,
            ToolKind toolType,
            ModeKind mode
        )
        {
            if (CookedHints.TryGetValue(id, out var cookedDict))
            {
                // The lookup key is the tool type and the first letter of the mode (e.g. "SD_N").
                var key = $"{toolType}_{mode.ToString()[0]}";

                // The first matching pattern wins, so the definition order is significant.
                foreach (var (regex, entry) in cookedDict)
                {
                    if (regex.IsMatch(key))
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the full path of the hint definition file located next to the executing assembly.
        /// </summary>
        /// <returns>The path of <c>Resources/Hints.json</c>.</returns>
        private static string HintDataPath()
        {
            return Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources",
                "Hints.json"
            );
        }

        /// <summary>
        /// Loads the hint definitions from the JSON resource file.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="HintData"/>, or <c>null</c> when the file is missing or invalid.
        /// </returns>
        public static HintData? Load()
        {
            try
            {
                var content = File.ReadAllText(HintDataPath());
                return JsonSerializer.Deserialize<HintData>(content);
            }
            catch (Exception)
            {
                // Hints are optional, so any failure is silently ignored.
                // EMPTY
            }

            return null;
        }
    }
}
