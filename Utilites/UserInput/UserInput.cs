﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace MS.Utilites
{
    public static class UserInput
    {
        /// <summary>
        /// Возвращает строку от пользователя, введенную в диалоговом окне.
        /// </summary>
        /// <param name="header">Заголовок диалогового окна</param>
        /// <param name="message">Сообщение для пользователя</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Строка от пользователя в lower case</returns>
        public static string GetStringFromUser(string header, string message, string defaultValue)
        {
            var stringFromUser = Interaction.InputBox(
                message,
                header,
                defaultValue);

            return stringFromUser;
        }

        /// <summary>
        /// Получить плоскую поверхность от пользователя.
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Плоская поверхность, выбранная пользователем.</returns>
        public static PlanarFace GetPlanarFaceFromUser(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference faceRef = uidoc.Selection.PickObject(ObjectType.Face, new SelectionFilterPlanarFaces(doc), "Please pick a planar face to set the work plane. ESC for cancel.");
            GeometryObject geoObject = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
            PlanarFace planarFace = geoObject as PlanarFace;

            return planarFace;
        }

        /// <summary>
        /// Получить ребро от пользователя.
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Ребро, выбранное пользователем.</returns>
        public static Edge GetEdgeFromUser(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference edgeRef = uidoc.Selection.PickObject(ObjectType.Edge, new SelectionFilterEdges(doc), "Please pick an edge to set the work plane. ESC for cancel.");
            GeometryObject geoObjectEdge = doc.GetElement(edgeRef).GetGeometryObjectFromReference(edgeRef);
            Edge edge = geoObjectEdge as Edge;

            return edge;
        }

        /// <summary>
        /// Возвращает DialogResult от пользователя
        /// </summary>
        /// <param name="header">Заголовок диалогового окна</param>
        /// <param name="message">Сообщение пользователю</param>
        /// <returns>Возвращаемый результат: Yes/No/Cancel</returns>
        public static DialogResult YesNoCancelInput(string header, string message)
        {
            DialogResult dialogResult = MessageBox.Show(
                message,
                header,
                MessageBoxButtons.YesNoCancel);

            return dialogResult;
        }
    }
}
