// -----------------------------------------------------------------------
// <copyright file="ControllerSettingValueExtension.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using static Epson.RoboticsShared.ExtensionsAPI.IRCXControllerAPI;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Provides extension methods for reading values from a controller setting dictionary.
    /// </summary>
    public static class ControllerSettingValueExtension
    {
        /// <summary>
        /// Gets the value associated with the specified key, but only when the stored
        /// entry matches the expected value type.
        /// </summary>
        /// <param name="settings">The controller settings keyed by setting name.</param>
        /// <param name="keyName">The name of the setting to look up.</param>
        /// <param name="valueType">The value type the setting is expected to have.</param>
        /// <returns>
        /// The setting value when the key exists and its type matches <paramref name="valueType"/>, 
        /// otherwise, <see langword="null"/>.
        /// </returns>
        public static object? GetValue(
            this IDictionary<string, ControllerSettingValue> settings,
            string keyName,
            Type valueType
        )
        {
            // Return the value only if the key exists and the declared type matches.
            if (
                settings.TryGetValue(keyName, out var entry)
                && entry.ValueType == valueType
            )
            {
                return entry.Value;
            }

            // Key missing or type mismatch: let the caller decide on a fallback.
            return null;
        }
    }
}
