using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello
{
	public class TrelloCustomFieldUpdate<T>
	{
		[JsonProperty(PropertyName = "value")]
		public T Value { get; set; }
	}

	public class ValueNumber
	{
		[JsonProperty(PropertyName = "number")]
		public string Number { get; set; }
	}

	public class ValueDate
	{
		[JsonProperty(PropertyName = "date")]
		public string Date { get; set; }
	}

    public class ValueText
    {
        public string Text { get; set; }
    }
}
