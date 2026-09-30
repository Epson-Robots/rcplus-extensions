// -----------------------------------------------------------------------
// <copyright file="RC3DView.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SpelAnalysisTool.Common;
using SpelAnalysisTool.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Epson.RoboticsShared.ExtensionsAPI;

namespace SpelAnalysisTool.View3D
{
    /// <summary>
    /// 3D view.
    /// </summary>
    public class RC3DView : Viewbox
    {
        /// <summary>
        /// Viewport3D control.
        /// </summary>
        public Viewport3D Control = new Viewport3D();

        /// <summary>
        /// 3D model item base.
        /// </summary>
        public class ModelBase
        {
            public bool Enabled { get; set; } = true;

            public ModelVisual3D Control { get; } = new ModelVisual3D();
        }

        /// <summary>
        /// View camera.
        /// </summary>
        public class ViewCam : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();

            public DirectionalLight LightDir = new DirectionalLight()
            {
                Color = Color.FromRgb(120, 120, 120),
                Direction = new Vector3D(0, -1, 0),
            };

            public const double ViewDistanceDef = 1.5;
            public const double ViewAngleHDef = Math.PI * 1.1;
            public const double ViewAngleVDef = 0.3;
            public const double ViewMoveHDef = 0.0;
            public const double ViewMoveVDef = 0.24;

            public double ViewDistance { get; set; }
            public double ViewAngleH { get; set; }
            public double ViewAngleV { get; set; }
            public double ViewMoveH { get; set; }
            public double ViewMoveV { get; set; }

            public bool IsOrtho
            {
                set
                {
                    _control.Camera = value ? _orthoCam : PerspectiveCam;
                    UpdateLightDir();
                }
                get => _control.Camera == _orthoCam;
            }

            private Viewport3D _control;
            private List<(double x, double z)> _orthoUpDirs = new List<(double x, double z)>()
            {
                (0, 1),
                (1, 0),
                (0, -1),
                (-1, 0),
            };
            private int _orthoUpDir;
            public int OrthoUpDir => _orthoUpDir;

            private OrthographicCamera _orthoCam = new OrthographicCamera()
            {
                Position = new Point3D(0, 2, 0),
                LookDirection = new Vector3D(0, -1, 0),
                Width = 2,
            };

            public enum OrthographicType
            {
                Above,
                Below,
                Front,
                Rear,
                Rightside,
                Leftside,
            }

            public OrthographicType OrthoType { get; set; } = OrthographicType.Above;

            private double _orthoReserveX;
            private double _orthoReserveY;
            private double _orthoReserveZ;

            public void ResetOrthoCam()
            {
                _orthoReserveX = 0;
                _orthoReserveY = 0;
                _orthoReserveZ = 0;

                _orthoCam.Position = new Point3D(0, 2, 0);
                _orthoCam.LookDirection = new Vector3D(0, -1, 0);
                _orthoCam.Width = 2;
                OrthoType = OrthographicType.Above;
                _orthoUpDir = 0;
                UpdateOrthoUpDirection();
            }

            private void UpdateLightDir()
            {
                var vec = IsOrtho ? _orthoCam.LookDirection : PerspectiveCam.LookDirection;
                LightDir.Direction = new Vector3D(vec.X, vec.Y, vec.Z);
            }

            private void UpdateOrthoUpDirection()
            {
                var dir = _orthoUpDirs[_orthoUpDir];
                _orthoCam.UpDirection = new Vector3D(dir.x, 0, dir.z);
            }

            public PerspectiveCamera PerspectiveCam = new PerspectiveCamera()
            {
                NearPlaneDistance = 0.01,
                FarPlaneDistance = 10,
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 45,
            };

            public void ResetPerspective()
            {
                IsOrtho = false;

                ViewDistance = ViewDistanceDef;
                ViewAngleH = ViewAngleHDef;
                ViewAngleV = ViewAngleVDef;
                ViewMoveH = ViewMoveHDef;
                ViewMoveV = ViewMoveVDef;

                UpdatePerspective();
            }

            public void ChangeViewAbove()
            {
                if (IsOrtho)
                {
                    UpdateOrthographic(OrthographicType.Above);
                }
                else
                {
                }
            }

            public void ChangeViewBelow()
            {
                if (IsOrtho)
                {
                    UpdateOrthographic(OrthographicType.Below);
                }
                else
                {
                }
            }

            public void ChangeViewFront()
            {
                if (IsOrtho)
                {
                    UpdateOrthographic(OrthographicType.Front);
                }
                else
                {
                    ResetPerspective();
                    ViewAngleH = Math.PI;
                    UpdatePerspective();
                }
            }

            public void ChangeViewRear()
            {
                if (IsOrtho)
                {
                    UpdateOrthographic(OrthographicType.Rear);
                }
                else
                {
                    ResetPerspective();
                    ViewAngleH = 0.0;
                    UpdatePerspective();
                }
            }

            public void ChangeViewRightside()
            {
                if (IsOrtho)
                {
                    UpdateOrthographic(OrthographicType.Rightside);
                }
                else
                {
                    ResetPerspective();
                    ViewAngleH = Math.PI / 2.0;
                    UpdatePerspective();
                }
            }

