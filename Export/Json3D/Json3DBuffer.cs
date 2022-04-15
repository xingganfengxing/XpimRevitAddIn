using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// A reference to the location and size of binary data.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#buffers-and-buffer-views
    /// </summary>
    public class Json3DBuffer
    {
        /// <summary>
        /// The uri of the buffer.
        /// </summary>
        public string uri { get; set; }
        /// <summary>
        /// The total byte length of the buffer.
        /// </summary>
        public int byteLength { get; set; }
    }
}
