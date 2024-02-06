using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.Revit.Extensions
{
    public static class FamilyInstance_Extensions
    {
        /// <summary>
        /// Checks whehter this <paramref name="famIns"/> belongs to this <paramref name="room"/> or not.
        /// </summary>
        /// <param name="famIns"></param>
        /// <param name="doc"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public static bool BelongToRoom(this FamilyInstance famIns, Document doc, Room room)
        {
            return (famIns.Room != null && famIns.Room.Id == room.Id)
            || (famIns.FromRoom != null && famIns.FromRoom.Id == room.Id)
            || (famIns.ToRoom != null && famIns.ToRoom.Id == room.Id)
            || ((famIns.Location is LocationPoint locPoint) && doc.GetRoomAtPoint(locPoint.Point)?.Id == room.Id);
        }
    }
}
