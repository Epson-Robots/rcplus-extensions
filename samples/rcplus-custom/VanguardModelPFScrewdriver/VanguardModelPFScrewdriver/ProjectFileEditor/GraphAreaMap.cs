// -----------------------------------------------------------------------
// <copyright file="DimensionMap.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Holds the drawing areas of multiple graphs and provides their bounding area.
    /// </summary>
    public class GraphAreaMap
    {
        /// <summary>
        /// Represents a rectangular area defined by X and Y ranges.
        /// </summary>
        public class Area
        {
            /// <summary>
            /// Gets or sets the range on the X axis (time).
            /// </summary>
            public FloatRange X { get; set; } = new(0, 0);

            /// <summary>
            /// Gets or sets the range on the Y axis (torque, in mN・m).
            /// </summary>
            public FloatRange Y { get; set; } = new(0, 0);
        }

        /// <summary>
        /// Gets a value indicating whether no area is registered.
        /// </summary>
        public bool IsEmpty => (_map.Count == 0);

        // Registered areas, keyed by an arbitrary owner object.
        private readonly Dictionary<object, Area> _map = [];

        /// <summary>
        /// Gets or sets the torque unit used when exposing the Y range.
        /// </summary>
        public TorqueUnitKind TorqueUnit { get; set; } = TorqueUnitKind.MNM;

        // Bounding range on the X axis of all registered areas.
        private FloatRange _innerX = new();

        // Bounding range on the Y axis of all registered areas (always kept in mN・m).
        private FloatRange _innerY = new();

        /// <summary>
        /// Gets the bounding range on the X axis of all registered areas.
        /// </summary>
        public FloatRange X => _innerX;

        /// <summary>
        /// Gets the bounding range on the Y axis, converted to <see cref="TorqueUnit"/>.
        /// </summary>
        public FloatRange Y
        {
            get
            {
                return TorqueUnit switch
                {
                    // Convert the internal mN・m values to kgf・cm when required.
                    TorqueUnitKind.KGFCM => new(
                        TorqueValueConverter.MNM2KGFCM(_innerY.Min),
                        TorqueValueConverter.MNM2KGFCM(_innerY.Max)
                    ),
                    _ => _innerY
                };
            }
        }

        /// <summary>
        /// Removes all registered areas.
        /// </summary>
        public void Clear()
        {
            _map.Clear();
        }

        /// <summary>
        /// Registers or replaces the area associated with the specified key,
        /// and expands the bounding ranges accordingly.
        /// </summary>
        /// <param name="key">Key identifying the area owner.</param>
        /// <param name="area">Area to register.</param>
        public void Add(
            object key,
            Area area
        )
        {
            if (_map.Count == 0)
            {
                // The first area becomes the initial bounding range.
                _innerX = area.X;
                _innerY = area.Y;
            }
            else
            {
                // Expand the bounding range so that it contains the new area.
                if (area.X.Min < _innerX.Min)
                {
                    _innerX.Min = area.X.Min;
                }
                if (area.X.Max > _innerX.Max)
                {
                    _innerX.Max = area.X.Max;
                }
                if (area.Y.Min < _innerY.Min)
                {
                    _innerY.Min = area.Y.Min;
                }
                if (area.Y.Max > _innerY.Max)
                {
                    _innerY.Max = area.Y.Max;
                }
            }

            _map[key] = area;
        }

        /// <summary>
        /// Removes the area associated with the specified key and
        /// recalculates the bounding ranges from the remaining areas.
        /// </summary>
        /// <param name="key">Key identifying the area owner.</param>
        public void Remove(
            object key
        )
        {
            _map.Remove(key);

            if (_map.Count > 0)
            {
                // Recalculate the bounding range from the remaining areas.
                _innerX.Min = _map.Values.Min(area => area.X.Min);
                _innerX.Max = _map.Values.Max(area => area.X.Max);
                _innerY.Min = _map.Values.Min(area => area.Y.Min);
                _innerY.Max = _map.Values.Max(area => area.Y.Max);
            }
        }
    }
}
