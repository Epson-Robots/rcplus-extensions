// -----------------------------------------------------------------------
// <copyright file="LogFieldAttribute.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Marks a property as a field of a log entry so that it is included when
    /// the log is exported. The properties are processed in declaration order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LogFieldAttribute : Attribute
    {
    }
}
