using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.Utils;

namespace RvtLib.Revit.EqualityComparers
{
    public class LineComparer : IEqualityComparer<Line>
    {
        public bool Equals(Line line1, Line line2)
        {
            return line1.GetEndPoint(0).IsAlmostEqualTo(line2.GetEndPoint(0), GeoUtils.Tolerance)
                && line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(1), GeoUtils.Tolerance);
        }

        public int GetHashCode(Line line)
        {
            return line.GetEndPoint(0).X.GetHashCode() + line.GetEndPoint(0).Y.GetHashCode() + line.GetEndPoint(0).Z.GetHashCode()
                + line.GetEndPoint(1).X.GetHashCode() + line.GetEndPoint(1).Y.GetHashCode() + line.GetEndPoint(1).Z.GetHashCode();
        }
    }
}
