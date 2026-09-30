// -----------------------------------------------------------------------
// <copyright file="RobotInfo.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.Windows.Media.Media3D;

namespace SpelAnalysisTool.Common
{
    /// <summary>
    /// Robot information.
    /// </summary>
    public class RobotInfo
    {
        /// <summary>
        /// Robot description.
        /// </summary>
        public RCXRobotDescription RoboDesc { get; } = new RCXRobotDescription();

        /// <summary>
        /// Joint position.
        /// </summary>
        public class JointPos
        {
            public RCXRobotDescription.Joint JointDesc { get; }
            public double CurrentPos;
            public Vector3D CurrentPosCoord = new Vector3D();

            public JointPos(RCXRobotDescription.Joint joint)
            {
                JointDesc = joint;
            }

            public void SetCurrentPosByDef()
            {
                CurrentPos = JointDesc.Def;
            }
        }

        /// <summary>
        /// Joints.
        /// </summary>
        public List<JointPos> Joints { get; } = new List<JointPos>();

        /// <summary>Current position X.</summary>
        public double CurrentPosX;

        /// <summary>Current position Y.</summary>
        public double CurrentPosY;

        /// <summary>Current position Z.</summary>
        public double CurrentPosZ;

        /// <summary>Current position U.</summary>
        public double CurrentPosU;

        /// <summary>Current position V.</summary>
        public double CurrentPosV;

        /// <summary>Current position W.</summary>
        public double CurrentPosW;

        /// <summary>
        /// Robot point offset Z. (Using for SCARA robot)
        /// </summary>
        public double RobotPointOffsetZ;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="roboDesc">Robot description.</param>
        public RobotInfo(RCXRobotDescription roboDesc)
        {
            RoboDesc = roboDesc;
            if (RoboDesc == null) return;

            RoboDesc.Joints.ForEach(joint => Joints.Add(new JointPos(joint)));
        }

        /// <summary>
        /// Get child link by parent.
        /// </summary>
        /// <param name="parent">Parent link.</param>
        /// <returns></returns>
        public RCXRobotDescription.Link GetLinkByParent(int parent)
        {
            return RoboDesc.Links.FirstOrDefault(link => link.ParentLink == parent)!;
        }

        /// <summary>
        /// Get child links by parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IEnumerable<RCXRobotDescription.Link> GetLinksByParent(int parent)
        {
            return RoboDesc.Links.FindAll(link => link.ParentLink == parent);
        }

        /// <summary>
        /// Reset position by default position.
        /// </summary>
        public void ResetPosition()
        {
            Joints.ForEach(joint => joint.SetCurrentPosByDef());
            UpdateCurrentPosCoord();
        }

        /// <summary>
        /// Update current position coordinates.
        /// </summary>
        /// <param name="offsetX">Offset X.</param>
        /// <param name="offsetY">Offset Y.</param>
        /// <param name="offsetZ">Offset Z.</param>
        public void UpdateCurrentPosCoord(double offsetX = 0, double offsetY = 0, double offsetZ = 0)
        {
            var link = GetLinkByParent(0);
            RCXRobotDescription.Link? linkParent = null;

            List<AffineTransform3D> listTransform = new List<AffineTransform3D>();

            listTransform.Add(new TranslateTransform3D(offsetX, offsetY, offsetZ));

            while (link != null)
            {
                var vector3dParent = new RCXRobotDescription.Vector3D();

                if (linkParent != null)
                {
                    vector3dParent = linkParent.JointPos;
                }

                var vector3d = link.JointPos;
                listTransform.Add(new TranslateTransform3D(vector3d.X - vector3dParent.X, vector3d.Y - vector3dParent.Y, vector3d.Z - vector3dParent.Z));

                if (link.Joints.Count > 0)
                {
                    link.Joints.ForEach(j =>
                    {
                        var joint = Joints[j];

                        if (joint.JointDesc.JointType == RCXRobotDescription.JointType.Rotary)
                        {
                            listTransform.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(joint.JointDesc.Axis.X, joint.JointDesc.Axis.Y, joint.JointDesc.Axis.Z), joint.CurrentPos * 180 / Math.PI)));
                        }
                        else
                        {
                            listTransform.Add(new TranslateTransform3D(joint.CurrentPos * joint.JointDesc.Axis.X, joint.CurrentPos * joint.JointDesc.Axis.Y, joint.CurrentPos * joint.JointDesc.Axis.Z));
                        }

                        {
                            Transform3DGroup transform3DGroup = new Transform3DGroup();

                            for (int i = listTransform.Count - 1; i >= 0; i--)
                            {
                                transform3DGroup.Children.Add(listTransform[i]);
                            }

                            var point = transform3DGroup.Transform(new Point3D(0, 0, 0));
                            joint.CurrentPosCoord.X = point.X;
                            joint.CurrentPosCoord.Y = point.Y;
                            joint.CurrentPosCoord.Z = point.Z;
                        }
                    });
                }

                linkParent = link;
                link = GetLinkByParent(link.Index);
            }
        }
    }
}
