using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    public class Json3DExtras
    {
        /// <summary>
        /// The Revit created UniqueId for this object
        /// </summary>
        public string UniqueId { get; set; }
        public GridParameters GridParameters { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
