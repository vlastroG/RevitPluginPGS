using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using System;
using System.Text;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ComplexAprtmntNmbr : IExternalCommand
    {
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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.ADSK_CountOfRooms,
            SharedParams.ADSK_TypeOfApartment,
            SharedParams.ADSK_IndexOfApartment
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\" " +
                    "присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nADSK_Количество комнат" +
                    "\nADSK_Тип квартиры" +
                    "\nADSK_Индекс квартиры",
                    "Ошибка");
                return Result.Cancelled;
            }

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
                    sb.Append(room.get_Parameter(SharedParams.ADSK_CountOfRooms).AsValueString());
                    sb.Append(room.get_Parameter(SharedParams.ADSK_TypeOfApartment).AsValueString());
                    sb.Append(room.get_Parameter(SharedParams.ADSK_IndexOfApartment).AsValueString());

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
