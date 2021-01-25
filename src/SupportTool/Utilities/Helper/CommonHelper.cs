using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Utilities.Helper
{
    public class CommonHelper
    {
        public string GetDataFromAppKey(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return string.Empty;
            }
            else
            {
                return ConfigurationManager.AppSettings[keyName];
            }
        }


        /// <summary>
        /// subPath need to ignore the first backslash
        /// Ex: asssets\json-data
        /// </summary>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public string GetRootPath(string subPath = "")
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var rootFolderPath = path.Substring(0, path.IndexOf(@"\src"));

            if (string.IsNullOrEmpty(subPath))
            {
                return rootFolderPath;
            }
            else
            {
                var findPath = $@"{rootFolderPath}\{subPath}";

                if (Directory.Exists(findPath))
                {
                    return findPath;
                }
                return "";
            }
        }
    }
}
