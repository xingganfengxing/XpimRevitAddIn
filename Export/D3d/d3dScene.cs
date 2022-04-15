using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{

    /// <summary>
    /// The scenes available to render.
    /// https://github.com/KhronosGroup/d3d/tree/master/specification/2.0#scenes
    /// </summary>
    public class D3dScene
    {
        public List<int> nodes = new List<int>();
    }
}
