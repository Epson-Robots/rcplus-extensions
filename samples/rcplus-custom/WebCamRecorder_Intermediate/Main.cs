// -----------------------------------------------------------------------
// <copyright file="Main.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Epson.RoboticsShared.ExtensionsAPI;
using Microsoft.Extensions.DependencyInjection;

namespace WebCamRecorder
{
    /// <summary>
    /// Extension : Main
    /// </summary>
    [Export(typeof(IRCXMain))]
    public partial class Main : IRCXMain
    {
        /// <summary>
        /// Extension ID
        /// </summary>
        public const string CommonId = "WebCamRecorder_3d67778c-1fcd-480a-9fca-b30af9ac0bb9";

        /// <inheritdoc />
        public string Id => CommonId;

        /// <summary>
        /// Common icon data
        /// </summary>
        public static ImageSource? CommonIcon { get; private set; }

        /// <inheritdoc />
        public ImageSource? Icon { get; set; }

        /// <inheritdoc />
        public string CaptionFilePath
        {
            get
            {
                var parentDir = Directory.GetParent(GetType().Assembly.Location)!.FullName;
                return Path.Combine(parentDir, "Resources", "Strings");
            }
        }

        /// <summary>
        /// Captions
        /// </summary>
        internal static IRCXCaptionGetter? Captions { get; private set; }

        /// <summary>
        /// Extension settings accessor
        /// </summary>
        internal static IRCXConfiguration? Settings { get; private set; }

        /// <summary>
        /// Container of API objects
        /// </summary>
        private static IServiceProvider? _apiProvider;

        /// <summary>
        /// Get API object of specified interface type
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <returns>API object</returns>
        internal static T GetAPI<T>() where T : notnull
        {
            return _apiProvider!.GetRequiredService<T>();
        }

        /// <inheritdoc />
        public bool Initialize(
            IServiceProvider apiProvider,
            IRCXCaptionGetter captionGetter,
            IRCXConfiguration settings
        )
        {
            _apiProvider = apiProvider;
            Captions = captionGetter;
            Captions.NameToNumber = Constants.Caption.NameToNumber;
            Settings = settings;

            CommonIcon = Load();
            Icon = CommonIcon;

            return true;
        }

        /// <inheritdoc />
        public void Terminate()
        {
        }

        /// <inheritdoc />
        public string GetDefaultDialogTitle()
        {
            return Captions?[Constants.Caption.ExtensionName] ?? string.Empty;
        }

        /// <summary>
        /// Load icon data
        /// </summary>
        /// <returns>Icon data or null if error</returns>
        private BitmapSource? Load()
        {
            var parentDir = Directory.GetParent(GetType().Assembly.Location)!.FullName;
            var iconPath = Path.Combine(parentDir, "Resources", "Extension.png");
            if (File.Exists(iconPath))
            {
                var image = new Bitmap(iconPath);
                return Imaging.CreateBitmapSourceFromHBitmap(
                    image.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }

            return null;
        }
    }
}
