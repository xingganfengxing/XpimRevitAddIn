using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    public class D3dPBR
    {
        public List<float> baseColorFactor { get; set; }
        public float metallicFactor { get; set; }
        public float roughnessFactor { get; set; }
    }
}
