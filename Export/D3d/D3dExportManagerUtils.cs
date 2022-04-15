using Autodesk.Revit.DB;
using XpimRevitAddIn.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.D3d
{
    static class D3dExportManagerUtils
    {
        static public List<double> ConvertXForm(Transform xform)
        {
            if (xform == null || xform.IsIdentity) return null;

            var BasisX = xform.BasisX;
            var BasisY = xform.BasisY;
            var BasisZ = xform.BasisZ;
            var Origin = xform.Origin;
            var OriginX = PointMeterDouble.ConvertFeetToMetres(Origin.X);
            var OriginY = PointMeterDouble.ConvertFeetToMetres(Origin.Y);
            var OriginZ = PointMeterDouble.ConvertFeetToMetres(Origin.Z);
            //var OriginX = PointInt.ConvertFeetToMillimetres(Origin.X);
            //var OriginY = PointInt.ConvertFeetToMillimetres(Origin.Y);
            //var OriginZ = PointInt.ConvertFeetToMillimetres(Origin.Z);

            List<double> glXform = new List<double>(16) {
                BasisX.X, BasisX.Y, BasisX.Z, 0,
                BasisY.X, BasisY.Y, BasisY.Z, 0,
                BasisZ.X, BasisZ.Y, BasisZ.Z, 0,
                OriginX, OriginY, OriginZ, 1
            };

            return glXform;
        } 

        public class HashSearch
        {
            string _S;
            public HashSearch(string s)
            {
                _S = s;
            }
            public bool EqualTo(HashedType d)
            {
                return d.hashcode.Equals(_S);
            }
        }

        static public string GenerateSHA256Hash<T>(T data)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, data);

            using (SHA256 hasher = SHA256.Create())
            {
                mStream.Position = 0;
                byte[] byteHash = hasher.ComputeHash(mStream);

                var sBuilder = new StringBuilder();
                for (int i = 0; i < byteHash.Length; i++)
                {
                    sBuilder.Append(byteHash[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }
    }
}
