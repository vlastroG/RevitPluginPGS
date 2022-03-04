using Autodesk.Revit.UI;
using System;
using MS.core;
using MS.res;

namespace MS.ui
{
    /// <summary>
    /// The Revit pushbutton methods.
    /// </summary>
    public static class RevitPushButton
    {
        /// <summary>
        /// Creates the push button based on the data provided in <see cref="RevitPushButtonDataModel"/>
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static PushButton Create(RevitPushButtonDataModel data)
        {   // The button name based on unique identifier.
            var btnDataName = Guid.NewGuid().ToString();

            // Sets the button data.
            var btnData = new PushButtonData(btnDataName, data.Label, CoreAssembly.GetAssemblyLocation(), data.CommandNamespacePath)
            {
                LargeImage = ResourceImage.GetIcon("СС.png"),
                ToolTipImage = ResourceImage.GetIcon("СС.png")
            };

            // Return created button and host it on panel provided in required data model.
            return data.Panel.AddItem(btnData) as PushButton;
        }
    }
}
