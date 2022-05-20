using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ComplexAprtmntNmbr : IExternalCommand
    {
        /// <summary>
        /// Guid параметра КоличествоЖилыхКомнат
        /// </summary>
        private readonly Guid guid_par_liv_rooms_count = Guid.Parse("f52108e1-0813-4ad6-8376-a38a1a23a55b");

        /// <summary>
        /// Guid параметра ТипКвартиры
        /// </summary>
        private readonly Guid guid_par_apartment_type = Guid.Parse("78e3b89c-eb68-4600-84a7-c523de162743");

        /// <summary>
        /// Guid параметра ИндексКвартиры
        /// </summary>
        private readonly Guid guid_par_apartment_index = Guid.Parse("a2985e5c-b28e-416a-acf6-7ab7e4ee6d86");


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }
    }
}
