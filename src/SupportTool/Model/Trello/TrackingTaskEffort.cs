using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello
{
	public class TrackingTaskEffort
	{
		public DateTime Date { get; set; }

		public int TotalTask { get; set; }

		public float TotalEffort { get; set; }

		public DateTime LastModifiedAt { get; set; }

		public List<string> IdCards { get; set; }
	}
}
