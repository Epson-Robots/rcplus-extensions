// -----------------------------------------------------------------------
// <copyright file="RC3DSupport.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SpelAnalysisTool.View3D
{
    /// <summary>
    /// 3D support functions.
    /// </summary>
    static class RC3DSupport
    {
        /// <summary>
        /// Calculate normals.
        /// </summary>
        /// <param name="p0">Point 0.</param>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <returns></returns>
        public static Vector3D CalculateNormals(Point3D p0, Point3D p1, Point3D p2)
        {
            return Vector3D.CrossProduct(p1 - p0, p2 - p1);
        }

        /// <summary>
        /// Add triangle shape to Model3DGroup.
        /// </summary>
        /// <param name="group">Model3DGroup</param>
        /// <param name="p0">Point 0.</param>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <param name="brush">Brush.</param>
        public static void AddTriangle(Model3DGroup group, Point3D p0, Point3D p1, Point3D p2, Brush brush)
        {
            var normal = CalculateNormals(p0, p1, p2);
            group.Children.Add(new GeometryModel3D(new MeshGeometry3D()
            {
                Positions = new Point3DCollection(new List<Point3D>() { p0, p1, p2 }),
                Normals = new Vector3DCollection(new List<Vector3D>() { normal, normal, normal }),
            }
            , new DiffuseMaterial(brush)));
        }

        /// <summary>
        /// Add line shape to Model3DGroup.
        /// </summary>
        /// <param name="group">Model3DGroup</param>
        /// <param name="thickness">Thickness.</param>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="brush">Brush.</param>
        public static void AddLine(Model3DGroup group, double thickness, Point3D start, Point3D end, Brush brush)
        {
            Vector3D targetDir = end - start;
            var length = targetDir.Length;
            targetDir.Normalize();

            Vector3D initialDir = new Vector3D(0, 0, 1);

            Vector3D axis = Vector3D.CrossProduct(initialDir, targetDir);
            axis.Normalize();

            double dot = Vector3D.DotProduct(initialDir, targetDir);
            double angle = Math.Acos(dot) * 180.0 / Math.PI;

            if (double.IsNaN(axis.X) || double.IsNaN(axis.Y) || double.IsNaN(axis.Z))
            {
                axis = new Vector3D(0, 1, 0);
                angle = (dot > 0) ? 0 : 180;
            }

            Quaternion rotation = new Quaternion(axis, angle);

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = new Point3DCollection()
                    {
                        new Point3D(-thickness, -thickness, 0),
                        new Point3D( thickness, -thickness, 0),
                        new Point3D( thickness,  thickness, 0),
                        new Point3D(-thickness,  thickness, 0),
                        new Point3D(-thickness, -thickness, length),
                        new Point3D( thickness, -thickness, length),
                        new Point3D( thickness,  thickness, length),
                        new Point3D(-thickness,  thickness, length)
                    };

            mesh.TriangleIndices = new Int32Collection()
                    {
                        0,1,5, 0,5,4,
                        1,2,6, 1,6,5,
                        2,3,7, 2,7,6,
                        3,0,4, 3,4,7,
                    };

            DiffuseMaterial material = new DiffuseMaterial(brush);
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(rotation)));
            transformGroup.Children.Add(new TranslateTransform3D(start.X, start.Y, start.Z));

            group.Children.Add(new GeometryModel3D(mesh, material) { Transform = transformGroup });
        }
    }
}
