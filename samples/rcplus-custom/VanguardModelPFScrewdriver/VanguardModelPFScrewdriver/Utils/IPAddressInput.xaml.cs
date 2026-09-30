// -----------------------------------------------------------------------
// <copyright file="IPAddressInput.xaml.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Interaction logic for IPAddressInput.xaml
    /// A custom user control for IP address input with four octet text boxes
    /// </summary>
    public partial class IPAddressInput : UserControl
    {
        /// <summary>
        /// Dependency property for the IP address value
        /// </summary>
        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register(
                nameof(Address),
                typeof(IPAddress),
                typeof(IPAddressInput),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnAddressChanged
                )
            );

        /// <summary>
        /// Gets or sets the IP address value
        /// </summary>
        public IPAddress? Address
        {
            get => (IPAddress?)GetValue(AddressProperty);
            set => SetValue(AddressProperty, value);
        }

        /// <summary>
        /// Dependency property for the clear token
        /// When this token value changes, the IP address input fields are cleared
        /// </summary>
        public static readonly DependencyProperty ClearTokenProperty =
            DependencyProperty.Register(
                nameof(ClearToken),
                typeof(int),
                typeof(IPAddressInput),
                new PropertyMetadata(0, OnClearTokenChanged)
            );

        /// <summary>
        /// Gets or sets the clear token
        /// Increment this value to trigger clearing of all octet text boxes
        /// </summary>
        public int ClearToken
        {
            get => (int)GetValue(ClearTokenProperty);
            set => SetValue(ClearTokenProperty, value);
        }

        public static readonly DependencyProperty OctetsFixedProperty =
            DependencyProperty.Register(
                nameof(OctetsFixed),
                typeof(byte),
                typeof(IPAddressInput),
                new PropertyMetadata((byte)0, OnOctetsFixedChanged)
            );

        public byte OctetsFixed
        {
            get => (byte)GetValue(OctetsFixedProperty);
            set => SetValue(OctetsFixedProperty, value);
        }

        /// <summary>
        /// Array of text boxes representing the four octets of the IP address
        /// </summary>
        private readonly TextBox[] _textBoxes;

        /// <summary>
        /// Array storing the current octet values as strings
        /// </summary>
        private string[] _octets = [string.Empty, string.Empty, string.Empty, string.Empty];

        /// <summary>
        /// Flag to prevent recursive updates when synchronizing UI and property values
        /// </summary>
        private bool _updating = false;

        /// <summary>
        /// Regex pattern for validating numeric input (digits only)
        /// </summary>
        private readonly static Regex _digitsPattern = DigitsOnlyPattern();

        /// <summary>
        /// Sets the octet values in the text boxes
        /// </summary>
        /// <param name="octets">Array of octet strings to set</param>
        private void SetOctets(
            string[] octets
        )
        {
            _updating = true;

            for (int i = 0; i < _octets.Length; i++)
            {
                _octets[i] = (i < octets.Length) ? octets[i] : string.Empty;
                _textBoxes[i].Text = _octets[i];
            }

            _updating = false;
        }

        /// <summary>
        /// Updates the Address property based on the current text box values
        /// </summary>
        private void UpdateAddress()
        {
            _octets = [.. _textBoxes.Select(x => x.Text ?? string.Empty)];
            var addressString = string.Join(".", _octets.Select(x => x ?? string.Empty));

            Address = IPAddress.TryParse(addressString, out var address) ? address : null;
        }

        /// <summary>
        /// Moves focus to the next text box
        /// </summary>
        /// <returns>True if focus was successfully moved, otherwise false</returns>
        private static bool MoveNext()
        {
            if (Keyboard.FocusedElement is not UIElement element)
            {
                return false;
            }

            var request = new TraversalRequest(FocusNavigationDirection.Next);
            element.MoveFocus(request);
            return true;
        }

        /// <summary>
        /// Handles text changed events for octet text boxes
        /// Updates the Address property and auto-advances to next box when 3 digits are entered
        /// </summary>
        /// <param name="sender">The text box that triggered the event</param>
        /// <param name="ev">Event arguments</param>
        private void OnTextChanged(
            object sender,
            TextChangedEventArgs ev
        )
        {
            if (_updating)
            {
                return;
            }

            UpdateAddress();

            if (sender is TextBox textBox && textBox.Text.Length == 3)
            {
                MoveNext();
            }
        }

        /// <summary>
        /// Handles preview text input to validate and filter input
        /// Only allows numeric input and handles period (.) as a navigation key
        /// </summary>
        /// <param name="sender">The text box receiving input</param>
        /// <param name="ev">Event arguments</param>
        private void OnPreviewTextInput(
            object sender,
            TextCompositionEventArgs ev
        )
        {
            if (ev.Text == ".")
            {
                if (MoveNext())
                {
                    ev.Handled = true;
                    return;
                }
            }
            
            if (!_digitsPattern.IsMatch(ev.Text))
            {
                ev.Handled = true;
            }
        }

        /// <summary>
        /// Handles paste operations to support pasting complete IP addresses
        /// Parses and distributes the IP address across the four octet text boxes
        /// </summary>
        /// <param name="sender">The text box where paste was initiated</param>
        /// <param name="ev">Event arguments containing the pasted data</param>
        private void OnPaste(
            object sender,
            DataObjectPastingEventArgs ev
        )
        {
            if (ev.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = ev.DataObject.GetData(DataFormats.Text) as string;
                if (IPAddress.TryParse(text, out var address))
                {
                    var addressBytes = address.GetAddressBytes();
                    if (addressBytes.Length == 4)
                    {
                        SetOctets([.. addressBytes.Select(x => x.ToString())]);
                        return;
                    }
                }
            }

            ev.CancelCommand();
        }

        /// <summary>
        /// Initializes a new instance of the IPAddressInput control
        /// Sets up event handlers and keyboard focus behavior
        /// </summary>
        public IPAddressInput()
        {
            InitializeComponent();

            _textBoxes = [Octet1, Octet2, Octet3, Octet4];

            foreach (var textBox in _textBoxes)
            {
                textBox.TextChanged += OnTextChanged;
                textBox.PreviewTextInput += OnPreviewTextInput;
            }

            DataObject.AddPastingHandler(Octet1, OnPaste);

            GotKeyboardFocus += (_, ev) =>
            {
                base.OnGotKeyboardFocus(ev);

                if (ev.OriginalSource == this)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        _textBoxes[0].Focus();
                    });
                }
            };
        }

        /// <summary>
        /// Callback invoked when the Address dependency property changes
        /// Synchronizes the text boxes with the new address value
        /// </summary>
        /// <param name="d">The IPAddressInput instance</param>
        /// <param name="ev">Event arguments containing the old and new values</param>
        private static void OnAddressChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            if (d is IPAddressInput addressInput)
            {
                if (ev.NewValue is not IPAddress address)
                {
                    return;
                }

                var addressBytes = address.GetAddressBytes();
                if (addressBytes.Length != 4)
                {
                    addressInput.SetOctets([]);
                    return;
                }

                addressInput.SetOctets([.. addressBytes.Select(x => x.ToString())]);
            }
        }

        /// <summary>
        /// Callback invoked when the ClearToken dependency property changes
        /// Clears all octet text boxes whenever the token value is updated
        /// </summary>
        /// <param name="d">The IPAddressInput instance</param>
        /// <param name="ev">Event arguments containing the old and new token values</param>
        private static void OnClearTokenChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            if (d is IPAddressInput addressInput)
            {
                addressInput.SetOctets([]);
            }
        }

        /// <summary>
        /// Callback invoked when the OctetsFixed dependency property changes
        /// Enables or disables each octet text box according to the new mask
        /// </summary>
        /// <param name="d">The IPAddressInput instance</param>
        /// <param name="ev">Event arguments containing the old and new mask values</param>
        private static void OnOctetsFixedChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            if (d is IPAddressInput addressInput)
            {
                if (ev.NewValue is byte fixedOctets)
                {
                    // A set bit marks the octet as fixed, so the text box is disabled.
                    for (int i = 0; i < addressInput._textBoxes.Length; i++)
                    {
                        addressInput._textBoxes[i].IsEnabled = ((fixedOctets & (1 << i)) == 0);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the compiled regular expression that matches numeric input only
        /// </summary>
        /// <returns>The generated regular expression</returns>
        [GeneratedRegex(@"^\d+")]
        private static partial Regex DigitsOnlyPattern();
    }
}
