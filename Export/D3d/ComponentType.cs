using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{

    /// <summary>
    /// Magic numbers to differentiate array buffer component
    /// types.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#accessor-element-size
    /// </summary>
    public enum ComponentType
    {
        BYTE = 5120,
        UNSIGNED_BYTE = 5121,
        SHORT = 5122,
        UNSIGNED_SHORT = 5123,
        UNSIGNED_INT = 5125,
        FLOAT = 5126
    }
}
