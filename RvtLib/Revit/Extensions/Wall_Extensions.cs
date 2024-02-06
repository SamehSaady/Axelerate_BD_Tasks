using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Utils;

namespace RvtLib.Revit.Extensions
{
    public static class Wall_Extensions
    {
        /// <summary>
        /// Retrieves the top PlanarFace (horizontal or inclined) of the passed Solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <remarks>
        /// You must use this method if you are working with an inclined top face of a wall and the wall contains an instance like Window.
        /// <br> Don't use [GeoUtils.GetSolid_TopPlanarFace()] or you will get a false Face. </br>
        /// </remarks>
        /// <returns>Top PlanarFace or null.</returns>
        public static PlanarFace GetWall_TopPlanarFace(this Wall wall, Solid solid)
        {
            return GeoUtils.GetSolid_TopPlanarFaces(solid).OrderByDescending(face =>
                GeoUtils.GetFace_ExtCurveLoop(face).Max(curve => curve.GetEndPoints().Max(p => p.Z)))
                .FirstOrDefault();
        }
    }
}
