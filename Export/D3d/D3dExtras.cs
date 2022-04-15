using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    public class D3dExtras
    {
        /// <summary>
        /// The Revit created UniqueId for this object
        /// </summary>
        public string UniqueId { get; set; }
        public GridParameters GridParameters { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
