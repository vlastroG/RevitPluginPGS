using MS.Commands.AR.DTO;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR
{
    public class OpeningsAreaViewModel : ViewModelBase
    {
        public ObservableCollection<RoomDto> RoomDtos { get; }

        public OpeningsAreaViewModel()
        {
            RoomDtos = new ObservableCollection<RoomDto>();
        }

        public OpeningsAreaViewModel(IEnumerable<RoomDto> roomDtos)
        {
            RoomDtos = new ObservableCollection<RoomDto>(roomDtos);
        }
    }
}
