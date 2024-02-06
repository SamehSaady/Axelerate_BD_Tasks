using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task3.Revit.Entry;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;
using RvtLib.Revit.Enums;

namespace Task3.Revit.Model
{
    internal class RoomData
    {
        /// <summary>
        /// Default Floor Type for Floors and Threshold to be modeled.
        /// </summary>
        public static FloorType DefFloorType { get; set; }

        public Room Room { get; set; }
        public XYZ LocPoint { get; set; }
        /// <summary>
        /// The Room edge lines of the bottom face projected on XY plane.
        /// </summary>
        public List<Line> EdgeLines_XY { get; set; }
        /// <summary>
        /// The Room edge points of the bottom face projected on XY plane.
        /// </summary>
        public List<XYZ> EdgePoints_XY { get; set; }
        public List<DoorData> DoorsData { get; set; }
        /// <summary>
        /// The same type of the existing Floor or a default "Generic" type.
        /// </summary>
        public FloorType FloorType { get; set; }
        public CurveLoop FloorBoundary { get; set; }
        /// <summary>
        /// The existing Floor in this Room or null if there's no Floor.
        /// </summary>
        public Floor ExistingFloor { get; set; }
        /// <summary>
        /// The newly created Floor with the Treshold.
        /// </summary>
        public Floor NewFloor { get; set; }



        public RoomData(Room room, List<FamilyInstance> doorsData, Floor existingFloor)
        {
            Room = room;
            LocPoint = (Room.Location as LocationPoint).Point;
            DoorsData = doorsData.Select(d => new DoorData(d, this)).ToList();
            ExistingFloor = existingFloor;

            FloorType = existingFloor != null ?
                ExtCmd.Doc.GetElement(existingFloor.GetTypeId()) as FloorType
                : DefFloorType;

            Solid solid = null;
            GeoUtils.GetGeoEle_Solid_InsGeo(room.ClosedShell, ref solid);
            PlanarFace botFace = GeoUtils.GetSolid_BotPlanarFace(solid);
            CurveLoop extCurveLoop = GeoUtils.GetFace_ExtCurveLoop(botFace);

            EdgeLines_XY = extCurveLoop.OfType<Line>()
                .Select(line => line.ProjectOn_XY())
                .ToList();
        }



        /// <summary>
        /// Retrieves the Threshold four points for each door in this Room.
        /// </summary>
        public void RetrieveThresholdPoints()
        {
            foreach (var doorData in DoorsData)
            {
                doorData.SpecifyRoomEdgeOfDoor();
                doorData.RetrieveThresholdPoints();
            }
        }

        /// <summary>
        /// Generates the Floor boundary as a CurveLoop that includes the Threshold boundary.
        /// </summary>
        public void GenerateFloorBoundary()
        {
            List<XYZ> edgePoints_XY = new List<XYZ>();

            foreach (Line line in EdgeLines_XY)
            {
                edgePoints_XY.Add(line.GetEndPoint(0));

                // Getting Doors on this Edge Line and sorting them from the nearest to the farest in order to generate a valid CurveLoop:
                List<DoorData> doorsOnLine = DoorsData.Where(d => d.RoomEdgeLine == line)
                    .OrderBy(d => (d.ThresholdPoints.First() - line.GetEndPoint(0)).GetLength())
                    .ToList();

                edgePoints_XY.AddRange(doorsOnLine.SelectMany(d => d.ThresholdPoints));
            }

            FloorBoundary = CurveLoop.Create(GeoUtils.CreateLinesFromPoints(edgePoints_XY).Cast<Curve>().ToList());
        }

        /// <summary>
        /// Creates a Floor in this Room that includes a Threshold.
        /// </summary>
        /// <remarks>
        /// - If there's an existing Floor in this Room, deletes it first, then creates a new one with the Threshold using the same FloorType and Offset.
        /// <br>- If there's no existing Floor in this Room, the new is created with a default "Generic" type and takes no offset.</br>
        /// </remarks>
        public void CreateFloorWithThreshold()
        {
            double offset = 0;

            if (ExistingFloor != null)
            {
                offset = ExistingFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                ExtCmd.Doc.Delete(ExistingFloor.Id);
            }

            NewFloor = Floor.Create(ExtCmd.Doc, new List<CurveLoop>() { FloorBoundary }, FloorType.Id, Room.Level.Id);
            NewFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(offset);
        }
    }
}
