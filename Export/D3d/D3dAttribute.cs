using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    /// <summary>
    /// The list of accessors available to the renderer for a particular mesh.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#meshes
    /// </summary>
    [Serializable]
    public class D3dAttribute
    {
        /// <summary>
        /// The index of the accessor for position data.
        /// </summary>
        public int POSITION { get; set; }
        //public int NORMAL { get; set; }
    }
}
