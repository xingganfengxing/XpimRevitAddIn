using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    public class Json3DPBR
    {
        public List<float> baseColorFactor { get; set; }
        public float metallicFactor { get; set; }
        public float roughnessFactor { get; set; }
    }
}
