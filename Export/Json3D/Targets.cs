using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    /// <summary>
    /// Magic numbers to differentiate scalar and vector 
    /// array buffers.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#buffers-and-buffer-views
    /// </summary>
    public enum Targets
    {
        ARRAY_BUFFER = 34962, // signals vertex data
        ELEMENT_ARRAY_BUFFER = 34963 // signals index or face data
    }
}
