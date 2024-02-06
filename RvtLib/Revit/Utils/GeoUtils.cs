using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RvtLib.Revit.EqualityComparers;
using RvtLib.CS;
using RvtLib.Revit.Extensions;

namespace RvtLib.Revit.Utils
{
    /// <summary>
    /// Contains utilities for Geometry in Revit API.
    /// </summary>
    public static class GeoUtils
    {
        /// <summary>
        /// Number of digits to round by.
        /// </summary>
        public const int DecimalPlaces = 7;
        /// <summary>
        /// Tolerance = 1E^-DecimalPlaces.
        /// </summary>
        public static readonly double Tolerance = Math.Pow(10, -DecimalPlaces);



        // Solid
        /// <summary>
        /// Retrieves the Solid (visible Geometry by default) of the passed Element.
        /// </summary>
        /// <param name="ele"> A Model Element. </param>
        /// <param name="view"> The view to retrieve geometry of the <paramref name="ele"/> from. </param>
        /// <param name="includeNonVisibleObjects"> Whether to include the non-visible geometry or not. </param>
        /// <returns></returns>
        public static Solid GetElementSolid(Element ele, View view = null, bool includeNonVisibleObjects = false)
        {
            Options options = new Options();
            options.ComputeReferences = true;  // Default value is 'false'.
            options.DetailLevel = ViewDetailLevel.Undefined;  // Default value is 'Medium'.
            options.IncludeNonVisibleObjects = includeNonVisibleObjects;

            if (view != null)
                options.View = view;  // Default value is 'null' however, you can't explicitly set it by 'null' or an exception will be raised.

            GeometryElement geoEle = ele.get_Geometry(options);
            Solid solid = null;
            GetGeoEle_Solid_InsGeo(geoEle, ref solid);

            return solid;
        }

        /// <summary>
        /// (Recursive Method) Retrieves Solid of the passed GeometryElement.
        /// <br> If <paramref name="geoEle"/> is 'GeometryInstance', retrieves Solid from Instance Geometry (with project Global coordinates for all geometry objs). </br>
        /// </summary>
        /// <param name="geoEle"></param>
        /// <param name="solid">Retrieved Solid. Initialize it by null before passing.</param>
        public static void GetGeoEle_Solid_InsGeo(GeometryElement geoEle, ref Solid solid)
        {
            foreach (GeometryObject geoObj in geoEle)
            {
                Solid famIns_Solid = geoObj as Solid;

                if (famIns_Solid != null && Math.Round(famIns_Solid.Volume, 9) != 0)
                {
                    solid = famIns_Solid;
                    break;
                }

                //If this 'GeometryObject' is a 'GeometryInstance', call GetGeoEle_Solid() again (occures with 'FamilyInstance' objs):
                GeometryInstance geomInst = geoObj as GeometryInstance;

                if (geomInst != null)
                {
                    GeometryElement geomEle_OfGeomIns = geomInst.GetInstanceGeometry();  // Global Coordinates from Instance Geometry.
                    GetGeoEle_Solid_InsGeo(geomEle_OfGeomIns, ref solid);
                }
                // If both curves & geomInst are null, then this geomObj will be skipped.
            }
        }

        /// <summary>
        /// Retrieves the Curves of the passed <paramref name="ele"/>.
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="view"> The view to retrieve geometry of the <paramref name="ele"/> from. </param>
        /// <param name="includeNonVisibleObjects"> Whether to include the non-visible geometry or not. </param>
        /// <param name="computeRefs">Whether to compute references for the retrieved Geometry Objects or not.</param>
        /// <param name="DetailLevel">The detail level of the geometry extracted.</param>
        /// <returns></returns>
        public static List<Curve> GetElementCurves(Element ele, View view = null, bool includeNonVisibleObjects = false, bool computeRefs = false, ViewDetailLevel DetailLevel = ViewDetailLevel.Undefined)
        {
            Options options = new Options();
            options.ComputeReferences = computeRefs;  // Default value is 'false'.
            options.IncludeNonVisibleObjects = includeNonVisibleObjects;

            if (view != null)
            {
                options.View = view;  // Default value is 'null' however, you can't explicitly set it by 'null' or an exception will be raised.
            }
            else
                options.DetailLevel = DetailLevel;  // Default value is 'Medium'.

            GeometryElement geoEle = ele.get_Geometry(options);
            List<Curve> curves = new List<Curve>();
            GetGeoEle_Curves_InsGeo(geoEle, ref curves);

            return curves;
        }

        /// <summary>
        /// (Recursive Method) Retrieves <paramref name="curves"/> of the passed <paramref name="geoEle"/>.
        /// <br> If <paramref name="geoEle"/> is 'GeometryInstance', retrieves Curves from Instance Geometry (with project Global coordinates for all geometry objs). </br>
        /// </summary>
        /// <param name="geoEle"></param>
        /// <param name="curves">
        /// Retrieved Curves.
        /// <br> Initialize it before passing.</br>
        /// </param>
        public static void GetGeoEle_Curves_InsGeo(GeometryElement geoEle, ref List<Curve> curves)
        {
            foreach (GeometryObject geoObj in geoEle)
            {
                if (geoObj is Curve curve)
                {
                    curves.Add(curve);
                    continue;
                }

                if (geoObj is GeometryInstance geomInst)
                {
                    GeometryElement geomEle_OfGeomIns = geomInst.GetInstanceGeometry();  // Global Coordinates from Instance Geometry.
                    GetGeoEle_Curves_InsGeo(geomEle_OfGeomIns, ref curves);
                }
            }
        }


