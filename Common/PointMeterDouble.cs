using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Common
{

    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter:
    /// https://github.com/va3c/RvtVa3c
    /// An meter double-based 3D point class.
    /// </summary>
    public class PointMeterDouble : IComparable<PointMeterDouble>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// Consider a Revit length zero 
        /// if is smaller than this.
        /// </summary>
        const double _eps = 1.0e-13;

        /// <summary>
        /// Conversion factor from feet to metres.
        /// </summary>
        const double _feet_to_m = 25.4 * 12 / 1000;

        /// <summary>
        /// Conversion a given length value 
        /// from feet to metre.
        /// </summary>
        public static double ConvertFeetToMetres(double d)
        {
            if (0 < d)
            {
                return _eps > d
                  ? 0
                  : (double)Math.Round(_feet_to_m * d + 0.0005, 4);
            }
            else
            {
                return _eps > -d
                  ? 0
                  : (double)Math.Round(_feet_to_m * d - 0.0005, 4);
            }
        }

        public PointMeterDouble(XYZ p, bool switch_coordinates)
        {
            X = ConvertFeetToMetres(p.X);
            Y = ConvertFeetToMetres(p.Y);
            Z = ConvertFeetToMetres(p.Z);

            if (switch_coordinates)
            {
                X = -X;
                double tmp = Y;
                Y = Z;
                Z = tmp;
            }
        }

        public int CompareTo(PointMeterDouble a)
        {
            double d = X - a.X;
            if (0 == d)
            {
                d = Y - a.Y;
                if (0 == d)
                {
                    d = Z - a.Z;
                }
            }
            return (0 == d) ? 0 : ((0 < d) ? 1 : -1);
        }
    }
}
