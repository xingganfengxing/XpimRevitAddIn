using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Common
{

    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter: 
    /// A vertex lookup class to eliminate 
    /// duplicate vertex definitions.
    /// </summary>
    public class VertexLookupMeterDouble : Dictionary<PointMeterDouble, int>
    {
        /// <summary>
        /// Define equality for integer-based PointInt.
        /// </summary>
        class PointMeterDoubleEqualityComparer : IEqualityComparer<PointMeterDouble>
        {
            public bool Equals(PointMeterDouble p, PointMeterDouble q)
            {
                return 0 == p.CompareTo(q);
            }

            public int GetHashCode(PointMeterDouble p)
            {
                return (p.X.ToString()
                  + "," + p.Y.ToString()
                  + "," + p.Z.ToString())
                  .GetHashCode();
            }
        }

        public VertexLookupMeterDouble() : base(new PointMeterDoubleEqualityComparer())
        {
        }

        /// <summary>
        /// Return the index of the given vertex,
        /// adding a new entry if required.
        /// </summary>
        public int AddVertex(PointMeterDouble p)
        {
            return ContainsKey(p)
              ? this[p]
              : this[p] = Count;
        }
    }
}
