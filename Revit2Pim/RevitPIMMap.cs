using XpimRevitAddIn.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Revit2Pim
{
    /// <summary>
    /// revit与pim对照
    /// </summary>
    public class RevitPimMap
    {
        #region Name
        private string _Name = "";
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }
        #endregion

        #region FamilyMap
        private Dictionary<string, FamilyItem> _FamilyMap = null;
        public bool ContainsFamilyItem(string categoryName, string familyName)
        {
            string key = categoryName + "." + familyName;
            return this._FamilyMap.ContainsKey(key);
        }
        public FamilyItem GetFamilyItem(string categoryName, string familyName)
        {
            string key = categoryName + "." + familyName;
            return _FamilyMap[key];
        }
        #endregion

        #region FamilySymbolMap
        private Dictionary<string, FamilySymbolItem> _FamilySymbolMap = null;
        public bool ContainsFamilySymbolItem(string categoryName, string familyName, string familySymbolName)
        {
            string key = categoryName + "." + familyName + "." + familySymbolName;
            return this._FamilySymbolMap.ContainsKey(key);
        }
        public FamilySymbolItem GetFamilySymbolItem(string categoryName, string familyName, string familySymbolName)
        {
            string key = categoryName + "." + familyName + "." + familySymbolName;
            return _FamilySymbolMap[key];
        }
        #endregion

        #region Families

        private List<FamilyItem> _Families = new List<FamilyItem>();
        public List<FamilyItem> Families
        {
            get
            {
                return this._Families;
            }
            set
            {
                this._Families = value;
            }
        }
        #endregion

        #region FamilySymbols
        private List<FamilySymbolItem> _FamilySymbols = new List<FamilySymbolItem>();
        public List<FamilySymbolItem> FamilySymbols
        {
            get
            {
                return this._FamilySymbols;
            }
            set
            {
                this._FamilySymbols = value;
            }
        }
        #endregion

        #region Load
        public static RevitPimMap Load(string filePath)
        {
            string jsonText = FileHelper.GetTextFromFile(filePath, Encoding.UTF8);
            JObject json = JObject.Parse(jsonText);

            RevitPimMap rpm = new RevitPimMap();
            rpm.Name = json.GetValue("name").ToString();

            JArray familyJArray = json.GetValue("families").ToObject<JArray>();
            for (int i = 0; i < familyJArray.Count; i++)
            {
                JObject familyJson = familyJArray[i].ToObject<JObject>();
                FamilyItem familyItem = new FamilyItem();
                familyItem.PimTypeCode = familyJson.GetValue("pimTypeCode").ToString();
                familyItem.PimTypeName = familyJson.GetValue("pimTypeName").ToString();
                familyItem.RevitCategoryName = familyJson.GetValue("revitCategoryName").ToString();
                familyItem.RevitFamilyName = familyJson.GetValue("revitFamilyName").ToString();

                JArray propertyJArray = familyJson.GetValue("properties").ToObject<JArray>();
                for (int j = 0; j < propertyJArray.Count; j++)
                {
                    JObject propertyJson = propertyJArray[j].ToObject<JObject>();
                    FamilyPropertyItem familyPropertyItem = new FamilyPropertyItem();
                    familyPropertyItem.PimPropertyName = propertyJson.GetValue("pimPropertyName").ToString();
                    familyPropertyItem.RevitPropertyName = propertyJson.GetValue("revitPropertyName").ToString();
                    familyItem.Properties.Add(familyPropertyItem);
                }
                rpm.Families.Add(familyItem);
            }

            JArray familySymbolJArray = json.GetValue("familySymbols").ToObject<JArray>();
            for (int i = 0; i < familySymbolJArray.Count; i++)
            {
                JObject familySymbolJson = familySymbolJArray[i].ToObject<JObject>();
                FamilySymbolItem familySymbolItem = new FamilySymbolItem();
                familySymbolItem.PimCom3dCode = familySymbolJson.GetValue("pimCom3dCode").ToString();
                familySymbolItem.PimCom3dName = familySymbolJson.GetValue("pimCom3dName").ToString();
                familySymbolItem.RevitCategoryName = familySymbolJson.GetValue("revitCategoryName").ToString();
                familySymbolItem.RevitFamilyName = familySymbolJson.GetValue("revitFamilyName").ToString();
                familySymbolItem.RevitFamilySymbolName = familySymbolJson.GetValue("revitFamilySymbolName").ToString();
                rpm.FamilySymbols.Add(familySymbolItem);
            }
            rpm.InitMap();
            return rpm;
        }
        #endregion

        #region InitMap
        private void InitMap()
        {
            this._FamilyMap = new Dictionary<string, FamilyItem>();
            for (int i = 0; i < this.Families.Count; i++)
            {
                FamilyItem familyItem = this.Families[i];
                string key = familyItem.RevitCategoryName + "." + familyItem.RevitFamilyName;
                this._FamilyMap.Add(key, familyItem);
            }
            this._FamilySymbolMap = new Dictionary<string, FamilySymbolItem>();
            for (int i = 0; i < this.FamilySymbols.Count; i++)
            {
                FamilySymbolItem familySymbolItem = this.FamilySymbols[i];
                string key = familySymbolItem.RevitCategoryName + "." + familySymbolItem.RevitFamilyName + "." + familySymbolItem.RevitFamilySymbolName;
                this._FamilySymbolMap.Add(key, familySymbolItem);
            }
        }
        #endregion 
    }
}
