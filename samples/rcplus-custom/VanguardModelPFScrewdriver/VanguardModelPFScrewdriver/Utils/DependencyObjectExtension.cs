// -----------------------------------------------------------------------
// <copyright file="DependencyObjectExtension.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Media;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Extension methods that simplify traversing the WPF visual tree
    /// of a <see cref="DependencyObject"/>.
    /// </summary>
    public static class DependencyObjectExtension
    {
        /// <summary>
        /// Enumerates the direct visual children of the specified object.
        /// </summary>
        /// <param name="obj">The object whose children are enumerated.</param>
        /// <returns>The direct visual children.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
        public static IEnumerable<DependencyObject> Children(
            this DependencyObject obj
        )
        {
            ArgumentNullException.ThrowIfNull(obj);

            return Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(obj))
                .Select(i => VisualTreeHelper.GetChild(obj, i))
                .Where(x => x != null);
        }

        /// <summary>
        /// Enumerates all visual descendants of the specified object
        /// by walking the visual tree depth-first.
        /// </summary>
        /// <param name="obj">The root object of the traversal.</param>
        /// <returns>All descendants in depth-first order.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
        public static IEnumerable<DependencyObject> Descendants(
            this DependencyObject obj
        )
        {
            ArgumentNullException.ThrowIfNull(obj);

            foreach (var child in obj.Children())
            {
                yield return child;

                // Recurse into the child's own subtree.
                foreach (var grandChild in child.Descendants())
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        /// Enumerates all visual descendants of the specified object that are of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the descendants to return.</typeparam>
        /// <param name="obj">The root object of the traversal.</param>
        /// <returns>The descendants of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
        public static IEnumerable<T> Descendants<T>(this DependencyObject obj) where T : DependencyObject
        {
            ArgumentNullException.ThrowIfNull(obj);

            return obj.Descendants().OfType<T>();
        }

        /// <summary>
        /// Searches the visual tree upwards for the nearest object of type <typeparamref name="T"/>.
        /// The object itself is included in the search.
        /// </summary>
        /// <typeparam name="T">The type of the ancestor to look for.</typeparam>
        /// <param name="obj">The object to start the search from.</param>
        /// <returns>
        /// The nearest ancestor of type <typeparamref name="T"/>, or <c>null</c> if none was found.
        /// </returns>
        public static T? Ancestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T ancestor)
                {
                    return ancestor;
                }

                // Move one level up in the visual tree.
                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }
    }
}
