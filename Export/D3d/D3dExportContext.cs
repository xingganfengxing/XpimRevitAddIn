using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using XpimRevitAddIn.UI;
using XpimRevitAddIn.Common;
using XpimRevitAddIn.Export.Converter;

namespace XpimRevitAddIn.Export.D3d
{

    class D3dExportContext : CommonExportContent
    {
        private D3dExportConfigs _Cfgs = new D3dExportConfigs();


        private bool _SkipElementFlag = false;

        private int _ExportNodeCount = 0;

        private D3dExportManager _Manager = new D3dExportManager();

        private IConverter _Converter = null;

        private Stack<Document> _DocumentStack = new Stack<Document>();
        protected override Document Doc
        {
            get
            {
                return _DocumentStack.Peek();
            }
        }

        public D3dExportContext(Document doc, string directory, string fileName, IConverter converter, D3dExportConfigs configs = null)
        {
            _DocumentStack.Push(doc);

            // ensure filename is really a file name and no extension 
            _Dir = directory;
            _FileName = fileName;
            _Cfgs = configs is null ? _Cfgs : configs;
            _Converter = converter;
        }

        /// <summary>
        /// Runs once at beginning of export. Sets up the root node
        /// and scene.
        /// </summary>
        /// <returns></returns>
        public override bool Start()
        {
            _AllInstanceElementDic = new Dictionary<string, Element>();
            _Manager.Start(_Cfgs.ExportProperties, _Cfgs.FlipCoords);
            return true;
        }

        private void FinishD3dExport()
        {
            D3dContainer container = _Manager.Finish();

            this._Converter.Process(container, this._Dir, this._FileName);
        }

        /// <summary>
        /// Runs once at end of export. Serializes the d3d
        /// properties and wites out the *.d3d and *.bin files.
        /// </summary>
        public override void Finish()
        {
            //导出d3d信息
            this.FinishD3dExport();
        }

        /// <summary>
        /// Runs once for each element.
        /// </summary>
        /// <param name="elementId">ElementId of Element being processed</param>
        /// <returns></returns>
        public override RenderNodeAction OnElementBegin(ElementId elementId)
        {
            _CurrentElementId = elementId;

            Element e = Doc.GetElement(elementId);

            if (_Manager.ContainsNode(e.UniqueId))
            {
                // Duplicate element, skip adding. 
                _SkipElementFlag = true;
                return RenderNodeAction.Skip;
            }
            else
            {
                _Manager.OpenNode(e);
                return RenderNodeAction.Proceed;
            }
        }

        /// <summary>
        /// Runs every time, and immediately prior to, a mesh being processed (OnPolymesh).
        /// It supplies the material for the mesh, and we use this to create a new material
        /// in our material container, or switch the current material if it already exists.
        /// TODO: Handle more complex materials.
        /// </summary>
        /// <param name="node"></param>
        public override void OnMaterial(MaterialNode matNode)
        {
            try
            {
                string matName;
                string uniqueId;

                ElementId id = matNode.MaterialId;
                if (id != ElementId.InvalidElementId)
                {
                    Element m = Doc.GetElement(matNode.MaterialId);
                    if (m == null)
                    {
                        uniqueId = string.Format("r{0}g{1}b{2}", matNode.Color.Red.ToString(), matNode.Color.Green.ToString(), matNode.Color.Blue.ToString());
                        matName = string.Format("MaterialNode_{0}_{1}", CommonFunction.ColorToInt(matNode.Color), CommonFunction.RealString(matNode.Transparency * 100));
                    }
                    else
                    {
                        matName = m.Name;
                        uniqueId = m.UniqueId;
                    }
                }
                else
                {
                    uniqueId = string.Format("r{0}g{1}b{2}", matNode.Color.Red.ToString(), matNode.Color.Green.ToString(), matNode.Color.Blue.ToString());
                    matName = string.Format("MaterialNode_{0}_{1}", CommonFunction.ColorToInt(matNode.Color), CommonFunction.RealString(matNode.Transparency * 100));
                }

                _Manager.SwitchMaterial(matNode, matName, uniqueId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Runs for every polymesh being processed. Typically this is a single face
        /// of an element's mesh. Vertices and faces are keyed on the element/material combination 
        /// (this is important because within a single element, materials can be changed and 
        /// repeated in unknown order).
        /// </summary>
        /// <param name="polymesh"></param>
        public override void OnPolymesh(PolymeshTopology polymesh)
        {
            _Manager.OnGeometry(polymesh);
            base.OnPolymesh(polymesh);
        }

        /// <summary>
        /// Runs at the end of an element being processed, after all other calls for that element.
        /// </summary>
        /// <param name="elementId"></param>
        public override void OnElementEnd(ElementId elementId)
        {
            if (_SkipElementFlag)
            {
                _SkipElementFlag = false;
                return;
            }

            _Manager.CloseNode();
            _ExportNodeCount++;
        }

        /// <summary>
        /// This is called when family instances are encountered, after OnElementBegin.
        /// We're using it here to maintain the transform stack for that element's heirarchy.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            ElementId symId = node.GetSymbolId();
            Element symElem = Doc.GetElement(symId);
            var nodeXform = node.GetTransform();
            _Manager.OpenNode(symElem, nodeXform.IsIdentity ? null : nodeXform, true);

            return base.OnInstanceBegin(node);
        }

        /// <summary>
        /// This is called when family instances are encountered, before OnElementEnd.
        /// We're using it here to maintain the transform stack for that element's heirarchy.
        /// </summary>
        /// <param name="node"></param>
        public override void OnInstanceEnd(InstanceNode node)
        {
            ElementId symId = node.GetSymbolId();
            Element symElem = Doc.GetElement(symId);

            _Manager.CloseNode(symElem, true);
        }

        public override RenderNodeAction OnLinkBegin(LinkNode node)
        {
            ElementId symId = node.GetSymbolId();
            Element symElem = Doc.GetElement(symId);

            var nodeXform = node.GetTransform();
            _Manager.OpenNode(symElem, nodeXform.IsIdentity ? null : nodeXform, true);

            _DocumentStack.Push(node.GetDocument());
            return RenderNodeAction.Proceed;
        }

        public override void OnLinkEnd(LinkNode node)
        {
            _Manager.CloseNode();

            _DocumentStack.Pop();
        }
    }
}
