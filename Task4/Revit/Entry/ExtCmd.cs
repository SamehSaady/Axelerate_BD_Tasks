using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace Task4.Revit.Entry
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


            // Testing:
            //implementTesting();


            Wall wall = SelectUtils.PromptSelect_Wall(UIDoc);

            List<Line> vlFramingLines = GetVerticalLinesOfFraming(wall, 3, true);

            using (Transaction trn = new Transaction(Doc, "Task 4"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(Doc, vlFramingLines, true);

                trn.Commit();
            }


            return Result.Succeeded;
        }


        /// <summary>
        /// Retrieves the vertical lines of a Framing separated by <paramref name="distance"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="distance">The distance between Framings.</param>
        /// <param name="eleminateRemainder">
        /// In case there's a remained length (i.e., <paramref name="wall"/> Length % <paramref name="distance"/> != 0),
        /// <br>Set it to true to recalculate the <paramref name="distance"/> to eleminate the ramainder, or false to use the passed <paramref name="distance"/>.</br>
        /// </param>
        /// <returns></returns>
        public List<Line> GetVerticalLinesOfFraming(Wall wall, double distance, bool eleminateRemainder)
        {
            List<Line> vlLines = new List<Line>();
            Outline winOutline = null;
            double winCenteroid_Z = 0;

            Solid wallSolid = GeoUtils.GetElementSolid(wall);
            PlanarFace wallTopFace = wall.GetWall_TopPlanarFace(wallSolid);
            Line wallLocLine = (wall.Location as LocationCurve).Curve as Line;

            List<XYZ> startPoints = wallLocLine.DivideByDist(distance, eleminateRemainder);

            FamilyInstance window = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .FirstOrDefault(w => w.Host.Id == wall.Id);

            if (window != null)
            {
                BoundingBoxXYZ winBBX = window.get_BoundingBox(null);
                winOutline = winBBX.GetOutline();
                winCenteroid_Z = winBBX.GetCentroid().Z;
            }

            foreach (XYZ stPoint in startPoints)
            {
                // This approach of [Projection] is ignored as in the (inclined top face wall) case,
                // the lines will be prependicular to the top face (not vertical).
                //XYZ endPoint = wallTopFace.Project(stPoint).XYZPoint;  // Ignored Approach.

                Line unboundLine = Line.CreateUnbound(stPoint, XYZ.BasisZ);
                XYZ endPoint = wallTopFace.GetIntersectionPoint(unboundLine);

                if (window != null && winOutline.Contains(new XYZ(stPoint.X, stPoint.Y, winCenteroid_Z), GeoUtils.Tolerance))
                {
                    XYZ botWinPoint = new XYZ(stPoint.X, stPoint.Y, winOutline.MinimumPoint.Z);
                    XYZ topWinPoint = new XYZ(stPoint.X, stPoint.Y, winOutline.MaximumPoint.Z);

                    vlLines.Add(Line.CreateBound(stPoint, botWinPoint));
                    vlLines.Add(Line.CreateBound(topWinPoint, endPoint));
                }
                else
                    vlLines.Add(Line.CreateBound(stPoint, endPoint));
            }

            return vlLines;
        }

        /// <summary>
        /// This method is called only for testing.
        /// </summary>
        private void implementTesting()
        {
            //Test.T1_DivideLine();
            Test.T2_GetVlLinesOfFraming_IgnoreWindows();
        }
    }
}
