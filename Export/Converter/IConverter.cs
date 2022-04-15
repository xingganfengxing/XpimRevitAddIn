using XpimRevitAddIn.Export.D3d;
using System.Text;

namespace XpimRevitAddIn.Export.Converter
{
    /// <summary>
    /// 转换接口
    /// </summary>
    public interface IConverter
    { 
        void Process(IContainer container, string dirPath, string fileName);
    }
}