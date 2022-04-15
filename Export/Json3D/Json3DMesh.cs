﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// The array of primitives defining the mesh of an object.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#meshes
    /// </summary>
    [Serializable]
    public class Json3DMesh
    {
        public List<Json3DMeshPrimitive> primitives { get; set; }
    }
}
