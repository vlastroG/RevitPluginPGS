using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR
{
    public class LintelsSections : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Категория == Обобщенная модель
            //Описание == Перемычка
            //PGS_МаркаПеремычки-- имеет значение

            //Element.Location - точка размещения(X Y координаты)
            //FamilyInstance.HandOrientation - вектор вдоль длины перемычки
            //FamilyInstance.GetSubComponentIds().GetFirst().Location.Z - отметка центра разреза по высоте.

            throw new NotImplementedException();
        }
    }
}
