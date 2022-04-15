using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection; 
using XpimRevitAddIn.Common;
using XpimRevitAddIn.Export;
using XpimRevitAddIn.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using XpimRevitAddIn.Export.Converter;
using XpimRevitAddIn.UI.Json3D;
using XpimRevitAddIn.Revit2Pim;

namespace XpimRevitAddIn
{
    /// <summary>
    /// 将rvt文件转换为xpim文件的命令
    /// Convert RVT file to XPIM file command
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]    
    public class RvtToXpimCommand : IExternalCommand
    {
        #region Execute
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document doc = uiDoc.Document;
            View3D view = doc.ActiveView as View3D;
            if (view == null)
            {
                // 提示切换到3D视图
                TaskDialog.Show("Alert", "Please switch to the 3D view.");
                return Result.Failed;
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = Path.GetFileNameWithoutExtension(uiDoc.Document.PathName); 
                sfd.AddExtension = true;
                sfd.Filter = "XPIM 3D File(*.xpim)|*.xpim"; 
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filePath = sfd.FileName;

                    ExecuteEventHandler executeEventHandler = new ExecuteEventHandler("rvtToXpim");
                    ExternalEvent externalEvent = ExternalEvent.Create(executeEventHandler); 

                    FormModelToXjson3DFile formModelToXjson3D = new FormModelToXjson3DFile();
                    formModelToXjson3D.Show(uiApp, uiDoc, view, executeEventHandler, externalEvent, filePath);
                }
                return Result.Succeeded;
            }
        }
        #endregion
    }
}
