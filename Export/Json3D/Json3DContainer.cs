using XpimRevitAddIn.Export.Converter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{

    public class Json3DContainer :IContainer
    {
        public Json3D json3D;
        public List<Json3DBinaryData> binaries;
        public Dictionary<string, Json3DBinaryData> binaryDic;
        public JObject modelTree;
        public JArray elements;
    }
}
