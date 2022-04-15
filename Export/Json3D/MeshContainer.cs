using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    public class MeshContainer : HashedType
    {
        //public string hashcode { get; set; }
        public Json3DMesh contents { get; set; }
    }
}
