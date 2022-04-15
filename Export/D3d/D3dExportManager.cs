using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using XpimRevitAddIn.Common;

namespace XpimRevitAddIn.Export.D3d
{
    class D3dExportManager
    {
        /// <summary>
        /// Flag to write coords as Z up instead of Y up (if true).
        /// CAUTION: With local coordinate systems and transforms, this no longer
        /// produces expected results. TODO on fixing it, however there is a larger
        /// philisophical debtate to be had over whether flipping coordinates in
        /// source CAD applications should EVER be the correct thing to do (as opposed to
        /// flipping the camera in the viewer).
        /// </summary>
        private bool _FlipCoords = false;
        /// <summary>
        /// Toggles the export of JSON properties as a d3d Extras
        /// object on each node.
        /// </summary>
        private bool _ExportProperties = true;

        /// <summary>
        /// Stateful, uuid indexable list of all materials in the export.
        /// </summary>
        private IndexedDictionary<D3dMaterial> _MaterialDict = new IndexedDictionary<D3dMaterial>();
        /// <summary>
        /// Dictionary of nodes keyed to their unique id.
        /// </summary>
        private Dictionary<string, D3dExportNode> _NodeDict = new Dictionary<string, D3dExportNode>();
        /// <summary>
        /// Hashable container for mesh data, to aid instancing.
        /// </summary>
        private List<MeshContainer> _MeshContainers = new List<MeshContainer>();

        /// <summary>
        /// List of root nodes defining scenes.
        /// </summary>
        private List<D3dScene> _Scenes = new List<D3dScene>();
        public List<D3dScene> Scenes
        {
            get
            {
                return this._Scenes;
            }
        }
        /// <summary>
        /// List of all buffers referencing the binary file data.
        /// </summary>
        private List<D3dBuffer> _Buffers = new List<D3dBuffer>();
        public List<D3dBuffer> Buffers
        {
            get
            {
                return this._Buffers;
            }
        }
        /// <summary>
        /// List of all BufferViews referencing the buffers.
        /// </summary>
        private List<D3dBufferView> _BufferViews = new List<D3dBufferView>();
        public List<D3dBufferView> BufferViews
        {
            get
            {
                return this._BufferViews;
            }
        }
        /// <summary>
        /// List of all Accessors referencing the BufferViews.
        /// </summary>
        private List<D3dAccessor> _Accessors = new List<D3dAccessor>();
        public List<D3dAccessor> Accessors
        {
            get
            {
                return this._Accessors;
            }
        }

        /// <summary>
        /// Container for the vertex/face/normal information
        /// that will be serialized into a binary format
        /// for the final *.bin files.
        /// </summary>
        private List<D3dBinaryData> _BinaryFileData = new List<D3dBinaryData>();
        public List<D3dBinaryData> BinaryFileData
        {
            get
            {
                return this._BinaryFileData;
            }
        }

        /// <summary>
        /// Ordered list of all nodes
        /// </summary>
        public List<D3dNode> Nodes
        {
            get
            {
                var list = _NodeDict.Values.ToList();
                return list.OrderBy(x => x.index).Select(x => x.ToD3DNode()).ToList();
            }
        }

        /// <summary>
        /// Returns true if the unique id is already present in the list of nodes.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool ContainsNode(string uniqueId)
        {
            return _NodeDict.ContainsKey(uniqueId);
        }

        /// <summary>
        /// List of all materials referenced by meshes.
        /// </summary>
        public List<D3dMaterial> Materials
        {
            get
            {
                return _MaterialDict.List;
            }
        }

        /// <summary>
        /// List of all meshes referenced by nodes.
        /// </summary>
        public List<D3dMesh> Meshes
        {
            get
            {
                return _MeshContainers.Select(x => x.contents).ToList();
            }
        }

        /// <summary>
        /// Stack maintaining the uniqueId's of each node down
        /// the current scene graph branch.
        /// </summary>
        private Stack<string> _ParentStack = new Stack<string>();
        private Stack<string> ParentStack
        {
            get
            {
                return _ParentStack;
            }
        }
        /// <summary>
        /// The uniqueId of the currently open node.
        /// </summary>
        private string CurrentNodeId
        {
            get
            {
                return ParentStack.Peek();
            }
        }

