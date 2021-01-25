using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello.Entities
{
    public class Card
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string IdBoard { get; set; }

        public int IdShort { get; set; }
        public string IdList { get; set; }
        public CardBadges Badges { get; set; }

        public float Pos { get; set; }

        public List<string> IdLabels { get; set; }

        public List<CardCustomField> CustomFieldItems { get; set; }

        public string DateLastActivity { get; set; }
    }
}
