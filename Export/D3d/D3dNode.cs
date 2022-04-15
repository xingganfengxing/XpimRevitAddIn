using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    /// <summary>
    /// The nodes defining individual (or nested) elements in the scene.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#nodes-and-hierarchy
    /// </summary>
    public class D3dNode
    {
        /// <summary>
        /// The user-defined name of this object
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        public int? mesh { get; set; } = null;
        /// <summary>
        /// Transform
        /// </summary>
        public Transform transform { get; set; }
        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column major order.
        /// </summary>
        public List<double> matrix { get; set; }
        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        public List<int> children { get; set; }
        /// <summary>
        /// The extras describing this node.
        /// </summary>
        public D3dExtras extras { get; set; }
    }
}