        /// <summary>
        /// Stack maintaining the geometry containers for each
        /// node down the current scene graph branch. These are popped
        /// as we retreat back up the graph.
        /// </summary>
        private Stack<Dictionary<string, GeometryData>> _GeometryStack = new Stack<Dictionary<string, GeometryData>>();
        private Stack<Dictionary<string, GeometryData>> GeometryStack
        {
            get
            {
                return _GeometryStack;
            }
        }
        /// <summary>
        /// The geometry container for the currently open node.
        /// </summary>
        private Dictionary<string, GeometryData> CurrentGeom
        {
            get
            {
                return GeometryStack.Peek();
            }
        }

        /// <summary>
        /// Returns proper tab alignment for displaying element
        /// hierarchy in debug printing.
        /// </summary>
        public string FormatDebugHeirarchy
        {
            get
            {
                string spaces = "";
                for (int i = 0; i < ParentStack.Count; i++)
                {
                    spaces += "  ";
                }
                return spaces;
            }
        }

        public void Start(bool exportProperties = true, bool flipCoords = true)
        {
            this._ExportProperties = exportProperties;
            this._FlipCoords = flipCoords;

            D3dExportNode rootNode = new D3dExportNode(0);
            rootNode.children = new List<int>();
            _NodeDict.Add(rootNode.id, rootNode);
            ParentStack.Push(rootNode.id);

            D3dScene defaultScene = new D3dScene();
            defaultScene.nodes.Add(0);
            Scenes.Add(defaultScene);
        }

        public D3dContainer Finish()
        {
            D3d model = new D3d();
            model.asset = new D3dVersion();
            model.scenes = Scenes;
            model.nodes = Nodes;
            model.meshes = Meshes;
            model.materials = Materials;
            model.buffers = Buffers;
            model.bufferViews = BufferViews;
            model.accessors = Accessors;



            D3dContainer container = new D3dContainer();
            container.d3d = model;
            container.binaries = BinaryFileData;
            container.binaryDic = this.GetBinaryDataDic(BinaryFileData);


            return container;
        }
        private Dictionary<string, D3dBinaryData> GetBinaryDataDic(List<D3dBinaryData> d3dBinaryDataList)
        {
            Dictionary<string, D3dBinaryData> binaryDataDic = new Dictionary<string, D3dBinaryData>();
            for (int i = 0; i < d3dBinaryDataList.Count; i++)
            {
                D3dBinaryData d3dBinaryData = d3dBinaryDataList[i];
                binaryDataDic.Add(d3dBinaryData.name, d3dBinaryData);
            }
            return binaryDataDic;
        }

        public void OpenNode(Element elem, Transform xform = null, bool isInstance = false)
        {
            //// TODO: [RM] Commented out because this is likely to be very buggy and not the 
            //// correct solution intent is to prevent creation of new nodes when a symbol 
            //// is a child of an instance of the same type.
            //// Witness: parking spaces and stair railings for examples of two
            //// different issues with the behavior
            //if (isInstance == true && elem is FamilySymbol)
            //{
            //    FamilyInstance parentInstance = nodeDict[currentNodeId].element as FamilyInstance;
            //    if (
            //        parentInstance != null &&
            //        parentInstance.Symbol != null &&
            //        elem.Name == parentInstance.Symbol.Name
            //    )
            //    {
            //        nodeDict[currentNodeId].matrix = ManagerUtils.ConvertXForm(xform);
            //        return;
            //    }

            //    //nodeDict[currentNodeId].matrix = ManagerUtils.ConvertXForm(xform);
            //    //return;
            //}
            bool exportNodeProperties = _ExportProperties;
            if (isInstance == true && elem is FamilySymbol) exportNodeProperties = false;

            D3dExportNode node = new D3dExportNode(elem, _NodeDict.Count, exportNodeProperties, isInstance, FormatDebugHeirarchy);

            if (ParentStack.Count > 0)
            {
                string parentId = ParentStack.Peek();
                D3dNode parentNode = _NodeDict[parentId];
                if (parentNode.children == null) parentNode.children = new List<int>();
                _NodeDict[parentId].children.Add(node.index);
            }

            ParentStack.Push(node.id);
            if (xform != null)
            {
                node.transform = xform;
                node.matrix = D3dExportManagerUtils.ConvertXForm(xform);
            }

            _NodeDict.Add(node.id, node);

            OpenGeometry();
        }

