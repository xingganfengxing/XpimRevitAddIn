using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Common
{
    /// <summary>
    /// 基础函数类
    /// </summary>
    public class CommonFunction
    {
        #region GetMd5
        /// <summary>
        /// 计算获取字符串的MD5值
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetMd5(string s)
        {
            //初始化MD5对象
            MD5 md5 = MD5.Create();

            //将源字符串转化为byte数组
            Byte[] soucebyte = Encoding.Default.GetBytes(s);

            //soucebyte转化为mf5的byte数组
            Byte[] md5bytes = md5.ComputeHash(soucebyte);

            //将md5的byte数组再转化为MD5数组
            StringBuilder sb = new StringBuilder();
            foreach (Byte b in md5bytes)
            {
                //x表示16进制，2表示2位
                sb.Append(b.ToString("x2"));

            }
            return sb.ToString();
        }
        #endregion
        
        #region FtToM
        public static double FtToM = 0.3048;
        #endregion

        #region 获取最大最小值
        public static int[] GetVec3MinMax(List<int> vec3)
        {
            int minVertexX = int.MaxValue;
            int minVertexY = int.MaxValue;
            int minVertexZ = int.MaxValue;
            int maxVertexX = int.MinValue;
            int maxVertexY = int.MinValue;
            int maxVertexZ = int.MinValue;
            for (int i = 0; i < vec3.Count; i += 3)
            {
                if (vec3[i] < minVertexX) minVertexX = vec3[i];
                if (vec3[i] > maxVertexX) maxVertexX = vec3[i];

                if (vec3[i + 1] < minVertexY) minVertexY = vec3[i + 1];
                if (vec3[i + 1] > maxVertexY) maxVertexY = vec3[i + 1];

                if (vec3[i + 2] < minVertexZ) minVertexZ = vec3[i + 2];
                if (vec3[i + 2] > maxVertexZ) maxVertexZ = vec3[i + 2];
            }
            return new int[] { minVertexX, maxVertexX, minVertexY, maxVertexY, minVertexZ, maxVertexZ };
        }
        #endregion

        #region 获取最大最小值
        public static long[] GetVec3MinMax(List<long> vec3)
        {
            long minVertexX = long.MaxValue;
            long minVertexY = long.MaxValue;
            long minVertexZ = long.MaxValue;
            long maxVertexX = long.MinValue;
            long maxVertexY = long.MinValue;
            long maxVertexZ = long.MinValue;
            for (int i = 0; i < (vec3.Count / 3); i += 3)
            {
                if (vec3[i] < minVertexX) minVertexX = vec3[i];
                if (vec3[i] > maxVertexX) maxVertexX = vec3[i];

                if (vec3[i + 1] < minVertexY) minVertexY = vec3[i + 1];
                if (vec3[i + 1] > maxVertexY) maxVertexY = vec3[i + 1];

                if (vec3[i + 2] < minVertexZ) minVertexZ = vec3[i + 2];
                if (vec3[i + 2] > maxVertexZ) maxVertexZ = vec3[i + 2];
            }
            return new long[] { minVertexX, maxVertexX, minVertexY, maxVertexY, minVertexZ, maxVertexZ };
        }
        #endregion

        #region 获取最大最小值
        public static float[] GetVec3MinMax(List<float> vec3)
        {

            List<float> xValues = new List<float>();
            List<float> yValues = new List<float>();
            List<float> zValues = new List<float>();
            for (int i = 0; i < vec3.Count; i++)
            {
                if ((i % 3) == 0) xValues.Add(vec3[i]);
                if ((i % 3) == 1) yValues.Add(vec3[i]);
                if ((i % 3) == 2) zValues.Add(vec3[i]);
            }

            float maxX = xValues.Max();
            float minX = xValues.Min();
            float maxY = yValues.Max();
            float minY = yValues.Min();
            float maxZ = zValues.Max();
            float minZ = zValues.Min();

            return new float[] { minX, maxX, minY, maxY, minZ, maxZ };
        }
        #endregion

        #region 获取最大最小值
        public static int[] GetScalarMinMax(List<int> scalars)
        {
            int minFaceIndex = int.MaxValue;
            int maxFaceIndex = int.MinValue;
            for (int i = 0; i < scalars.Count; i++)
            {
                int currentMin = Math.Min(minFaceIndex, scalars[i]);
                if (currentMin < minFaceIndex) minFaceIndex = currentMin;

                int currentMax = Math.Max(maxFaceIndex, scalars[i]);
                if (currentMax > maxFaceIndex) maxFaceIndex = currentMax;
            }
            return new int[] { minFaceIndex, maxFaceIndex };
        }
        #endregion

        #region 数值转字符串
        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }
        #endregion

        #region 数值转字符串
        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for an XYZ point
        /// or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }
        #endregion

        #region 颜色转数值
        /// <summary>
        /// 颜色转数值
        /// </summary>
        public static int ColorToInt(Color color)
        {
            return ((int)color.Red) << 16
              | ((int)color.Green) << 8
              | (int)color.Blue;
        }
        #endregion

        #region 获取元素属性值
        /// <summary>
        /// 获取元素属性值
        /// </summary>
        public static Dictionary<string, string> GetElementProperties(Element e, bool includeType)
        {
            IList<Parameter> parameters = e.GetOrderedParameters();

            Dictionary<string, string> a = new Dictionary<string, string>(parameters.Count);

            // Add element category
            if (e.Category != null)
            {
                a.Add("Element Category", e.Category.Name);
            }



            foreach (Parameter p in parameters)
            {
                string key = p.Definition.Name;

                if (!a.ContainsKey(key))
                {
                    string val;
                    if (StorageType.String == p.StorageType)
                    {
                        val = p.AsString();
                    }
                    else
                    {
                        val = p.AsValueString();
                    }
                    if (!string.IsNullOrEmpty(val))
                    {
                        a.Add(key, val);
                    }
                }
            }

            if (includeType)
            {
                ElementId idType = e.GetTypeId();

                if (idType != null && ElementId.InvalidElementId != idType)
                {
                    Document doc = e.Document;
                    Element typ = doc.GetElement(idType);
                    parameters = typ.GetOrderedParameters();
                    foreach (Parameter p in parameters)
                    {
                        string key = "Type " + p.Definition.Name;

                        if (!a.ContainsKey(key))
                        {
                            string val;
                            if (StorageType.String == p.StorageType)
                            {
                                val = p.AsString();
                            }
                            else
                            {
                                val = p.AsValueString();
                            }
                            if (!string.IsNullOrEmpty(val))
                            {
                                a.Add(key, val);
                            }
                        }
                    }
                }
            }

            if (a.Count == 0) return null;
            else return a;
        }
        #endregion

        #region 获取十六进制颜色值
        /// <summary>
        /// 获取十六进制颜色值
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetColorHex(Color color)
        {
            if (color.IsValid)
            {
                string colorHex = color.Red.ToString("X").PadLeft(2, '0') + color.Green.ToString("X").PadLeft(2, '0') + color.Blue.ToString("X").PadLeft(2, '0');
                return colorHex;
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region 十六进制颜色转为十进制浮点值数组
        /// <summary>
        /// 十六进制颜色转为十进制浮点值数组
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private float[] GetColorRGBValues(string color)
        {
            float r = (float)Convert.ToInt32(color.Substring(0, 2), 16) / 255;
            float g = (float)Convert.ToInt32(color.Substring(2, 2), 16) / 255;
            float b = (float)Convert.ToInt32(color.Substring(4, 2), 16) / 255;
            return new float[] { r, g, b };
        }
        #endregion
    }
}
