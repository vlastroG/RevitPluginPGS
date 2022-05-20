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

        /// <summary>
        /// Guid параметра СоставнойНомерКвартиры
        /// </summary>
        private readonly BuiltInParameter guid_par_cmplx_aprtm_num = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
        //private readonly Guid guid_par_cmplx_aprtm_num = Guid.Parse("");


        /// <summary>
        /// Заполнение составного параметра номера квартиры
        /// Сейчас составной параметр записавается в Комментарии.
        /// Чтобы записывалось в общий параметр надо поменять поле guid_par_cmplx_aprtm_num
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var rooms_filter = new FilteredElementCollector(doc);
            var rooms = rooms_filter
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Составные номера квартир");

                foreach (var room in rooms)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(room.get_Parameter(guid_par_liv_rooms_count).AsValueString());
                    sb.Append(room.get_Parameter(guid_par_apartment_type).AsValueString());
                    sb.Append(room.get_Parameter(guid_par_apartment_index).AsValueString());

                    var cmplx_aprtm_num = sb.ToString();
                    if (cmplx_aprtm_num == "0")
                    {
                        cmplx_aprtm_num = String.Empty;
                    }
                    room.get_Parameter(guid_par_cmplx_aprtm_num).Set(cmplx_aprtm_num);
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