        public void CloseNode(Element elem = null, bool isInstance = false)
        {
            //// TODO: [RM] Commented out because this is likely to be very buggy and not the 
            //// correct solution intent is to prevent creation of new nodes when a symbol 
            //// is a child of an instance of the same type.
            //// Witness: parking spaces and stair railings for examples of two
            //// different issues with the behavior
            //if (isInstance && elem is FamilySymbol)
            //{
            //    FamilyInstance parentInstance = nodeDict[currentNodeId].element as FamilyInstance;
            //    if (
            //        parentInstance != null &&
            //        parentInstance.Symbol != null &&
            //        elem.Name == parentInstance.Symbol.Name
            //    )
            //    {
            //        return;
            //    }
            //    //return;
            //} 

            if (CurrentGeom != null)
            {
                CloseGeometry();
            }

            ParentStack.Pop();
        }

        public void SwitchMaterial(MaterialNode matNode, string name = null, string id = null)
        {
            D3dMaterial gl_mat = new D3dMaterial();
            gl_mat.name = name;

            D3dPBR pbr = new D3dPBR();
            pbr.baseColorFactor = new List<float>() {
                matNode.Color.Red / 255f,
                matNode.Color.Green / 255f,
                matNode.Color.Blue / 255f,
                1f - (float)matNode.Transparency
            };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = 1f;
            gl_mat.pbrMetallicRoughness = pbr;
            if (matNode.Transparency != 0)
            {
                gl_mat.alphaMode = "BLEND";
            }

            _MaterialDict.AddOrUpdateCurrent(id, gl_mat);
        }

        public void OpenGeometry()
        {
            GeometryStack.Push(new Dictionary<string, GeometryData>());
        }

        public void OnGeometry(PolymeshTopology polymesh)
        {
            if (CurrentNodeId == null) throw new Exception();

            string vertex_key = CurrentNodeId + "_" + _MaterialDict.CurrentKey;
            if (CurrentGeom.ContainsKey(vertex_key) == false)
            {
                CurrentGeom.Add(vertex_key, new GeometryData());
            }

            // Populate normals from this polymesh
            IList<XYZ> norms = polymesh.GetNormals();
            foreach (XYZ norm in norms)
            {
                CurrentGeom[vertex_key].normals.Add(norm.X);
                CurrentGeom[vertex_key].normals.Add(norm.Y);
                CurrentGeom[vertex_key].normals.Add(norm.Z);
            }

            // Populate vertex and faces data
            IList<XYZ> pts = polymesh.GetPoints();
            foreach (PolymeshFacet facet in polymesh.GetFacets())
            {
                int v1 = CurrentGeom[vertex_key].vertDictionary.AddVertex(new PointMeterDouble(pts[facet.V1], _FlipCoords));
                int v2 = CurrentGeom[vertex_key].vertDictionary.AddVertex(new PointMeterDouble(pts[facet.V2], _FlipCoords));
                int v3 = CurrentGeom[vertex_key].vertDictionary.AddVertex(new PointMeterDouble(pts[facet.V3], _FlipCoords));

                CurrentGeom[vertex_key].faces.Add(v1);
                CurrentGeom[vertex_key].faces.Add(v2);
                CurrentGeom[vertex_key].faces.Add(v3);
            }
        }

