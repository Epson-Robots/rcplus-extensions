// -----------------------------------------------------------------------
// <copyright file="CameraInfo.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WebCamRecorder
{
    /// <summary>
    /// Camera information
    /// </summary>
    public class CameraInfo
    {
        /// <summary>
        /// Friendly name (may not be unique)
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// Unique symbolic link
        /// </summary>
        public string SymbolicLink { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="friendlyName">Friendly name</param>
        /// <param name="symbolicLink">Symbolic link</param>
        public CameraInfo(
            string friendlyName,
            string symbolicLink
        )
        {
            FriendlyName = friendlyName;
            SymbolicLink = symbolicLink;
        }
    }
}
