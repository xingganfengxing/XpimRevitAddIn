using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Revit2Pim
{
    /// <summary>
    /// FamilySymbolItem
    /// </summary>
    public class FamilySymbolItem
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

        #region RevitFamilySymbolName
        private string _RevitFamilySymbolName;
        public string RevitFamilySymbolName
        {
            get
            {
                return this._RevitFamilySymbolName;
            }
            set
            {
                this._RevitFamilySymbolName = value;
            }
        }
        #endregion

        #region PimCom3dCode
        private string _PimCom3dCode;
        public string PimCom3dCode
        {
            get
            {
                return this._PimCom3dCode;
            }
            set
            {
                this._PimCom3dCode = value;
            }
        }
        #endregion

        #region PimCom3dName
        private string _PimCom3dName;
        public string PimCom3dName
        {
            get
            {
                return this._PimCom3dName;
            }
            set
            {
                this._PimCom3dName = value;
            }
        }
        #endregion
    }
}
