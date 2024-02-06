using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Extensions;

namespace RvtLib.Revit.Utils
{
    public static class GeoVisUtils
    {
        /// <summary>
        /// Visualizes the passed <paramref name="points"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="points"></param>
        /// <param name="category">The category of the visualized <paramref name="points"/>.</param>
        public static void VisualizePoints(Document doc, List<XYZ> points, BuiltInCategory category = BuiltInCategory.OST_GenericModel)
        {
            foreach (var point in points)
                point.Visualize(doc, category);
        }

        /// <summary>
        /// Visualizes the passed <paramref name="points"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="lines"></param>
        /// <param name="visualizeEndPoints">Whether to visualize the end points of these <paramref name="lines"/> or not.</param>
        /// <param name="category">The category of the visualized <paramref name="lines"/>.</param>
        public static void VisualizeLines(Document doc, List<Line> lines, bool visualizeEndPoints = false, BuiltInCategory category = BuiltInCategory.OST_GenericModel)
        {
            foreach (var line in lines)
                line.Visualize(doc, visualizeEndPoints, category);
        }
    }
}
