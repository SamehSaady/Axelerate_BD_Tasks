using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.Revit.Extensions
{
    public static class Face_Extensions
    {

        /// <summary>
        /// Checks whether this <paramref name="face"/> intersects with <paramref name="line"/> in a point or line, or not.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool IsIntersected(this Face face, Line line)
        {
            // Overlap, Subset => intersection.
            // Disjoint => no intersectiion.
            return face.Intersect(line) != SetComparisonResult.Disjoint;
        }

        /// <summary>
        /// Checks whether this <paramref name="face"/> contains the whole or part of <paramref name="line"/>, or not.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool Contains(this Face face, Line line)
        {
            return face.Intersect(line) == SetComparisonResult.Subset;
        }


        /// <summary>
        /// Retrieves the intersection point between this <paramref name="face"/> and <paramref name="line"/>, or null if there is no intersection.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="line">Bound/unbound Line.</param>
        /// <returns></returns>
        public static XYZ GetIntersectionPoint(this Face face, Line line)
        {
            face.Intersect(line, out IntersectionResultArray resultArr);  // [resultArr] is null or empty if there's no intersection.

            if (resultArr == null || resultArr.IsEmpty)  // No intersection.
                return null;

            return resultArr.get_Item(0).XYZPoint;
        }
    }
}
