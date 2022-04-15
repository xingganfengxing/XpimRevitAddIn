using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Common
{
    /// <summary>
    /// 三维坐标最大最小值
    /// </summary>
    public class MinMaxXYZ
    {
        private double _MinX = double.MaxValue;
        private double _MinY = double.MaxValue;
        private double _MinZ = double.MaxValue;
        private double _MaxX = double.MinValue;
        private double _MaxY = double.MinValue;
        private double _MaxZ = double.MinValue;
        public double MinX
        {
            get
            {
                return this._MinX;
            }
            set
            {
                this._MinX = value;
            }
        }
        public double MinY
        {
            get
            {
                return this._MinY;
            }
            set
            {
                this._MinY = value;
            }
        }
        public double MinZ
        {
            get
            {
                return this._MinZ;
            }
            set
            {
                this._MinZ = value;
            }
        }
        public double MaxX
        {
            get
            {
                return this._MaxX;
            }
            set
            {
                this._MaxX = value;
            }
        }
        public double MaxY
        {
            get
            {
                return this._MaxY;
            }
            set
            {
                this._MaxY = value;
            }
        }
        public double MaxZ
        {
            get
            {
                return this._MaxZ;
            }
            set
            {
                this._MaxZ = value;
            }
        }
    }
}
