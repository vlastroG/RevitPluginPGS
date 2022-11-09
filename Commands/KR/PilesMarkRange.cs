using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS.Utilites;
using MS.Shared;
using System.Windows;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PilesMarkRange : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.Mrk_MarkOfConstruction,
            SharedParams.Org_PositionRange
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_StructuralColumns,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Несущие колонны\" " +
                    "отсутствуют необходимые общие параметры:" +
                    "\nМрк.МаркаКонструкции" +
                    "\nОрг.ДиапазонПозиций",
                    "Ошибка");
                return Result.Cancelled;
            }

            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory> { BuiltInCategory.OST_StructuralColumns },
                false);

            List<Element> piles = null;
            try
            {
                piles = uidoc.Selection
                    .PickObjects(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        filter,
                        "Выберите сваи (несущие колонны)")
                    .Select(e => doc.GetElement(e))
                    .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            // Словарь пар значений параметров Мрк.МаркаКонструкции и списка Марок свай
            Dictionary<string, List<int>> mrkMarkPairs = new Dictionary<string, List<int>>();
            List<ElementId> errors = new List<ElementId>();
            foreach (var pile in piles)
            {
                string mrkValue = pile.get_Parameter(SharedParams.Mrk_MarkOfConstruction)
                    .AsValueString() ?? String.Empty;
                string markValueString = pile.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)
                    .AsValueString() ?? String.Empty;
                int markValueInt = 0;
                if (!int.TryParse(markValueString, out markValueInt))
                {
                    errors.Add(pile.Id);
                    continue;
                }
                if (mrkMarkPairs.ContainsKey(mrkValue))
                {
                    mrkMarkPairs[mrkValue].Add(markValueInt);
                }
                else
                {
                    mrkMarkPairs.Add(mrkValue, new List<int>() { markValueInt });
                }
            }
            Dictionary<string, string> mrkRangePairs = new Dictionary<string, string>();
            int setCount = 0;
            foreach (var pair in mrkMarkPairs)
            {
                StringBuilder sb = new StringBuilder();
                List<int> values = pair.Value;
                values = values.Distinct().ToList();
                values.Sort();
                sb.Append(values[0]);
                for (int i = 1; i < values.Count; i++)
                {
                    if ((values[i - 1] + 1) == values[i]
                        && (i + 1) != values.Count)
                    {
                        // последовательность сохраняется и текущий элемент не последний
                        continue;
                    }
                    else if ((values[i - 1] + 1) == values[i]
                        && (i + 1) == values.Count)
                    {
                        // последовательность сохраняется и текущий элемент последний
                        sb.Append('-');
                        sb.Append(values[i]);
                    }
                    else if ((values[i - 1] + 1) != values[i])
                    {
                        // Последовательность не сохраняется
                        if (i != 1 && (values[i - 2] + 1) == values[i - 1])
                        {
                            // последовательность до этого сохранялась и текущий элемент не второй
                            sb.Append('-');
                            sb.Append(values[i - 1]);
                            sb.Append(',');
                            sb.Append(' ');
                            sb.Append(values[i]);
                        }
                        else
                        {
                            // последовательность до этого не сохранялась и уже выполнялось предыдущее условие
                            sb.Append(',');
                            sb.Append(' ');
                            sb.Append(values[i]);
                        }
                    }
                }
                mrkRangePairs.Add(pair.Key, sb.ToString());
            }
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Диапазон позиций свай");
                foreach (var pile in piles)
                {
                    if (!errors.Contains(pile.Id))
                    {
                        var mrkValue = pile.get_Parameter(SharedParams.Mrk_MarkOfConstruction)
                            .AsValueString() ?? String.Empty;
                        pile.get_Parameter(SharedParams.Org_PositionRange).Set(mrkRangePairs[mrkValue]);
                        setCount++;
                    }
                }
                trans.Commit();
            }
            if (errors.Count > 0)
            {
                var errorIds = String.Join(", ", errors);
                MessageBox.Show($"Нельзя преобразовать Марки сваи (несущей колонны) в целые числа, " +
                    $"Id: {errorIds}" +
                    $"\n\nДиапазон марок свай назначен {setCount} раз.",
                    "Диапазон марок назначен с ошибками!");
            }
            else
            {
                MessageBox.Show($"Диапазон марок свай назначен {setCount} раз.",
                    "Диапазон марок назначен без ошибок");
            }
            return Result.Succeeded;
        }
    }
}
