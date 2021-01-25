using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello.Entities
{
    public class ListCard
    {
        public List<Card> Cards { get; set; }
    }
}
