using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishing : IExternalCommand
    {
        /// <summary>
        /// Команда для назначения элементам, образующим границы помещения и являющихся внутренней отделкой, 
        /// номер этого помещения. Команда в разработке, необходимо определиться с параметрами, 
        /// куда писать номера помещений и проверить логику работы функции нахождения элементов.
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            List<Room> rooms = new List<Room>(
              sel.PickElementsByRectangle()
              .Cast<Room>());

            if (1 != rooms.Count)
            {
                message = "Please select exactly one room.";

                return Result.Failed;
            }

            View3D view3d
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));

            if (null == view3d)
            {
                MessageBox.Show("Не нейден {3D} вид по умолчанию", "Ошибка");

                return Result.Failed;
            }

            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> loops
                  = room.GetBoundarySegments(
                    new SpatialElementBoundaryOptions());

                int n = loops.Count;

                string testOut = "";
                StringBuilder sb = new StringBuilder();
                foreach (IList<BoundarySegment> loop in loops)
                {
                    n = loop.Count;

                    foreach (BoundarySegment seg in loop)
                    {
                        Element e = doc.GetElement(seg.ElementId);

                        if (null == e)
                        {
                            e = WorkWithGeometry.GetElementByRay(doc, view3d,
                              seg.GetCurve());
                        }

                        ElementId eTypeId = e.GetTypeId();

                        ElementType type = doc.GetElement(eTypeId) as ElementType;


                        try
                        {
                            sb.Append(type.LookupParameter("Имя типа").AsString());


                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }
                testOut = sb.ToString();

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("PGS_RoomsFinishing");

                    room.LookupParameter("Отделка потолка").Set(testOut);

                    trans.Commit();
                }
            }
            return Result.Succeeded;
        }


    }
}
