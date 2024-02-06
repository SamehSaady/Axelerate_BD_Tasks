using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Utils;

namespace RvtLib.Revit.Extensions
{
    public static class XYZ_Extensions
    {
        /// <summary>
        /// Visualizes this <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="category">The category of the visualized <paramref name="point"/>.</param>
        public static void Visualize(this XYZ point, Document doc, BuiltInCategory category = BuiltInCategory.OST_GenericModel)
        {
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(category));
            ds.SetShape(new List<GeometryObject>() { Point.Create(point) });
        }

        /// <summary>
        /// Rounds the components of this <paramref name="xyz"/> by <paramref name="decimalPlaces"/>.
        /// </summary>
        /// <param name="xyz"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static XYZ Round(this XYZ xyz, int decimalPlaces = GeoUtils.DecimalPlaces)
        {
            return new XYZ(Math.Round(xyz.X, decimalPlaces), Math.Round(xyz.Y, decimalPlaces), Math.Round(xyz.Z, decimalPlaces));
        }

        /// <summary>
        /// Checks whether this <paramref name="vector1"/> is parallel (Collinear) with <paramref name="vector2"/> or not.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <remarks>
        /// Cross Product of the two parallel vectors must be zero (0, 0, 0), i.e. its magnitude (length) is zero.
        /// <br> Only the 'Direction' of the two vectors is important in the definition of collinearity. Thus, 'Magnitude' and 'Orientation' play no role. </br>
        /// </remarks>
        /// <returns></returns>
        public static bool IsParallel(this XYZ vector1, XYZ vector2)
        {
            XYZ crossProduct = vector1.CrossProduct(vector2);

            return crossProduct.IsZeroLength();
        }

        /// <summary>
        /// Checks whether this <paramref name="vector1"/> is Perpendicular (Orthogonal) to <paramref name="vector2"/> or not.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <remarks>
        /// Dot (Scalar) Product of the two perpendicular vectors must be zero.
        /// </remarks>
        /// <returns></returns>
        public static bool IsPerpendicular(this XYZ vector1, XYZ vector2)
        {
            return Math.Round(vector1.DotProduct(vector2), GeoUtils.DecimalPlaces) == 0;
        }

        /// <summary>
        /// Projects this <paramref name="point"/> on the XY plane.
        /// <br>i.e., the Z component equals zero.</br>
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ ProjectOn_XY(this XYZ point)
        {
            return new XYZ(point.X, point.Y, 0);
        }

        /// <summary>
        /// Retrieves a new XYZ with absolute (+ve) values of this <paramref name="xyz"/>.
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Abs(this XYZ xyz)
        {
            return new XYZ(Math.Abs(xyz.X), Math.Abs(xyz.Y), Math.Abs(xyz.Z));
        }

        /// <summary>
        /// Retrieves the perpendicular counter clock-wise vector to the this <paramref name="unitVector_XY"/>, in XY plane.
        /// </summary>
        /// <param name="unitVector_XY">A 2D unit vector in XY plane ([0-1], [0-1], 0).</param>
        /// <returns></returns>
        public static XYZ GetPerpendicularDirection_CCW_XY(this XYZ unitVector_XY)
        {
            return new XYZ(-unitVector_XY.Y, unitVector_XY.X, 0);
        }

        /// <summary>
        /// Retrieves the perpendicular clock-wise vector to the this <paramref name="unitVector_XY"/>, in XY plane.
        /// </summary>
        /// <param name="unitVector_XY">A 2D unit vector in XY plane ([0-1], [0-1], 0).</param>
        /// <returns></returns>
        public static XYZ GetPerpendicularDirection_CW_XY(this XYZ unitVector_XY)
        {
            return new XYZ(unitVector_XY.Y, -unitVector_XY.X, 0);
        }


    }
}
