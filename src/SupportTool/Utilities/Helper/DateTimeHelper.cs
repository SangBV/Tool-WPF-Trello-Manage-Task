using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Utilities.Helper
{
    public class DateTimeHelper
    {
        public string ConvertDateToUTC(DateTime datetime)
        {
            return datetime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }
    }
}
