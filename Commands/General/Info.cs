using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Info : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var messagebox = MessageBox.Show(
                "Плагин содержит основные команды" +
                "\nдля проектировщиков ПГС Проект." +
                "\n" +
                "\nДля подробной информации обратитесь в BIM-отдел.",
                "Информация");
            return Result.Succeeded;
        }
    }
}
