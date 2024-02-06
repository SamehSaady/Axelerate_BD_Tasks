using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace Task2.Revit.Model
{
    /// <summary>
    /// Represents a Linear Wall.
    /// </summary>
    internal class WallData
    {
        public Wall Wall { get; set; }
        public Solid Solid { get; set; }
        /// <summary>
        /// The location line (C.L) of the Wall.
        /// </summary>
        public Line LocLine { get; set; }
        /// <summary>
        /// The longitudinal lines that are parallel to the Wall C.L projected on the XY Plane.
        /// </summary>
        /// <remarks>
        /// They might be more than two lines.
        /// </remarks>
        //public List<Line> LongitEdgeLines_XY { get; set; }  // Ignored approach.
        public List<PlanarFace> LongitSideFaces { get; set; }
        public PlanarFace TargetFace { get; set; }
        /// <summary>
        /// Indicates if this wall is horizontal or vertical.
        /// </summary>
        public bool  HzWall { get; set; }



        public WallData(Wall wall)
        {
            Wall = wall;
            Solid = GeoUtils.GetElementSolid(wall, null);
            LocLine = (Wall.Location as LocationCurve).Curve as Line;
            //LongitEdgeLines_XY = getLongitEdgeLines_XY(Solid, LocLine);  // Ignored approach.

            LongitSideFaces = GeoUtils.GetSolid_SidePlanarFaces(Solid)
                .Where(face => face.FaceNormal.IsPerpendicular(LocLine.Direction))
                .ToList();
        }


        private List<Line> getLongitEdgeLines_XY(Solid solid, Line locLine)  // Ignored Approach.
        {
            // A Wall may have multiple bottom faces and missing faces due to the existense of the Doors,
            // so it's better to work with it's top Face.
            List<PlanarFace> topFaces = GeoUtils.GetSolid_TopPlanarFaces(solid);
            List<CurveLoop> extCurveLoops = topFaces.Select(face => GeoUtils.GetFace_ExtCurveLoop(face)).ToList();

            return extCurveLoops.SelectMany(loop => loop)
                .Cast<Line>()
                .Where(line => line.IsParallel(locLine))
                .Select(line => line.ProjectOn_XY())
                .ToList();
        }

        public Room GetNearestRoom(List<Room> rooms)
        {
            XYZ wallCentroid_XY = (LocLine.GetEndPoint(0).ProjectOn_XY() + LocLine.GetEndPoint(1).ProjectOn_XY()) / 2;
            
            return rooms.OrderBy(room => ((room.Location as LocationPoint).Point.ProjectOn_XY() - wallCentroid_XY).GetLength())
                .First();
        }
    }
}
