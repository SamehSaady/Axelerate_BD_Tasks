using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using Task2.Revit.Entry;
using Task2.Revit.Model;

using RvtLib.Revit.Extensions;
using RvtLib.Revit.SelectionFilters;
using RvtLib.Revit.Utils;

namespace Task2.Revit
{
    internal static class Test
    {
        private static UIDocument uidoc = ExtCmd.UIDoc;
        private static Document doc = ExtCmd.Doc;
        private static TaskDialog td = new TaskDialog("Testing");
        private static StringBuilder sb = new StringBuilder();



        public static void T1_WallLongitEdgeLines_XY()
        {
            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);

            if (wall == null)
                return;

            WallData wallData = new WallData(wall);

            using (Transaction trn = new Transaction(doc, "T1_WallLongitEdgeLines_XY"))
            {
                trn.Start();

                wallData.LocLine.Visualize(doc);
                //GeoVisUtils.VisualizeLines(doc, wallData.LongitEdgeLines_XY);  // Ingnored Approach.

                trn.Commit();
            }
        }

        public static void T2_RoomEdgeLines()
        {
            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;

            if (room == null)
                return;

            RoomData roomData = new RoomData(room, null);

            using (Transaction trn = new Transaction(doc, "T1_WallLongitEdgeLines_XY"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, roomData.EdgeLines);
                //GeoVisUtils.VisualizeLines(doc, roomData.EdgeLines_XY);

                trn.Commit();
            }
        }

        public static void T3_TwoContainedLinesIntersecResult()
        {
            var p1 = SelectUtils.PromptSelect_Point(uidoc);
            var p2 = SelectUtils.PromptSelect_Point(uidoc);
            var p3 = SelectUtils.PromptSelect_Point(uidoc);
            var p4 = SelectUtils.PromptSelect_Point(uidoc);

            Line line1 = Line.CreateBound(p1, p2);
            Line line2 = Line.CreateBound(p3, p4);


            using (Transaction trn = new Transaction(doc, "T3_TwoContainedLinesIntersecResult"))
            {
                trn.Start();

                line1.Visualize(doc);
                line2.Visualize(doc);

                trn.Commit();
            }

            sb.AppendLine(line1.Contains(line2).ToString());
            sb.AppendLine(line2.Contains(line1).ToString());

            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T4_WallLongitPlanarFaces()
        {
            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);

            if (wall == null)
                return;

            WallData wallData = new WallData(wall);

            foreach (var face in wallData.LongitSideFaces)
                sb.AppendLine(GeoUtils.XYZ_AsString(face.FaceNormal, 2));


            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T5_FaceAndLineIntersecResult()
        {
            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);
            WallData wallData = new WallData(wall);

            var p1 = SelectUtils.PromptSelect_Point(uidoc);
            var p2 = SelectUtils.PromptSelect_Point(uidoc);
            Line line = Line.CreateBound(p1, p2);

            using (Transaction trn = new Transaction(doc, "T5_FaceAndLineIntersecResult"))
            {
                trn.Start();

                line.Visualize(doc);

                trn.Commit();
            }

            foreach (var face in wallData.LongitSideFaces)
                sb.AppendLine($"Face N: {GeoUtils.XYZ_AsString(face.FaceNormal, 2)}  ||  {face.Intersect(line)}");


            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T6_TargetWallFaceAndRoomEdge()
        {
            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);
            WallData wallData = new WallData(wall);

            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;
            RoomData roomData = new RoomData(room, wallData);

            using (Transaction trn = new Transaction(doc, "T6_TargetWallFaceAndRoomEdge"))
            {
                trn.Start();

                roomData.TargetEdgeLine?.Visualize(doc);

                trn.Commit();
            }

            sb.AppendLine($"{(wallData.TargetFace != null ? GeoUtils.XYZ_AsString(wallData.TargetFace.FaceNormal, 2) : "This Wall doesn't belong to this Room.")}");

            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T7_GetRoomsAndDoors()
        {
            List<Room> rooms = FilterUtils.GetRooms(doc, uidoc.ActiveView);
            List<FamilyInstance> doors = FilterUtils.GetDoors(doc, uidoc.ActiveView);

            sb.AppendLine($"Rooms: {rooms.Count}");
            sb.AppendLine($"Doors: {doors.Count}");


            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T8_FarestEdgePointFromDoor()
        {
            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);
            WallData wallData = new WallData(wall);

            List<Room> bathrooms = FilterUtils.GetRooms(doc, uidoc.ActiveView)
               .Where(r => r.GetRoomName().Equals(RoomData.TargetRoomName, StringComparison.OrdinalIgnoreCase))
               .ToList();

            Room bathroom = wallData.GetNearestRoom(bathrooms);
            RoomData roomData = new RoomData(bathroom, wallData);

            if (roomData.TargetEdgeLine == null)
            {
                td.MainContent = $"This Wall doesn't belong to a Bathroom.";
                td.Show();

                return;
            }

            if (roomData.Door == null)
            {
                td.MainContent = $"This Bathroom doesn't have a door.";
                td.Show();

                return;
            }

            using (Transaction trn = new Transaction(doc, "T8_FarestEdgePointFromDoor"))
            {
                trn.Start();

                //roomData.TargetEdgeLine?.Visualize(doc);
                Line.CreateBound(roomData.DoorLocPoint, roomData.FarestEdgePointFromDoor).Visualize(doc, true);

                trn.Commit();
            }


            if (string.IsNullOrEmpty(sb.ToString()))
                return;

        }

        public static void T9_RoomLocPoint()
        {
            Room room = SelectUtils.PromptSelect_Ele(uidoc) as Room;


            using (Transaction trn = new Transaction(doc, "T9_RoomLocPoint"))
            {
                trn.Start();

                (room.Location as LocationPoint).Point.Visualize(doc);

                trn.Commit();
            }
        }

        public static void T10_CreateWCAndShowCorner()
        {
            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);
            WallData wallData = new WallData(wall);

            List<Room> bathrooms = FilterUtils.GetRooms(doc, uidoc.ActiveView)
               .Where(r => r.GetRoomName().Equals(RoomData.TargetRoomName, StringComparison.OrdinalIgnoreCase))
               .ToList();

            Room bathroom = wallData.GetNearestRoom(bathrooms);
            RoomData roomData = new RoomData(bathroom, wallData);

            if (roomData.TargetEdgeLine == null)
            {
                td.MainContent = $"This Wall doesn't belong to a Bathroom.";
                td.Show();

                return;
            }

            if (roomData.Door == null)
            {
                td.MainContent = $"This Bathroom doesn't have a door.";
                td.Show();

                return;
            }

            using (Transaction trn = new Transaction(doc, "T8_FarestEdgePointFromDoor"))
            {
                trn.Start();

                //roomData.TargetEdgeLine?.Visualize(doc);
                Line.CreateBound(roomData.DoorLocPoint, roomData.FarestEdgePointFromDoor).Visualize(doc, true);
                roomData.CreateWC();

                trn.Commit();
            }


            td.MainContent = roomData.WCCorner.ToString();
            td.Show();
        }
    }
}
