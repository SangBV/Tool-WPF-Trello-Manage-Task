using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModel.Trello
{
    public class CardViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string IdBoard { get; set; }

        public int IdShort { get; set; }

        public string IdList { get; set; }

        public CardBadgesViewModel Badges { get; set; }

        public float Pos { get; set; }

        public List<string> IdLabels { get; set; }

        public List<CardCustomFieldViewModel> CustomFieldItems { get; set; }

        public string ListName { get; set; }

        public List<string> ListLabelTagged { get; set; }

        public int EffortInDay { get; set; }

        public string DateLastActivity { get; set; }
    }
}
