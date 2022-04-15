using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace XpimRevitAddIn.Common
{
    /// <summary>
    /// 几何数据
    /// </summary>
    public class GeometryData
    { 
        public VertexLookupMeterDouble vertDictionary = new VertexLookupMeterDouble();
        public List<double> vertices = new List<double>();
        public List<double> normals = new List<double>();
        public List<double> uvs = new List<double>();
        public List<int> faces = new List<int>();
    }
}
