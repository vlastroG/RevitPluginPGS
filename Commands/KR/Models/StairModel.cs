using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using MS.Commands.KR.Models;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.KR
{
    /// <summary>
    /// Вспомогательный класс для армирования лестничных маршей.
    /// В конструктор подается элемент валидной категории (Лестницы, Каркас несущий),
    /// из которого получается геометрия лестничных маршей.
    /// Элемент для обработки должен помимо валидной категории быть замоделирован так,
    /// чтобы лестничные марши НЕ были соединены с площадкой.
    /// </summary>
    internal sealed class StairModel
    {
        /// <summary>
        /// Список валидных категорий элемента, который подается в конструктор для получения геометрии.
        /// </summary>
        private readonly List<BuiltInCategory> _validCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_Stairs,
            BuiltInCategory.OST_StructuralFraming
        };

        /// <summary>
        /// Список лестничных маршей, полученных из элемента. Может содержать 1 или 2 элемента.
        /// </summary>
        private readonly List<Solid> _stairFlightSolids = new List<Solid>();

        /// <summary>
        /// Список лестничных маршей
        /// </summary>
        private readonly List<StairFlight> _stairFlights = new List<StairFlight>();

        /// <summary>
        /// Опции для получения геометрии элемента лестницы - низкий уровень детализации.
        /// </summary>
        private static readonly Options _options = new Options() { DetailLevel = ViewDetailLevel.Coarse };


        /// <summary>
        /// Конструктор модели лестницы для армирования.
        /// </summary>
        /// <param name="element">Элемент валидной категории для получения геометрии лестницы.</param>
        /// <exception cref="ArgumentException">Исключение, если категория подаваемого элемента невалидная.</exception>
        public StairModel(Element element)
        {
            StairValidation(element);

            ClearAllGeometryData();

            FillStairSolidsList(element);

            FillStairFlightList(_stairFlightSolids);
        }


        /// <summary>
        /// Валидация элемента, из которого берется геометрия для создания StairModel.
        /// </summary>
        /// <param name="element">Элемент для валидации.</param>
        /// <exception cref="ArgumentException">Элемент для сздания StairModel не валидный.</exception>
        private void StairValidation(Element element)
        {
            // Валидация входного элемента
            if (!(element is Stairs)
                && !_validCategories.Contains(
                    (BuiltInCategory)WorkWithParameters.GetCategoryIdAsInteger(element)))
            {
                throw new ArgumentException(nameof(element));
            }
        }

        /// <summary>
        /// Заполняет список solid лестничных маршей данной лестницы.
        /// Из элемента получаются solid, имеющие ненулевой объем,
        /// далее этот локальный список сортируется по количеству surface.
        /// В заполняемый список solid лестничных маршей попадают первые 2,
        /// имеющие наибольшее количество surface.
        /// </summary>
        /// <param name="element">Элемент лестницы для анализа.</param>
        private void FillStairSolidsList(Element element)
        {
            GeometryElement geoElement = element.get_Geometry(_options);

            foreach (GeometryObject geoObject in geoElement)
            {
                GeometryInstance geoInstance = geoObject as GeometryInstance;
                if (geoInstance != null)
                {
                    GeometryElement instanceGeometryElement = geoInstance.GetInstanceGeometry();
                    List<Solid> allProtoGeoSolids = new List<Solid>();
                    foreach (GeometryObject protoGeoObject in instanceGeometryElement)
                    {

                        Solid solid = protoGeoObject as Solid;
                        if (solid != null && solid.Volume > 0)
                        {
                            allProtoGeoSolids.Add(solid);
                        }
                    }
                    allProtoGeoSolids.Sort((s1, s2) => s1.Faces.Size.CompareTo(s2.Faces.Size));
                    allProtoGeoSolids.Reverse();

                    if (allProtoGeoSolids.Count == 1)
                    {
                        _stairFlightSolids.Add(allProtoGeoSolids.First());
                    }
                    else if (allProtoGeoSolids.Count >= 2)
                    {
                        _stairFlightSolids.AddRange(allProtoGeoSolids.GetRange(0, 2));
                    }
                    else
                    {
                        throw new ArgumentException(nameof(allProtoGeoSolids));
                    }
                }
            }
        }

        /// <summary>
        /// Заполняет список моделей лестничных маршей.
        /// </summary>
        /// <param name="solids">Лестничные марши.</param>
        private void FillStairFlightList(List<Solid> solids)
        {
            foreach (var solid in solids)
            {
                StairFlight stairFlight = new StairFlight(solid);
                _stairFlights.Add(stairFlight);
            }
        }

        /// <summary>
        /// Очистка списков элементов геометрии с предыдущей команды.
        /// </summary>
        private void ClearAllGeometryData()
        {
            _stairFlightSolids.Clear();
            _stairFlightSolids.Clear();
        }
    }
}
