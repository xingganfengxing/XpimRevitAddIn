using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Revit2Pim
{
    /// <summary>
    /// FamilyItem
    /// </summary>
    public class FamilyItem
    {
        #region RevitCategoryName
        private string _RevitCategoryName;
        public string RevitCategoryName
        {
            get
            {
                return this._RevitCategoryName;
            }
            set
            {
                this._RevitCategoryName = value;
            }
        }
        #endregion

        #region RevitFamilyName
        private string _RevitFamilyName;
        public string RevitFamilyName
        {
            get
            {
                return this._RevitFamilyName;
            }
            set
            {
                this._RevitFamilyName = value;
            }
        }
        #endregion

        #region PimTypeName
        private string _PimTypeName;
        public string PimTypeName
        {
            get
            {
                return this._PimTypeName;
            }
            set
            {
                this._PimTypeName = value;
            }
        }
        #endregion

        #region PimTypeCode
        private string _PimTypeCode;
        public string PimTypeCode
        {
            get
            {
                return this._PimTypeCode;
            }
            set
            {
                this._PimTypeCode = value;
            }
        }
        #endregion

        #region Properties
        private List<FamilyPropertyItem> _Properties = new List<FamilyPropertyItem>();
        public List<FamilyPropertyItem> Properties
        {
            get
            {
                return this._Properties;
            }
            set
            {
                this._Properties = value;
            }
        }
        #endregion

        #region propertyMap
        private Dictionary<string, string> _PropertyMap = null;

        private void InitPropertyMap()
        {
            Dictionary<string, string>  propertyMap = new Dictionary<string, string>();
            for(int i =0; i < this.Properties.Count; i++)
            {
                FamilyPropertyItem familyPropertyItem = this.Properties[i];
                propertyMap.Add(familyPropertyItem.RevitPropertyName, familyPropertyItem.PimPropertyName);
            }
            this._PropertyMap = propertyMap;

        }

        public bool ContainsProperty(string revitPropertyName)
        {
            if (this._PropertyMap == null)
            {
                this.InitPropertyMap();
            }
            return this._PropertyMap.ContainsKey(revitPropertyName);
        }
        #endregion

        #region GetPimPropertyName
        public string GetPimPropertyName(string revitPropertyName)
        {
            if (this._PropertyMap == null)
            {
                this.InitPropertyMap();
            }
            return this._PropertyMap[revitPropertyName];
        }
        #endregion
    }
}
