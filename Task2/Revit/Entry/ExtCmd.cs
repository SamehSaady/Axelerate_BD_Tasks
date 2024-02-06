using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

using RvtLib.Revit.Utils;
using Task2;
using System.Reflection;
using System.IO;
using Autodesk.Revit.DB.Architecture;
using RvtLib.Revit.Extensions;
using Task2.Revit.Model;

namespace Task2.Revit.Entry
{
    [Transaction(TransactionMode.Manual)]
    public class ExtCmd : IExternalCommand
    {
        public static UIDocument UIDoc { get; private set; }
        public static Document Doc { get; private set; }
        public static List<Line> Lines { get; private set; }



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            Doc = UIDoc.Document;
            TaskDialog td = new TaskDialog("Task 1");


            // Testing:
            //implementTesting();


            Wall wall = SelectUtils.PromptSelect_Wall(UIDoc);
            WallData wallData = new WallData(wall);

            List<Room> bathrooms = FilterUtils.GetRooms(Doc, UIDoc.ActiveView)
               .Where(r => r.GetRoomName().Equals(RoomData.TargetRoomName, StringComparison.OrdinalIgnoreCase))
               .ToList();

            Room bathroom = wallData.GetNearestRoom(bathrooms);
            RoomData roomData = new RoomData(bathroom, wallData);

            if (roomData.TargetEdgeLine == null)
            {
                td.MainContent = $"This Wall doesn't belong to a Bathroom.";
                td.Show();

                return Result.Succeeded;
            }

            if (roomData.Door == null)
            {
                td.MainContent = $"This Bathroom doesn't have a door.";
                td.Show();

                return Result.Succeeded;
            }

            using (Transaction trn = new Transaction(Doc, "Task 2"))
            {
                trn.Start();

                roomData.CreateWC();

                trn.Commit();
            }



            return Result.Succeeded;
        }


        /// <summary>
        /// This method is called only for testing.
        /// </summary>
        private void implementTesting()
        {
            //Test.T1_WallLongitEdgeLines_XY();
            //Test.T2_RoomEdgeLines();
            //Test.T3_TwoContainedLinesIntersecResult();
            //Test.T4_WallLongitPlanarFaces();
            //Test.T5_FaceAndLineIntersecResult();
            //Test.T6_TargetWallFaceAndRoomEdge();
            //Test.T7_GetRoomsAndDoors();
            //Test.T8_FarestEdgePointFromDoor();
            //Test.T9_RoomLocPoint();
            Test.T10_CreateWCAndShowCorner();
        }
    }
}
