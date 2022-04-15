using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    public class Json3DExportConfigs
    {
        /// <summary>
        /// Flag to export all buffers into a single .bin file (if true).
        /// </summary>
        public bool SingleBinary = true;

        /// <summary>
        /// Flag to export all the properties for each element.
        /// </summary>
        public bool ExportProperties = true;

        /// <summary>
        /// Flag to write coords as Z up instead of Y up (if true).
        /// </summary>
        public bool FlipCoords = true;

        /// <summary>
        /// Include non-standard elements that are not part of
        /// official d3d spec. If false, non-standard elements will be excluded
        /// </summary>
        public bool IncludeNonStdElements = true;
    }
}
