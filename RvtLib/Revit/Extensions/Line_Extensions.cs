using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Utils;

namespace RvtLib.Revit.Extensions
{
    public static class Line_Extensions
    {
        /// <summary>
        /// Visualizes this <paramref name="line"/> with and option to visualize its end points.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="doc"></param>
        /// <param name="visualizeEndPoints">Whether to visualize the end points of this <paramref name="line"/> or not.</param>
        /// <param name="category">The category of the visualized <paramref name="line"/>.</param>
        public static void Visualize(this Line line, Document doc, bool visualizeEndPoints = false, BuiltInCategory category = BuiltInCategory.OST_GenericModel)
        {
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(category));
            ds.SetShape(new List<GeometryObject>() { line });

            if (visualizeEndPoints)
            {
                line.GetEndPoint(0).Visualize(doc, category);
                line.GetEndPoint(1).Visualize(doc, category);
            }
        }

        /// <summary>
        /// Checks whether this <paramref name="line1"/> intersects with <paramref name="line2"/> in a point or line.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static bool IsIntersected(this Line line1, Line line2)
        {
            // Overlap, Subset, Superset, Equal => intersection.
            // Disjoint => no intersectiion.
            return line1.Intersect(line2) != SetComparisonResult.Disjoint;
        }

        /// <summary>
        /// Checks whether this <paramref name="line1"/> contains the whole or part of <paramref name="line2"/> (and vice versa), or not.
        /// </summary>
        /// <param name="line1">Bound Line.</param>
        /// <param name="line2">Bound Line.</param>
        /// <remarks>
        /// The longer line can be <paramref name="line1"/> or <paramref name="line2"/>.
        /// <br> If the two lines intersect at a point, returns false.</br>
        /// </remarks>
        /// <returns></returns>
        public static bool Contains(this Line line1, Line line2)
        {
            return line1.Intersect(line2) == SetComparisonResult.Equal;
        }

        /// <summary>
        /// Retrieves the intersection point between this <paramref name="line1"/> and <paramref name="line2"/>, or null if there is no intersection.
        /// </summary>
        /// <param name="line1">Bound/unbound Line.</param>
        /// <param name="line2">Bound/unbound Line.</param>
        /// <returns></returns>
        public static XYZ GetIntersectionPoint(this Line line1, Line line2)
        {
            line1.Intersect(line2, out IntersectionResultArray resultArr);  // [resultArr] is null or empty if there's no intersection.

            if (resultArr == null || resultArr.IsEmpty)  // No intersection.)  // No intersection.
                return null;

            return resultArr.get_Item(0).XYZPoint;
        }

        /// <summary>
        /// Checks whether this <paramref name="line1"/> and <paramref name="line2"/> are intersected at an end point or intersect at a non-end point.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <remarks>
        /// Make sure that <paramref name="line1"/> and <paramref name="line2"/> intersect at a point.
        /// </remarks>
        /// <returns>True if they are intersected at an end point, false if they intersect at another point.</returns>
        public static bool IsIntersectedAtEndPoint(this Line line1, Line line2)
        {
            XYZ[] endPoints =
            {
                line1.GetEndPoint(0),
                line1.GetEndPoint(1),
                line2.GetEndPoint(0),
                line2.GetEndPoint(1),
            };

            XYZ intersecPoint = line1.GetIntersectionPoint(line2);

            return endPoints.Any(endPoint => endPoint.IsAlmostEqualTo(intersecPoint));
        }

        /// <summary>
        /// Checks whether this <paramref name="line1"/> is parallel (Collinear) with <paramref name="line2"/> or not.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <remarks>
        /// Cross Product of the two parallel lines vectors must be zero (0, 0, 0), i.e. its magnitude (length) is zero.
        /// <br> Only the 'Direction' of the two vectors is important in the definition of collinearity. Thus, 'Magnitude' and 'Orientation' play no role. </br>
        /// </remarks>
        /// <returns></returns>
        public static bool IsParallel(this Line line1, Line line2)
        {
            return line1.Direction.IsParallel(line2.Direction);
        }

        /// <summary>
        /// Projects this <paramref name="line"/> on the XY plane.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Line ProjectOn_XY(this Line line)
        {
            return Line.CreateBound(new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, 0)
                , new XYZ(line.GetEndPoint(1).X, line.GetEndPoint(1).Y, 0));
        }

        /// <summary>
        /// Divides this <paramref name="line"/> by the passed <paramref name="distance"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="distance">in ft.</param>
        /// <param name="eleminateRemainder">
        /// In case there's a remained length (i.e., <paramref name="line"/> Length % <paramref name="distance"/> != 0),
        /// <br>Set it to true to recalculate the <paramref name="distance"/> to eleminate the ramainder, or false to use the passed <paramref name="distance"/>.</br>
        /// </param>
        /// <param name="appendStartPoint">Whether to append the <paramref name="line"/> start point to the returned points.</param>
        /// <param name="appendEndPoint">Whether to append the <paramref name="line"/> end point to the returned points.</param>
        /// <returns></returns>
        public static List<XYZ> DivideByDist(this Line line, double distance, bool eleminateRemainder = false, bool appendStartPoint = false, bool appendEndPoint = false)
        {
            List<XYZ> points = new List<XYZ>();
            double cumDist = 0;  // Cumulative distance.

            // Num of points between the end points:
            int numOfPoints = (int)Math.Floor(line.Length / distance);
            // If the last point matches the end point, exclude it:
            double remainder = line.Length % distance;

            if (Math.Round(remainder, GeoUtils.DecimalPlaces) == 0)
            {
                // If the last point matches the end point, exclude it:
                numOfPoints--;
            }
            else if (eleminateRemainder)
            {
                distance = line.Length / numOfPoints;
                numOfPoints = (int)Math.Floor(line.Length / distance) - 1;  // Excluding the end point
            }

            for (int i = 0; i < numOfPoints; i++)
            {
                cumDist += distance;
                points.Add(line.Evaluate(cumDist, false));
            }

            if (appendStartPoint)
                points.Insert(0, line.GetEndPoint(0));

            if (appendEndPoint)
                points.Insert(points.Count, line.GetEndPoint(1));

            return points;
        }
    }
}