        public void CloseGeometry()
        {
            // Create the new mesh and populate the primitives with GeometryData
            D3dMesh mesh = new D3dMesh();
            mesh.primitives = new List<D3dMeshPrimitive>();

            // transfer ordered vertices from vertex dictionary to vertices list
            foreach (KeyValuePair<string, GeometryData> key_geom in CurrentGeom)
            {
                string key = key_geom.Key;
                GeometryData geom = key_geom.Value;
                foreach (KeyValuePair<PointMeterDouble, int> point_index in geom.vertDictionary)
                {
                    PointMeterDouble point = point_index.Key;
                    geom.vertices.Add(point.X);
                    geom.vertices.Add(point.Y);
                    geom.vertices.Add(point.Z);
                }

                // convert GeometryData objects into d3dMeshPrimitive
                string material_key = key.Split('_')[1];

                D3dBinaryData bufferMeta = ProcessGeometry(geom, key);
                if (bufferMeta.hashcode != null)
                {
                    BinaryFileData.Add(bufferMeta);
                }

                D3dMeshPrimitive primative = new D3dMeshPrimitive();

                primative.attributes.POSITION = bufferMeta.vertexAccessorIndex;
                primative.indices = bufferMeta.indexAccessorIndex;
                primative.material = _MaterialDict.GetIndexFromUUID(material_key);
                // TODO: Add normal attribute accessor index here

                mesh.primitives.Add(primative);
            }

            // d3d entity can not be empty
            if (mesh.primitives.Count() > 0)
            {
                // Prevent mesh duplication by hash checking
                string meshHash = D3dExportManagerUtils.GenerateSHA256Hash(mesh);
                D3dExportManagerUtils.HashSearch hs = new D3dExportManagerUtils.HashSearch(meshHash);
                int idx = _MeshContainers.FindIndex(hs.EqualTo);

                if (idx != -1)
                {
                    // set the current nodes mesh index to the already
                    // created mesh location.
                    _NodeDict[CurrentNodeId].mesh = idx;
                }
                else
                {
                    // create new mesh and add it's index to the current node.
                    MeshContainer mc = new MeshContainer();
                    mc.hashcode = meshHash;
                    mc.contents = mesh;
                    _MeshContainers.Add(mc);
                    _NodeDict[CurrentNodeId].mesh = _MeshContainers.Count - 1;
                }

            }

            GeometryStack.Pop();
            return;
        }

