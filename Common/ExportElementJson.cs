using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
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
    /// 输出给定元素的属性名和值
    /// </summary>
    public class ExportElementJson
    {
        #region GetElementParamterJsons
        /// <summary>
        /// 获取所有的元素属性名和值
        /// </summary>
        /// <param name="document"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static JArray GetElementParamterJsons(Document document, IList<Element> elements)
        {
            Transaction trans = null;

            try
            {
                trans = new Transaction(document);
                trans.Start("trans1");
                Units units = document.GetUnits();

                FormatOptions areaFormatOptions = new FormatOptions(DisplayUnitType.DUT_SQUARE_METERS, 0.0001);
                units.SetFormatOptions(UnitType.UT_Area, areaFormatOptions);

                FormatOptions lengthFormatOptions = new FormatOptions(DisplayUnitType.DUT_METERS, 0.01);
                units.SetFormatOptions(UnitType.UT_Length, lengthFormatOptions);

                FormatOptions volumeFormatOptions = new FormatOptions(DisplayUnitType.DUT_CUBIC_METERS, 0.000001);
                units.SetFormatOptions(UnitType.UT_Volume, volumeFormatOptions);

                document.SetUnits(units);

                JArray rootArray = new JArray();

                foreach (Element element in elements)
                {
                    JObject elementObj = new JObject();
                    string elementId = element.Id.ToString();
                    elementObj.Add("id", elementId);
                    elementObj.Add("name", element.Name);

                    JArray parameterArray = new JArray();
                    ParameterSet paramSet = element.Parameters;
                    String levelId = element.LevelId.ToString();

                    elementObj.Add("levelId", levelId);

                    foreach (Parameter param in paramSet)
                    {
                        JObject parameterObj = new JObject();
                        if (param.Definition != null)
                        {
                            string name = param.Definition.Name;
                            parameterObj.Add("name", name);
                            if (param.HasValue)
                            {
                                string value = "";
                                if (CheckNeedConvertValue(param.Definition.UnitType))
                                {
                                    value = param.AsValueString();
                                    //    double doubleValue = this.ConvertToMetric(displayUnit, param.Definition.UnitType, param.AsDouble());
                                    //    value = doubleValue.ToString();
                                }
                                else
                                {
                                    value = param.AsValueString();
                                    if (value == null || value.Length == 0)
                                    {
                                        value = param.AsString();
                                    }
                                }
                                parameterObj.Add("value", value);
                            }
                            parameterArray.Add(parameterObj);
                        }
                    }

                    elementObj.Add("parameters", parameterArray);
                    rootArray.Add(elementObj);
                }
                return rootArray;
            }
            catch (Exception ex)
            {
                throw new Exception("ExportElementJson failed." + ex.Message, ex);
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
            JArray rootArray = GetElementParamterJsons(document, elements);
            using (TextWriter tw = new StreamWriter(txtFilePath))
            {
                string jsonText = rootArray.ToString();
                tw.Write(jsonText);
                tw.Flush();
            }
        }
        #endregion

        #region CheckNeedConvertValue
        /// <summary>
        /// 判定需要保存的文件
        /// </summary>
        /// <param name="unitType"></param>
        /// <returns></returns>
        private static bool CheckNeedConvertValue(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.UT_Length:
                case UnitType.UT_Area:
                case UnitType.UT_Volume:
                    return true;
                default:
                    return false;
            }
        }
        #endregion  
    }
}