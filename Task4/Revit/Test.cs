using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using Task4.Revit.Entry;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.SelectionFilters;
using RvtLib.Revit.Utils;

namespace Task4.Revit
{
    internal static class Test
    {
        private static UIDocument uidoc = ExtCmd.UIDoc;
        private static Document doc = ExtCmd.Doc;
        private static TaskDialog td = new TaskDialog("Testing");
        private static StringBuilder sb = new StringBuilder();



        public static void T1_DivideLine()
        {
            XYZ p1 = SelectUtils.PromptSelect_Point(uidoc);
            XYZ p2 = SelectUtils.PromptSelect_Point(uidoc);
            Line line = Line.CreateBound(p1, p2);


            using (Transaction trn = new Transaction(doc, "T1_DivideLine"))
            {
                trn.Start();

                line.Visualize(doc);

                //GeoVisUtils.VisualizePoints(doc, line.DivideByDist(1.5));
                //GeoVisUtils.VisualizePoints(doc, line.DivideByDist(1.5, false, true, true));
                GeoVisUtils.VisualizePoints(doc, line.DivideByDist(1.5, true));
                //GeoVisUtils.VisualizePoints(doc, line.DivideByDist(1.5, true, true, true));

                trn.Commit();
            }
        }

        public static void T2_GetVlLinesOfFraming_IgnoreWindows()
        {
            double distance = 3;
            bool eleminateRemainder = false;

            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);

            if (wall == null)
                return;

            Solid wallSolid = GeoUtils.GetElementSolid(wall);
            PlanarFace wallTopFace = wall.GetWall_TopPlanarFace(wallSolid);
            Line wallLocLine = (wall.Location as LocationCurve).Curve as Line;
            
            FamilyInstance window = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .FirstOrDefault(w => w.Host.Id == wall.Id);

            Outline winOutline = window?.get_BoundingBox(null).GetOutline();  // null if there's no Window at this Wall

            List<XYZ> startPoints = wallLocLine.DivideByDist(distance, eleminateRemainder);

            using (Transaction trn = new Transaction(doc, "T2_GetVlLinesOfFraming_IgnoreWindows"))
            {
                trn.Start();

                wallLocLine.Visualize(doc, true);

                foreach (XYZ stPoint in startPoints)
                {
                    Line unboundLine = Line.CreateUnbound(stPoint, XYZ.BasisZ);
                    XYZ endPoint = wallTopFace.GetIntersectionPoint(unboundLine);

                    if (endPoint != null)
                        Line.CreateBound(stPoint, endPoint).Visualize(doc, true);
                }

                trn.Commit();
            }
        }

        public static void T3_GetVlLinesOfFraming_ConsierWindows()
        {
            double distance = 3;
            bool eleminateRemainder = false;
            
            List<Line> vlLines = new List<Line>();
            Outline winOutline = null;
            double winCenteroid_Z = 0;

            Wall wall = SelectUtils.PromptSelect_Wall(uidoc);

            if (wall == null)
                return;

            Solid wallSolid = GeoUtils.GetElementSolid(wall);
            PlanarFace wallTopFace = wall.GetWall_TopPlanarFace(wallSolid);
            Line wallLocLine = (wall.Location as LocationCurve).Curve as Line;

            FamilyInstance window = new FilteredElementCollector(doc)
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

            List<XYZ> startPoints = wallLocLine.DivideByDist(distance, eleminateRemainder);

            foreach (XYZ stPoint in startPoints)
            {
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

            using (Transaction trn = new Transaction(doc, "T2_GetVlLinesOfFraming_IgnoreWindows"))
            {
                trn.Start();

                wallLocLine.Visualize(doc, true);
                GeoVisUtils.VisualizeLines(doc, vlLines, true);

                trn.Commit();
            }
        }
    }
}
