using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using Task3.Revit.Entry;
using Task3.Revit.Model;

using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;


namespace Task3.Revit
{
    internal static class Test
    {
        private static UIDocument uidoc = ExtCmd.UIDoc;
        private static Document doc = ExtCmd.Doc;
        private static TaskDialog td = new TaskDialog("Testing");
        private static StringBuilder sb = new StringBuilder();



        public static void T1_DoorEdgeLines()
        {
            FamilyInstance ele = SelectUtils.PromptSelect_Ele(uidoc) as FamilyInstance;

            if (ele == null)
                return;

            var doorData = new DoorData(ele, null);

            using (Transaction trn = new Transaction(doc, "T1_DoorEdgeLines"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, doorData.EdgeLines_XY, false);

                trn.Commit();
            }

            sb.AppendLine($"{doorData.EdgeLines_XY.Count} Lines\n");

            for (int i = 0; i < doorData.EdgeLines_XY.Count; i++)
            {
                Line line = doorData.EdgeLines_XY[i];
                sb.AppendLine($"{GeoUtils.XYZ_AsString(line.GetEndPoint(0))}\n{GeoUtils.XYZ_AsString(line.GetEndPoint(1))}\n");
            }


            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T2_RoomEdgeLines()
        {
            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;

            if (room == null)
                return;

            var roomData = new RoomData(room, new List<FamilyInstance>(), null);

            using (Transaction trn = new Transaction(doc, "T2_RoomEdgeLines"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, roomData.EdgeLines_XY, false);

                trn.Commit();
            }

            sb.AppendLine($"{roomData.EdgeLines_XY.Count} Lines\n");


            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T3_RoomOfAFloor()
        {
            Element floor = SelectUtils.PromptSelect_Ele(uidoc);

            if (floor == null)
                return;

            Solid floorSolid = GeoUtils.GetElementSolid(floor);
            Room room = doc.GetRoomAtPoint(floorSolid.ComputeCentroid());

            if (room != null)
                sb.AppendLine($"{room.Number} - {room.GetRoomName()}");
            else
                sb.AppendLine("No Rooms here");

            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T4_RoomEdgesOfDoors()
        {
            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;

            List<(Floor floor, Room room)> floorsWithRoom = FilterUtils.GetFloors(doc, ExtCmd.UIDoc.ActiveView)
                .Select(floor => (floor, doc.GetRoomAtPoint(GeoUtils.GetElementSolid(floor).ComputeCentroid())))
                .ToList();

            List<FamilyInstance> doors = FilterUtils.GetDoors(doc, uidoc.ActiveView).ToList();
            List<FamilyInstance> roomDoors = doors.Where(d => d.BelongToRoom(doc, room)).ToList();
            Floor roomFloor = floorsWithRoom.FirstOrDefault(tuple => tuple.room?.Id == room.Id).floor;

            RoomData roomData = new RoomData(room, roomDoors, roomFloor);

            roomData.RetrieveThresholdPoints();

            using (Transaction trn = new Transaction(doc, "T4_RoomEdgesOfDoors"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, roomData.DoorsData.Select(d => d.RoomEdgeLine).ToList(), false);

                trn.Commit();
            }
        }

        public static void T5_DoorOrientations()
        {
            FamilyInstance door = SelectUtils.PromptSelect_Ele(uidoc) as FamilyInstance;

            if (door == null)
                return;

            sb.AppendLine($"Facing Orientation: {GeoUtils.XYZ_AsString(door.FacingOrientation, 3)}");
            sb.AppendLine($"Hand Orientation: {GeoUtils.XYZ_AsString(door.HandOrientation, 3)}");

            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T6_Threshold_FourPoints()
        {
            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;

            List<(Floor floor, Room room)> floorsWithRoom = FilterUtils.GetFloors(doc, ExtCmd.UIDoc.ActiveView)
                .Select(floor => (floor, doc.GetRoomAtPoint(GeoUtils.GetElementSolid(floor).ComputeCentroid())))
                .ToList();

            List<FamilyInstance> doors = FilterUtils.GetDoors(doc, uidoc.ActiveView).ToList();
            List<FamilyInstance> roomDoors = doors.Where(d => d.BelongToRoom(doc, room)).ToList();
            Floor roomFloor = floorsWithRoom.FirstOrDefault(tuple => tuple.room?.Id == room.Id).floor;

            RoomData roomData = new RoomData(room, roomDoors, roomFloor);

            roomData.RetrieveThresholdPoints();

            using (Transaction trn = new Transaction(doc, "T6_Threshold_FourPoints"))
            {
                trn.Start();

                GeoVisUtils.VisualizePoints(doc, roomData.DoorsData.SelectMany(d => d.ThresholdPoints).ToList());

                trn.Commit();
            }
        }

        public static void T7_GenerateFloorBoundary()
        {
            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;

            List<(Floor floor, Room room)> floorsWithRoom = FilterUtils.GetFloors(doc, ExtCmd.UIDoc.ActiveView)
                .Select(floor => (floor, doc.GetRoomAtPoint(GeoUtils.GetElementSolid(floor).ComputeCentroid())))
                .ToList();

            List<FamilyInstance> doors = FilterUtils.GetDoors(doc, uidoc.ActiveView).ToList();
            List<FamilyInstance> roomDoors = doors.Where(d => d.BelongToRoom(doc, room)).ToList();
            Floor roomFloor = floorsWithRoom.FirstOrDefault(tuple => tuple.room?.Id == room.Id).floor;

            RoomData roomData = new RoomData(room, roomDoors, roomFloor);

            roomData.RetrieveThresholdPoints();
            roomData.GenerateFloorBoundary();

            using (Transaction trn = new Transaction(doc, "T7_GenerateFloorBoundary"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, roomData.FloorBoundary.Cast<Line>().ToList());

                trn.Commit();
            }
        }

        public static void T8_CreateFloorAndThreshold()
        {
            RoomData.DefFloorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .OfCategory(BuiltInCategory.OST_Floors)
                .Cast<FloorType>()
                .FirstOrDefault(type => type.Name.Contains("Generic"));

            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;

            List<(Floor floor, Room room)> floorsWithRoom = FilterUtils.GetFloors(doc, ExtCmd.UIDoc.ActiveView)
                .Select(floor => (floor, doc.GetRoomAtPoint(GeoUtils.GetElementSolid(floor).ComputeCentroid())))
                .ToList();

            List<FamilyInstance> doors = FilterUtils.GetDoors(doc, uidoc.ActiveView).ToList();
            List<FamilyInstance> roomDoors = doors.Where(d => d.BelongToRoom(doc, room)).ToList();
            Floor roomFloor = floorsWithRoom.FirstOrDefault(tuple => tuple.room?.Id == room.Id).floor;

            RoomData roomData = new RoomData(room, roomDoors, roomFloor);

            roomData.RetrieveThresholdPoints();
            roomData.GenerateFloorBoundary();


            using (Transaction trn = new Transaction(doc, "T8_CreateFloorAndThreshold"))
            {
                trn.Start();

                roomData.CreateFloorWithThreshold();

                trn.Commit();
            }
        }
    }
}
