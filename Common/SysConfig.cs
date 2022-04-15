using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XpimRevitAddIn.Common
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public static class SysConfig
    {
        private static string _DllDir = null;
        public static string DllDir
        {
            get
            {
                if (SysConfig._DllDir == null)
                {
                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    string dllDir = Path.GetDirectoryName(path);
                    SysConfig._DllDir = dllDir;
                }
                return SysConfig._DllDir;
            }
        }

        private static string _TempDir = null;
        public static string TempDir
        {
            get
            {
                if (SysConfig._TempDir == null)
                {
                    string tempDir = Path.Combine(DllDir, "temp");
                    SysConfig._TempDir = tempDir;
                }
                return SysConfig._TempDir;
            }
        }

        private static string _AssemblyPath = null;
        public static string AssemblyPath
        {
            get
            {
                if (SysConfig._AssemblyPath == null)
                {
                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path); 
                    SysConfig._AssemblyPath = path;
                }
                return SysConfig._AssemblyPath;
            }
        } 

        private static SortedDictionary<string, string> _Lods = null;
        public static SortedDictionary<string, string> Lods
        {
            get
            {
                if (_Lods == null)
                {
                    Load();
                }
                return _Lods;
            }
        }

        public static List<string> GetAllLods()
        {
            List<string> lods = new List<string>();
            foreach (string lod in Lods.Keys)
            {
                lods.Add(lod);
            }
            return lods;
        }

        public static Dictionary<string, string> GetLodDictionary()
        {
            Dictionary<string, string> lodDic = new Dictionary<string, string>();
            foreach (string lod in Lods.Keys)
            {
                lodDic.Add(lod, Lods[lod]);
            }
            return lodDic;
        } 

        private static Dictionary<SystemParameterType, string> _SystemParamterValues = null;
        public static string GetSystemParameterValue(SystemParameterType paramType)
        {
            if (_SystemParamterValues == null)
            {
                Load();
            }
            return _SystemParamterValues[paramType];
        }

        private static void Load()
        {
            _SystemParamterValues = new Dictionary<SystemParameterType, string>();
            string configFilePath = Path.Combine(SysConfig.DllDir, "setting.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFilePath);
            XmlElement rootElement = xmlDoc.DocumentElement;
            XmlNode revitElement = rootElement.SelectSingleNode("Revit"); 

            //Lods
            _Lods = GetMapValues(revitElement, SystemParameterType.Lods, SystemParameterType.Lod);
        }

        private static SortedDictionary<string, string> GetMapValues(XmlNode parentNode, SystemParameterType listNodeName, SystemParameterType nodeName)
        {
            XmlNodeList nodeList = parentNode.SelectSingleNode(listNodeName.ToString()).SelectNodes(nodeName.ToString());
            SortedDictionary<string, string> map = new SortedDictionary<string, string>();
            for(int i = 0; i < nodeList.Count; i++)
            {
                XmlNode node = nodeList.Item(i);
                string key = node.Attributes["key"].Value;
                string value = node.Attributes["value"].Value;
                map.Add(key, value);
            }
            return map;
        }

        private static string GetNodeInnerText(XmlNode parentNode, string nodeName)
        {
            return parentNode.SelectSingleNode(nodeName).InnerText;
        }

        public static string DecimalFormatString
        {
            get
            {
                return "0.####";
            }
        } 
    }
}