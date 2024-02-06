using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

using Task3;
using System.Reflection;
using System.IO;
using Autodesk.Revit.DB.Architecture;
using Task3.Revit.Model;

using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace Task3.Revit.Entry
{
    [Transaction(TransactionMode.Manual)]
    public class ExtCmd : IExternalCommand
    {
        public static UIDocument UIDoc { get; private set; }
        public static Document Doc { get; private set; }



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            Doc = UIDoc.Document;


            // Testing:
            //implementTesting();


            CreateFloorWithThreshold();

            return Result.Succeeded;
        }


        private void CreateFloorWithThreshold()
        {
            RoomData.DefFloorType = new FilteredElementCollector(Doc)
                .OfClass(typeof(FloorType))
                .OfCategory(BuiltInCategory.OST_Floors)
                .Cast<FloorType>()
                .FirstOrDefault(type => type.Name.Contains("Generic"));

            List<RoomData> roomsData = new List<RoomData>();
            List<Room> rooms = FilterUtils.GetRooms(Doc, UIDoc.ActiveView);

            List<(Floor floor, Room room)> floorsWithRoom = FilterUtils.GetFloors(Doc, UIDoc.ActiveView)
                .Select(floor => (floor, Doc.GetRoomAtPoint(GeoUtils.GetElementSolid(floor).ComputeCentroid())))
                .ToList();

            List<FamilyInstance> doors = FilterUtils.GetDoors(Doc, UIDoc.ActiveView).ToList();

            foreach (Room room in rooms)
            {
                List<FamilyInstance> roomDoors = doors.Where(d => d.BelongToRoom(Doc, room)).ToList();
                Floor roomFloor = floorsWithRoom.FirstOrDefault(tuple => tuple.room?.Id == room.Id).floor;
                roomsData.Add(new RoomData(room, roomDoors, roomFloor));
            }

            foreach (RoomData roomData in roomsData)
            {
                roomData.RetrieveThresholdPoints();
                roomData.GenerateFloorBoundary();
            }


            using (Transaction trn = new Transaction(Doc, "Task 3"))
            {
                trn.Start();

                foreach (RoomData roomData in roomsData)
                    roomData.CreateFloorWithThreshold();

                trn.Commit();
            }
        }

        /// <summary>
        /// This method is called only for testing.
        /// </summary>
        private void implementTesting()
        {
            //Test.T1_DoorEdgeLines();
            //Test.T2_RoomEdgeLines();
            //Test.T3_RoomOfAFloor();
            //Test.T4_RoomEdgesOfDoors();
            //Test.T5_DoorOrientations();
            //Test.T6_Threshold_FourPoints();
            Test.T7_GenerateFloorBoundary();
            //Test.T8_CreateFloorAndThreshold();
        }
    }
}