            public void ChangeViewLeftside()
            {
                if (IsOrtho)
                {
                    UpdateOrthographic(OrthographicType.Leftside);
                }
                else
                {
                    ResetPerspective();
                    ViewAngleH = -Math.PI / 2.0;
                    UpdatePerspective();
                }
            }

            public void UpdatePerspective()
            {
                {
                    var dx = 0.0;
                    var dy = 0.0;
                    var dz = ViewDistance;
                    var upDir = 1.0;

                    {
                        var tmpz = dz * Math.Cos(ViewAngleV) - dy * Math.Sin(ViewAngleV);
                        var tmpy = dz * Math.Sin(ViewAngleV) + dy * Math.Cos(ViewAngleV);
                        dz = tmpz;
                        dy = tmpy;

                        upDir = (tmpz < 0) ? -1 : 1;
                    }

                    {
                        var tmpz = dz * Math.Cos(ViewAngleH) - dx * Math.Sin(ViewAngleH);
                        var tmpx = dz * Math.Sin(ViewAngleH) + dx * Math.Cos(ViewAngleH);
                        dz = tmpz;
                        dx = tmpx;
                    }

                    PerspectiveCam.LookDirection = new Vector3D(-dx, -dy, -dz);
                    PerspectiveCam.UpDirection = new Vector3D(0, upDir, 0);
                }

                {
                    var dx = 0.0 + ViewMoveH;
                    var dy = 0.0 + ViewMoveV;
                    var dz = ViewDistance;

                    {
                        var tmpz = dz * Math.Cos(ViewAngleV) - dy * Math.Sin(ViewAngleV);
                        var tmpy = dz * Math.Sin(ViewAngleV) + dy * Math.Cos(ViewAngleV);
                        dz = tmpz;
                        dy = tmpy;
                    }

                    {
                        var tmpz = dz * Math.Cos(ViewAngleH) - dx * Math.Sin(ViewAngleH);
                        var tmpx = dz * Math.Sin(ViewAngleH) + dx * Math.Cos(ViewAngleH);
                        dz = tmpz;
                        dx = tmpx;
                    }

                    PerspectiveCam.Position = new Point3D(dx, dy, dz);
                }

                UpdateLightDir();
            }

            public void UpdateOrthographic(OrthographicType nextType)
            {
                var x = _orthoCam.Position.X;
                var y = _orthoCam.Position.Y;
                var z = _orthoCam.Position.Z;

                if (OrthoType == OrthographicType.Above || OrthoType == OrthographicType.Below)
                {
                    y = _orthoReserveY;
                }
                else if (OrthoType == OrthographicType.Front || OrthoType == OrthographicType.Rear)
                {
                    z = _orthoReserveZ;
                }
                else if (OrthoType == OrthographicType.Rightside || OrthoType == OrthographicType.Leftside)
                {
                    x = _orthoReserveX;
                }

                if (nextType == OrthographicType.Below && !(OrthoType == OrthographicType.Above || OrthoType == OrthographicType.Below))
                {
                    _orthoUpDir = (_orthoUpDir + 2) % 4;
                }

                if ((nextType == OrthographicType.Above && OrthoType == OrthographicType.Below) ||
                    (nextType == OrthographicType.Below && OrthoType == OrthographicType.Above))
                {
                    if (_orthoUpDir % 2 != 0)
                    {
                        _orthoUpDir = (_orthoUpDir + 2) % 4;
                    }

                    x *= -1;
                }
                else if ((nextType == OrthographicType.Front && OrthoType == OrthographicType.Rear) ||
                    (nextType == OrthographicType.Rear && OrthoType == OrthographicType.Front))
                {
                    x *= -1;
                }
                else if ((nextType == OrthographicType.Front && OrthoType == OrthographicType.Above) ||
                    (nextType == OrthographicType.Above && OrthoType == OrthographicType.Front))
                {

                    if (_orthoUpDir % 4 == 2)
                    {
                        x *= -1;
                    }
                }

                OrthoType = nextType;

                if (OrthoType == OrthographicType.Above)
                {
                    _orthoCam.Position = new Point3D(x, 2, z);
                    _orthoCam.LookDirection = new Vector3D(0, -1, 0);
                    UpdateOrthoUpDirection();
                }
                else if (OrthoType == OrthographicType.Below)
                {
                    _orthoCam.Position = new Point3D(x, -2, z);
                    _orthoCam.LookDirection = new Vector3D(0, 1, 0);
                    UpdateOrthoUpDirection();
                }
                else if (OrthoType == OrthographicType.Front)
                {
                    _orthoCam.Position = new Point3D(x, y, -2);
                    _orthoCam.LookDirection = new Vector3D(0, 0, 1);
                    _orthoCam.UpDirection = new Vector3D(0, 1, 0);
                    _orthoUpDir = 0;
                }
                else if (OrthoType == OrthographicType.Rear)
                {
                    _orthoCam.Position = new Point3D(x, y, 2);
                    _orthoCam.LookDirection = new Vector3D(0, 0, -1);
                    _orthoCam.UpDirection = new Vector3D(0, 1, 0);
                    _orthoUpDir = 2;
                }
                else if (OrthoType == OrthographicType.Rightside)
                {
                    _orthoCam.Position = new Point3D(2, y, z);
                    _orthoCam.LookDirection = new Vector3D(-1, 0, 0);
                    _orthoCam.UpDirection = new Vector3D(0, 1, 0);
                    _orthoUpDir = 3;
                }
                else if (OrthoType == OrthographicType.Leftside)
                {
                    _orthoCam.Position = new Point3D(-2, y, z);
                    _orthoCam.LookDirection = new Vector3D(1, 0, 0);
                    _orthoCam.UpDirection = new Vector3D(0, 1, 0);
                    _orthoUpDir = 1;
                }

                if (OrthoType == OrthographicType.Above || OrthoType == OrthographicType.Below)
                {
                    _orthoReserveZ = _orthoCam.Position.Z;
                }
                else
                {
                    _orthoReserveY = _orthoCam.Position.Y;
                }

                UpdateLightDir();
            }

