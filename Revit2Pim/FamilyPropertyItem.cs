using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Revit2Pim
{
    /// <summary>
    /// FamilyPropertyItem
    /// </summary>
    public class FamilyPropertyItem
    {
        #region RevitPropertyName
        private string _RevitPropertyName;
        public string RevitPropertyName
        {
            get
            {
                return this._RevitPropertyName;
            }
            set
            {
                this._RevitPropertyName = value;
            }
        }
        #endregion

        #region PimPropertyName
        private string _PimPropertyName;
        public string PimPropertyName
        {
            get
            {
                return this._PimPropertyName;
            }
            set
            {
                this._PimPropertyName = value;
            }
        }
        #endregion
    }
}
