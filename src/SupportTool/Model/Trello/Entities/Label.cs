﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Model.Trello.Entities
{
	public class Label
	{
		public string Id { get; set; }
		public string IdBoard { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }
	}
}
