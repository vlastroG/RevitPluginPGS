using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.ui
{
    /// <summary>
    /// Represents Revit push button data model
    /// </summary>
    public class RevitPushButtonDataModel
    {
        /// <summary>
        /// Gets or sets the Label of the button.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the pannel on which button is hosted.
        /// </summary>
        /// <value>
        /// The panel.
        /// </value>
        public RibbonPanel Panel { get; set; }

        /// <summary>
        /// Gets or sets the command namespace path.
        /// </summary>
        /// <value>
        /// The command namespace path.
        /// </value>
        public string CommandNamespacePath { get; set; }

        /// <summary>
        /// Gets or sets the tooltip
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string Tooltip { get; set; }

        /// <summary>
        /// Gets or sets the name of the icon image.
        /// </summary>
        /// <value>
        /// The name of the icon image.
        /// </value>
        public string IconImageName { get; set; }

        /// <summary>
        /// Gets or sets the name of the tooltip image.
        /// </summary>
        /// <value>
        /// The name of the tooltip image
        /// </value>
        public string TooltipImageName { get; set; }


        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of thr <see cref="RevitPushButtonDataModel"/> class.
        /// </summary>
        public RevitPushButtonDataModel()
        {

        }
    }
}
