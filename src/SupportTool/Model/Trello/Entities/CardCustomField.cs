using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello.Entities
{
	public class CardCustomField
	{
		public string IdCustomField { get; set; }

		public CustomFieldValue Value { get; set; }
	}
}
