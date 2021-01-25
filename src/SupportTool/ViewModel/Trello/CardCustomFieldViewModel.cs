using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModel.Trello
{
    public class CardCustomFieldViewModel
    {
        public string IdCustomField { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public CustomFieldValueViewModel Value { get; set; }
    }

    public class CustomFieldValueViewModel
    {
        public string Date { get; set; }
        public string Text { get; set; }
        public string Number { get; set; }
    }
}
