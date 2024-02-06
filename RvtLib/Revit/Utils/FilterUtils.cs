using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.Revit.Utils
{
    public static class FilterUtils
    {
        /// <summary>
        /// Retrieves all the Rooms in <paramref name="view"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="view">Null to get all Rooms in <paramref name="doc"/>.</param>
        /// <returns></returns>
        public static List<Room> GetRooms(Document doc, View view = null)
        {
            var collector = view == null ? new FilteredElementCollector(doc) : new FilteredElementCollector(doc, view.Id);
            
            return collector.WherePasses(new RoomFilter())
                .Cast<Room>()
                .ToList();
        }

        /// <summary>
        /// Retrieves all the Doors in <paramref name="view"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="view">Null to get all Rooms in <paramref name="doc"/>.</param>
        /// <returns></returns>
        public static List<FamilyInstance> GetDoors(Document doc, View view = null)
        {
            var collector = view == null ? new FilteredElementCollector(doc) : new FilteredElementCollector(doc, view.Id);

            return collector.OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();
        }

        /// <summary>
        /// Retrieves all Floors in <paramref name="view"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="view">Null to get all Rooms in <paramref name="doc"/>.</param>
        /// <returns></returns>
        public static List<Floor> GetFloors(Document doc, View view = null)
        {
            var collector = view == null ? new FilteredElementCollector(doc) : new FilteredElementCollector(doc, view.Id);

            return collector.OfClass(typeof(Floor))
                .OfCategory(BuiltInCategory.OST_Floors)
                .Cast<Floor>()
                .ToList();
        }
    }
}
