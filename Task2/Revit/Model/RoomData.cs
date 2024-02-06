using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task2.Revit.Entry;

using RvtLib.Revit.Enums;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace Task2.Revit.Model
{
    internal class RoomData
    {
        public const string TargetRoomName = "bathroom";
        public const string wcFamName = "Toilet-Wall_Mount-KOHLER-Veil-K-76395 [KBD]";
        public const string wcTypeName = "ADA";
        public const double wcWidth = 5;
        public const double wcDeltaMin = 1.5;


        public Room Room { get; set; }
        public Solid Solid { get; set; }
        /// <summary>
        /// The Room edge lines of the bottom face.
        /// </summary>
        public List<Line> EdgeLines { get; set; }
        /// <summary>
        /// The Room edge lines projected on the XY Plane.
        /// </summary>
        public Line TargetEdgeLine { get; set; }
        public XYZ FarestEdgePointFromDoor { get; set; }
        public XYZ WCInsertionPoint { get; set; }
        public Corner WCCorner { get; set; }

        public WallData WallData { get; set; }
        public FamilyInstance Door { get; set; }
        public XYZ DoorLocPoint { get; set; }
        public FamilyInstance WC { get; set; }



        public RoomData(Room room, WallData wallData)
        {
            Room = room;
            WallData = wallData;
            Solid solid = null;
            GeoUtils.GetGeoEle_Solid_InsGeo(room.ClosedShell, ref solid);
            Solid = solid;
            PlanarFace botFace = GeoUtils.GetSolid_BotPlanarFace(solid);
            CurveLoop extCurveLoop = GeoUtils.GetFace_ExtCurveLoop(botFace);
            EdgeLines = extCurveLoop.OfType<Line>().ToList();

            (WallData.TargetFace, TargetEdgeLine) = retrieveTargetWallFaceAndRoomEdge(WallData.LongitSideFaces, EdgeLines);

            if (TargetEdgeLine == null)
                return;

            WallData.HzWall = WallData.TargetFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisY)
                || WallData.TargetFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisY.Negate());

            Door = FilterUtils.GetDoors(ExtCmd.Doc, ExtCmd.UIDoc.ActiveView)
                .FirstOrDefault(door => (door.ToRoom != null && door.ToRoom.Id == Room.Id)
                || (door.FromRoom != null && door.FromRoom.Id == Room.Id));

            if (Door == null)
                return;

            DoorLocPoint = (Door.Location as LocationPoint).Point;
            FarestEdgePointFromDoor = getFarsetEdgePointFromDoor(DoorLocPoint, TargetEdgeLine);
            WCCorner = specifyWC_Corner(Room, FarestEdgePointFromDoor);
            WCInsertionPoint = get_WC_InsPoint(WallData.HzWall, WCCorner, FarestEdgePointFromDoor);
            //WCInsertionPoint = FarestEdgePointFromDoor;
        }



        private (PlanarFace face, Line targetLine) retrieveTargetWallFaceAndRoomEdge(List<PlanarFace> wallLongitSideFaces, List<Line> roomEdgeLines)
        {
            foreach (var face in wallLongitSideFaces)
            {
                foreach (var line in roomEdgeLines)
                {
                    if (face.Contains(line))
                    {
                        return (face, line);
                    }
                }
            }

            return (null, null);
        }

        private XYZ getFarsetEdgePointFromDoor(XYZ doorLocPoint, Line roomTargetEdgeLine)
        {
            return new XYZ[] { roomTargetEdgeLine.GetEndPoint(0), roomTargetEdgeLine.GetEndPoint(1) }
            .OrderByDescending(edgeP => (doorLocPoint - edgeP).GetLength())
            .First();
        }

        private Corner specifyWC_Corner(Room room, XYZ farestEdgePointFromDoor)
        {
            BoundingBoxXYZ bbx = room.get_BoundingBox(null);

            return bbx.GetCorners_XY()
                .OrderBy(cornerData => (farestEdgePointFromDoor.ProjectOn_XY() - cornerData.CornerPoint).GetLength())
                .FirstOrDefault().Corner;
        }

        public void CreateWC()
        {
            FamilySymbol wcType = new FilteredElementCollector(ExtCmd.Doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(type => type.Name == wcTypeName && type.FamilyName == wcFamName);

            WC = ExtCmd.Doc.Create.NewFamilyInstance(WCInsertionPoint, wcType, WallData.Wall, Room.Level, StructuralType.NonStructural);

            adjust_WC_FacingOrientation();
            adjust_WC_HandOrientation();
        }

        private void adjust_WC_FacingOrientation()
        {
            if (!WC.FacingOrientation.IsAlmostEqualTo(WallData.TargetFace.FaceNormal))
                WC.flipFacing();
        }

        private void adjust_WC_HandOrientation()
        {
            if (!WallData.HzWall && (WCCorner == Corner.BotLeft || WCCorner == Corner.BotRight)
                || WallData.HzWall && (WCCorner == Corner.BotRight || WCCorner == Corner.TopLeft))
            {
                WC.flipHand();
            }
        }

        private XYZ get_WC_InsPoint(bool hzWall, Corner wcCorner, XYZ farestEdgePointFromDoor)
        {
            XYZ direction;
            //double distance;

            if (hzWall && (wcCorner == Corner.BotLeft || wcCorner == Corner.TopLeft))
            {
                direction = XYZ.BasisX;
                //distance = wcCorner == Corner.BotLeft ? wcDeltaMin : wcWidth - wcDeltaMin;
            }

            else if (hzWall && (wcCorner == Corner.BotRight || wcCorner == Corner.TopRight))
            {
                direction = XYZ.BasisX.Negate();
                //distance = wcCorner == Corner.BotRight ? wcDeltaMin : wcWidth - wcDeltaMin;
            }
            else if (!hzWall && (wcCorner == Corner.BotLeft || wcCorner == Corner.BotRight))
            {
                direction = XYZ.BasisY;
                //distance = wcCorner == Corner.BotLeft ? wcDeltaMin : wcWidth - wcDeltaMin;
            }
            else
            {
                direction = XYZ.BasisY.Negate();
                //distance = wcCorner == Corner.BotRight ? wcDeltaMin : wcWidth - wcDeltaMin;
            }

            return farestEdgePointFromDoor + (direction * wcDeltaMin);
        }
    }
}
