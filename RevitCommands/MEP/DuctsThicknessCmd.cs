using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MS.RevitCommands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DuctsThicknessCmd : IExternalCommand
    {
        private static Document _doc;

        private static UIDocument _uidoc;

        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_DuctCurves
        };


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            if (!ValidateSharedParams()) return Result.Cancelled;

            var ducts = GetDucts();
            int count = 0;

            List<int> errors = new List<int>();
            using (Transaction setThickness = new Transaction(_doc))
            {
                setThickness.Start("Толщины воздуховодов");

                foreach (var duct in ducts)
                {
                    (bool isCircle, double maxDimension) = (false, 0);
                    try
                    {
                        (isCircle, maxDimension) = GetOpeningDimensions(duct);
                    }
                    catch (ArgumentException)
                    {
                        errors.Add(duct.Id.IntegerValue);
                    }
                    if (IsSmoke(duct))
                    {
                        if (isCircle)
                        {
                            if (maxDimension <= 800)
                            {
                                SetSideThickness(duct, 0.8, ref count);
                            }
                            else if (maxDimension <= 1250)
                            {
                                SetSideThickness(duct, 1.0, ref count);
                            }
                            else
                            {
                                SetSideThickness(duct, 1.4, ref count);
                            }
                        }
                        else
                        {
                            if (maxDimension <= 1000)
                            {
                                SetSideThickness(duct, 0.8, ref count);
                            }
                            else
                            {
                                SetSideThickness(duct, 0.9, ref count);
                            }
                        }
                    }
                    else
                    {
                        if (isCircle)
                        {
                            if (maxDimension <= 200)
                            {
                                SetSideThickness(duct, 0.5, ref count);
                            }
                            else if (maxDimension <= 450)
                            {
                                SetSideThickness(duct, 0.6, ref count);
                            }
                            else if (maxDimension <= 800)
                            {
                                SetSideThickness(duct, 0.7, ref count);
                            }
                            else if (maxDimension <= 1250)
                            {
                                SetSideThickness(duct, 1.0, ref count);
                            }
                            else
                            {
                                SetSideThickness(duct, 1.4, ref count);
                            }
                        }
                        else
                        {
                            if (maxDimension <= 250)
                            {
                                SetSideThickness(duct, 0.5, ref count);
                            }
                            else if (maxDimension <= 1000)
                            {
                                SetSideThickness(duct, 0.7, ref count);
                            }
                            else
                            {
                                SetSideThickness(duct, 0.9, ref count);
                            }
                        }
                    }
                }
                setThickness.Commit();
            }
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"Значение параметра 'ADSK_Толщина стенки' обновлено у {count} воздуховодов." +
                    $"\nВоздуховоды не обработаны: {string.Join(", ", errors)}.",
                    "Выполнено с ошибками");
            }
            else
            {
                MessageBox.Show(
                    $"Значение параметра 'ADSK_Толщина стенки' обновлено у {count} воздуховодов.",
                    "Толщины воздуховодов");
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Назначить толщину стенки воздуховода
        /// </summary>
        /// <param name="duct">Воздуховод</param>
        /// <param name="value">Значение толщины в мм</param>
        private void SetSideThickness(in Duct duct, double value, ref int count)
        {
            double sideThickness =
                Math.Round(duct
                .get_Parameter(SharedParams.ADSK_SideThickness)
                .AsDouble() * SharedValues.FootToMillimeters, 1);

            if (value != sideThickness)
            {
                duct.get_Parameter(SharedParams.ADSK_SideThickness)
                    .Set(value / SharedValues.FootToMillimeters);
                count++;
            }
        }

        private bool IsSmoke(in Duct duct)
        {
            string systemName = duct
                .get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)
                .AsValueString()?.ToLower() ?? String.Empty;
            bool isExhaustSmoke = systemName.Contains("дв");
            if (isExhaustSmoke) return true;

            bool isIntakeSmoke = systemName.Contains("дп");
            if (isIntakeSmoke) return true;


            var dependentEls = duct
               .GetDependentElements(new ElementClassFilter(typeof(DuctInsulation)));

            if (dependentEls != null && dependentEls.Count > 0)
            {
                return _doc.GetElement(
                    _doc.GetElement(
                         dependentEls
                            .First())
                            .GetTypeId())
                            .get_Parameter(BuiltInParameter.WINDOW_TYPE_ID)
                            .AsValueString()?
                            .ToLower()
                            .Contains("огнезащита") ?? false;
            }
            return false;
        }


        /// <summary>
        /// Возвращает максимальный габарит воздуховода и тип его сечения
        /// </summary>
        /// <param name="duct">Воздуховод для получения размеров (в мм)</param>
        /// <returns>Кортеж (Воздуховод - круглый ?, максимальный габарит сечения)</returns>
        private (bool isCircle, double maxDimension) GetOpeningDimensions(in Duct duct)
        {
            bool isCircle = false;
            double maxDim = 0;
            ConnectorProfileType shape = duct.ConnectorManager.Connectors.GetFirst().Shape;
            switch (shape)
            {
                case ConnectorProfileType.Invalid:
                    throw new ArgumentException();
                case ConnectorProfileType.Round:
                    {
                        // Воздуховод круглого сечения
                        isCircle = true;
                        maxDim = Math.Round(duct
                            .get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM)
                            .AsDouble() * SharedValues.FootToMillimeters);
                    }
                    break;
                default:
                    {
                        // Если воздуховод прямоугольный или овальный
                        double dimH = Math.Round(duct
                            .get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM)
                            .AsDouble() * SharedValues.FootToMillimeters);
                        double dimW = Math.Round(duct
                            .get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)
                            .AsDouble() * SharedValues.FootToMillimeters);

                        maxDim = dimH >= dimW ? dimH : dimW;
                        break;
                    }
            }

            return (isCircle, maxDim);
        }

        private IEnumerable<Duct> GetDucts()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Duct))
                .WhereElementIsNotElementType()
                .Cast<Duct>();
        }

        private bool ValidateSharedParams()
        {
            Guid[] _sharedParamsForDucts = new Guid[] {
            SharedParams.ADSK_SideThickness
            };
            foreach (var category in _categories)
            {
                if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                    _doc,
                    category,
                    _sharedParamsForDucts))
                {
                    MessageBox.Show("В текущем проекте параметр 'ADSK_Толщина стенки' " +
                        "отсутствует у категории воздуховоды, команда будет ОТМЕНЕНА.",
                        "Ошибка");
                    return false;
                }
            }
            return true;
        }
    }
}