            public void RotOrtho(bool clockwise)
            {
                if (OrthoType == OrthographicType.Above || OrthoType == OrthographicType.Below)
                {
                    if (OrthoType == OrthographicType.Above ? clockwise : !clockwise)
                    {
                        _orthoUpDir++;
                    }
                    else
                    {
                        _orthoUpDir--;
                    }

                    _orthoUpDir = (_orthoUpDir + 4) % 4;
                    UpdateOrthoUpDirection();
                }
            }

            public double GetOrthoWidth()
            {
                return _orthoCam.Width;
            }

            public void SetOrthoWidth(double w)
            {
                _orthoCam.Width = w;
            }

            public void SetOrthoPosConsiderRot(double dx, double dy)
            {
                var rot = _orthoUpDir % 4;

                if (OrthoType == OrthographicType.Above || OrthoType == OrthographicType.Below)
                {
                    var x = _orthoCam.Position.X;
                    var y = _orthoCam.Position.Z;

                    var mx = OrthoType == OrthographicType.Above ? dx : -dx;
                    var my = dy;

                    var h = OrthoType == OrthographicType.Above ? 2.0 : -2.0;

                    _orthoCam.Position = rot switch
                    {
                        1 => new Point3D(x + my, h, y - mx),
                        2 => new Point3D(x - mx, h, y - my),
                        3 => new Point3D(x - my, h, y + mx),
                        _ => new Point3D(x + mx, h, y + my),
                    };
                }
                else if (OrthoType == OrthographicType.Front || OrthoType == OrthographicType.Rear)
                {
                    var x = _orthoCam.Position.X;
                    var y = _orthoCam.Position.Y;

                    var mx = OrthoType == OrthographicType.Rear ? dx : -dx;
                    var my = dy;

                    var h = OrthoType == OrthographicType.Rear ? 2.0 : -2.0;

                    _orthoCam.Position = new Point3D(x - mx, y + my, h);
                }
                else if (OrthoType == OrthographicType.Rightside || OrthoType == OrthographicType.Leftside)
                {
                    var x = _orthoCam.Position.Z;
                    var y = _orthoCam.Position.Y;

                    var mx = OrthoType == OrthographicType.Rightside ? dx : -dx;
                    var my = dy;

                    var h = OrthoType == OrthographicType.Rightside ? 2.0 : -2.0;

                    _orthoCam.Position = new Point3D(h, y + my, x + mx);
                }

                if (OrthoType == OrthographicType.Above || OrthoType == OrthographicType.Below)
                {
                    _orthoReserveZ = _orthoCam.Position.Z;
                }
                else
                {
                    _orthoReserveY = _orthoCam.Position.Y;
                }
            }

            public ViewCam(Viewport3D control)
            {
                Control.Content = _group;
                _group.Children.Add(LightDir);
                _control = control;
                IsOrtho = false;
                UpdateOrthoUpDirection();
                ResetPerspective();
            }
        }

        public ViewCam VCam { get; }

        private List<ModelBase> _models = new List<ModelBase>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public RC3DView()
        {
            VCam = new ViewCam(Control);

            GetType().GetProperties().Where(p => p.PropertyType.IsAssignableTo(typeof(ModelBase)))
                .ToList().ForEach(p => _models.Add((ModelBase)p.GetValue(this)!));
        }

        /// <summary>
        /// Refresh to switch the display of items.
        /// </summary>
        public void Refresh()
        {
            Control.Children.Clear();
            _models.Where(m => m.Enabled).ToList().ForEach(m => Control.Children.Add(m.Control));
        }

        /// <summary>
        /// Light.
        /// </summary>
        public class ModelLight : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();

            public DirectionalLight LightDownward = new DirectionalLight()
            {
                Color = Color.FromRgb(120, 120, 120),
                Direction = new Vector3D(0, -1, 0),
            };

            public AmbientLight LightAmbient = new AmbientLight()
            {
                Color = Color.FromRgb(80, 80, 80),
            };

