using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Task1.Revit.Entry;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.Extensions;
using RvtLib.Revit.Utils;

namespace Task1.Revit
{
    internal static class Test
    {
        private static UIDocument uidoc = ExtCmd.UIDoc;
        private static Document doc = ExtCmd.Doc;
        private static TaskDialog td = new TaskDialog("Testing");
        private static StringBuilder sb = new StringBuilder();



        public static void T1_VisualizeLines()
        {
            using (Transaction trn = new Transaction(doc, "T1_VisualizeLines"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, ExtCmd.Lines, true);

                trn.Commit();
            }
        }
        public static void T2_IsClosedLoop()
        {
            List<Line> closedLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
                Line.CreateBound(new XYZ(5, 5, 0), new XYZ())
            };

            List<Line> openLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
                Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0))
            };


            sb.AppendLine($"closedLines: {GeoUtils.IsClosedLoop(closedLines)}");
            sb.AppendLine($"openLines: {GeoUtils.IsClosedLoop(openLines)}");
            sb.AppendLine($"Source Lines: {GeoUtils.IsClosedLoop(ExtCmd.Lines)}");
            
            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T3_AreContinuousLines()
        {
            List<Line> contClosedLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
                Line.CreateBound(new XYZ(5, 5, 0), new XYZ())
            };

            List<Line> contOpenLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
                Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0))
            };

            List<Line> nonContClosedLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 5, 0), new XYZ()),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0))
            };

            List<Line> nonContOpenLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
                Line.CreateBound(new XYZ(10, 10, 0), new XYZ(20, 20, 0))
            };


            sb.AppendLine($"closedLines: {GeoUtils.AreContinuousLines(contClosedLines, true)}");
            sb.AppendLine($"openLines: {GeoUtils.AreContinuousLines(contOpenLines, false)}");
            sb.AppendLine($"overlapped: {GeoUtils.AreContinuousLines(nonContClosedLines, true)}");
            sb.AppendLine($"nonContOpenLines: {GeoUtils.AreContinuousLines(nonContOpenLines, false)}\n");
            sb.AppendLine($"Source Lines: {GeoUtils.AreContinuousLines(ExtCmd.Lines, true)}");

            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T4_MakeContinuousLines()
        {
            List<Line> nonContClosedLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),  // 1
                Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0)),  // 3
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),  // 2
                Line.CreateBound(new XYZ(0, 5, 0), new XYZ())  // 4
            };

            List<Line> randomLines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
                Line.CreateBound(new XYZ(25, 7, 0), new XYZ(23, 9, 0))
            };

            //List<Line> contLines = GeoUtils.MakeContinuousLines(overlapped, out bool succeeded);
            //List<Line> contLines = GeoUtils.MakeContinuousLines(randomLines, out bool succeeded);
            List<Line> contLines = GeoUtils.MakeContinuousLines(ExtCmd.Lines, out bool succeeded);

            if (succeeded)
            {
                for (int i = 0; i < contLines.Count; i++)
                    sb.AppendLine($"Line {i + 1}: {GeoUtils.XYZ_AsString(contLines[i].GetEndPoint(0), 0)} || {GeoUtils.XYZ_AsString(contLines[i].GetEndPoint(1), 0)}");
            }
            else
                sb.AppendLine("Failed");

            td.MainContent = sb.ToString();
            td.Show();
        }

        public static void T5_LinesIntersectionResult()
        {
            // overlapped at an end point:
            //List<Line> lines = new List<Line>()
            //{
            //    Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
            //    Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0))
            //};

            // Overlapped at a non-end point:
            List<Line> lines = new List<Line>()
            {
                Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
                Line.CreateBound(new XYZ(3, -3, 0), new XYZ(3, 10, 0))
            };


            using (Transaction trn = new Transaction(doc, "T5_LinesIntersectionResult"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, lines);

                trn.Commit();
            }

            bool intersectedAtEndPoint = lines[0].IsIntersectedAtEndPoint(lines[1]);

            sb.AppendLine(lines[0].Intersect(lines[1]).ToString());
            sb.AppendLine($"Intersected at {(intersectedAtEndPoint ? "an " : "a non-")}end point");

            td.MainContent = sb.ToString();
            td.Show();

            // Note: at both cases, the result is [SetComparisonResult.Overlap]
        }

        public static void T6_IsAnyLineIntersectedAtNonEndPoint()
        {
            // overlapped at end points (open loop):
            //List<Line> lines = new List<Line>()
            //{
            //    Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
            //    Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
            //    Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0))
            //};

            // overlapped at end points (closed loop):
            //List<Line> lines = new List<Line>()
            //{
            //    Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
            //    Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
            //    Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0)),
            //    Line.CreateBound(new XYZ(0, 5, 0), new XYZ())
            //};

            // overlapped at non-end points (open loop):
            //List<Line> lines = new List<Line>()
            //{
            //    Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
            //    Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
            //    Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0)),
            //    Line.CreateBound(new XYZ(0, 5, 0), new XYZ(3, -2, 0))
            //};

            // overlapped at non-end points (closed loop):
            //List<Line> lines = new List<Line>()
            //{
            //    Line.CreateBound(new XYZ(), new XYZ(5, 0, 0)),
            //    Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)),
            //    Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0)),
            //    Line.CreateBound(new XYZ(0, 5, 0), new XYZ(3, -2, 0)),
            //    Line.CreateBound(new XYZ(3, -2, 0), new XYZ())
            //};

            List<Line> lines = ExtCmd.Lines;


            using (Transaction trn = new Transaction(doc, "T6_IsAnyLineIntersectedAtNonEndPoint"))
            {
                trn.Start();

                GeoVisUtils.VisualizeLines(doc, lines);

                trn.Commit();
            }

            sb.AppendLine($"Intersected at {(GeoUtils.IsAnyLineIntersectedAtNonEndPoint(lines) ? "non-" : "")}end points");

            td.MainContent = sb.ToString();
            td.Show();
        }
    }
}
