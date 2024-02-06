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

namespace Task5.Revit.Entry
{
    [Transaction(TransactionMode.Manual)]
    public class ExtCmd : IExternalCommand
    {
        public static UIDocument UIDoc { get; private set; }
        public static Document Doc { get; private set; }
        /// <summary>
        /// Default Floor Type for Floors and Threshold to be modeled.
        /// </summary>
        public static FloorType DefFloorType { get; set; }



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            Doc = UIDoc.Document;


            // Testing:
            implementTesting();




            return Result.Succeeded;
        }


        /// <summary>
        /// This method is called only for testing.
        /// </summary>
        private void implementTesting()
        {
            //Test.T1_CreateSectionView();
            Test.T2_CreateSectionViewWithTwoPoints();
        }
    }
}
