// -----------------------------------------------------------------------
// <copyright file="ViewDirectionControl.xaml.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Controls;
using System.Windows.Media;

namespace SpelAnalysisTool.View3D
{
    /// <summary>
    /// Interaction logic for ViewDirectionControl.xaml
    /// </summary>
    public partial class ViewDirectionControl : UserControl
    {
        private RC3DView? _rc3dview;
        private List<Button> _buttons;
        private Dictionary<RC3DView.ViewCam.OrthographicType, Button> _btnsOrthoType;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewDirectionControl()
        {
            InitializeComponent();

            _buttons = new List<Button>()
            {
                _btnRotL,
                _btnRotR,
                _btnAbove,
                _btnBelow,
                _btnFront,
                _btnRear,
                _btnRightside,
                _btnLeftside,
            };

            _btnsOrthoType = new Dictionary<RC3DView.ViewCam.OrthographicType, Button>()
            {
                { RC3DView.ViewCam.OrthographicType.Above, _btnAbove },
                { RC3DView.ViewCam.OrthographicType.Below, _btnBelow },
                { RC3DView.ViewCam.OrthographicType.Front, _btnFront },
                { RC3DView.ViewCam.OrthographicType.Rear, _btnRear },
                { RC3DView.ViewCam.OrthographicType.Rightside, _btnRightside },
                { RC3DView.ViewCam.OrthographicType.Leftside, _btnLeftside },
            };
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="rc3dview">RC3DView</param>
        public void Init(RC3DView rc3dview)
        {
            _rc3dview = rc3dview;
            _radioBtnPerspective.IsChecked = !rc3dview.VCam.IsOrtho;

            UpdateView();

            _radioBtnPerspective.Checked += (s, e) => { rc3dview.VCam.IsOrtho = false; UpdateView(); };
            _radioBtnPerspective.Unchecked += (s, e) => { rc3dview.VCam.IsOrtho = true; UpdateView(); };
            _btnRotL.Click += (s, e) => { rc3dview.VCam.RotOrtho(false); UpdateView(); };
            _btnRotR.Click += (s, e) => { rc3dview.VCam.RotOrtho(true); UpdateView(); };
            _btnAbove.Click += (s, e) => { rc3dview.VCam.ChangeViewAbove(); UpdateView(); };
            _btnBelow.Click += (s, e) => { rc3dview.VCam.ChangeViewBelow(); UpdateView(); };
            _btnFront.Click += (s, e) => { rc3dview.VCam.ChangeViewFront(); UpdateView(); };
            _btnRear.Click += (s, e) => { rc3dview.VCam.ChangeViewRear(); UpdateView(); };
            _btnRightside.Click += (s, e) => { rc3dview.VCam.ChangeViewRightside(); UpdateView(); };
            _btnLeftside.Click += (s, e) => { rc3dview.VCam.ChangeViewLeftside(); UpdateView(); };
        }

        /// <summary>
        /// Update view.
        /// </summary>
        private void UpdateView()
        {
            if (_rc3dview == null) return;

            _buttons.ForEach(btn =>
            {
                btn.Background = Brushes.White;
                btn.Foreground = Brushes.Black;
            });

            if (_rc3dview.VCam.IsOrtho)
            {
                _btnAbove.IsEnabled = true;
                _btnBelow.IsEnabled = true;

                if (_rc3dview.VCam.OrthoType == RC3DView.ViewCam.OrthographicType.Above ||
                    _rc3dview.VCam.OrthoType == RC3DView.ViewCam.OrthographicType.Below)
                {
                    _btnRotL.IsEnabled = true;
                    _btnRotR.IsEnabled = true;
                }
                else
                {
                    _btnRotL.IsEnabled = false;
                    _btnRotR.IsEnabled = false;
                }

                Button btn = _btnsOrthoType[_rc3dview.VCam.OrthoType];

                btn.Background = Brushes.DodgerBlue;
                btn.Foreground = Brushes.White;
            }
            else
            {
                _btnAbove.IsEnabled = false;
                _btnBelow.IsEnabled = false;
                _btnRotL.IsEnabled = false;
                _btnRotR.IsEnabled = false;
            }
        }

        /// <summary>
        /// Request update.
        /// </summary>
        public void ReqUpdate()
        {
            if (_rc3dview == null) return;

            _radioBtnPerspective.IsChecked = !_rc3dview.VCam.IsOrtho;
            _radioBtnOrthographic.IsChecked = _rc3dview.VCam.IsOrtho;
            UpdateView();
        }
    }
}
