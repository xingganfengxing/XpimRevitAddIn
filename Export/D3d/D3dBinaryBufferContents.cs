using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{

    [Serializable]
    public class D3dBinaryBufferContents
    {
        public List<float> vertexBuffer { get; set; } = new List<float>();
        public List<int> indexBuffer { get; set; } = new List<int>();
    }
}
