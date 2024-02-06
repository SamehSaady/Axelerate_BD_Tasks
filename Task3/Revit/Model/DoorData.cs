using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task3.Revit.Entry;

using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;
using RvtLib.Revit.EqualityComparers;

namespace Task3.Revit.Model
{
    internal class DoorData
    {
        public FamilyInstance Door { get; set; }
        /// <summary>
        /// The Edge Lines of this Door projected on XY plane.
        /// </summary>
        public List<Line> EdgeLines_XY { get; set; }
        /// <summary>
        /// The Edge Line that contains this Door.
        /// </summary>
        public Line RoomEdgeLine { get; set; }
        /// <summary>
        /// The width of the Threshold, in ft.
        /// </summary>
        public double ThresholdWidth { get; set; }
        /// <summary>
        /// The direction of the Threshold.
        /// </summary>
        public XYZ ThresholdDir { get; set; }
        /// <summary>
        /// The four points of the Threshold.
        /// </summary>
        public List<XYZ> ThresholdPoints { get; set; }
        public RoomData RoomData { get; set; }



        public DoorData(FamilyInstance door, RoomData roomData)
        {
            Door = door;
            RoomData = roomData;
            EdgeLines_XY = GeoUtils.GetElementCurves(Door, null, true)
                .OfType<Line>()
                .Where(line => !line.Direction.Abs().IsAlmostEqualTo(XYZ.BasisZ))  // Excluding Vertical Lines (in Z).
                .Select(line => line.ProjectOn_XY())
                .Distinct(new LineComparer())
                .ToList();

            ThresholdWidth = (Door.Host as Wall).Width / 2;
            ThresholdDir = Door.FacingOrientation;
        }



        /// <summary>
        /// Specifies the Room Edge Line of this Doors.
        /// </summary>
        public void SpecifyRoomEdgeOfDoor()
        {
            RoomEdgeLine = RoomData.EdgeLines_XY.FirstOrDefault(roomLine => EdgeLines_XY.Any(doorLine => roomLine.Contains(doorLine)));
        }


        private XYZ getThresholdDir()
        {
            XYZ pointOnRoomEdge = (RoomEdgeLine.GetEndPoint(0) + RoomEdgeLine.GetEndPoint(1)) / 2;

            // The direction that leads to the farest point from the Room:
            return new XYZ[] { Door.FacingOrientation, Door.FacingOrientation.Negate() }
                .OrderByDescending(dir => (pointOnRoomEdge + dir - RoomData.LocPoint).GetLength())
                .FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the four points of the Threshold.
        /// </summary>
        public void RetrieveThresholdPoints()
        {
            ThresholdDir = getThresholdDir();

            // Getting the Door Edge Lines that are parallel to the Door (facing) Direction:
            List<Line> parallelEdgeLines_XY = EdgeLines_XY.Where(line => line.Direction.IsParallel(ThresholdDir)).ToList();

            List<XYZ> points = parallelEdgeLines_XY.Where(line => line.IsIntersected(RoomEdgeLine))
                .Select(line => line.GetIntersectionPoint(RoomEdgeLine))
                .Distinct(new XYZComparer())
                .OrderBy(p => (p - RoomEdgeLine.GetEndPoint(0)).GetLength())  // Sorting them so that they can form a continuous CurveLoop.
                .ToList();
            // They might be more than two points, so take the nearest point and farest point from the [RoomEdgeLine] start point.

            XYZ thresholdVector = ThresholdWidth * ThresholdDir;
            ThresholdPoints = new List<XYZ>()
            {
                points.First(),
                points.First() + thresholdVector,
                points.Last() + thresholdVector,
                points.Last()
            };
        }
    }
}
