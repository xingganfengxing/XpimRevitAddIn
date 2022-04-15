using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// The d3d PBR Material format.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#materials
    /// </summary>
    public class Json3DMaterial
    {
        public string name { get; set; }
        public Json3DPBR pbrMetallicRoughness { get; set; }

        public string alphaMode { get; set; }
    }
}
