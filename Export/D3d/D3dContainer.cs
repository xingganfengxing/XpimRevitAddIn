using XpimRevitAddIn.Export.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{

    public class D3dContainer :IContainer
    {
        public D3d d3d;
        public List<D3dBinaryData> binaries;
        public Dictionary<string, D3dBinaryData> binaryDic;
    }
}
