using Autodesk.Revit.DB;
using XpimRevitAddIn.Common;
using XpimRevitAddIn.Export.D3d;
using XpimRevitAddIn.Export.Json3D;
using XpimRevitAddIn.Revit2Pim;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XpimRevitAddIn.Export.Converter
{
    /// <summary>
    /// pim转换
    /// </summary>
    public class Json3DConverter : IConverter
    {
        #region FileExtension
        /// <summary>
        /// 文件后缀名
        /// </summary>
        public static string FileExtension
        {
            get
            {
                return ".pim";
            }
        }
        #endregion

        #region RevitPimMap
        private RevitPimMap _RevitPimMap = null;
        public RevitPimMap RevitPimMap
        {
            get
            {
                return this._RevitPimMap;
            }
            set
            {
                this._RevitPimMap = value;
            }
        }
        #endregion

        #region Process
        public void Process(IContainer container, string dirPath, string fileName)
        {
            try {
                Json3DContainer json3DContainer = (Json3DContainer)container;

                //所有elementId与属性的对照
                Dictionary<string, JObject> elementId2ParamterJsons = new Dictionary<string, JObject>();
                for (int i = 0; i < json3DContainer.elements.Count; i++)
                {
                    JObject parameterJson = json3DContainer.elements[i] as JObject;
                    string elementId = parameterJson.GetValue("id").ToString();
                    elementId2ParamterJsons.Add(elementId, parameterJson);
                }

                //所有elementId与类型、族的对照
                Dictionary<string, JObject> elementId2CatFamilyJsons = new Dictionary<string, JObject>();
                JArray elementsJArray = json3DContainer.modelTree.GetValue("elements") as JArray;
                for (int i = 0; i < elementsJArray.Count; i++)
                {
                    JObject elementsJson = elementsJArray[i] as JObject;
                    string elementId = elementsJson.GetValue("id").ToString();
                    elementId2CatFamilyJsons.Add(elementId, elementsJson);
                }
                
                JObject rootJson = new JObject();
                List<JObject> modelUnitJsonList = new List<JObject>();
                for (int i = 0; i < json3DContainer.json3D.scenes.Count; i++)
                {
                    Json3DScene Json3DScene = json3DContainer.json3D.scenes[i];
                    for (int j = 0; j < Json3DScene.nodes.Count; j++)
                    {
                        int rootNodeIndex = Json3DScene.nodes[j];
                        Json3DNode rootJson3DNode = json3DContainer.json3D.nodes[rootNodeIndex];
                        Transform rootTransform = rootJson3DNode.transform;
                        for (int k = 0; k < rootJson3DNode.children.Count; k++)
                        {
                            int nodeIndex = rootJson3DNode.children[k];
                            Json3DNode json3DNode = json3DContainer.json3D.nodes[nodeIndex];

                            if (elementId2ParamterJsons.ContainsKey(json3DNode.name))
                            {
                                this.ProcessUnitNode(json3DNode, json3DContainer, rootJson, modelUnitJsonList, rootTransform);
                            }
                        }
                    }
                } 

                JObject modelJson = new JObject();
                modelJson.Add("version", "#PIM 1.0");
                modelJson.Add("upAxis", "Z");

                JObject formatOptionsJson = new JObject();
                formatOptionsJson.Add("unitLength", "SquareMeters");
                formatOptionsJson.Add("unitArea", "Meters");
                formatOptionsJson.Add("unitVolume", "CubicMeters");

                //过滤出需要处理的图元（有category和element的）
                List<JObject> modelElementUnitJsonList = new List<JObject>();
                for (int i = 0; i < modelUnitJsonList.Count; i++)
                {
                    JObject modelUnitJson = modelUnitJsonList[i];
                    string elementId = modelUnitJson.GetValue("elementId").ToString();
                    if (elementId2ParamterJsons.ContainsKey(elementId) && elementId2CatFamilyJsons.ContainsKey(elementId))
                    {
                        modelElementUnitJsonList.Add(modelUnitJson);
                    }
                }

                Dictionary<string, JObject> name2MaterialJsons = new Dictionary<string, JObject>();

                //输出的所有unit节点
                JArray unitMeshJArray = new JArray();
                Dictionary<string, int> unitKey2Index = new Dictionary<string, int>();

                //从所有节点提取所有unitmesh
                for (int i = 0; i < modelElementUnitJsonList.Count; i++)
                {
                    JObject modelElementUnitJson = modelElementUnitJsonList[i];
                    JArray meshJArray = modelElementUnitJson.GetValue("meshes") as JArray;
                    if(meshJArray == null)
                    {
                        //revit没有生成mesh，例如一棵树，这里不做处理
                    }
                    else if (meshJArray.Count == 1)
                    {
                        //只有一个Mesh，直接对应unit
                        JObject meshJson = meshJArray[0] as JObject;
                        JObject unitMeshJson = new JObject();
                        unitMeshJson.Add("type", "Mesh");
                        string pointsStr = meshJson.GetValue("points").ToString();
                        string facesStr = meshJson.GetValue("faces").ToString();
                        unitMeshJson.Add("points", pointsStr);
                        unitMeshJson.Add("faces", facesStr);
                        string unitKey = CommonFunction.GetMd5(pointsStr + ";" + facesStr);
                        if (!unitKey2Index.ContainsKey(unitKey))
                        {
                            unitKey2Index.Add(unitKey, unitMeshJArray.Count);
                            unitMeshJArray.Add(unitMeshJson);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < meshJArray.Count; j++)
                        {
                            //只有一个Mesh，直接对应unit
                            JObject meshJson = meshJArray[j] as JObject;
                            JObject unitMeshJson = new JObject();
                            unitMeshJson.Add("type", "Mesh");
                            string pointsStr = meshJson.GetValue("points").ToString();
                            string facesStr = meshJson.GetValue("faces").ToString();
                            unitMeshJson.Add("points", pointsStr);
                            unitMeshJson.Add("faces", facesStr);
                            string unitKey = CommonFunction.GetMd5(pointsStr + ";" + facesStr);
                            if (!unitKey2Index.ContainsKey(unitKey))
                            {
                                unitKey2Index.Add(unitKey, unitMeshJArray.Count);
                                unitMeshJArray.Add(unitMeshJson);
                            }
                        }
                    }
                }

                JArray nodeJArray = new JArray();

                //第一个node节点，root
                JObject rootNodeJson = new JObject();
                rootNodeJson.Add("name", "default");
                JArray rootChildJArray = new JArray();
                for (int i = 0; i < modelElementUnitJsonList.Count; i++)
                {
                    rootChildJArray.Add(i + 1);
                }
                rootNodeJson.Add("nodes", rootChildJArray);
                nodeJArray.Add(rootNodeJson);

                //各个Node的组成部分（对应一个mesh的unit）
                List<JObject> subNodeList = new List<JObject>();

                //所有节点
                for (int i = 0; i < modelElementUnitJsonList.Count; i++)
                {
                    JObject modelElementUnitJson = modelElementUnitJsonList[i];
                    string elementId = modelElementUnitJson.GetValue("elementId").ToString();
                    JObject parameterJson = elementId2ParamterJsons.ContainsKey(elementId) ? elementId2ParamterJsons[elementId] : null;
                    //string name = parameterJson == null ? elementId : (elementId + " " + parameterJson.GetValue("name").ToString());

                    //构造node节点
                    JObject nodeJson = new JObject();
                    nodeJson.Add("name", elementId);

                    JObject elementCategoryFamilyJson = elementId2CatFamilyJsons[elementId];
                    string revitCategoryName = (string)elementCategoryFamilyJson.GetValue("categoryName");
                    string revitFamilyName = (string)elementCategoryFamilyJson.GetValue("familyName");
                    string revitFamilySymbolName = (string)elementCategoryFamilyJson.GetValue("familySymbolName");

                    FamilyItem revitFamilyItem = null;
                    FamilySymbolItem revitFamilySymbolItem = null;
                    if (this.RevitPimMap.ContainsFamilyItem(revitCategoryName, revitFamilyName))
                    {
                        revitFamilyItem = this.RevitPimMap.GetFamilyItem(revitCategoryName, revitFamilyName);
                        nodeJson.Add("pimTypeCode", revitFamilyItem.PimTypeCode);
                        nodeJson.Add("pimTypeName", revitFamilyItem.PimTypeName);
                    }
                    if (this.RevitPimMap.ContainsFamilySymbolItem(revitCategoryName, revitFamilyName, revitFamilySymbolName))
                    {
                        revitFamilySymbolItem = this.RevitPimMap.GetFamilySymbolItem(revitCategoryName, revitFamilyName, revitFamilySymbolName);
                        nodeJson.Add("pimCom3dCode", revitFamilySymbolItem.PimCom3dCode);
                        nodeJson.Add("pimCom3dName", revitFamilySymbolItem.PimCom3dName);
                    }
                    
                    //属性
                    if (parameterJson != null)
                    {
                        JArray pvJArray = parameterJson.GetValue("parameters") as JArray;
                        for(int j = 0; j < pvJArray.Count; j++)
                        {
                            JObject pvJson = pvJArray[j].ToObject<JObject>();
                            string paramName = pvJson.GetValue("name").ToString(); 
                            if (revitFamilyItem != null && revitFamilyItem.ContainsProperty(paramName))
                            {
                                pvJson.Add("pimPropertyName", revitFamilyItem.GetPimPropertyName(paramName));
                            }
                        }

                        //增加所属层的属性
                        string levelId = parameterJson.GetValue("levelId").ToString();
                        JObject levelJson = new JObject();
                        levelJson.Add("name", "levelId");
                        levelJson.Add("value", "levelId");
                        pvJArray.Add(levelJson);
                        nodeJson.Add("parameters", pvJArray);
                    }

                    JArray meshJArray = modelElementUnitJson.GetValue("meshes") as JArray;
                    if (meshJArray == null)
                    {
                        //revit没有生成mesh，例如一棵树
                        nodeJson.Add("transform", modelElementUnitJson.GetValue("transform").ToString());
                    }
                    else if(meshJArray.Count == 1)
                    {
                        //只有一个Mesh，直接对应unit
                        JObject meshJson = meshJArray[0] as JObject;
                        nodeJson.Add("transform", meshJson.GetValue("transform").ToString());

                        string materialName = meshJson.GetValue("materialName").ToString();
                        JArray materialJArray = new JArray();
                        materialJArray.Add(materialName);
                        nodeJson.Add("materials", materialJArray);

                        string pointsStr = meshJson.GetValue("points").ToString();
                        string facesStr = meshJson.GetValue("faces").ToString();
                        string unitKey = CommonFunction.GetMd5(pointsStr + ";" + facesStr);
                        int unitIndex = unitKey2Index[unitKey];
                        nodeJson.Add("unit", unitIndex);

                        if (!name2MaterialJsons.ContainsKey(materialName))
                        {
                            JObject materialJson = new JObject();
                            materialJson.Add("name", materialName);
                            materialJson.Add("color", meshJson.GetValue("color").ToString());
                            materialJson.Add("opacity", double.Parse(meshJson.GetValue("opacity").ToString()));
                            materialJson.Add("roughness", double.Parse(meshJson.GetValue("roughness").ToString()));
                            materialJson.Add("metalness", double.Parse(meshJson.GetValue("metalness").ToString()));
                            name2MaterialJsons.Add(materialName, materialJson);
                        }
                    }
                    else
                    {
                        JArray subNodeNumsJArray = new JArray();
                        //多个mesh，嵌套一层node
                        for (int j = 0; j < meshJArray.Count; j++)
                        {
                            //只有一个Mesh，直接对应unit
                            JObject meshJson = meshJArray[j] as JObject;
                            JObject subNodeJson = new JObject();
                            subNodeJson.Add("name", "部分");
                            subNodeJson.Add("category", "Part");

                            subNodeJson.Add("transform", meshJson.GetValue("transform").ToString());

                            string materialName = meshJson.GetValue("materialName").ToString();
                            JArray materialJArray = new JArray();
                            materialJArray.Add(materialName);
                            subNodeJson.Add("materials", materialJArray);

                            string pointsStr = meshJson.GetValue("points").ToString();
                            string facesStr = meshJson.GetValue("faces").ToString();
                            string unitKey = CommonFunction.GetMd5(pointsStr + ";" + facesStr);
                            int unitIndex = unitKey2Index[unitKey];
                            subNodeJson.Add("unit", unitIndex);

                            subNodeList.Add(subNodeJson);
                            subNodeNumsJArray.Add(modelElementUnitJsonList.Count + subNodeList.Count);

                            if (!name2MaterialJsons.ContainsKey(materialName))
                            {
                                JObject materialJson = new JObject();
                                materialJson.Add("name", materialName);
                                materialJson.Add("color", meshJson.GetValue("color").ToString());
                                materialJson.Add("opacity", double.Parse(meshJson.GetValue("opacity").ToString()));
                                materialJson.Add("roughness", double.Parse(meshJson.GetValue("roughness").ToString()));
                                materialJson.Add("metalness", double.Parse(meshJson.GetValue("metalness").ToString()));
                                name2MaterialJsons.Add(materialName, materialJson);
                            }
                        }
                        nodeJson.Add("nodes", subNodeNumsJArray);
                    }
                    nodeJArray.Add(nodeJson);
                }

                for (int i = 0; i < subNodeList.Count; i++)
                {
                    nodeJArray.Add(subNodeList[i]);
                }
                modelJson.Add("nodes", nodeJArray);

                modelJson.Add("units", unitMeshJArray);

                JObject materialMapJson = new JObject();
                foreach (string name in name2MaterialJsons.Keys)
                {
                    JObject materialJson = name2MaterialJsons[name];
                    materialMapJson.Add(name, materialJson);
                }
                modelJson.Add("materials", materialMapJson);

                //附加信息
                JObject additionsJson = new JObject();
                additionsJson.Add("modelTree", json3DContainer.modelTree);
                modelJson.Add("additions", additionsJson);

                File.WriteAllText(Path.Combine(dirPath, fileName + Json3DConverter.FileExtension), modelJson.ToString(), new System.Text.UTF8Encoding(false));
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ProcessUnitNode
        /// <summary>
        /// 获取单个对象的信息，存储到json里，方便后续生成Json3D使用
        /// </summary>
        /// <param name="json3DNode"></param>
        /// <param name="json3DContainer"></param>
        /// <param name="rootJson"></param>
        /// <param name="unitJsonList"></param>
        /// <param name="parentTransform"></param>
        private void ProcessUnitNode(Json3DNode json3DNode, Json3DContainer json3DContainer, JObject rootJson, List<JObject> unitJsonList, Transform parentTransform)
        {
            Transform nodeTransform = parentTransform == null ? json3DNode.transform : (json3DNode.transform == null ? parentTransform : parentTransform.Multiply(json3DNode.transform));
            json3DNode.xTransform = nodeTransform;

            List<JObject> meshJsonList = new List<JObject>();
            List<JObject> meshJsons = this.ProcessNodeMesh(json3DNode.mesh, json3DNode, json3DContainer);
            meshJsonList.AddRange(meshJsons.ToArray());
            List <JObject> groupMeshJsons = this.ProcessGroupNodeChildren(json3DNode, json3DContainer, nodeTransform);
            meshJsonList.AddRange(groupMeshJsons.ToArray());

            if (meshJsonList.Count > 0)
            {
                JArray meshJArray = new JArray(); 
                for (int i = 0; i < meshJsonList.Count; i++)
                {
                    meshJArray.Add(meshJsonList[i]);
                }  

                //创建一个unit，包含mesh
                JObject unitJson = new JObject();
                string unitId = Guid.NewGuid().ToString();
                string unitName = json3DNode.name;
                unitJson.Add("id", unitId);
                unitJson.Add("elementId", unitName);
                unitJson.Add("meshes", meshJArray); 
                unitJsonList.Add(unitJson);
            }
            else
            {
                //不包含mesh的，例如一棵树 
                int childNodeIndex = json3DNode.children[0];
                Json3DNode childJson3DNode = json3DContainer.json3D.nodes[childNodeIndex];

                JObject unitJson = new JObject();
                string unitId = Guid.NewGuid().ToString();
                string unitName = json3DNode.name;
                unitJson.Add("id", unitId);
                unitJson.Add("elementId", unitName);

                List<double> xForm = Json3DExportManagerUtils.ConvertXForm(childJson3DNode.xTransform);
                string xFormStr = this.GetTranformString(xForm);
                unitJson.Add("transform", xFormStr);

                unitJsonList.Add(unitJson);
            }
        }
        #endregion

        #region ProcessNodeMesh
        /// <summary>
        /// 获取单个Mesh信息
        /// </summary>
        /// <param name="meshIndex"></param>
        /// <param name="json3DNode"></param>
        /// <param name="json3DContainer"></param>
        /// <returns></returns>
        private List<JObject> ProcessNodeMesh(int? meshIndex, Json3DNode json3DNode, Json3DContainer json3DContainer)
        {
            List<JObject> meshJsons = new List<JObject>();
            if(meshIndex != null)
            {
                Json3DMesh Json3DMesh = json3DContainer.json3D.meshes[(int)meshIndex];
                for(int i = 0; i < Json3DMesh.primitives.Count; i++)
                {
                    Json3DMeshPrimitive Json3DMeshPrimitive = Json3DMesh.primitives[i];
                    Json3DAttribute Json3DAttribute = Json3DMeshPrimitive.attributes;
                    Json3DMaterial Json3DMaterial = json3DContainer.json3D.materials[(int)Json3DMeshPrimitive.material];
                    Json3DAccessor Json3DAccessor = json3DContainer.json3D.accessors[Json3DMeshPrimitive.indices];
                    Json3DBufferView Json3DBufferView = json3DContainer.json3D.bufferViews[Json3DAccessor.bufferView];
                    Json3DBuffer Json3DBuffer = json3DContainer.json3D.buffers[Json3DBufferView.buffer];
                    Json3DBinaryData Json3DBinaryData = json3DContainer.binaryDic[Json3DBuffer.uri];
                    JObject meshJson = this.GetMeshJson(Json3DMaterial, json3DNode, Json3DBinaryData); 
                    meshJsons.Add(meshJson);
                }
            }
            return meshJsons;
        }
        #endregion

        #region GetMeshJson
        /// <summary>
        /// 将单个mesh生成json，方便后续生成Json3D的几何信息
        /// </summary>
        /// <param name="json3DMaterial"></param>
        /// <param name="json3DNode"></param>
        /// <param name="Json3DBinaryData"></param>
        /// <returns></returns>
        private JObject GetMeshJson(Json3DMaterial json3DMaterial, Json3DNode json3DNode, Json3DBinaryData json3DBinaryData)
        {
            Json3DBinaryBufferContents json3DBinaryBufferContents = json3DBinaryData.contents;

            StringBuilder pointsStr = new StringBuilder(); 
            for (int i = 0; i < json3DBinaryBufferContents.vertexBuffer.Count; i = i + 3)
            {
                if (i > 0)
                {
                    pointsStr.Append(",");
                }
                double x = json3DBinaryBufferContents.vertexBuffer[i];
                double y = json3DBinaryBufferContents.vertexBuffer[i + 1];
                double z = json3DBinaryBufferContents.vertexBuffer[i + 2];  

                pointsStr.Append( x.ToString(SysConfig.DecimalFormatString) + " " + y.ToString(SysConfig.DecimalFormatString) + " " + z.ToString(SysConfig.DecimalFormatString)); 

            }

            StringBuilder facesStr = new StringBuilder();
            for (int i = 0; i < json3DBinaryBufferContents.indexBuffer.Count; i = i + 3)
            {
                if (i > 0)
                {
                    facesStr.Append(",");
                }
                int a = json3DBinaryBufferContents.indexBuffer[i];
                int b = json3DBinaryBufferContents.indexBuffer[i + 1];
                int c = json3DBinaryBufferContents.indexBuffer[i + 2];
                facesStr.Append(a + " " + b + " " + c);
            }

            List<double> xForm = Json3DExportManagerUtils.ConvertXForm(json3DNode.xTransform);
            string xFormStr = this.GetTranformString(xForm);

            string materialName = json3DMaterial.name;
            string color = this.GetColorRGB(json3DMaterial);
            float opacity = json3DMaterial.pbrMetallicRoughness.baseColorFactor[3] * 100;
            float roughness = json3DMaterial.pbrMetallicRoughness.roughnessFactor * 100;
            float metalness = json3DMaterial.pbrMetallicRoughness.metallicFactor * 100;

            JObject meshJson = new JObject();
            meshJson.Add("points", pointsStr.ToString());
            meshJson.Add("faces", facesStr.ToString());
            meshJson.Add("materialName", materialName);
            meshJson.Add("color", color);
            meshJson.Add("opacity", opacity);
            meshJson.Add("roughness", roughness);
            meshJson.Add("metalness", metalness);
            meshJson.Add("transform", xFormStr); 
            return meshJson;
        }
        #endregion

        #region GetMeshJson
        /// <summary>
        /// transform数组转字符串
        /// </summary>
        /// <param name="xForm"></param>
        /// <returns></returns>
        private string GetTranformString(List<double> xForm)
        {
            string xFormStr = xForm == null ? "" : ("((" + xForm[0] + ", " + xForm[1] + ", " + xForm[2] + ", " + xForm[3] + "), (" + xForm[4] + ", " + xForm[5] + ", " + xForm[6] + ", " + xForm[7] + "), (" + xForm[8] + ", " + xForm[9] + ", " + xForm[10] + ", " + xForm[11] + "), (" + xForm[12] + ", " + xForm[13] + ", " + xForm[14] + ", " + xForm[15] + "))");
            return xFormStr;
        }
        #endregion

        #region GetMeshJson
        /// <summary>
        /// 从Json3DMaterial里获取rgb十六进制值
        /// </summary>
        /// <param name="json3DMaterial"></param>
        /// <returns></returns>
        private string GetColorRGB(Json3DMaterial json3DMaterial)
        {
            int r = (int)Math.Round(json3DMaterial.pbrMetallicRoughness.baseColorFactor[0] * 255);
            int g = (int)Math.Round(json3DMaterial.pbrMetallicRoughness.baseColorFactor[1] * 255);
            int b = (int)Math.Round(json3DMaterial.pbrMetallicRoughness.baseColorFactor[2] * 255);
            return Convert.ToString(r, 16).PadLeft(2, '0') + Convert.ToString(g, 16).PadLeft(2, '0') + Convert.ToString(b, 16).PadLeft(2, '0');
        }
        #endregion

        #region ProcessGroupNodeChildren
        /// <summary>
        /// 组内的节点处理（例如对象的mesh节点）
        /// </summary>
        /// <param name="groupJson3DNode"></param>
        /// <param name="json3DContainer"></param>
        /// <param name="parentTransform"></param>
        /// <returns></returns>
        private List<JObject> ProcessGroupNodeChildren(Json3DNode groupJson3DNode, Json3DContainer json3DContainer, Transform parentTransform)
        {
            List<JObject> groupMeshJsons = new List<JObject>();
            List<int> childrenIndices = groupJson3DNode.children;
            if (childrenIndices != null)
            {
                for (int i = 0; i < childrenIndices.Count; i++)
                {
                    int nodeIndex = childrenIndices[i];
                    Json3DNode json3DNode = json3DContainer.json3D.nodes[nodeIndex];

                    Transform nodeTransform = parentTransform == null ? json3DNode.transform : (json3DNode.transform == null ? parentTransform : parentTransform.Multiply(json3DNode.transform));
                    json3DNode.xTransform = nodeTransform;

                    List<JObject> meshJsons = this.ProcessNodeMesh(json3DNode.mesh, json3DNode, json3DContainer);
                    groupMeshJsons.AddRange(meshJsons.ToArray());
                    List<JObject> subGroupMeshJsons = this.ProcessGroupNodeChildren(json3DNode, json3DContainer, nodeTransform);
                    groupMeshJsons.AddRange(subGroupMeshJsons.ToArray());
                }
            }
            return groupMeshJsons;
        }
        #endregion 
    }
}
