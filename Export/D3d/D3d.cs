using System;
using System.Collections.Generic;

namespace XpimRevitAddIn.Export.D3d
{
    /// <summary>
    /// The json serializable d3d file format.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0
    /// </summary>
    public struct D3d
    {
        public D3dVersion asset;
        public List<D3dScene> scenes;
        public List<D3dNode> nodes;
        public List<D3dMesh> meshes;
        public List<D3dBuffer> buffers;
        public List<D3dBufferView> bufferViews;
        public List<D3dAccessor> accessors;
        public List<D3dMaterial> materials;
    }
}
