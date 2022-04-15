using XpimRevitAddIn.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace XpimRevitAddIn.Export.Xpim
{
    /// <summary>
    /// XPIM文件处理类
    /// </summary>
    public class XpimProcessor
    {
        #region GenerateXpim
        /// <summary>
        /// 使用pim文件压缩生成xpim文件
        /// </summary>
        /// <param name="lods"></param>
        /// <param name="pimFiles"></param>
        /// <param name="xpimTempDir"></param>
        /// <param name="xpimFilePath"></param>
        public void GenerateXpim(string[] lods, Dictionary<string, string> pimFiles, string xpimTempDir, string xpimFilePath)
        {
            string mainPimFilePath = pimFiles[lods[lods.Length - 1]];

            JObject meshesJson = new JObject();
            JObject parametersJson = new JObject();

            //读取pim json
            string pimText = FileHelper.GetTextFromFile(mainPimFilePath, Encoding.UTF8);
            JObject pimJson = JObject.Parse(pimText);

            //构造xpim的json
            List<JObject> allXpimUnitJsons = new List<JObject>();
            JObject xpimJson = this.CreateNewObject(pimJson, parametersJson, lods, allXpimUnitJsons);

            //构造tree json
            JObject treeJson = this.CreateTreeJson(pimJson);


            //按照lod级别构造对应的mesh文件
            Dictionary<string, string> meshKey2Guid = new Dictionary<string, string>();
            for (int i = 0; i < lods.Length; i++)
            {
                string lod = lods[i];
                string pimFilePath = pimFiles[lod];
                this.CreateLodMesh(xpimJson, lod, pimFilePath, meshesJson, meshKey2Guid, allXpimUnitJsons);
            }

            //units
            JArray xpimUnitJArray = new JArray();
            for(int i = 0; i < allXpimUnitJsons.Count; i++)
            {
                xpimUnitJArray.Add(allXpimUnitJsons[i]);
            }
            xpimJson.Add("units", xpimUnitJArray);

            //lods
            JArray xpimLodJArray = new JArray();
            for (int i = 0; i < lods.Length; i++)
            {
                xpimLodJArray.Add(lods[i]);
            }
            xpimJson.Add("lods", xpimLodJArray);

            //保存
            string xpimTempFilePath = Path.Combine(xpimTempDir, "root.pim");
            string xpimText = xpimJson.ToString();
            FileHelper.SaveTextToFile(xpimText, xpimTempFilePath, Encoding.UTF8);
            string meshesTempFilePath = Path.Combine(xpimTempDir, "meshes");
            string meshesText = meshesJson.ToString();
            FileHelper.SaveTextToFile(meshesText, meshesTempFilePath, Encoding.UTF8);
            string parametersTempFilePath = Path.Combine(xpimTempDir, "parameters");
            string parametersText = parametersJson.ToString();
            FileHelper.SaveTextToFile(parametersText, parametersTempFilePath, Encoding.UTF8);
            string treeTempFilePath = Path.Combine(xpimTempDir, "tree");
            string treeText = treeJson.ToString();
            FileHelper.SaveTextToFile(treeText, treeTempFilePath, Encoding.UTF8);

            //压缩,
            //生成32位的文件，为了兼容其它软件
            FileHelper.ZipDirectory(xpimTempDir, xpimFilePath, UseZip64.Off);
        }
        #endregion

        #region CreateLodMesh
        /// <summary>
        /// 创建不同lod级别的mesh文件
        /// </summary>
        /// <param name="xpimJson"></param>
        /// <param name="lod"></param>
        /// <param name="pimFilePath"></param>
        /// <param name="meshesJson"></param>
        /// <param name="meshKey2Guid"></param>
        /// <param name="allXpimUnitJsons"></param>
        private void CreateLodMesh(JObject xpimJson, string lod, string pimFilePath, JObject meshesJson, Dictionary<string,string> meshKey2Guid, List<JObject> allXpimUnitJsons)
        {
            string pimText = FileHelper.GetTextFromFile(pimFilePath, Encoding.UTF8);
            JObject pimJson = JObject.Parse(pimText);
            JArray pimUnitJArray = pimJson.GetValue("units").ToObject<JArray>(); 
            for (int i = 0;i <  pimUnitJArray.Count; i++)
            {
                JObject pimUnitJson = pimUnitJArray[i].ToObject<JObject>();

                string pimUnitText = pimUnitJson.ToString();
                string meshKey = CommonFunction.GetMd5(pimUnitText);
                string guid = null;
                string meshFileUrl = null;

                if (meshKey2Guid.ContainsKey(meshKey))
                {
                    guid = meshKey2Guid[meshKey];
                    meshFileUrl = guid;
                }
                else
                {
                    guid = Guid.NewGuid().ToString();
                    meshKey2Guid.Add(meshKey, guid);
                    meshFileUrl = guid;
                    meshesJson.Add(guid, pimUnitJson);
                }

                JObject xpimUnitJson = allXpimUnitJsons[i];
                xpimUnitJson.Add(lod, meshFileUrl);
            }
        }
        #endregion

        #region CreateNewObject
        /// <summary>
        /// 创建新的JSON对象
        /// </summary>
        /// <param name="pimFilePath"></param>
        /// <param name="parametersJson"></param> 
        /// <param name="lods"></param>
        /// <param name="allXpimUnitJsons"></param>
        /// <returns></returns>
        private JObject CreateNewObject(JObject pimJson, JObject parametersJson, string[] lods, List<JObject> allXpimUnitJsons)
        {
            JObject xpimJson = new JObject();
            xpimJson.Add("version", pimJson.GetValue("version"));
            xpimJson.Add("upAxis", pimJson.GetValue("upAxis"));

            //materials
            xpimJson.Add("materials", pimJson.GetValue("materials"));

            //parameters map
            Dictionary<string, string> parameterKey2Guid = new Dictionary<string, string>();

            //nodes
            JArray pimNodes = pimJson.GetValue("nodes").ToObject<JArray>();
            JArray xpimNodes = new JArray();
            for (int i = 0; i < pimNodes.Count; i++)
            {
                JObject pimNode = pimNodes[i].ToObject<JObject>();
                JObject xpimNode = new JObject();

                //name
                xpimNode.Add("name", pimNode.GetValue("name"));

                //nodes
                if (pimNode.ContainsKey("nodes"))
                {
                    xpimNode.Add("nodes", pimNode.GetValue("nodes"));
                }

                //unit
                if (pimNode.ContainsKey("unit"))
                {
                    xpimNode.Add("unit", pimNode.GetValue("unit"));
                }

                //transform
                if (pimNode.ContainsKey("transform"))
                {
                    xpimNode.Add("transform", pimNode.GetValue("transform"));
                }

                //materials
                if (pimNode.ContainsKey("materials"))
                {
                    xpimNode.Add("materials", pimNode.GetValue("materials"));
                }

                //pimTypeCode
                if (pimNode.ContainsKey("pimTypeCode"))
                {
                    xpimNode.Add("pimTypeCode", pimNode.GetValue("pimTypeCode"));
                    xpimNode.Add("pimTypeName", pimNode.GetValue("pimTypeName"));
                }

                //parameters
                if (pimNode.ContainsKey("parameters"))
                {
                    JArray parameterJArray = pimNode["parameters"].ToObject<JArray>();
                    string parameterText = parameterJArray.ToString();
                    string pKey = CommonFunction.GetMd5(parameterText);
                    string guid = null;
                    string parameterFileUrl = null;

                    if (parameterKey2Guid.ContainsKey(pKey))
                    {
                        guid = parameterKey2Guid[pKey];
                        parameterFileUrl = guid;
                    }
                    else
                    {
                        guid = Guid.NewGuid().ToString();
                        parameterKey2Guid.Add(pKey, guid);
                        parameterFileUrl = guid;
                        parametersJson.Add(guid, parameterJArray); 
                    }
                    xpimNode.Add("paramUrl", parameterFileUrl);
                }
                xpimNodes.Add(xpimNode);
            }
            xpimJson.Add("nodes", xpimNodes);

            //units
            JArray pimUnitJArray = pimJson.GetValue("units").ToObject<JArray>(); 
            for(int i = 0; i < pimUnitJArray.Count; i++)
            {
                JObject xpimUnitJson = new JObject();
                xpimUnitJson.Add("type", "LodUrl"); 
                allXpimUnitJsons.Add(xpimUnitJson);
            } 

            return xpimJson;
        }
        #endregion

        #region 排序
        private List<JObject> Sort(JArray jsons, string parameterName)
        {
            List<JObject> jsonList = new List<JObject>();
            for(int i = 0; i < jsons.Count; i++)
            {
                JObject json = jsons[i].ToObject<JObject>();
                string paramValue = json.GetValue(parameterName).ToString();
                int insertIndex = 0;
                for(int j = 0; j < jsonList.Count; j++)
                {
                    JObject tempJson = jsonList[j];
                    string tempValue = tempJson.GetValue(parameterName).ToString();
                    if (tempValue.CompareTo(paramValue) < 0)
                    {
                        insertIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                jsonList.Insert(insertIndex, json);
            }
            return jsonList;
        }
        #endregion

        #region 构造tree json
        private JObject CreateTreeJson(JObject pimJson)
        {
            JObject treeRootJson = new JObject();
            JArray treeLevelJArray = new JArray();

            JArray pimNodes = pimJson.GetValue("nodes").ToObject<JArray>();
            JObject rootPimNodeJson = pimNodes[0].ToObject<JObject>();
            JArray childNodeIndexes = rootPimNodeJson.GetValue("nodes").ToObject<JArray>();
            Dictionary<string, bool> allGeoElementMap = new Dictionary<string, bool>();
            for(int i = 0; i < childNodeIndexes.Count; i++)
            {
                int nodeIndex = int.Parse(childNodeIndexes[i].ToString());
                JObject nodeJson = pimNodes[nodeIndex].ToObject<JObject>();
                if (nodeJson.ContainsKey("nodes") || nodeJson.ContainsKey("unit"))
                {
                    string elementId = nodeJson.GetValue("name").ToString();
                    allGeoElementMap.Add(elementId, true);
                }
            }

            JObject additionsJson = pimJson.GetValue("additions").ToObject<JObject>();
            JObject modelTreeJson = additionsJson.GetValue("modelTree").ToObject<JObject>();
            List<JObject> levelJArray = this.Sort(modelTreeJson.GetValue("levels").ToObject<JArray>(), "name");
            List<JObject> categoryJArray = this.Sort(modelTreeJson.GetValue("categories").ToObject<JArray>(), "name");
            List<JObject> familySymbolJArray = this.Sort(modelTreeJson.GetValue("familySymbols").ToObject<JArray>(), "name");
            List<JObject> elementJArray = this.Sort(modelTreeJson.GetValue("elements").ToObject<JArray>(), "name");

            Dictionary<string, Dictionary<string, Dictionary<string, List<JObject>>>> level2Category2FamilySymbol2ElementMap = new Dictionary<string, Dictionary<string, Dictionary<string, List<JObject>>>>();
            for(int i = 0; i < elementJArray.Count; i++)
            {
                JObject elementJson = elementJArray[i];
                string elementId = elementJson.GetValue("id").ToString();
                string familySymbolId = elementJson.GetValue("familySymbolId").ToString();

                if (allGeoElementMap.ContainsKey(elementId) && familySymbolId.Length > 0)
                {
                    string levelId = elementJson.GetValue("levelId").ToString();
                    string categoryId = elementJson.GetValue("categoryId").ToString();
                    if (!level2Category2FamilySymbol2ElementMap.ContainsKey(levelId))
                    {
                        level2Category2FamilySymbol2ElementMap.Add(levelId, new Dictionary<string, Dictionary<string, List<JObject>>>());
                    }
                    Dictionary<string, Dictionary<string, List<JObject>>> category2FamilySymbol2ElementMap = level2Category2FamilySymbol2ElementMap[levelId];
                    if (!category2FamilySymbol2ElementMap.ContainsKey(categoryId))
                    {
                        category2FamilySymbol2ElementMap.Add(categoryId, new Dictionary<string, List<JObject>>());
                    }
                    Dictionary<string, List<JObject>> familySymbol2ElementMap = category2FamilySymbol2ElementMap[categoryId];
                    if (!familySymbol2ElementMap.ContainsKey(familySymbolId))
                    {
                        familySymbol2ElementMap.Add(familySymbolId, new List<JObject>());
                    }
                    List<JObject> elementList = familySymbol2ElementMap[familySymbolId];
                    elementList.Add(elementJson);
                }
            }

            for(int i = 0; i < levelJArray.Count; i++)
            {
                JObject levelJson = levelJArray[i];
                string levelId = levelJson.GetValue("id").ToString();
                if (level2Category2FamilySymbol2ElementMap.ContainsKey(levelId))
                {
                    string levelName = levelJson.GetValue("name").ToString();
                    JObject treeLevelJson = new JObject();
                    treeLevelJson.Add("id", levelId);
                    treeLevelJson.Add("name", levelName);

                    JArray levelChildrenJArray = new JArray();

                    Dictionary<string, Dictionary<string, List<JObject>>> category2FamilySymbol2ElementMap = level2Category2FamilySymbol2ElementMap[levelId];
                    for (int j = 0; j < categoryJArray.Count; j++)
                    {
                        JObject categoryJson = categoryJArray[j];
                        string categoryId = categoryJson.GetValue("id").ToString();
                        if (category2FamilySymbol2ElementMap.ContainsKey(categoryId))
                        {
                            string categoryName = categoryJson.GetValue("name").ToString();
                            JObject treeCategoryJson = new JObject();
                            treeCategoryJson.Add("id", levelId + "_" + categoryId);
                            treeCategoryJson.Add("name", categoryName);

                            JArray categoryChildrenJArray = new JArray();

                            Dictionary<string, List<JObject>> familySymbol2ElementMap = category2FamilySymbol2ElementMap[categoryId];
                            for( int k = 0; k < familySymbolJArray.Count; k++)
                            {
                                JObject familySymbolJson = familySymbolJArray[k];
                                string familySymbolId = familySymbolJson.GetValue("id").ToString();
                                if (familySymbol2ElementMap.ContainsKey(familySymbolId))
                                {
                                    string familySymbolName = familySymbolJson.GetValue("name").ToString();
                                    JObject treeFamilySymbolJson = new JObject();
                                    treeFamilySymbolJson.Add("id", levelId + "_" + categoryId + "_" + familySymbolId);
                                    treeFamilySymbolJson.Add("name", familySymbolName);

                                    JArray familySymbolChildrenJArray = new JArray();

                                    List<JObject> elementJsons = familySymbol2ElementMap[familySymbolId];
                                    foreach(JObject elementJson in elementJsons)
                                    {
                                        string elementId = elementJson.GetValue("id").ToString();
                                        string elementName = elementJson.GetValue("name").ToString();
                                        JObject treeElementJson = new JObject();
                                        treeElementJson.Add("id", elementId);
                                        treeElementJson.Add("name", elementId); 
                                        familySymbolChildrenJArray.Add(treeElementJson);
                                    }
                                    treeFamilySymbolJson.Add("children", familySymbolChildrenJArray);
                                    categoryChildrenJArray.Add(treeFamilySymbolJson);
                                }
                            }
                            treeCategoryJson.Add("children", categoryChildrenJArray);
                            levelChildrenJArray.Add(treeCategoryJson);
                        }
                    }
                    treeLevelJson.Add("children", levelChildrenJArray);
                    treeLevelJArray.Add(treeLevelJson);
                }
            }
            treeRootJson.Add("children", treeLevelJArray);
            return treeRootJson;
        }
        #endregion
    }
}
