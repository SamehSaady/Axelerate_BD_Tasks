using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvtLib.Revit.SelectionFilters;

namespace RvtLib.Revit.Utils
{
    public class SelectUtils
    {
        /// <summary>
        /// Prompts the user to select an element in the active view.
        /// </summary>
        /// <param name="uidoc"></param>
        /// <returns>The selected element or null if the user cancelled selection.</returns>
        public static Element PromptSelect_Ele(UIDocument uidoc)
        {
            try
            {
                return uidoc.Document.GetElement(
                    uidoc.Selection.PickObject(ObjectType.Element, "Please select an element."));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Prompts the user to select a wall in the active view.
        /// </summary>
        /// <param name="uidoc"></param>
        /// <returns>The selected wall or null if the user cancelled selection.</returns>
        public static Wall PromptSelect_Wall(UIDocument uidoc)
        {
            try
            {
                return uidoc.Document.GetElement(
                    uidoc.Selection.PickObject(ObjectType.Element, new EleSelFilter(ele => ele is Wall), "Please select a Wall."))
                    as Wall;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Prompts the user to select a point in the active view.
        /// </summary>
        /// <param name="uidoc"></param>
        /// <returns>The selected point or null if the user cancelled selection.</returns>
        public static XYZ PromptSelect_Point(UIDocument uidoc)
        {
            try
            {
                return uidoc.Selection.PickPoint("Please select a point.");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
