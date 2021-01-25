using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModel.Trello
{
    public class CardAddNewViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime? DuedateAt { get; set; }
    }
}
