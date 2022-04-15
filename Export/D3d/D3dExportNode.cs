using Autodesk.Revit.DB;
using XpimRevitAddIn.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    class D3dExportNode : D3dNode
    {
        public int index;
        public string id;
        public bool isFinalized = false;
        public Element element;
        public bool isRoot = false;

        public D3dExportNode(Element elem, int index, bool exportProperties = true, bool isInstance = false, string heirarchyFormat = "")
        {

            this.element = elem;
            //this.name = Util.ElementDescription(elem);
            this.name = elem == null ? "<null>" : elem.Id.ToString();
            this.id = isInstance ? elem.UniqueId + "::" + Guid.NewGuid().ToString() : elem.UniqueId;
            this.index = index;

            if (exportProperties)
            {
                // get the extras for this element
                D3dExtras extras = new D3dExtras();
                extras.UniqueId = elem.UniqueId;

                //var properties = Util.GetElementProperties(elem, true);
                //if (properties != null) extras.Properties = properties;
                extras.Properties = CommonFunction.GetElementProperties(elem, true);
                this.extras = extras;
            }
        }
        public D3dExportNode(int index)
        {
            this.name = "::rootNode::";
            this.id = System.Guid.NewGuid().ToString();
            this.index = index;
            this.isRoot = true;
        }

        public D3dNode ToD3DNode()
        {
            D3dNode node = new D3dNode();
            node.name = this.name;
            node.mesh = this.mesh;
            node.matrix = this.matrix;
            node.transform = this.transform;
            node.extras = this.extras;
            node.children = this.children;
            return node;
        }
    }
}
