using Autodesk.Revit.DB;
using XpimRevitAddIn.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Common
{
    class CommonExportContent : IExportContext
    {
        /// <summary>
        /// 导出的目标文件夹
        /// </summary>
        protected string _Dir;

        /// <summary>
        /// 导出的文件名称（无后缀）
        /// </summary>
        protected string _FileName;

        /// <summary>
        /// 几何信息的精细化程度
        /// </summary>
        protected int _LevelOfDetail = 4; 

        /// <summary>
        /// 待导出的rvt/rfa文档
        /// </summary>
        protected virtual Document Doc
        {
            get
            {
                return null; 
            }
        }

        protected Dictionary<string, Element> _AllInstanceElementDic = null;
        /// <summary>
        /// 所有实例化的对象
        /// </summary>
        /// <returns></returns>
        public IList<Element> GetAllInstanceElements()
        {
            return this._AllInstanceElementDic.Values.ToList<Element>();
        }

        /// <summary>
        /// 当前正在导出的对象
        /// </summary>
        protected ElementId _CurrentElementId = null;
        
        /// <summary>
        /// 导出完成
        /// </summary>
        public virtual void Finish()
        { 
        }

        /// <summary>
        /// 取消导出
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCanceled()
        {
            return false;
        }

        /// <summary>
        /// 开始处理一个对象
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public virtual RenderNodeAction OnElementBegin(ElementId elementId)
        {
            _CurrentElementId = elementId;
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// 结束处理一个对象
        /// </summary>
        /// <param name="elementId"></param>
        public virtual void OnElementEnd(ElementId elementId)
        { 
        }

        /// <summary>
        /// 开始处理一个面
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// 完成处理一个面
        /// </summary>
        /// <param name="node"></param>
        public virtual void OnFaceEnd(FaceNode node)
        { 
        }
         
        /// <summary>
        /// 开始处理一个实例
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            Element element = this.Doc.GetElement(_CurrentElementId);
            if (element is FamilySymbol
                || element is Family
                || element is RevitLinkType
                || element.Category == null)
            {
                //不是实例，那么不处理
            }
            else if (!this._AllInstanceElementDic.ContainsKey(element.Id.ToString()))
            {
                this._AllInstanceElementDic.Add(element.Id.ToString(), element);
            }
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// 完成处理一个实例
        /// </summary>
        /// <param name="node"></param>
        public virtual void OnInstanceEnd(InstanceNode node)
        { 
        }

        /// <summary>
        /// 处理灯光
        /// </summary>
        /// <param name="node"></param>
        public virtual void OnLight(LightNode node)
        { 
        }

        /// <summary>
        /// 处理链接
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual RenderNodeAction OnLinkBegin(LinkNode node)
        {
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// 完成链接
        /// </summary>
        /// <param name="node"></param>
        public virtual void OnLinkEnd(LinkNode node)
        { 
        }

        /// <summary>
        /// 处理材质
        /// </summary>
        /// <param name="node"></param>
        public virtual void OnMaterial(MaterialNode node)
        { 
        }

        //处理一个mesh
        public virtual void OnPolymesh(PolymeshTopology node)
        {
            Element element = this.Doc.GetElement(_CurrentElementId);
            if (element is FamilySymbol
                || element is Family
                || element is RevitLinkType
                || element.Category == null)
            {
                //不是实例，不做处理
            }
            else if (!this._AllInstanceElementDic.ContainsKey(element.Id.ToString()))
            {
                this._AllInstanceElementDic.Add(element.Id.ToString(), element);
            }
        }

        /// <summary>
        /// RPC
        /// </summary>
        /// <param name="node"></param>
        public virtual void OnRPC(RPCNode node)
        { 
        }

        /// <summary>
        /// 开始处理视图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual RenderNodeAction OnViewBegin(ViewNode node)
        { 
            node.LevelOfDetail = this._LevelOfDetail;
            return RenderNodeAction.Proceed;
        }

        /// <summary>
        /// 完成处理视图
        /// </summary>
        /// <param name="elementId"></param>
        public virtual void OnViewEnd(ElementId elementId)
        { 
        }

        /// <summary>
        /// 开始处理
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        { 
            _AllInstanceElementDic = new Dictionary<string, Element>();
            return true;
        }
    }
}
