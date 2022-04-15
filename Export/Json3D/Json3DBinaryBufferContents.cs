using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{

    [Serializable]
    public class Json3DBinaryBufferContents
    {
        public List<float> vertexBuffer { get; set; } = new List<float>();
        public List<int> indexBuffer { get; set; } = new List<int>();
    }
}
