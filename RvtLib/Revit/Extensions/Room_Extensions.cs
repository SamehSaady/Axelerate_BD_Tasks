using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.Revit.Extensions
{
    public static class Room_Extensions
    {
        /// <summary>
        /// Retrieves the name of this <paramref name="room"/>.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static string GetRoomName(this Room room)
        {
            return room.get_Parameter(BuiltInParameter.ROOM_NAME)
                .AsString();
        }

        /// <summary>
        /// Filters the passed <paramref name="famIns"/> and returns only the ones that belong to this <paramref name="room"/>.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="doc"></param>
        /// <param name="famIns"></param>
        /// <returns></returns>
        public static List<FamilyInstance> FilterFamIns(this Room room, Document doc, List<FamilyInstance> famIns)
        {
            return famIns.Where(ins => (ins.BelongToRoom(doc, room))).ToList();
        }
    }
}
