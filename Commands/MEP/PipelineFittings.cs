using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using MS.GUI.MEP;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PipelineFittings : IExternalCommand
    {
        /// <summary>
        /// Обнуляет значение параметра ADSK_Номер квартиры для всех элементов
        /// </summary>
        /// <param name="doc">Документ Revit, в котором будет происходить транзакция</param>
        /// <param name="elems">Элементы для обнуления</param>
        private void ClearParamValues(Document doc, IList<Element> elems)
        {
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("MEP in rooms clear values");

                foreach (var elem in elems)
                {
                    elem.get_Parameter(SharedParams.ADSK_NumberOfApartment).Set(String.Empty);
                }

                trans.Commit();
            }
        }

        /// <summary>
        /// Валидация файла Revit на наличие необходимых общих параметров
        /// </summary>
        /// <param name="doc">Файл Revit</param>
        /// <returns>True, если файл валидный, иначе false</returns>
        private bool ValidateRevitFile(Document doc)
        {
            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.ADSK_NumberOfApartment
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_PipeAccessory,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Арматура трубопроводов\" " +
                    "отсутствует необходимый общий параметр:" +
                    "\nADSK_Номер квартиры",
                    "Ошибка");
                return false;
            }
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_MechanicalEquipment,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Оборудование\" " +
                    "отсутствует необходимый общий параметр:" +
                    "\nADSK_Номер квартиры",
                    "Ошибка");
                return false;
            }
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_DuctTerminal,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Воздухораспределители\" " +
                    "отсутствует необходимый общий параметр:" +
                    "\nADSK_Номер квартиры",
                    "Ошибка");
                return false;
            }
            return true;
        }

        private (List<Element> Elements, ElementId Error) GetIntersectedElems(
            Document doc,
            Element SpatialEl,
            List<BuiltInCategory> categories,
            RevitLinkInstance link)
        {
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            Solid solid;
            if (!(SpatialEl is SpatialElement))
            {
                throw new ArgumentException($"{SpatialEl} не является наследником класса {nameof(SpatialElement)}");
            }
            SpatialElement spatial = SpatialEl as SpatialElement;
            try
            {
                SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(spatial);
                solid = results.GetGeometry();
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                return (null, SpatialEl.Id);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                return (null, SpatialEl.Id);
            }

            //Изначально solid создается по координатам из связанного файла.
            //Если связь перемещена, то solid необходимо переместить в эту позицию:
            if (link != null)
            {
                Transform transform = link.GetTransform();
                if (!transform.AlmostEqual(Transform.Identity))
                {
                    solid = SolidUtils
                        .CreateTransformed(solid, transform);
                }
            }
            var filter = new ElementMulticategoryFilter(categories);
            var elems = new FilteredElementCollector(doc)
                .WherePasses(filter)
                .WherePasses(new ElementIntersectsSolidFilter(solid))
                .ToList();
            return (elems, null);
        }

        /// <summary>
        /// Назначение номера квартиры (из помещений в связях) арматуре трубопроводов и оборудованию,
        /// которые расположена в ней.
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            if (!ValidateRevitFile(doc))
            {
                return Result.Cancelled;
            }

            var ui = new MEPinSpatials();
            ui.ShowDialog();
            if (ui.DialogResult != true)
            {
                return Result.Cancelled;
            }
            var categories = ui.Categories.ToList();
            var spatialsFromLinks = ui.SpatialsFromLinks;

            //var filter_links = new FilteredElementCollector(doc);
            //var linked_docs = filter_links
            //    .OfCategory(BuiltInCategory.OST_RvtLinks)
            //    .WhereElementIsNotElementType()
            //    .Where(e => e.Name.Contains("АР"))
            //    .ToList();

            var filter_pipe_stuff = new FilteredElementCollector(doc);
            var pipe_stuff_categories = new ElementMulticategoryFilter(categories);

            var pipe_stuff = filter_pipe_stuff.WherePasses(pipe_stuff_categories)
                .WhereElementIsNotElementType()
                .ToElements();

            // Обнуление значений номеров квартир у арматуры трубопроводов
            ClearParamValues(doc, pipe_stuff);
            var setCount = 0;

            var iterationsCount = 0;

            List<ElementId> errorIds = new List<ElementId>();

            var spaces = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType()
                .Cast<Space>()
                .ToList();
            //------------------------------------------------------Если выбор пространств
            //Назначение номеров квартир арматуре трубопроводов
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Арматура труб в пространствах");

                if (spaces != null)
                {
                    foreach (var room in spaces)
                    {
                        SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
                        Solid room_solid;
                        try
                        {
                            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);
                            room_solid = results.GetGeometry();
                        }
                        catch (Autodesk.Revit.Exceptions.ArgumentException)
                        {
                            errorIds.Add(room.Id);
                            continue;
                        }
                        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                        {
                            errorIds.Add(room.Id);
                            continue;
                        }


                        var pipe_acs_in_room = new FilteredElementCollector(doc)
                            .WherePasses(pipe_stuff_categories)
                            .WherePasses(new ElementIntersectsSolidFilter(room_solid))
                            .ToElements();

                        foreach (var pipe_acs in pipe_acs_in_room)
                        {
                            iterationsCount++;
                            var fam_inst = pipe_acs as FamilyInstance;
                            fam_inst
                                .get_Parameter(SharedParams.ADSK_NumberOfApartment)
                                .Set(room.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NUMBER).AsValueString());
                            setCount++;
                        }
                    }
                }

                trans.Commit();
                if (errorIds.Count > 0)
                {
                    string ids = String.Join(", ", errorIds.Select(e => e.ToString()));
                    MessageBox.Show($"Ошибка, пространства не обработаны, нельзя определить их объемы. Id: {ids}." +
                        $"\n\nНомера пространств назначены {setCount} раз " +
                        $"экземплярам категорий Оборудование и Арматура трубопроводов",
                        "Номера пространств для MEP, выполнено с ошибками!");
                }
                else
                {
                    MessageBox.Show($"Номера пространств назначены {setCount} раз " +
                        $"экземплярам категорий Оборудование и Арматура трубопроводов",
                        "Номера пространств для MEP, выполнено без ошибок");
                }
            }
            //-----------------------------------------------------выбор пространств окончание

            //foreach (var link in linked_docs)
            //{
            //    //Назначение номеров квартир арматуре трубопроводов
            //    using (Transaction trans = new Transaction(doc))
            //    {
            //        trans.Start("Арматура труб в квартирах");

            //        var linked_instance = link as RevitLinkInstance;
            //        var room_filter = new FilteredElementCollector(linked_instance.GetLinkDocument());
            //        var rooms = room_filter
            //               .OfCategory(BuiltInCategory.OST_Rooms)
            //               .WhereElementIsNotElementType()
            //               .Select(e => e as Room)
            //               .ToList();

            //        if (rooms != null)
            //        {
            //            foreach (var room in rooms)
            //            {
            //                SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            //                Solid room_solid;
            //                try
            //                {
            //                    SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);
            //                    room_solid = results.GetGeometry();
            //                }
            //                catch (Autodesk.Revit.Exceptions.ArgumentException)
            //                {
            //                    errorIds.Add(room.Id);
            //                    continue;
            //                }
            //                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            //                {
            //                    errorIds.Add(room.Id);
            //                    continue;
            //                }

            //                //Изначально solid создается по координатам из связанного файла.
            //                //Если связь перемещена, то solid необходимо переместить в эту позицию:
            //                Transform transform = linked_instance.GetTransform();
            //                if (!transform.AlmostEqual(Transform.Identity))
            //                {
            //                    room_solid = SolidUtils
            //                        .CreateTransformed(room_solid, transform);
            //                }

            //                var pipe_acs_in_room = new FilteredElementCollector(doc)
            //                    .WherePasses(pipe_stuff_categories)
            //                    .WherePasses(new ElementIntersectsSolidFilter(room_solid))
            //                    .Cast<FamilyInstance>()
            //                    .ToList();

            //                foreach (var pipe_acs in pipe_acs_in_room)
            //                {
            //                    var fam_inst = pipe_acs as FamilyInstance;
            //                    fam_inst
            //                        .get_Parameter(SharedParams.ADSK_NumberOfApartment)
            //                        .Set(room.get_Parameter(SharedParams.ADSK_NumberOfApartment).AsValueString());
            //                    setCount++;
            //                }
            //            }
            //        }

            //        trans.Commit();
            //        if (errorIds.Count > 0)
            //        {
            //            string ids = String.Join(", ", errorIds.Select(e => e.ToString()));
            //            MessageBox.Show($"Ошибка, помещения не обработаны, нельзя определить их объемы. Id: {ids}." +
            //                $"\n\nНомера квартир назначены {setCount} раз " +
            //                $"экземплярам категорий Оборудование и Арматура трубопроводов",
            //                "Номера квартир для MEP, выполнено с ошибками!");
            //        }
            //        else
            //        {
            //            MessageBox.Show($"Номера квартир назначены {setCount} раз " +
            //                $"экземплярам категорий Оборудование и Арматура трубопроводов",
            //                "Номера квартир для MEP, выполнено без ошибок");
            //        }
            //    }
            //}
            return Result.Succeeded;
        }
    }
}
