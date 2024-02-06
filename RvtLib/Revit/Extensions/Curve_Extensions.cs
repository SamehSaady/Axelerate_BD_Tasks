using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.Revit.Extensions
{
    public static class Curve_Extensions
    {

        /// <summary>
        /// Retrieves the Start Point and End Point of this <paramref name="curve"/> in an Array.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static XYZ[] GetEndPoints(this Curve curve)
        {
            return new XYZ[] { curve.GetEndPoint(0), curve.GetEndPoint(1) };
        }
    }
}
