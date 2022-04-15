using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using XpimRevitAddIn.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace XpimRevitAddIn.Common
{
    /// <summary>
    /// 获取Revit族类型、族、类型、标高的信息及之间的对应关系
    /// </summary>
    public class ExportModelTreeJson
    {
        #region 未知标高名称
        private static string UnknownLevelId = "unknown";
        #endregion

        #region GetModelTreeJson
        /// <summary>
        /// 获取获取Revit族类型、族、类型、标高的信息及之间的对应关系
        /// </summary>
        /// <param name="document"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static JObject GetModelTreeJson(Document document, IList<Element> elements)
        {
            Transaction trans = null;

            try
            {
                trans = new Transaction(document);
                trans.Start("trans1");
                //这几种类型单独处理Level问题
                Dictionary<string, bool> partCategoryDic = new Dictionary<string, bool>();
                /*
                partCategoryDic.Add(Category.GetCategory(document, BuiltInCategory.OST_StairsRuns).Id.ToString(), true);
                partCategoryDic.Add(Category.GetCategory(document, BuiltInCategory.OST_StairsRailing).Id.ToString(), true);
                partCategoryDic.Add(Category.GetCategory(document, BuiltInCategory.OST_StairsLandings).Id.ToString(), true);
                */

                Dictionary<string, JObject> categoryDic = new Dictionary<string, JObject>();
                Categories allCategories = document.Settings.Categories;
                foreach (Category category in allCategories)
                {
                    AddCategoryToKVTree(document, category.Id.ToString(), category, categoryDic, "");
                    CategoryNameMap subCategories = category.SubCategories;
                    foreach (Category subCategory in subCategories)
                    {
                        AddCategoryToKVTree(document, subCategory.Id.ToString(), subCategory, categoryDic, category.Id.ToString());
                    }
                }

                Dictionary<string, JObject> levelDic = new Dictionary<string, JObject>();
                Dictionary<double, string> elevationToLevelIdDic = new Dictionary<double, string>();
                PlanTopologySet planTopologySet = document.PlanTopologies;
                foreach (PlanTopology planTopology in planTopologySet)
                {
                    Level level = planTopology.Level;
                    double levelElevation = level.Elevation;
                    string levelId = level.Id.ToString();
                    AddLevelToKVTree(document, levelId, level, levelDic);
                    if (!elevationToLevelIdDic.ContainsKey(levelElevation))
                    {
                        elevationToLevelIdDic.Add(levelElevation, levelId);
                    }
                }
                AddLevelToKVTree(document, UnknownLevelId, null, levelDic);
                elevationToLevelIdDic.Add(-1, UnknownLevelId);

                //获取材质
                Dictionary<string, JObject> materialDic = new Dictionary<string, JObject>();
                ICollection<ElementId> allMaterialIds = new FilteredElementCollector(document).OfClass(typeof(Material)).ToElementIds();
                if (allMaterialIds != null)
                {
                    foreach (ElementId materialId in allMaterialIds)
                    {
                        string materialIdStr = materialId.ToString();
                        if (!materialDic.ContainsKey(materialIdStr))
                        {
                            Material material = document.GetElement(materialId) as Material;
                            string materialName = material.Name;
                            String materialColor = CommonFunction.GetColorHex(material.Color);
                            JObject materialJson = new JObject();
                            materialJson.Add("id", materialIdStr);
                            materialJson.Add("name", materialName);
                            materialJson.Add("color", materialColor);
                            materialDic.Add(materialIdStr, materialJson);
                        }
                    }
                }


                //获取族
                FilteredElementCollector familyCollector = new FilteredElementCollector(document);
                Dictionary<string, JObject> familyDic = new Dictionary<string, JObject>();
                ElementClassFilter familyFitler = new ElementClassFilter(typeof(Family));
                ICollection<ElementId> allFamilyIds = familyCollector.WherePasses(familyFitler).ToElementIds();
                if (allFamilyIds != null)
                {
                    foreach (ElementId familyId in allFamilyIds)
                    {
                        string familyIdStr = familyId.ToString();
                        if (!familyDic.ContainsKey(familyIdStr))
                        {
                            Family family = document.GetElement(familyId) as Family;
                            string familyName = family.Name;
                            string categoryId = family.FamilyCategory == null ? null : family.FamilyCategory.Id.ToString();
                            JObject familyJson = new JObject();
                            familyJson.Add("id", familyIdStr);
                            familyJson.Add("name", familyName);
                            familyJson.Add("categoryId", categoryId);
                            familyDic.Add(familyIdStr, familyJson);
                        }
                    }
                }

                //获取族类型
                FilteredElementCollector familySymbolCollector = new FilteredElementCollector(document);
                Dictionary<string, JObject> familySymbolDic = new Dictionary<string, JObject>();
                ElementClassFilter familySymbolFitler = new ElementClassFilter(typeof(FamilySymbol)); 
                ICollection<ElementId> allFamilySymbolIds = familySymbolCollector.WherePasses(familySymbolFitler).ToElementIds();
                if (allFamilySymbolIds != null)
                {
                    foreach (ElementId familySymbolId in allFamilySymbolIds)
                    {
                        string familySymbolIdStr = familySymbolId.ToString();
                        if (!familySymbolDic.ContainsKey(familySymbolIdStr))
                        {
                            FamilySymbol familySymbol = document.GetElement(familySymbolId) as FamilySymbol;
                            string familySymbolName = familySymbol.Name;
                            string familyId = familySymbol.Family == null ? null : familySymbol.Family.Id.ToString();
                            JObject familySymbolJson = new JObject();
                            familySymbolJson.Add("id", familySymbolIdStr);
                            familySymbolJson.Add("name", familySymbolName);
                            familySymbolJson.Add("familyId", familyId);

                            familySymbolDic.Add(familySymbolIdStr, familySymbolJson);
                        }
                    }
                }

                Dictionary<string, JObject> elementDic = new Dictionary<string, JObject>();
                foreach (Element element in elements)
                {
                    string elementId = element.Id.ToString();
                    if (element.Category == null)
                    {

                    }
                    if (!elementDic.ContainsKey(elementId))
                    {
                        string familyId = null;
                        string familyName = null;
                        string familySymbolId = null;
                        string familySymbolName = null;
                        FamilySymbol familySymbol = null;
                        if (element is FamilyInstance)
                        {
                            familySymbol = (element as FamilyInstance).Symbol;
                            familySymbolName = familySymbol.Name;
                            familySymbolId = familySymbol.Id.ToString();
                            familyName = familySymbol.Family.Name;
                            familyId = familySymbol.Family.Id.ToString();
                        }
                        if (familySymbol == null)
                        {
                            ElementId typeId = element.GetTypeId();
                            if (typeId != null)
                            {
                                Element systemFamilySymbol = document.GetElement(typeId);
                                if (systemFamilySymbol != null)
                                {
                                    Parameter familyParameter = systemFamilySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                                    familyName = familyParameter == null ? null : familyParameter.AsString();

                                    familySymbolName = systemFamilySymbol.Name;
                                    familySymbolId = systemFamilySymbol.Id.ToString();

                                    if (!familySymbolDic.ContainsKey(familySymbolId))
                                    {
                                        string systemFamilySymbolName = systemFamilySymbol.Name;
                                        JObject familySymbolJson = new JObject();
                                        familySymbolJson.Add("id", familySymbolId);
                                        familySymbolJson.Add("name", systemFamilySymbolName);
                                        familySymbolJson.Add("familyId", familyId);

                                        familySymbolDic.Add(familySymbolId, familySymbolJson);
                                    }
                                }
                            }
                        }
                        AddElementToKVTree(document, elementId, element, element.Category, familyId, familyName, familySymbolId, familySymbolName, elementDic, elevationToLevelIdDic, categoryDic, partCategoryDic);
                    }
                }
                JObject rootObj = new JObject();

                JArray levelArray = new JArray();
                foreach (string levelId in levelDic.Keys)
                {
                    JObject levelObj = levelDic[levelId];
                    levelArray.Add(levelObj);
                }
                rootObj.Add("levels", levelArray);

                JArray categoryArray = new JArray();
                foreach (string categoryId in categoryDic.Keys)
                {
                    JObject categoryObj = categoryDic[categoryId];
                    categoryArray.Add(categoryObj);
                }
                rootObj.Add("categories", categoryArray);

                JArray elementArray = new JArray();
                foreach (string elementId in elementDic.Keys)
                {
                    JObject elementObj = elementDic[elementId];
                    elementArray.Add(elementObj);
                }
                rootObj.Add("elements", elementArray);

                JArray materialArray = new JArray();
                foreach (string materialId in materialDic.Keys)
                {
                    JObject materialObj = materialDic[materialId];
                    materialArray.Add(materialObj);
                }
                rootObj.Add("materials", materialArray);

                JArray familyArray = new JArray();
                foreach (string familyId in familyDic.Keys)
                {
                    JObject familyObj = familyDic[familyId];
                    familyArray.Add(familyObj);
                }
                rootObj.Add("families", familyArray);

                JArray familySymbolArray = new JArray();
                foreach (string familySymbolId in familySymbolDic.Keys)
                {
                    JObject familySymbolObj = familySymbolDic[familySymbolId];
                    familySymbolArray.Add(familySymbolObj);
                }
                rootObj.Add("familySymbols", familySymbolArray);
                return rootObj;
            }
            catch (Exception ex)
            {
                throw new Exception("ExportModelTreeJson failed." + ex.Message, ex);
            }
            finally
            {
                if (trans != null)
                {
                    trans.RollBack();
                    trans.Dispose();
                }
            }
        }
        #endregion

        #region SaveToJsonFile
        /// <summary>
        /// 保存为JSON文件
        /// </summary>
        /// <param name="document"></param>
        /// <param name="elements"></param>
        /// <param name="destDir"></param>
        /// <param name="fileName"></param>
        public static void SaveToJsonFile(Document document, IList<Element> elements, string destDir, string fileName)
        {

            string txtFilePath = Path.Combine(destDir, fileName + ".json");
            JObject rootObj = GetModelTreeJson(document, elements);
                rootObj.Add("name", fileName);

                using (TextWriter tw = new StreamWriter(txtFilePath))
                {
                    string rootStr = rootObj.ToString();
                    tw.Write(rootStr);
                    tw.Flush();
                }
        }
        #endregion

        #region AddLevelToKVTree
        /// <summary>
        /// 增加标高信息
        /// </summary>
        /// <param name="document"></param>
        /// <param name="levelId"></param>
        /// <param name="level"></param>
        /// <param name="levelDic"></param>
        private static void AddLevelToKVTree(Document document, string levelId, Level level, Dictionary<String, JObject> levelDic)
        {
            JObject levelObj = new JObject();
            levelObj.Add("id", levelId);
            levelObj.Add("name", level == null ? "未知层" : level.Name);
            levelDic.Add(levelId, levelObj);
        }
        #endregion

        #region AddCategoryToKVTree
        /// <summary>
        /// 增加族类型信息
        /// </summary>
        /// <param name="document"></param>
        /// <param name="categoryId"></param>
        /// <param name="category"></param>
        /// <param name="categoryDic"></param>
        /// <param name="parentCategoryId"></param>
        private static void AddCategoryToKVTree(Document document, string categoryId, Category category, Dictionary<String, JObject> categoryDic, string parentCategoryId)
        {
            if(category.CategoryType == CategoryType.Model && !categoryDic.ContainsKey(categoryId))
            {
                JObject categoryObj = new JObject();
                categoryObj.Add("id", categoryId);
                categoryObj.Add("name", category.Name);
                categoryObj.Add("type", category.CategoryType.ToString());
                categoryObj.Add("parentCategoryId", parentCategoryId); 
                categoryDic.Add(categoryId, categoryObj); 
            }
        }
        #endregion

        #region AddElementToKVTree
        /// <summary>
        /// 增加元素信息
        /// </summary>
        /// <param name="document"></param>
        /// <param name="elementId"></param>
        /// <param name="elementName"></param>
        /// <param name="category"></param>
        /// <param name="familyId"></param>
        /// <param name="familyName"></param>
        /// <param name="familySymbolId"></param>
        /// <param name="familySymbolName"></param>
        /// <param name="levelId"></param>
        /// <param name="elementDic"></param>
        private static void AddElementToKVTree(Document document, string elementId, string elementName, Category category, string familyId, string familyName, string familySymbolId, string familySymbolName, string levelId, Dictionary<String, JObject> elementDic)
        {
            if(category.CategoryType == CategoryType.Model)
            {
                JObject elementObj = new JObject();
                elementObj.Add("id", elementId);
                elementObj.Add("name", elementName);
                elementObj.Add("familyName", familyName == null ? null : familyName);
                elementObj.Add("familyId", familyId == null ? null : familyId);
                elementObj.Add("familySymbolName", familySymbolName == null ? null : familySymbolName);
                elementObj.Add("familySymbolId", familySymbolId == null ? null : familySymbolId);
                elementObj.Add("categoryName", category.Name);
                elementObj.Add("categoryId", category.Id.ToString());
                elementObj.Add("levelId", levelId); 
                elementDic.Add(elementId, elementObj);
            }
        }
        #endregion

        #region AddElementToKVTree
        /// <summary>
        /// 增加元素信息
        /// </summary>
        /// <param name="document"></param>
        /// <param name="elementId"></param>
        /// <param name="element"></param>
        /// <param name="category"></param>
        /// <param name="familyId"></param>
        /// <param name="familyName"></param>
        /// <param name="familySymbolId"></param>
        /// <param name="familySymbolName"></param>
        /// <param name="elementDic"></param>
        /// <param name="elevationToLevelId"></param>
        /// <param name="categoryDic"></param>
        /// <param name="partCategoryDic"></param>
        private static void AddElementToKVTree(Document document, string elementId, Element element, Category category, string familyId, string familyName, string familySymbolId, string familySymbolName, Dictionary<String, JObject> elementDic, Dictionary<double, string> elevationToLevelId, Dictionary<string, JObject> categoryDic, Dictionary<string, bool> partCategoryDic)
        {
            JObject elementObj = new JObject();
            elementObj.Add("id", elementId);
            elementObj.Add("categoryId", category.Id.ToString());
            elementObj.Add("familyId", familyId == null ? "": familyId.ToString());
            elementObj.Add("familySymbolId", familySymbolId == null ? "" : familySymbolId.ToString());

            string elementName = "";
            ParameterSet paramSet = element.Parameters;
            Dictionary<BuiltInParameter, Parameter> parameters = new Dictionary<BuiltInParameter, Parameter>();

            foreach (Parameter param in paramSet)
            {
                JObject parameterObj = new JObject();
                if (param.Definition != null)
                {
                    InternalDefinition definition = param.Definition as InternalDefinition;
                    if (definition.BuiltInParameter == BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)
                    {
                        elementName = param.AsValueString();
                    }
                    if (!parameters.ContainsKey(definition.BuiltInParameter))
                    {
                        parameters.Add(definition.BuiltInParameter, param);
                    }
                }

            }
            elementObj.Add("name", elementName); 
            if (element.LevelId != ElementId.InvalidElementId)
            { 
                string levelId = element.LevelId.ToString();
                AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
            }
            else
            {
                if (element.Category.CategoryType == CategoryType.Model && !partCategoryDic.ContainsKey(element.Category.Id.ToString()))
                {
                    if (element is Stairs)
                    {
                        /* 楼梯不做处理
                        Stairs stairs = element as Stairs;
                        double baseElevation = stairs.BaseElevation;
                        string levelId = elevationToLevelId.ContainsKey(baseElevation) ? elevationToLevelId[baseElevation] : UnknownLevelId;

                        AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic); 
                        */
                    }
                    else if (element is FamilyInstance)
                    {
                        FamilyInstance familyInstance = element as FamilyInstance;
                        if (familyInstance.Host is Level)
                        {
                            String levelId = (familyInstance.Host as Level).Id.ToString();
                            AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                        }
                        else
                        {
                            if (categoryDic.ContainsKey(category.Id.ToString()))
                            {
                                //不属于任意一层  
                                String levelId = UnknownLevelId;
                                AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                            }
                            else
                            {
                                //不记录此类
                            }
                        }
                    }
                    else if (category.Id.ToString() == Category.GetCategory(document, BuiltInCategory.OST_Ramps).Id.ToString())
                    {
                        string levelId = UnknownLevelId;
                        if (parameters.ContainsKey(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM))
                        {
                            double baseElevation = parameters[BuiltInParameter.STAIRS_BASE_LEVEL_PARAM].AsDouble();
                            levelId = elevationToLevelId.ContainsKey(baseElevation) ? elevationToLevelId[baseElevation] : UnknownLevelId;
                        }
                        AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                    }
                    else if (parameters.ContainsKey(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM))
                    {
                        double baseElevation = parameters[BuiltInParameter.STAIRS_BASE_LEVEL_PARAM].AsDouble();
                        string levelId = elevationToLevelId.ContainsKey(baseElevation) ? elevationToLevelId[baseElevation] : UnknownLevelId;
                        AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                    }
                    else if (parameters.ContainsKey(BuiltInParameter.STAIRS_RAILING_BASE_LEVEL_PARAM))
                    {
                        double baseElevation = parameters[BuiltInParameter.STAIRS_RAILING_BASE_LEVEL_PARAM].AsDouble();
                        string levelId = elevationToLevelId.ContainsKey(baseElevation) ? elevationToLevelId[baseElevation] : UnknownLevelId;
                        AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                    }
                    else if (parameters.ContainsKey(BuiltInParameter.STAIRS_LANDING_BASE_ELEVATION))
                    {
                        double baseElevation = parameters[BuiltInParameter.STAIRS_LANDING_BASE_ELEVATION].AsDouble();
                        string levelId = elevationToLevelId.ContainsKey(baseElevation) ? elevationToLevelId[baseElevation] : UnknownLevelId;
                        AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                    }
                    else if (parameters.ContainsKey(BuiltInParameter.STAIRS_RUN_BOTTOM_ELEVATION))
                    {
                        double baseElevation = parameters[BuiltInParameter.STAIRS_RUN_BOTTOM_ELEVATION].AsDouble();
                        string levelId = elevationToLevelId.ContainsKey(baseElevation) ? elevationToLevelId[baseElevation] : UnknownLevelId;
                        AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                    }
                    else
                    {
                        if (categoryDic.ContainsKey(category.Id.ToString()))
                        {
                            String levelId = UnknownLevelId;
                            AddElementToKVTree(document, elementId, elementName, category, familyId, familyName, familySymbolId, familySymbolName, levelId, elementDic);
                        }
                        else
                        {
                            //不记录此类
                        }
                    }
                }
                else
                {
                    //CategoryType不是Model
                }
            }
        }
        #endregion 
    }
}
