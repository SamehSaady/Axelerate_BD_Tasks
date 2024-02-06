using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLib.Revit.SelectionFilters
{
    public class EleSelFilter : ISelectionFilter
    {
        private readonly Predicate<Element> ValidateEle;
        private readonly Predicate<Reference> _validateRef;



        public EleSelFilter(Predicate<Element> validateEle)
        {
            ValidateEle = validateEle;
        }

        public EleSelFilter(Predicate<Element> validateEle, Predicate<Reference> validateRef) : this(validateEle)
        {
            _validateRef = validateRef;
        }



        public bool AllowElement(Element ele)
        {
            return ValidateEle(ele);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            // If the [validateRef] is null, return true (i.e., allow all references):
            return _validateRef?.Invoke(reference) ?? true;
        }
    }
}