        // Faces:
        /// <summary>
        /// Retrieves the top PlanarFace (horizontal or inclined) of the passed Solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <remarks>
        /// An instance may have more than one PlanarFace or,
        /// <br> For an inclined instance, the [FaceNormal] of a side Face may have Z value != 0, </br>
        /// <br> that's why all faces with Z value != 0 must be retrieved and queried for the minimum Z value. </br>
        /// </remarks>
        /// <returns>Top PlanarFace or null.</returns>
        public static PlanarFace GetSolid_TopPlanarFace(Solid solid)
        {
            // Don't use this method if you are working with an inclined top face of a wall and the wall contains an instance like Window.
            return GetSolid_TopPlanarFaces(solid).OrderByDescending(face => face.FaceNormal.Z).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the top PlanarFaces (horizontal or inclined) of the passed Solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <remarks>
        /// An instance may have more than one PlanarFace or,
        /// <br> For an inclined instance, the [FaceNormal] of a side Face may have Z value != 0, </br>
        /// <br> that's why all faces with Z value != 0 must be retrieved and queried for the minimum Z value. </br>
        /// </remarks>
        /// <returns></returns>
        public static List<PlanarFace> GetSolid_TopPlanarFaces(Solid solid)
        {
            List<PlanarFace> topPlanarFaces = new List<PlanarFace>();
            PlanarFace topPlanarFace;

            foreach (Face face in solid.Faces)
            {
                topPlanarFace = face as PlanarFace;

                if (topPlanarFace != null && Math.Round(topPlanarFace.FaceNormal.Z, DecimalPlaces) > 0)
                    topPlanarFaces.Add(topPlanarFace);
            }

            return topPlanarFaces;
        }

        /// <summary>
        /// Retrieves the bottom PlanarFaces (horizontal or inclined) of the passed Solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <remarks>
        /// An instance may have more than one PlanarFace or,
        /// <br> For an inclined instance, the [FaceNormal] of a side Face may have Z value != 0, </br>
        /// <br> that's why all faces with Z value != 0 must be retrieved and queried for the minimum Z value. </br>
        /// </remarks>
        /// <returns></returns>
        public static List<PlanarFace> GetSolid_BotPlanarFaces(Solid solid)
        {
            List<PlanarFace> botPlanarFaces = new List<PlanarFace>();
            PlanarFace botPlanarFace;

            foreach (Face face in solid.Faces)
            {
                botPlanarFace = face as PlanarFace;

                if (botPlanarFace != null && Math.Round(botPlanarFace.FaceNormal.Z, DecimalPlaces) < 0)
                    botPlanarFaces.Add(botPlanarFace);
            }

            return botPlanarFaces;
        }

        /// <summary>
        /// Retrieves the bottom PlanarFace (horizontal or inclined) of the passed Solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <remarks>
        /// An instance may have more than one PlanarFace or,
        /// <br> For an inclined instance, the [FaceNormal] of a side Face may have Z value != 0, </br>
        /// <br> that's why all faces with Z value != 0 must be retrieved and queried for the minimum Z value. </br>
        /// </remarks>
        /// <returns>Bottom PlanarFace or null</returns>
        public static PlanarFace GetSolid_BotPlanarFace(Solid solid)
        {
            return GetSolid_BotPlanarFaces(solid).OrderBy(face => face.FaceNormal.Z).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves side PlanarFaces of the passed <paramref name="solid"/>.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Side PlanarFaces or empty list.</returns>
        public static List<PlanarFace> GetSolid_SidePlanarFaces(Solid solid)
        {
            List<Face> allFaces = new List<Face>();

            if (solid != null)
            {
                foreach (Face face in solid.Faces)
                    allFaces.Add(face);
            }

            return allFaces.Where(face => face is PlanarFace planarFace
                && Math.Abs(Math.Round(planarFace.FaceNormal.Z, DecimalPlaces)) != 1)
                .Cast<PlanarFace>()
                .ToList();
        }


        // Curves & Edges:
        /// <summary>
        /// Retrieves the external CurveLoop of the passed <paramref name="face"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <remarks>
        /// - A Face that has openings will have external + internal Edges.
        /// <br> - The first CurveLoop always represents the external Edges of a Face. </br>
        /// </remarks>
        /// <returns></returns>
        public static CurveLoop GetFace_ExtCurveLoop(Face face)
        {
            return face.GetEdgesAsCurveLoops().FirstOrDefault();
        }

        /// <summary>
        /// Checks whether the passed <paramref name="lines"/> form a closed loop.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static bool IsClosedLoop(List<Line> lines)
        {
            return GetUniqueEndPoints(lines).Count == lines.Count;
        }

        /// <summary>
        /// Checks whether the passed <paramref name="lines"/> are continuous or not.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="closedLoop">True if the <paramref name="lines"/> form a closed loop, else false. </param>
        /// <remarks>
        /// The <paramref name="lines"/> are continuous if the end of each line coincides with the start of the next one.
        /// <br> If one or zero <paramref name="lines"/> are passed, returns false. </br>
        /// </remarks>
        /// <returns>True if they're continuous, else false.</returns>
        public static bool AreContinuousLines(List<Line> lines, bool closedLoop)

        {
            bool areCont = false;

            if (lines.Count <= 1)
                return false;

            for (int i = 0; i < lines.Count - 1; i++)
            {
                areCont = lines[i].GetEndPoint(1).IsAlmostEqualTo(lines[i + 1].GetEndPoint(0));

                if (!areCont)
                    return false;
            }

            if (closedLoop)
                areCont = lines.LastOrDefault().GetEndPoint(1).IsAlmostEqualTo(lines.FirstOrDefault().GetEndPoint(0));

            return areCont;
        }

        /// <summary>
        /// Re-arranges the passed <paramref name="lines"/> and makes them continuous.
        /// </summary>
        /// <param name="lines">Non-continuous curves that form a closed loop.</param>
        /// <param name="succeeded">Indicates whether the <paramref name="lines"/> became continuous successfully, or not.</param>
        /// <returns>True if the <paramref name="lines"/> are re-arranged to be continuous successfully, else false.</returns>
        public static List<Line> MakeContinuousLines(List<Line> lines, out bool succeeded)
        {
            succeeded = false;

            if (lines.Count <= 1)
                return lines;

            List<Line> contLines = new List<Line>() { lines.First() };
            Line currentLine;
            Line nextLine;

            for (int i = 0; i < lines.Count - 1; i++)
            {
                currentLine = contLines.Last();

                nextLine = lines.Except(new List<Line>() { currentLine }, new LineComparer())
                    .FirstOrDefault(line => currentLine.GetEndPoint(1).IsAlmostEqualTo(line.GetEndPoint(0)));

                if (nextLine == null)
                    return new List<Line>();

                contLines.Add(nextLine);
            }
            succeeded = true;

            return contLines;
        }

        /// <summary>
        /// Checks whether any line of the passed <paramref name="lines"/> is intersected with another line at a point that is not an end point of any of them.
        /// </summary>
        /// <param name="lines">intersected curves (closed/opened loop).</param>
        /// <returns></returns>
        public static bool IsAnyLineIntersectedAtNonEndPoint(List<Line> lines)
        {
            List<Line> linesExceptCurrent;
            bool AreIntersecAt_NonEndPoint;

            for (int i = 0; i < lines.Count; i++)
            {
                linesExceptCurrent = lines.Except(new List<Line>() { lines[i] }, new LineComparer()).ToList();
                AreIntersecAt_NonEndPoint = linesExceptCurrent.Any(line => line.IsIntersected(lines[i])
                    && !line.IsIntersectedAtEndPoint(lines[i]));

                if (AreIntersecAt_NonEndPoint)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates Segments between the passed <paramref name="points"/>.
        /// </summary>
        /// <param name="points">Must be at least two points.</param>
        /// <remarks>
        /// - A curve will be created from the last point to the first point if more than two <paramref name="points"/> are passed.
        /// <br> - Only one curve will be created in case two <paramref name="points"/> are passed. </br>
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<Line> CreateLinesFromPoints(List<XYZ> points)
        {
            if (points.Count < 2)
                throw new ArgumentException("You must pass at least two points");

            List<Line> lines = new List<Line>();
            int j;

            // If two points only are passed, just create one Line between them.
            if (points.Count == 2)
            {
                lines.Add(Line.CreateBound(points[0], points[1]));

                return lines;
            }

            for (int i = 0; i < points.Count; i++)
            {
                j = i + 1;
                j = j == points.Count ? 0 : j;

                lines.Add(Line.CreateBound(points[i], points[j]));
            }

            return lines;
        }


        // Vectors & Points:
        /// <summary>
        /// Retrieves the unique starting and ending points of the passed <paramref name="lines"/>.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<XYZ> GetUniqueEndPoints(List<Line> lines)
        {
            return lines.Select(line => line.GetEndPoint(0)).ToList();
        }






        /// <summary>
        /// Retrieves the passed <paramref name="point"/> as a string.
        /// </summary>
        /// <param name="point">Coordinate to be converted.</param>
        /// <param name="decimalPlaces">A value within range (0 to 15).
        /// <br> If the value is less than 0 or bigger than 15, no rounding will occure.</br>
        /// </param>
        /// <returns>The point as a string: (X, Y, Z).</returns>
        public static string XYZ_AsString(XYZ point, int decimalPlaces = -1)
        {
            bool round = decimalPlaces >= 0 && decimalPlaces <= 15;  // Allowed values to pass to Math.Round().
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            if (round)
            {
                x = Math.Round(point.X, decimalPlaces);
                y = Math.Round(point.Y, decimalPlaces);
                z = Math.Round(point.Z, decimalPlaces);
            }

            return $"({x}, {y}, {z})";
        }
    }
}
