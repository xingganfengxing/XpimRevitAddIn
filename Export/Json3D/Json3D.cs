using System;
using System.Collections.Generic;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// The json serializable d3d file format.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0
    /// </summary>
    public struct Json3D
    {
        public Json3DVersion asset;
        public List<Json3DScene> scenes;
        public List<Json3DNode> nodes;
        public List<Json3DMesh> meshes;
        public List<Json3DBuffer> buffers;
        public List<Json3DBufferView> bufferViews;
        public List<Json3DAccessor> accessors;
        public List<Json3DMaterial> materials;
    }
}
