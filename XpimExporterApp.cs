using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using XpimRevitAddIn.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace XpimRevitAddIn
{
    /// <summary>
    /// 文件转换插件
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class XpimExporterApp : IExternalApplication
    {
        /// <summary>
        /// 启动rvt程序，并允许加载此插件后，执行这个函数
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "XPIM";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                // 如果创建toolbar上tab页出错（例如存在重名），那么不重复创建tab页
            }

            string assemblyPath = SysConfig.AssemblyPath;
            string imageDir = Path.Combine(SysConfig.DllDir, "images");

            //添加新的ribbon panel（导出）
            RibbonPanel generatePanel = application.CreateRibbonPanel(tabName, "Export");

            //toXpimFile
            PushButtonData toXpimFileBtnData = new PushButtonData("toXpimFile", "ToXpim", assemblyPath, "XpimRevitAddIn.RvtToXpimCommand");
            string toXpimFileBtnImagePath = Path.Combine(imageDir, "toXpimFile_32x32.png");
            toXpimFileBtnData.LargeImage = new BitmapImage(new Uri(toXpimFileBtnImagePath));
            toXpimFileBtnData.ToolTip = "Convert rvt to xpim";
            toXpimFileBtnData.ToolTipImage = new BitmapImage(new Uri(toXpimFileBtnImagePath));
            generatePanel.AddItem(toXpimFileBtnData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
