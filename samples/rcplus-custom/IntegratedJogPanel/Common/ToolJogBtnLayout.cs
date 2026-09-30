// -----------------------------------------------------------------------
// <copyright file="ToolJogBtnLayout.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IntegratedJogPanel.Common
{
    public static class ToolJogBtnLayout
    {
        /// <summary>
        /// Tool jog button layout option
        /// </summary>
        public enum Option
        {
            /// <summary>View direction toward Tool Z-, X is left</summary>
            TwdZMinusXLeft = 0,

            /// <summary>View direction toward Tool Z-, X is bottom</summary>
            TwdZMinusXBottom = 1,

            /// <summary>View direction toward Tool Z-, X is right</summary>
            TwdZMinusXRight = 2,

            /// <summary>View direction toward Tool Z-, X is top</summary>
            TwdZMinusXTop = 3,

            /// <summary>View direction toward Tool Z+, X is left</summary>
            TwdZPlusXLeft = 4,

            /// <summary>View direction toward Tool Z+, X is bottom</summary>
            TwdZPlusXBottom = 5,

            /// <summary>View direction toward Tool Z+, X is right</summary>
            TwdZPlusXRight = 6,

            /// <summary>View direction toward Tool Z+, X is top</summary>
            TwdZPlusXTop = 7,
        }

        /// <summary>
        /// Get axes labels.
        /// </summary>
        /// <param name="opt">Option</param>
        /// <returns>Button label array</returns>
        public static string[] GetAxesLabels(Option opt)
        {
            //  3
            // 0 2
            //  1
            return (opt switch
            {
                Option.TwdZMinusXLeft =>
                                "X,Y, , ",
                Option.TwdZMinusXBottom =>
                                " ,X,Y, ",
                Option.TwdZMinusXRight =>
                                " , ,X,Y",
                Option.TwdZMinusXTop =>
                                "Y, , ,X",
                Option.TwdZPlusXLeft =>
                                "X, , ,Y",
                Option.TwdZPlusXBottom =>
                                "Y,X, , ",
                Option.TwdZPlusXRight =>
                                " ,Y,X, ",
                Option.TwdZPlusXTop =>
                                " , ,Y,X",
                _ => "X,Y, , ",
            }).Split(",");
        }

        /// <summary>
        /// Get jog button labels.
        /// </summary>
        /// <param name="opt">Option</param>
        /// <returns>Button 4 labels array</returns>
        public static string[] GetJogBtnLabels(Option opt)
        {
            //  1
            // 0 3 
            //  2
            return (opt switch
            {
                Option.TwdZMinusXLeft =>
                                "X+,Y-,Y+,X-",
                Option.TwdZMinusXBottom =>
                                "Y-,X-,X+,Y+",
                Option.TwdZMinusXRight =>
                                "X-,Y+,Y-,X+",
                Option.TwdZMinusXTop =>
                                "Y+,X+,X-,Y-",
                Option.TwdZPlusXLeft =>
                                "X+,Y+,Y-,X-",
                Option.TwdZPlusXBottom =>
                                "Y+,X-,X+,Y-",
                Option.TwdZPlusXRight =>
                                "X-,Y-,Y+,X+",
                Option.TwdZPlusXTop =>
                                "Y-,X+,X-,Y+",
                _ => "X+,Y-,Y+,X-",
            }).Split(",");
        }
    }
}