        /// <summary>
        /// Takes the intermediate geometry data and performs the calculations
        /// to convert that into d3d buffers, views, and accessors.
        /// </summary>
        /// <param name="geomData"></param>
        /// <param name="name">Unique name for the .bin file that will be produced.</param>
        /// <returns></returns>
        private D3dBinaryData ProcessGeometry(GeometryData geom, string name)
        {
            // TODO: rename this type to d3dBufferMeta ?
            D3dBinaryData bufferData = new D3dBinaryData();
            D3dBinaryBufferContents bufferContents = new D3dBinaryBufferContents();

            foreach (var coord in geom.vertices)
            {
                float vFloat = Convert.ToSingle(coord);
                bufferContents.vertexBuffer.Add(vFloat);
            }
            foreach (var index in geom.faces)
            {
                bufferContents.indexBuffer.Add(index);
            }

            // Prevent buffer duplication by hash checking
            string calculatedHash = D3dExportManagerUtils.GenerateSHA256Hash(bufferContents);
            D3dExportManagerUtils.HashSearch hs = new D3dExportManagerUtils.HashSearch(calculatedHash);
            var match = BinaryFileData.Find(hs.EqualTo);

            if (match != null)
            {
                // return previously created buffer metadata
                bufferData.vertexAccessorIndex = match.vertexAccessorIndex;
                bufferData.indexAccessorIndex = match.indexAccessorIndex;
                return bufferData;
            }
            else
            {
                // add a buffer
                D3dBuffer buffer = new D3dBuffer();
                buffer.uri = name + ".bin";
                //buffer.uri = Guid.NewGuid().ToString() + ".bin";
                Buffers.Add(buffer);
                int bufferIdx = Buffers.Count - 1;

                /**
                 * Buffer Data
                 **/
                bufferData.name = buffer.uri;
                bufferData.contents = bufferContents;
                // TODO: Uncomment for normals
                //foreach (var normal in geomData.normals)
                //{
                //    bufferData.normalBuffer.Add((float)normal);
                //}

                // Get max and min for vertex data
                float[] vertexMinMax = CommonFunction.GetVec3MinMax(bufferContents.vertexBuffer);
                // Get max and min for index data
                int[] faceMinMax = CommonFunction.GetScalarMinMax(bufferContents.indexBuffer);
                // TODO: Uncomment for normals
                // Get max and min for normal data
                //float[] normalMinMax = getVec3MinMax(bufferData.normalBuffer);

                /**
                 * BufferViews
                 **/
                // Add a vec3 buffer view
                int elementsPerVertex = 3;
                int bytesPerElement = 4;
                int bytesPerVertex = elementsPerVertex * bytesPerElement;
                int numVec3 = (geom.vertices.Count) / elementsPerVertex;
                int sizeOfVec3View = numVec3 * bytesPerVertex;
                D3dBufferView vec3View = new D3dBufferView();
                vec3View.buffer = bufferIdx;
                vec3View.byteOffset = 0;
                vec3View.byteLength = sizeOfVec3View;
                vec3View.target = Targets.ARRAY_BUFFER;
                BufferViews.Add(vec3View);
                int vec3ViewIdx = BufferViews.Count - 1;

                // TODO: Add a normals (vec3) buffer view

                // Add a faces / indexes buffer view
                int elementsPerIndex = 1;
                int bytesPerIndexElement = 4;
                int bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
                int numIndexes = geom.faces.Count;
                int sizeOfIndexView = numIndexes * bytesPerIndex;
                D3dBufferView facesView = new D3dBufferView();
                facesView.buffer = bufferIdx;
                facesView.byteOffset = vec3View.byteLength;
                facesView.byteLength = sizeOfIndexView;
                facesView.target = Targets.ELEMENT_ARRAY_BUFFER;
                BufferViews.Add(facesView);
                int facesViewIdx = BufferViews.Count - 1;

                Buffers[bufferIdx].byteLength = vec3View.byteLength + facesView.byteLength;

                /**
                 * Accessors
                 **/
                // add a position accessor
                D3dAccessor positionAccessor = new D3dAccessor();
                positionAccessor.bufferView = vec3ViewIdx;
                positionAccessor.byteOffset = 0;
                positionAccessor.componentType = ComponentType.FLOAT;
                positionAccessor.count = geom.vertices.Count / elementsPerVertex;
                positionAccessor.type = "VEC3";
                positionAccessor.max = new List<float>() { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
                positionAccessor.min = new List<float>() { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };
                Accessors.Add(positionAccessor);
                bufferData.vertexAccessorIndex = Accessors.Count - 1;

                // TODO: Uncomment for normals
                // add a normals accessor
                //d3dAccessor normalsAccessor = new d3dAccessor();
                //normalsAccessor.bufferView = vec3ViewIdx;
                //normalsAccessor.byteOffset = (positionAccessor.count) * bytesPerVertex;
                //normalsAccessor.componentType = ComponentType.FLOAT;
                //normalsAccessor.count = geom.data.normals.Count / elementsPerVertex;
                //normalsAccessor.type = "VEC3";
                //normalsAccessor.max = new List<float>() { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
                //normalsAccessor.min = new List<float>() { normalMinMax[0], normalMinMax[2], normalMinMax[4] };
                //this.accessors.Add(normalsAccessor);
                //bufferData.normalsAccessorIndex = this.accessors.Count - 1;

                // add a face accessor
                D3dAccessor faceAccessor = new D3dAccessor();
                faceAccessor.bufferView = facesViewIdx;
                faceAccessor.byteOffset = 0;
                faceAccessor.componentType = ComponentType.UNSIGNED_INT;
                faceAccessor.count = numIndexes;
                faceAccessor.type = "SCALAR";
                faceAccessor.max = new List<float>() { faceMinMax[1] };
                faceAccessor.min = new List<float>() { faceMinMax[0] };
                Accessors.Add(faceAccessor);
                bufferData.indexAccessorIndex = Accessors.Count - 1;

                bufferData.hashcode = calculatedHash;

                return bufferData;
            }
        }
    } 
}
