using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// A binary data store serialized to a *.bin file
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#binary-data-storage
    /// </summary>
    public class Json3DBinaryData : HashedType
    {
        public Json3DBinaryBufferContents contents { get; set; }
        //public List<float> vertexBuffer { get; set; } = new List<float>();
        //public List<int> indexBuffer { get; set; } = new List<int>();
        //public List<float> normalBuffer { get; set; } = new List<float>();
        public int vertexAccessorIndex { get; set; }
        public int indexAccessorIndex { get; set; }
        //public int normalsAccessorIndex { get; set; }
        public string name { get; set; }
        //public string hashcode { get; set; }
    }
}
