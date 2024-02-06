using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace RvtLib.Revit.EqualityComparers
{
    public class XYZComparer : IEqualityComparer<XYZ>
    {
        public bool Equals(XYZ p1, XYZ p2)
        {
            //return p1.IsAlmostEqualTo(p2, GeoUtils.Tolerance);
            //return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;

            p1 = p1.Round(GeoUtils.DecimalPlaces);
            p2 = p2.Round(GeoUtils.DecimalPlaces);

            return (p1 - p2).IsZeroLength();
        }

        public int GetHashCode(XYZ point)
        {
            return point.X.GetHashCode() + point.Y.GetHashCode() + point.Z.GetHashCode();
        }
    }
}
