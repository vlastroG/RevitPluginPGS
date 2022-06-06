using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MS.Utilites.WorkWithGeometry;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PipelineFittings : IExternalCommand
    {
        /// <summary>
        /// Guid параметра НомерКвартиры
        /// </summary>
        private readonly Guid guid_par_apartment_number = Guid.Parse("10fb72de-237e-4b9c-915b-8849b8907695");


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

            var filter_links = new FilteredElementCollector(doc);
            var linked_docs = filter_links
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsNotElementType()
                .ToElements();

            var filter_pipe_stuff = new FilteredElementCollector(doc);
            var pipe_stuff_categories = new ElementMulticategoryFilter(new Collection<BuiltInCategory> {
                     BuiltInCategory.OST_PipeAccessory,
                     BuiltInCategory.OST_MechanicalEquipment});

            var pipe_stuff = filter_pipe_stuff.WherePasses(pipe_stuff_categories)
                .WhereElementIsNotElementType()
                .ToElements();

            // Обнуление значений номеров квартир у арматуры трубопроводов
            using (Transaction trans_renew = new Transaction(doc))
            {
                trans_renew.Start("Арматура труб обнуление");

                foreach (var pipe_acs in pipe_stuff)
                {
                    pipe_acs.get_Parameter(guid_par_apartment_number).Set(String.Empty);
                }

                trans_renew.Commit();
            }

            foreach (var link in linked_docs)
            {
                //Назначение номеров квартир арматуре трубопроводов
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Амратура труб в квартирах");

                    var linked_instance = link as RevitLinkInstance;
                    var room_filter = new FilteredElementCollector(linked_instance.GetLinkDocument());
                    var rooms = room_filter
                           .OfCategory(BuiltInCategory.OST_Rooms)
                           .WhereElementIsNotElementType()
                           .Select(e => e as Room);

                    if (rooms != null)
                    {
                        foreach (var room in rooms)
                        {
                            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
                            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);
                            Solid room_solid = results.GetGeometry();

                            //Изначально solid создается по координатам из связанного файла.
                            //Если связь перемещена, то solid необходимо переместить в эту позицию:
                            Transform transform = linked_instance.GetTransform();
                            if (!transform.AlmostEqual(Transform.Identity))
                            {
                                room_solid = SolidUtils
                                    .CreateTransformed(room_solid, transform);
                            }

                            var pipe_acs_in_room = new FilteredElementCollector(doc)
                                .WherePasses(pipe_stuff_categories)
                                .WherePasses(new ElementIntersectsSolidFilter(room_solid))
                                .Cast<FamilyInstance>()
                                .ToList();

                            foreach (var pipe_acs in pipe_acs_in_room)
                            {
                                var fam_inst = pipe_acs as FamilyInstance;
                                fam_inst
                                    .get_Parameter(guid_par_apartment_number)
                                    .Set(room.get_Parameter(guid_par_apartment_number).AsValueString());
                            }
                        }
                    }

                    trans.Commit();
                }
            }
            return Result.Succeeded;
        }
    }
}