            public ModelLight()
            {
                Control.Content = _group;
                _group.Children.Add(LightDownward);
                _group.Children.Add(LightAmbient);
            }
        }

        public ModelLight Light { get; } = new ModelLight();

        /// <summary>
        /// Canvas.
        /// </summary>
        public class ModelCanvas : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();
            private double _verticalHeight;

            public ModelCanvas(double verticalHegiht)
            {
                Control.Content = _group;
                _verticalHeight = verticalHegiht;
            }

            public void Draw(Canvas canvas, double size)
            {
                _group.Children.Clear();

                var floorHalfSize = size / 2;
                var floorHeight = _verticalHeight;
                var divNum = 80;
                var divSize = size / divNum;
                double divRateSize = (double)1 / divNum;

                var geometryModel3D = new GeometryModel3D();
                var meshGeometry3D = new MeshGeometry3D();

                var counter = 0;

                for (var idxX = 0; idxX < divNum; idxX++)
                {
                    for (var idxZ = 0; idxZ < divNum; idxZ++)
                    {
                        var xMin = -floorHalfSize + divSize * idxX;
                        var xMax = -floorHalfSize + divSize * idxX + divSize;
                        var zMin = -floorHalfSize + divSize * idxZ;
                        var zMax = -floorHalfSize + divSize * idxZ + divSize;

                        var txMin = idxX * divRateSize;
                        var txMax = idxX * divRateSize + divRateSize;
                        var tyMin = idxZ * divRateSize;
                        var tyMax = idxZ * divRateSize + divRateSize;

                        meshGeometry3D.TextureCoordinates.Add(new Point(txMin, tyMin));
                        meshGeometry3D.TextureCoordinates.Add(new Point(txMax, tyMin));
                        meshGeometry3D.TextureCoordinates.Add(new Point(txMax, tyMax));
                        meshGeometry3D.TextureCoordinates.Add(new Point(txMin, tyMax));
                        meshGeometry3D.Positions.Add(new Point3D(xMin, floorHeight, zMin));
                        meshGeometry3D.Positions.Add(new Point3D(xMax, floorHeight, zMin));
                        meshGeometry3D.Positions.Add(new Point3D(xMax, floorHeight, zMax));
                        meshGeometry3D.Positions.Add(new Point3D(xMin, floorHeight, zMax));
                        meshGeometry3D.Normals.Add(new Vector3D(0, 1, 0));
                        meshGeometry3D.Normals.Add(new Vector3D(0, 1, 0));
                        meshGeometry3D.Normals.Add(new Vector3D(0, 1, 0));
                        meshGeometry3D.Normals.Add(new Vector3D(0, 1, 0));
                        meshGeometry3D.TriangleIndices.Add(counter + 1);
                        meshGeometry3D.TriangleIndices.Add(counter + 0);
                        meshGeometry3D.TriangleIndices.Add(counter + 2);
                        meshGeometry3D.TriangleIndices.Add(counter + 0);
                        meshGeometry3D.TriangleIndices.Add(counter + 3);
                        meshGeometry3D.TriangleIndices.Add(counter + 2);

                        counter += 4;
                    }
                }

                geometryModel3D.Geometry = meshGeometry3D;

                var diffuseMaterial = new DiffuseMaterial();
                diffuseMaterial.Brush = new VisualBrush() { Visual = canvas };
                geometryModel3D.Material = diffuseMaterial;

                _group.Children.Add(geometryModel3D);

                Control.Content = null;
                Control.Content = _group;
            }
        }

        public ModelCanvas FloorScale { get; } = new ModelCanvas(-0.001);

        /// <summary>
        /// Robot.
        /// </summary>
        public class ModelRobot : ModelBase
        {
            private List<Model3DGroup> _groups = new List<Model3DGroup>();
            private bool _created;
            private int _componentIndex;
            private RobotInfo? _robotInfo;
            private int _representation;

            public RobotInfo? RInfo => _robotInfo;

            public void SetRobotInfo(RobotInfo robotInfo, int representation = 0)
            {
                _robotInfo = robotInfo;
                _representation = representation;
                _groups.Clear();
                Control.Children.Clear();
                _created = false;
            }

            private void AddComponent(Transform3DGroup transform)
            {
                if (_created)
                {
                    _groups[_componentIndex].Transform = transform.Clone();
                }
                else
                {
                    _groups.Add(new Model3DGroup());
                    Control.Children.Add(new ModelVisual3D() { Content = _groups.Last() });
                    _groups.Last().Transform = transform.Clone();
                }

                _componentIndex++;
            }

            private void AddGeometry(GeometryModel3D geometryModel3D)
            {
                if (_created)
                {
                    return;
                }

                _groups.Last().Children.Add(geometryModel3D);
            }

            private Color F4ToColor(RCXRobotDescription.Color4F f4)
            {
                var color = new Color();

                //color.A = (byte)(f4.A * 255);
                color.A = 255; // For LS10-B602S
                color.R = (byte)(f4.R * 255);
                color.G = (byte)(f4.G * 255);
                color.B = (byte)(f4.B * 255);

                return color;
            }

            private void DrawComponent(RCXRobotDescription.Component component)
            {
                if (component.Representations.Count > _representation)
                {
                    var representation = component.Representations[_representation];

                    representation.Geometries.ForEach((geometory) =>
                    {
                        var geometryModel3D = new GeometryModel3D();
                        var meshGeometry3D = new MeshGeometry3D();
                        geometryModel3D.Geometry = meshGeometry3D;

                        if (geometory.MaterialDefEnable)
                        {
                            if (!_created)
                            {
                                geometryModel3D.Material = new DiffuseMaterial() { Brush = new SolidColorBrush(F4ToColor(geometory.MaterialDef.DiffuseColor)) };
                            }
                        }
                        else
                        {
                            geometryModel3D.Material = new DiffuseMaterial() { Brush = Brushes.Red };
                        }

                        if (!_created)
                        {
                            for (int i = 0; i < geometory.CoordIndices.Count; i++)
                            {
                                if (geometory.CoordIndices[i] < representation.Coordinates3D.Count)
                                {
                                    var coordinate = representation.Coordinates3D[geometory.CoordIndices[i]];
                                    var normal = representation.Normals3D[geometory.NormalIndices[i]];

                                    meshGeometry3D.Positions.Add(new Point3D(coordinate.X, coordinate.Y, coordinate.Z));
                                    meshGeometry3D.Normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
                                }
                            }
                        }

                        AddGeometry(geometryModel3D);
                    });
                }
            }

            private void ProcessLink(RCXRobotDescription.Link linkParent, Transform3DGroup transformCurrent)
            {
                if (_robotInfo == null) return;

                AddComponent(transformCurrent);
                DrawComponent(_robotInfo.RoboDesc.Components[linkParent.Component]);

                IEnumerable<RCXRobotDescription.Link> links = _robotInfo.GetLinksByParent(linkParent.Index);

                foreach (var link in links)
                {
                    var transform = new Transform3DGroup();

                    foreach (var x in transformCurrent.Children)
                    {
                        transform.Children.Add(x);
                    }

                    {
                        var vector3dParent = linkParent.JointPos;
                        var vector3d = link.JointPos;
                        transform.Children.Insert(0, new TranslateTransform3D(vector3d.X - vector3dParent.X, vector3d.Y - vector3dParent.Y, vector3d.Z - vector3dParent.Z));
                    }

                    link.Joints.ForEach(idxJoint =>
                    {
                        var joint = _robotInfo.Joints[idxJoint];

                        if (joint.JointDesc.JointType == RCXRobotDescription.JointType.Rotary)
                        {
                            // Rotation
                            var rotateTransform = new RotateTransform3D();
                            var axisAngleRotation = new AxisAngleRotation3D();
                            axisAngleRotation.Axis = new Vector3D(joint.JointDesc.Axis.X, joint.JointDesc.Axis.Y, joint.JointDesc.Axis.Z);
                            axisAngleRotation.Angle = joint.CurrentPos * 180 / Math.PI;
                            rotateTransform.Rotation = axisAngleRotation;
                            transform.Children.Insert(0, rotateTransform);
                        }
                        else
                        {
                            // Translation
                            transform.Children.Insert(0, new TranslateTransform3D(joint.CurrentPos * joint.JointDesc.Axis.X, joint.CurrentPos * joint.JointDesc.Axis.Y, joint.CurrentPos * joint.JointDesc.Axis.Z));
                        }
                    });

                    ProcessLink(link, transform);
                }
            }

            public void DrawRobot(double offsetX = 0, double offsetY = 0, double offsetZ = 0)
            {
                if (_robotInfo == null)
                {
                    return;
                }

                if (_robotInfo.RoboDesc.Links.Count == 0)
                {
                    return;
                }

                _componentIndex = 0;
                var transformGroup = new Transform3DGroup();

                var transX = offsetX;
                var transY = offsetZ;
                var transZ = -offsetY;

                // For T6-B602S
                if (_robotInfo.RoboDesc.Components.Count > 0)
                {
                    var mt = _robotInfo.RoboDesc.Components[0].MatrixTransform;

                    transY += mt.M13;
                    transZ += mt.M23;
                }

                // For CW
                if (_robotInfo.RoboDesc.IkPoss.Count > 1)
                {
                    transX -= _robotInfo.RoboDesc.IkPoss[1].X;
                    transY -= _robotInfo.RoboDesc.IkPoss[1].Y;
                    transZ -= _robotInfo.RoboDesc.IkPoss[1].Z;
                }

                transformGroup.Children.Add(new TranslateTransform3D(transX, transY, transZ));

                ProcessLink(_robotInfo.RoboDesc.Links[0], transformGroup);

                _created = true;
            }
        }

        public ModelRobot Robot { get; } = new ModelRobot();

        /// <summary>
        /// Duct.
        /// </summary>
        public class ModelDuct : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();
            private RobotInfo? _robotInfo;
            private int _numItem = 100;
            private int _numCircleDetail = 12;

            public ModelDuct()
            {
                Control.Content = _group;
            }

            public void SetRobotInfo(RobotInfo robotInfo, int representation = 0)
            {
                _robotInfo = robotInfo;
                _group.Children.Clear();

                if (_robotInfo.RoboDesc.Ducts.Count <= 0)
                {
                    return;
                }

                var brush = new SolidColorBrush(Color.FromRgb(60, 60, 60));

                for (var i = 0; i < _numItem; i++)
                {
                    var group = new Model3DGroup();

                    var sz = _robotInfo.RoboDesc.Ducts[0].Radius;

                    var pA = new Point3D(0, +sz, 0);
                    var pB = new Point3D(0, -sz, 0);

                    var radDiff = Math.PI * 2.0 / _numCircleDetail;

                    for (var idxP = 0; idxP < _numCircleDetail; idxP++)
                    {
                        var radA = radDiff * idxP;
                        var radB = radDiff * (idxP + 1);

                        var p0 = new Point3D(sz * Math.Cos(radA), 0, sz * Math.Sin(radA));
                        var p1 = new Point3D(sz * Math.Cos(radB), 0, sz * Math.Sin(radB));

                        RC3DSupport.AddTriangle(group, pA, p1, p0, brush);
                        RC3DSupport.AddTriangle(group, pB, p0, p1, brush);
                    }

                    _group.Children.Add(group);
                }
            }

            public void Draw()
            {
                if (_robotInfo == null)
                {
                    return;
                }


                if (_robotInfo.RoboDesc.Ducts.Count <= 0)
                {
                    return;
                }

                var duct = _robotInfo.RoboDesc.Ducts[0];

                if (duct.ControlPoints.Count < 4)
                {
                    return;
                }

                _robotInfo.UpdateCurrentPosCoord();

                var points = new List<Vector3D>();

                foreach (var cp in duct.ControlPoints)
                {
                    var pt = new Vector3D();

                    pt.X = cp.Offset.X;
                    pt.Y = cp.Offset.Y;
                    pt.Z = cp.Offset.Z;

                    //var joint =_robotInfo.Joints.FirstOrDefault(j => j.Index == cp.Joint);
                    var joint = _robotInfo.Joints[cp.Joint - 1];

                    if (joint != null)
                    {
                        pt.X += joint.CurrentPosCoord.X;
                        pt.Y += joint.CurrentPosCoord.Y;
                        pt.Z += joint.CurrentPosCoord.Z;
                    }

                    points.Add(pt);
                }

                var idx = 0;

                Vector3D ptBefore;

                foreach (var item in _group.Children)
                {
                    var t = 1.0 * idx / _numItem;

                    var a = Math.Pow(1.0 - t, 3);
                    var b = 3.0 * Math.Pow(1.0 - t, 2) * t;
                    var c = 3.0 * (1.0 - t) * t * t;
                    var d = Math.Pow(t, 3);

                    var pt = new Vector3D();

                    pt.X = a * points[0].X + b * points[1].X + c * points[2].X + d * points[3].X;
                    pt.Y = a * points[0].Y + b * points[1].Y + c * points[2].Y + d * points[3].Y;
                    pt.Z = a * points[0].Z + b * points[1].Z + c * points[2].Z + d * points[3].Z;

                    if (item is Model3DGroup model)
                    {
                        Transform3DGroup transformGroup = new Transform3DGroup();

                        if (idx != 0)
                        {
                            Vector3D targetDir = pt - ptBefore;
                            targetDir.Normalize();

                            Vector3D initialDir = new Vector3D(0, 1, 0);

                            Vector3D axis = Vector3D.CrossProduct(initialDir, targetDir);
                            axis.Normalize();

                            double dot = Vector3D.DotProduct(initialDir, targetDir);
                            double angle = Math.Acos(dot) * 180.0 / Math.PI;

                            if (double.IsNaN(axis.X) || double.IsNaN(axis.Y) || double.IsNaN(axis.Z))
                            {
                                axis = new Vector3D(0, 1, 0);
                                angle = (dot > 0) ? 0 : 180;
                            }

                            transformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(new Quaternion(axis, angle))));
                        }

                        transformGroup.Children.Add(new TranslateTransform3D(pt));
                        item.Transform = transformGroup;
                    }

                    ptBefore = pt;

                    idx++;
                }
            }
        }

        public ModelDuct Duct { get; } = new ModelDuct();

        /// <summary>
        /// Bellows.
        /// </summary>
        public class ModelBellows : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();
            private RobotInfo? _robotInfo;

            class Bellows
            {
                public Model3DGroup _group;
                private double _offset;
                private double _fixedPos;
                private double _movingPos;
                private double _radius;
                private int _numCircleDetail = 12;
                private double _baseHeight = 0.1;

                public Bellows(Model3DGroup group, double offset, double fixedPos, double movingPos, double radius)
                {
                    _group = group;
                    _offset = offset;
                    _fixedPos = fixedPos;
                    _movingPos = movingPos;
                    _radius = radius;

                    var sz = _radius;
                    var szInner = _radius * 0.6;

                    var brush = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200));

                    var unitNum = 10;
                    var zDiff = _baseHeight / unitNum;
                    var radDiff = Math.PI * 2.0 / _numCircleDetail;

                    for (var idxZ = 0; idxZ < unitNum; idxZ++)
                    {
                        var z = zDiff * idxZ;

                        for (var idxP = 0; idxP < _numCircleDetail; idxP++)
                        {
                            var radA = radDiff * idxP;
                            var radB = radDiff * (idxP + 1);

                            var p0Inner = new Point3D(szInner * Math.Cos(radA), z, szInner * Math.Sin(radA));
                            var p1Inner = new Point3D(szInner * Math.Cos(radB), z, szInner * Math.Sin(radB));

                            var p0 = new Point3D(sz * Math.Cos(radA), z + zDiff / 2, sz * Math.Sin(radA));
                            var p1 = new Point3D(sz * Math.Cos(radB), z + zDiff / 2, sz * Math.Sin(radB));

                            var p0Inner2 = new Point3D(szInner * Math.Cos(radA), z + zDiff, szInner * Math.Sin(radA));
                            var p1Inner2 = new Point3D(szInner * Math.Cos(radB), z + zDiff, szInner * Math.Sin(radB));

                            RC3DSupport.AddTriangle(_group, p0Inner, p0, p1Inner, brush);
                            RC3DSupport.AddTriangle(_group, p1Inner, p0, p1, brush);

                            RC3DSupport.AddTriangle(_group, p0Inner2, p1Inner2, p0, brush);
                            RC3DSupport.AddTriangle(_group, p1Inner2, p1, p0, brush);
                        }
                    }
                }

                public void Update(RobotInfo.JointPos joint)
                {
                    Transform3DGroup transformGroup = new Transform3DGroup();

                    var zfix = _offset + _fixedPos;
                    var zmov = _offset + _movingPos + joint.CurrentPos;
                    var len = Math.Abs(zfix - zmov);
                    var z = Math.Min(zfix, zmov);

                    transformGroup.Children.Add(new ScaleTransform3D(1.0, len / _baseHeight, 1.0, 0, 0, 0));
                    transformGroup.Children.Add(new TranslateTransform3D(joint.CurrentPosCoord.X, z, joint.CurrentPosCoord.Z));
                    _group.Transform = transformGroup;
                }
            }

            private List<Bellows> _listBellows = new List<Bellows>();

            public ModelBellows()
            {
                Control.Content = _group;
            }

            public void SetRobotInfo(RobotInfo robotInfo)
            {
                _robotInfo = robotInfo;
                _group.Children.Clear();
                _listBellows.Clear();

                if (_robotInfo.RoboDesc.Bellows.Count <= 0)
                {
                    return;
                }

                foreach (var b in _robotInfo.RoboDesc.Bellows)
                {
                    var group = new Model3DGroup();
                    _listBellows.Add(new Bellows(
                        group,
                        _robotInfo.RobotPointOffsetZ,
                        b.FixedPos,
                        b.MovingPos,
                        b.Radius));
                }
            }

            public void Draw()
            {
                if (_listBellows.Count == 0) return;
                if (_robotInfo == null) return;
                if (_robotInfo.Joints.Count < 3) return;

                var joint = _robotInfo.Joints[2];
                if (joint == null) return;

                if (_group.Children.Count == 0)
                {
                    _listBellows.ForEach(b => _group.Children.Add(b._group));
                }

                _robotInfo.UpdateCurrentPosCoord();
                _listBellows.ForEach(b => b.Update(joint));
            }
        }

        public ModelBellows Bellows { get; } = new ModelBellows();

        /// <summary>
        /// Current point.
        /// </summary>
        public class ModelCurPoint : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();

            public ModelCurPoint()
            {
                Control.Content = _group;
                Draw();
            }

            private void Draw()
            {
                _group.Children.Clear();

                var brush = Brushes.Green;

                {
                    var sz = 0.005;
                    var szh = 0.01;

                    var pA = new Point3D(0, 0, 0);

                    var p0 = new Point3D(-sz, -szh, -sz);
                    var p1 = new Point3D(-sz, -szh, +sz);
                    var p2 = new Point3D(+sz, -szh, +sz);
                    var p3 = new Point3D(+sz, -szh, -sz);

                    RC3DSupport.AddTriangle(_group, pA, p0, p1, brush);
                    RC3DSupport.AddTriangle(_group, pA, p1, p2, brush);
                    RC3DSupport.AddTriangle(_group, pA, p2, p3, brush);
                    RC3DSupport.AddTriangle(_group, pA, p3, p0, brush);

                    RC3DSupport.AddTriangle(_group, p0, p2, p1, brush);
                    RC3DSupport.AddTriangle(_group, p0, p3, p2, brush);
                }

                {
                    var len = 0.04;
                    var thickness = 0.0002;
                    RC3DSupport.AddLine(_group, thickness, new Point3D(-len, 0, 0), new Point3D(len, 0, 0), brush);
                    RC3DSupport.AddLine(_group, thickness, new Point3D(0, 0, -len), new Point3D(0, 0, len), brush);
                }

                {
                    var x = 0.04;
                    var y = 0.0;
                    var z = 0.0;

                    var sz = 0.004;

                    var pA = new Point3D(x, z + sz, -y);
                    var pB = new Point3D(x, z - sz, -y);

                    var p0 = new Point3D(x - sz, z, -y - sz);
                    var p1 = new Point3D(x - sz, z, -y + sz);
                    var p2 = new Point3D(x + sz, z, -y + sz);
                    var p3 = new Point3D(x + sz, z, -y - sz);

                    RC3DSupport.AddTriangle(_group, pA, p0, p1, brush);
                    RC3DSupport.AddTriangle(_group, pA, p1, p2, brush);
                    RC3DSupport.AddTriangle(_group, pA, p2, p3, brush);
                    RC3DSupport.AddTriangle(_group, pA, p3, p0, brush);

                    RC3DSupport.AddTriangle(_group, pB, p1, p0, brush);
                    RC3DSupport.AddTriangle(_group, pB, p2, p1, brush);
                    RC3DSupport.AddTriangle(_group, pB, p3, p2, brush);
                    RC3DSupport.AddTriangle(_group, pB, p0, p3, brush);
                }
            }

            public void SetPos(double x, double y, double z, double u, double v, double w, double offsetZ)
            {
                Transform3DGroup transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), w)));
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, -1), v)));
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), u)));
                transformGroup.Children.Add(new TranslateTransform3D(x, z + offsetZ, -y));
                _group.Transform = transformGroup;
            }
        }

        public ModelCurPoint CurPoint { get; } = new ModelCurPoint();

        /// <summary>
        /// Robot points.
        /// </summary>
        public class ModelRobotPoints : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();

            public ModelRobotPoints()
            {
                Control.Content = _group;
            }

            public void Draw(List<LogDataMotion.Record> records, double offsetZ)
            {
                _group.Children.Clear();

                foreach (var pt in records)
                {
                    var x = pt.X / 1000;
                    var y = pt.Y / 1000;
                    var z = pt.Z / 1000 + offsetZ;

                    var sz = 0.001;

                    var brush = Brushes.DodgerBlue;

                    if (pt.IsNG())
                    {
                        brush = Brushes.Red;
                    }

                    var pA = new Point3D(x, z + sz, -y);
                    var pB = new Point3D(x, z - sz, -y);

                    var p0 = new Point3D(x - sz, z, -y - sz);
                    var p1 = new Point3D(x - sz, z, -y + sz);
                    var p2 = new Point3D(x + sz, z, -y + sz);
                    var p3 = new Point3D(x +sz, z, -y - sz);

                    var group = new Model3DGroup();
                    
                    RC3DSupport.AddTriangle(group, pA, p0, p1, brush);
                    RC3DSupport.AddTriangle(group, pA, p1, p2, brush);
                    RC3DSupport.AddTriangle(group, pA, p2, p3, brush);
                    RC3DSupport.AddTriangle(group, pA, p3, p0, brush);

                    RC3DSupport.AddTriangle(group, pB, p1, p0, brush);
                    RC3DSupport.AddTriangle(group, pB, p2, p1, brush);
                    RC3DSupport.AddTriangle(group, pB, p3, p2, brush);
                    RC3DSupport.AddTriangle(group, pB, p0, p3, brush);

                    _group.Children.Add(group);
                }
            }
        }

        public ModelRobotPoints RobotPoints { get; } = new ModelRobotPoints();

        /// <summary>
        /// Robot points locus.
        /// </summary>
        public class ModelRobotLocus : ModelBase
        {
            private Model3DGroup _group = new Model3DGroup();

            public ModelRobotLocus()
            {
                Control.Content = _group;
            }

            public void Draw(List<LogDataMotion.Record> records, double offsetZ)
            {
                _group.Children.Clear();

                LogDataMotion.Record? before = null;

                foreach (var pt in records)
                {
                    if (before == null)
                    {
                        before = pt;
                        continue;
                    }

                    var prex = before.X / 1000;
                    var prey = before.Y / 1000;
                    var prez = before.Z / 1000 + offsetZ;

                    var x = pt.X / 1000;
                    var y = pt.Y / 1000;
                    var z = pt.Z / 1000 + offsetZ;

                    var start = new Point3D(prex, prez, -prey);
                    var end = new Point3D(x, z, -y);

                    var thickness = 0.0002;

                    var brush = Brushes.DodgerBlue;

                    if (pt.IsNG() || before.IsNG())
                    {
                        brush = Brushes.Red;
                    }

                    RC3DSupport.AddLine(_group, thickness, start, end, brush);

                    before = pt;
                }
            }
        }

        public ModelRobotLocus RobotLocus { get; } = new ModelRobotLocus();

        /// <summary>
        /// Draw robot.
        /// </summary>
        public void DrawRobot()
        {
            Robot.DrawRobot();
            Duct.Draw();
            Bellows.Draw();
        }

        /// <summary>
        /// Set RobotInfo.
        /// </summary>
        /// <param name="robotInfo">RobotInfo.</param>
        public void SetRobotInfo(RobotInfo robotInfo)
        {
            if (robotInfo == null)
            {
                return;
            }

            Robot.SetRobotInfo(robotInfo);
            Duct.SetRobotInfo(robotInfo);
            Bellows.SetRobotInfo(robotInfo);
        }

        /// <summary>
        /// Clear robot.
        /// </summary>
        public void ClearRobot()
        {
            SetRobotInfo(new RobotInfo(new RCXRobotDescription()));
            DrawRobot();
        }

        private double _x;
        private double _y;
        private double _z;
        private double _u;
        private double _v;
        private double _w;

        /// <summary>
        /// Update current position.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="z">Z.</param>
        /// <param name="u">U.</param>
        /// <param name="v">V.</param>
        /// <param name="w">W.</param>
        public void UpdateCurPos(double x, double y, double z, double u, double v, double w)
        {
            _x = x;
            _y = y;
            _z = z;
            _u = u;
            _v = v;
            _w = w;

            UpdateCurPos();
        }

        /// <summary>
        /// Update current position.
        /// </summary>
        public void UpdateCurPos()
        {
            var offsetZ = 0.0;

            if (Robot.RInfo != null)
            {
                offsetZ = Robot.RInfo.RobotPointOffsetZ;
            }

            CurPoint.SetPos(_x, _y, _z, _u, _v, _w, offsetZ);
        }

        /// <summary>
        /// Update robot points.
        /// </summary>
        /// <param name="records">Motion records.</param>
        public void UpdateRobotPoints(List<LogDataMotion.Record> records)
        {
            var offsetZ = 0.0;

            if (Robot.RInfo != null)
            {
                offsetZ = Robot.RInfo.RobotPointOffsetZ;
            }

            RobotPoints.Draw(records, offsetZ);
            RobotLocus.Draw(records, offsetZ);
        }
    }
}
