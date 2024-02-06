using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Enums;

namespace RvtLib.Revit.Extensions
{
    public static class BBX_Extensions
    {
        /// <summary>
        /// Retrieves the Outline that represents this <paramref name="bbx"/>.
        /// </summary>
        /// <param name="bbx"></param>
        /// <returns></returns>
        public static Outline GetOutline(this BoundingBoxXYZ bbx)
        {
            return new Outline(bbx.Min, bbx.Max);
        }

        /// <summary>
        /// Retrieves the centroid of this <paramref name="bbx"/>.
        /// </summary>
        /// <param name="bbx"></param>
        /// <returns></returns>
        public static XYZ GetCentroid(this BoundingBoxXYZ bbx)
        {
            return (bbx.Min + bbx.Max) / 2;
        }

        /// <summary>
        /// Retrieves the Corner points of this <paramref name="bbx"/> projected on the XY plane.
        /// </summary>
        /// <param name="bbx"></param>
        /// <returns>
        /// Corner points with their corner enumeration:
        /// <br> 1) BotLeft</br>
        /// <br> 2) BotRight</br>
        /// <br> 3) TopLeft</br>
        /// <br> 4) TopRight</br>
        /// </returns>
        public static List<(XYZ CornerPoint, Corner Corner)> GetCorners_XY(this BoundingBoxXYZ bbx)
        {
            return new List<(XYZ CornerPoint, Corner Corner)>()
            {
                (bbx.Min.ProjectOn_XY(), Corner.BotLeft),
                (new XYZ(bbx.Max.X, bbx.Min.Y, 0), Corner.BotRight),
                (bbx.Max.ProjectOn_XY(), Corner.TopRight),
                (new XYZ(bbx.Min.X, bbx.Max.Y, 0).ProjectOn_XY(), Corner.TopLeft)
            };
        }
    }
}
