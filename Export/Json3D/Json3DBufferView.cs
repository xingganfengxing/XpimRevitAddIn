using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// A reference to a subsection of a buffer containing either vector or scalar data.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#buffers-and-buffer-views
    /// </summary>
    public class Json3DBufferView
    {
        /// <summary>
        /// The index of the buffer.
        /// </summary>
        public int buffer { get; set; }
        /// <summary>
        /// The offset into the buffer in bytes.
        /// </summary>
        public int byteOffset { get; set; }
        /// <summary>
        /// The length of the bufferView in bytes.
        /// </summary>
        public int byteLength { get; set; }
        /// <summary>
        /// The target that the GPU buffer should be bound to.
        /// </summary>
        public Targets target { get; set; }
        /// <summary>
        /// A user defined name for this view.
        /// </summary>
        public string name { get; set; }
    }
}
