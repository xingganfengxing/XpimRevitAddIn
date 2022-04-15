using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    /// <summary>
    /// A reference to a subsection of a BufferView containing a particular data type.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#accessors
    /// </summary>
    public class D3dAccessor
    {
        /// <summary>
        /// The index of the bufferView.
        /// </summary>
        public int bufferView { get; set; }
        /// <summary>
        /// The offset relative to the start of the bufferView in bytes.
        /// </summary>
        public int byteOffset { get; set; }
        /// <summary>
        /// the datatype of the components in the attribute
        /// </summary>
        public ComponentType componentType { get; set; }
        /// <summary>
        /// The number of attributes referenced by this accessor.
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// Specifies if the attribute is a scalar, vector, or matrix
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Maximum value of each component in this attribute.
        /// </summary>
        public List<float> max { get; set; }
        /// <summary>
        /// Minimum value of each component in this attribute.
        /// </summary>
        public List<float> min { get; set; }
        /// <summary>
        /// A user defined name for this accessor.
        /// </summary>
        public string name { get; set; }
    }
}
