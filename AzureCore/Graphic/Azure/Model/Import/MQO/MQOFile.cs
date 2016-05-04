using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureCore.Graphic.Azure.Model.Import.MQO
{
    /**
     * mqoファイルを読み込んでDOMなオブジェクトに格納したり
     * DOMなオブジェクトをmqo形式で書きだしたりする
     * pmxとの相互変換に必要な部分だけの手抜き実装
     * 
     * publicなものは先頭大文字、private,internalなものは先頭小文字
     * 
     * このファイルはDOMの定義部分
     */
    public partial class MQOFile : IDisposable
    {
        static internal Encoding sjis = Encoding.GetEncoding("Shift_JIS");

        public Decimal Version = -1;

        public MQOScene Scene = null;
        public List<MQOBackImage> BackImage = new List<MQOBackImage>();
        public List<MQOMaterial> Material = new List<MQOMaterial>();
        public Dictionary<int, MQOObject> Object = new Dictionary<int, MQOObject>();
        public List<MQOObject> ObjectByList = new List<MQOObject>();

        public void Dispose()
        {
            Scene = null;
            BackImage.ForEach(b => b.Dispose());
            BackImage = null;
            Material.ForEach(m => m.Dispose());
            Material = null;
            foreach (var o in Object.Values)
                 o.Dispose();
            Object = null;
        }

    }

    public partial class MQOScene : IDisposable
    {
        List<MQOAttribute> Attribute = new List<MQOAttribute>();
        public void Dispose()
        {
            Attribute = null;
        }
    }
    public partial class MQOBackImage : IDisposable
    {
        public string Part, Path;
        public Decimal X, Y, W, H;
        public MQOBackImage(string part, string path, Decimal x, Decimal y, Decimal w, Decimal h)
        {
            Part = part;
            Path = path;
            X = x;
            Y = y;
            W = w;
            H = h;
        }
        public void Dispose()
        {
            Part = null;
            Path = null;
        }
    }
    public class MQOColor
    {
        public Decimal R, G, B, A = 0;
        public void SetValue(int i, Decimal v)
        {
            switch (i)
            {
                case 0:
                    R = v; break;
                case 1:
                    G = v; break;
                case 2:
                    B = v; break;
                case 3:
                    A = v; break;
                default:
                    throw new Exception("out of index");
            }
        }
    }
    public partial class MQOMaterial : IDisposable
    {
        public string Name;
        public string Tex, Alpha, Bump;
        public MQOColor Color;
        public Decimal Diffuse, Ambient, Emission, Specular, Power = 0;
        public MQOMaterial(string name)
        {
            Name = name;
            Tex = "";
            Alpha = "";
            Bump = "";
            Color = new MQOColor();
        }
        public void Dispose()
        {
            Name = null;
            Tex = null;
            Alpha = null;
            Bump = null;
            Color = null;
        }
    }
    public partial class MQOObject : IDisposable
    {
        public int UID;
        public string Name;
        public List<MQOAttribute> Attribute = new List<MQOAttribute>();
        public List<MQOVertex> Vertex = new List<MQOVertex>();
        public List<MQOUV> UV = new List<MQOUV>();
        public List<MQOFace> Face = new List<MQOFace>();
        public List<int> VertexIDs = new List<int>();
        public MQOObject(string name)
        {
            Name = name;
        }
        public void Dispose()
        {
            Attribute = null;
            Vertex = null;
            UV = null;
            Face = null;
        }
        public bool Visible
        {
            get
            {
                bool visible = false;
                try
                {
                    visible = Attribute.Find(a => a.Name == "visible").Values[0] == 15;
                }
                catch
                {
                    visible = true; // visible属性がない場合はtrueとして処理
                }
                return visible;
            }
            set
            {
                try
                {
                    Attribute.Find(a => a.Name == "visible").Values[0] = value ? 15 : 0;
                }
                catch
                {
                    MQOAttribute a = new MQOAttribute();
                    a.Name = "visible";
                    a.Values = new Decimal[] { value ? 15 : 0 };
                    Attribute.Add(a);
                }
            }
        }
        internal int getVertexIndex(Decimal x, Decimal y, Decimal z, Boolean addFlag = true)
        {
            int idx = Vertex.FindLastIndex(xyz => xyz.X == x && xyz.Y == y && xyz.Z == z);
            if (addFlag && idx < 0)
            {
                idx = Vertex.Count;
                Vertex.Add(new MQOVertex(x, y, z));
            }
            return idx;
        }
        internal int getUVIndex(Decimal u, Decimal v, Boolean addFlag = true)
        {
            int idx = UV.FindLastIndex(uv => uv.U == u && uv.V == v);
            if (addFlag && idx < 0)
            {
                idx = UV.Count;
                UV.Add(new MQOUV(u, v));
            }
            return idx;
        }
    }
    public partial class MQOAttribute : IDisposable
    {
        public string Name = "";
        public Decimal[] Values = null;
        public void Dispose()
        {
            Values = null;
        }
    }
    public partial class MQOVertex
    {
        public Decimal X, Y, Z = 0;
        public MQOVertex(Decimal x, Decimal y, Decimal z)
        {
            X = x; Y = y; Z = z;
        }
    }
    public partial class MQOUV
    {
        public Decimal U, V;
        public MQOUV(Decimal u, Decimal v)
        {
            U = u; V = v;
        }
        public bool Equals(MQOUV other)
        {
            return U == other.U && V == other.V;
        }
    }
    public partial class MQOFace : IDisposable
    {
        public int MatID;
        public int[] VertexID;
        public int[] UVID;
        public void Dispose()
        {
            VertexID = null;
            UVID = null;
        }
    }
}
