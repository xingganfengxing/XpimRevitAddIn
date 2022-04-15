using Autodesk.Revit.DB;
using Autodesk.Revit.UI; 
using XpimRevitAddIn.Common;
using XpimRevitAddIn.Export;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using XpimRevitAddIn.Export.D3d;
using XpimRevitAddIn.Export.Converter;
using XpimRevitAddIn.Export.Json3D;
using XpimRevitAddIn.Revit2Pim;
using XpimRevitAddIn.Export.Xpim;

namespace XpimRevitAddIn.UI.Json3D
{
    /// <summary>
    /// Json3D转换进度展示
    /// </summary>
    public partial class FormModelToXjson3DFile : System.Windows.Forms.Form 
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FormModelToXjson3DFile()
        {
            InitializeComponent();
            this.Shown += FormModelToXjson3D_Shown;
        }

        /// <summary>
        /// 当前打开的revit程序
        /// </summary>
        private UIApplication _UIApp = null;

        /// <summary>
        /// 当前打开的revit文档（rvt、rfa等）
        /// </summary>
        private UIDocument _UIDoc = null;

        /// <summary>
        /// 待转换的视图
        /// </summary>
        private View3D _View = null; 

        /// <summary>
        /// 执行句柄
        /// </summary>
        private ExecuteEventHandler _ExecuteEventHandler = null;

        /// <summary>
        /// ExternalEvent
        /// </summary>
        private ExternalEvent _ExternalEvent = null;

        /// <summary>
        /// 导出的文件路径
        /// </summary>
        private string _FilePath = null;
        
        /// <summary>
        /// 显示进度窗口
        /// </summary>
        /// <param name="uiApp"></param>
        /// <param name="uiDoc"></param>
        /// <param name="view"></param>
        /// <param name="executeEventHandler"></param>
        /// <param name="externalEvent"></param>
        /// <param name="filePath"></param>
        public void Show(UIApplication uiApp, UIDocument uiDoc, View3D view, ExecuteEventHandler executeEventHandler, ExternalEvent externalEvent, string filePath)
        {
            this._UIApp = uiApp;
            this._UIDoc = uiDoc;
            this._View = view; 
            this._ExecuteEventHandler = executeEventHandler;
            this._ExternalEvent = externalEvent;
            this._FilePath = filePath; 

            this.Show(new WindowWrapper(uiApp.MainWindowHandle));
        }

        /// <summary>
        /// 窗口显示后，开始转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormModelToXjson3D_Shown(object sender, EventArgs e)
        {
            Convert();
        }
         
        /// <summary>
        /// 更新显示转换状态信息
        /// </summary>
        /// <param name="statusText"></param>
        private void SetConvertStatus(string statusText)
        {
            this.labelStatus.Invoke(new EventHandler(delegate
            {
                this.labelStatus.Text = statusText;
            }));
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        public void Convert()
        {
            if (_ExternalEvent != null)
            {
                _ExecuteEventHandler.ExecuteAction = new Action<UIApplication>((app) =>
                {
                    try
                    {

                        Document doc = this._UIDoc.Document;
                        View3D view3d = this._View;

                        //加载revit与pim的类型、属性对照文件
                        string revitPimMapFilePath = Path.Combine(SysConfig.DllDir, "revitPimMap.json");
                        RevitPimMap revitPimMap = RevitPimMap.Load(revitPimMapFilePath);

                        string[] lods = SysConfig.GetAllLods().ToArray();
                        Dictionary<string, string> pimFilePaths = new Dictionary<string, string>();

                        string pimTempDir = Path.Combine(SysConfig.TempDir, Guid.NewGuid().ToString());
                        Directory.CreateDirectory(pimTempDir);

                        string xpimTempDir = Path.Combine(pimTempDir, "xpim");
                        Directory.CreateDirectory(xpimTempDir);


                        foreach (string levelOfDevelopment in lods)
                        {
                            int levelOfDetail = int.Parse(SysConfig.Lods[levelOfDevelopment]);

                            this.SetConvertStatus("Conversion in progress, " + levelOfDevelopment + " ...");
                            Json3DExportConfigs configs = new Json3DExportConfigs();
                            configs.ExportProperties = false;
                            configs.FlipCoords = false;
                            configs.IncludeNonStdElements = false;
                            configs.SingleBinary = true;

                            Json3DConverter converter = new Json3DConverter();
                            converter.RevitPimMap = revitPimMap;

                            string pimFilePath = Path.Combine(pimTempDir, levelOfDevelopment + Json3DConverter.FileExtension);
                            pimFilePaths.Add(levelOfDevelopment, pimFilePath);

                            Json3DExportContext ctx = new Json3DExportContext(doc, pimTempDir, levelOfDevelopment, levelOfDetail, converter, configs);
                            CustomExporter exporter = new CustomExporter(doc, ctx);
                            exporter.ShouldStopOnError = true;
                            exporter.Export(view3d);
                            this.SetConvertStatus(levelOfDevelopment + "exported.");
                        }

                        //压缩保存
                        XpimProcessor xpimProcessor = new XpimProcessor();
                        xpimProcessor.GenerateXpim(lods, pimFilePaths, xpimTempDir, this._FilePath) ; 
                        this.SetConvertStatus("All Exported!");
                    }
                    catch (Exception ex)
                    {
                        this.SetConvertStatus("Failed\r\n" + ex.Message);
                    }
                });
                _ExternalEvent.Raise();
            }
        }  
    }
}
