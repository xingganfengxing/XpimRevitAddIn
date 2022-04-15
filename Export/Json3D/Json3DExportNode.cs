using Autodesk.Revit.DB;
using XpimRevitAddIn.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    class Json3DExportNode : Json3DNode
    {
        public int index;
        public string id; 
        public Element element;
        public bool isRoot = false;

        public Json3DExportNode(Element elem, int index, bool exportProperties = true, bool isInstance = false, string heirarchyFormat = "")
        {

            this.element = elem;
            //this.name = Util.ElementDescription(elem);
            this.name = elem == null ? "<null>" : elem.Id.ToString();
            this.id = isInstance ? elem.UniqueId + "::" + Guid.NewGuid().ToString() : elem.UniqueId;
            this.isInstance = isInstance;
            this.index = index;

            if (exportProperties)
            {
                // get the extras for this element
                Json3DExtras extras = new Json3DExtras();
                extras.UniqueId = elem.UniqueId;

                //var properties = Util.GetElementProperties(elem, true);
                //if (properties != null) extras.Properties = properties;
                extras.Properties = CommonFunction.GetElementProperties(elem, true);
                this.extras = extras;
            }
        }
        public Json3DExportNode(int index)
        {
            this.name = "::rootNode::";
            this.id = System.Guid.NewGuid().ToString();
            this.index = index;
            this.isRoot = true;
        }

        public Json3DNode ToJson3DNode()
        {
            Json3DNode node = new Json3DNode();
            node.name = this.name;
            node.mesh = this.mesh;
            node.matrix = this.matrix;
            node.transform = this.transform;
            node.extras = this.extras;
            node.children = this.children;
            node.isInstance = this.isInstance;
            return node;
        }
    }
}
