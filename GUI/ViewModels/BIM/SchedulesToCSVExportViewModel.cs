using MS.Commands.BIM.DTO;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.BIM
{
    public class SchedulesToCSVExportViewModel : ViewModelBase
    {
        public ObservableCollection<ScheduleToCSVExportDto> Schedules { get; }

        public SchedulesToCSVExportViewModel()
        {

        }
    }
}
