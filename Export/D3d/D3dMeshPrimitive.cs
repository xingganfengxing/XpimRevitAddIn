﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    /// <summary>
    /// Properties defining where the GPU should look to find the mesh and material data.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#meshes
    /// </summary>
    [Serializable]
    public class D3dMeshPrimitive
    {
        public D3dAttribute attributes { get; set; } = new D3dAttribute();
        public int indices { get; set; }
        public int? material { get; set; } = null;
        public int mode { get; set; } = 4; // 4 is triangles
    }
}
