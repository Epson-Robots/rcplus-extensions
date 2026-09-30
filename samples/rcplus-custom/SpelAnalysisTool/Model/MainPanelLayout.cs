// -----------------------------------------------------------------------
// <copyright file="MainPanelLayout.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
using System.Text.Json.Serialization;

namespace SpelAnalysisTool.Model
{
    /// <summary>
    /// Main panel layout.
    /// </summary>
    public class MainPanelLayout
    {
        // Layout numbering is
        // 1 4 7 
        // 2 5 8
        // 3 6 9

        public const int ROW_NUM = 3;
        public const int COL_NUM = 3;

        /// <summary>
        /// Layout item.
        /// </summary>
        public class LayoutItem
        {
            [JsonIgnore]
            public string Name { get; set; } = "";

            // 0 Hidden 1 - 9 Loc
            public int LayoutLoc { get; set; }

            [JsonIgnore]
            public bool IsHidden => LayoutLoc == 0;

            [JsonIgnore]
            public int Row => (LayoutLoc - 1) % MainPanelLayout.ROW_NUM;

            [JsonIgnore]
            public int Column => (LayoutLoc - 1) / MainPanelLayout.ROW_NUM;

            [JsonIgnore]
            private int _layoutLocDef;

            public LayoutItem()
            {
            }

            public LayoutItem(int layoutLoc)
            {
                _layoutLocDef = layoutLoc;
                LayoutLoc = layoutLoc;
            }

            public void Reset()
            {
                LayoutLoc = _layoutLocDef;
            }

            public void SetDefault(int def)
            {
                _layoutLocDef = def;
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
        public class LayoutItemAttribute : Attribute
        {
            public string Name { get; }
            public int LayoutLocDef { get; }

            public LayoutItemAttribute(string name, int layoutLocDef)
            {
                Name = name;
                LayoutLocDef = layoutLocDef;
            }
        }

        [LayoutItem("ChkParams", 1)]
        public LayoutItem ChkParams { get; set; } = new LayoutItem(1);

        [LayoutItem("AdjParams", 2)]
        public LayoutItem AdjParams { get; set; } = new LayoutItem(2);

        [LayoutItem("View", 4)]
        public LayoutItem View { get; set; } = new LayoutItem(4);

        [LayoutItem("Cycle", 7)]
        public LayoutItem Cycle { get; set; } = new LayoutItem(7);

        [LayoutItem("Section", 8)]
        public LayoutItem Section { get; set; } = new LayoutItem(8);

        [LayoutItem("Motion", 9)]
        public LayoutItem Motion { get; set; } = new LayoutItem(9);

        [JsonIgnore]
        public List<LayoutItem> Items { get; set; } = new List<LayoutItem>();

        /// <summary>
        /// Size rate of column.
        /// </summary>
        public class SizeRateColumn
        {
            public double SizeRate { get; set; } = 1.0;

            public List<double> SizeRateRows { get; set; } = new List<double>();

            public SizeRateColumn()
            {
            }
        }

        public List<SizeRateColumn> SizeRateColumns { get; set; } = new List<SizeRateColumn>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPanelLayout()
        {
            SetItems();
        }

        /// <summary>
        /// Set properties to Items.
        /// </summary>
        public void SetItems()
        {
            Items.Clear();

            foreach (var prop in GetType().GetProperties().Where(x => x.PropertyType == typeof(LayoutItem)))
            {
                var item = (LayoutItem?)prop.GetValue(this);
                if (item == null) continue;
                item.Name = prop.Name;

                if (prop.GetCustomAttribute(typeof(LayoutItemAttribute), true) is LayoutItemAttribute attr)
                {
                    item.Name = attr.Name;
                    item.SetDefault(attr.LayoutLocDef);
                }

                Items.Add(item);
            }
        }
    }
}
