using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.UI
{
    public class ExecuteEventHandler : IExternalEventHandler
    {
        #region Name
        public string Name { get; private set; }
        public string GetName()
        {
            return this.Name;
        }
        #endregion

        #region ExecuteAction
        public Action<UIApplication> ExecuteAction { get; set; }
        #endregion

        #region 构造函数
        public ExecuteEventHandler(string name)
        {
            Name = name;
        }
        #endregion

        #region Execute

        public void Execute(UIApplication app)
        {
            if (ExecuteAction != null)
            {
                try
                {
                    ExecuteAction(app);
                }
                catch
                { }
            }
        }
        #endregion 
    } 
}
