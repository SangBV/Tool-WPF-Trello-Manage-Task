using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello
{
    public class TrelloCardTemplate
    {
        public string IdList { get; set; }
        public List<string> IdLabels { get; set; }

        public List<CustomFieldTemplate> CustomField { get; set; }
    }

    public class CustomFieldTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public ValueNumber ValueNumber { get; set; }
        public ValueDate ValueDate { get; set; }
        public ValueText ValueText { get; set; }
    }
}
