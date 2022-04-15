using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    public class MeshContainer : HashedType
    {
        //public string hashcode { get; set; }
        public D3dMesh contents { get; set; }
    }
}
