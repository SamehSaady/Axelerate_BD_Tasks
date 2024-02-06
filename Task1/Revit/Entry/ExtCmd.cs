using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace Task1.Revit.Entry
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
            TaskDialog td = new TaskDialog("Task 1");

            Lines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(79, 0, 0)),
                Line.CreateBound(new XYZ(44, 25, 0), new XYZ(13, 25, 0)),
                Line.CreateBound(new XYZ(13, 40, 0), new XYZ(-8, 40, 0)),
                Line.CreateBound(new XYZ(55, 34, 0), new XYZ(55, 10, 0)),
                Line.CreateBound(new XYZ(79, 34, 0), new XYZ(55, 34, 0)),
                Line.CreateBound(new XYZ(0, 20, 0), new XYZ()),
                Line.CreateBound(new XYZ(55, 10, 0), new XYZ(44, 12, 0)),
                Line.CreateBound(new XYZ(-8, 40, 0), new XYZ(-8, 20, 0)),
                Line.CreateBound(new XYZ(79, 0, 0), new XYZ(79, 34, 0)),
                Line.CreateBound(new XYZ(44, 12, 0), new XYZ(44, 25, 0)),
                Line.CreateBound(new XYZ(-8, 20, 0), new XYZ(0, 20, 0)),
                Line.CreateBound(new XYZ(13, 25, 0), new XYZ(13, 40, 0))
            };


            // Testing:
            //implementTesting();


            if (!GeoUtils.IsClosedLoop(Lines))
            {
                td.MainContent = "You can't create a Floor with these lines as they don't form a closed loop.";
                td.Show();

                return Result.Succeeded;
            }

            bool areContinuousLines = GeoUtils.AreContinuousLines(Lines, true);
            Lines = areContinuousLines ? Lines : GeoUtils.MakeContinuousLines(Lines, out _);


            // At this point, all the Lines are continuous and form a closed loop and hence, they overlap at end points.
            // But there's still a chance that a line may overlap at another line at a non-end point.
            if (GeoUtils.IsAnyLineIntersectedAtNonEndPoint(Lines))
            {
                td.MainContent = "You can't create a Floor with these lines as one of them intersects with another at a non-end point.";
                td.Show();

                return Result.Succeeded;
            }

            CurveLoop curveLoop = CurveLoop.Create(Lines.Cast<Curve>().ToList());

            FloorType floorType = new FilteredElementCollector(Doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .First(type => type.FamilyName == "Floor");

            Level level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .FirstOrDefault() as Level;


            using (Transaction trn = new Transaction(Doc, "Create Floor"))
            {
                trn.Start();

                // In case the project has no Levels:
                if (level == null)
                {
                    level = Level.Create(Doc, 0);
                    level.Name = "Level 1";
                }

                Floor.Create(Doc, new List<CurveLoop>() { curveLoop }, floorType.Id, level.Id);

                trn.Commit();
            }

            return Result.Succeeded;
        }


        /// <summary>
        /// This method is called only for testing.
        /// </summary>
        private void implementTesting()
        {
            //Test.T1_VisualizeLines();
            //Test.T2_IsClosedLoop();
            //Test.T3_AreContinuousLines();
            //Test.T4_MakeContinuousLines();
            //Test.T5_LinesIntersectionResult();
            Test.T6_IsAnyLineIntersectedAtNonEndPoint();
        }
    }
}
