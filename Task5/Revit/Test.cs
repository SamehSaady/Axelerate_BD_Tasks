using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RvtLib.Revit.Extensions;
using Task5.Revit.Entry;
using RvtLib.Revit.EqualityComparers;
using RvtLib.Revit.SelectionFilters;
using RvtLib.Revit.Utils;

namespace Task5.Revit
{
    internal static class Test
    {
        private static UIDocument uidoc = ExtCmd.UIDoc;
        private static Document doc = ExtCmd.Doc;
        private static TaskDialog td = new TaskDialog("Testing");
        private static StringBuilder sb = new StringBuilder();



        public static void T1_CreateSectionView()
        {
            ViewFamilyType type = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(t => ViewFamily.Section == t.ViewFamily);

            Level lvl = uidoc.ActiveView.GenLevel;

            Transform transform = Transform.Identity;
            transform.Origin = new XYZ();
            transform.BasisX = XYZ.BasisX;
            transform.BasisY = XYZ.BasisZ;
            transform.BasisZ = -XYZ.BasisY;

            BoundingBoxXYZ bbx = new BoundingBoxXYZ()
            {
                Transform = transform,
                Min = new XYZ(-5, lvl.Elevation - 10, -2),
                Max = new XYZ(5, lvl.Elevation + 10, 2)
            };


            using (Transaction trn = new Transaction(doc, "T1_CreateSectionView"))
            {
                trn.Start();

                ViewSection.CreateSection(doc, type.Id, bbx);


                trn.Commit();
            }
        }

        public static void T2_CreateSectionViewWithTwoPoints()
        {
            XYZ p1 = SelectUtils.PromptSelect_Point(uidoc);
            XYZ p2 = SelectUtils.PromptSelect_Point(uidoc);
            XYZ p12 = p2 - p1;

            ViewFamilyType type = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(t => ViewFamily.Section == t.ViewFamily);

            Level lvl = uidoc.ActiveView.GenLevel;

            Transform transform = Transform.Identity;
            transform.Origin = (p1 + p2) / 2;
            transform.BasisX = p12.Normalize();
            transform.BasisY = XYZ.BasisZ;
            transform.BasisZ = p12.Normalize().GetPerpendicularDirection_CW_XY();

            BoundingBoxXYZ bbx = new BoundingBoxXYZ()
            {
                Transform = transform,
                Min = new XYZ(p1.X, lvl.Elevation - 10, 0),
                Max = new XYZ(p2.X, lvl.Elevation + 10, 10)
            };


            using (Transaction trn = new Transaction(doc, "T2_CreateSectionViewWithTwoPoints"))
            {
                trn.Start();

                ViewSection.CreateSection(doc, type.Id, bbx);

                trn.Commit();
            }
        }

    }
}
