using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AzureCore.Graphic.Azure.Model.Import.MQO
{
    /**
     * MQOFileクラスのmqoファイル読み込み部分
     * 色々手抜き読み込み
     */
    public partial class MQOFile : IDisposable
    {
        public static MQOFile load(string path, bool triangle_only=false)
        {
            using (TextReader tr = new StreamReader(path, sjis))
            {
                return load(tr, triangle_only);
            }
        }

        public static MQOFile load(TextReader tr, bool triangle_only=false)
        {
            MQOFile mqo = new MQOFile();
            if (!mqo.parse(tr, triangle_only)) { mqo.Dispose(); mqo = null; }
            return mqo;
        }

        private bool parse(TextReader tr, bool triangle_only = false)
        {
            //try
            {
                if (!parseHeader(tr)) return false;
                string str;
                while ((str = tr.ReadLine()) != null)
                {
                    str = str.Trim();
                    if (str.StartsWith("Eof")) return true;
                    if (str.StartsWith("Scene"))
                    {
                        Scene = new MQOScene();
                        if (!Scene.parse(tr))
                            return false;
                        continue;
                    }
                    if (str.StartsWith("BackImage"))
                    {
                        if (!parseBackImage(tr)) 
                            return false;
                        continue;
                    }
                    if (str.StartsWith("Material") && !str.StartsWith("MaterialEx"))
                    {
                        if (!parseMaterial(tr)) 
                            return false;
                        continue;
                    }
                    if (str.StartsWith("Object"))
                    {
                        Match m = MQORegex.Object.Match(str);
                        if (!m.Success) 
                            return false;
                        MQOObject mo = new MQOObject(m.Groups[1].Value);
                        if (!mo.parse(tr, triangle_only)) { mo.Dispose(); mo = null;
                            return false; }

                        if (mo.VertexIDs.Count < mo.Vertex.Count)
                        {
                            for (int i = mo.VertexIDs.Count; i < mo.Vertex.Count; i++)
                                mo.VertexIDs.Add(i);
                        }

                        if (mo.UID > 0)
                            Object.Add(mo.UID, mo);
                        ObjectByList.Add(mo);
                        continue;
                    }
                    // unknown block
                    if (str.EndsWith("{"))
                    {
                        if (!skipBlock(tr))
                            return false;
                        continue;
                    }
                    // unknown line
                }
                // "Eof"行が出現する前に終端に達した場合
                return false;
            }
            //catch (Exception ex)
            {
            //    MessageBox.Show(ex.ToString(),"エラー",MessageBoxButtons.OK,MessageBoxIcon.Error);
            //    return false;
            }
        }

        private bool parseHeader(TextReader tr)
        {
            if ("Metasequoia Document" != tr.ReadLine()) return false;
            Match m = new Regex(@"^Format Text Ver (\d+(?:\.\d+)?)$").Match(tr.ReadLine());
            if (m.Success)
            {
                Version = Decimal.Parse(m.Groups[1].Value);
                return true;
            }
            return false;
        }

        private bool parseBackImage(TextReader tr)
        {
            while (true)
            {
                string str = tr.ReadLine().Trim();
                Match m = MQORegex.BackImage.Match(str);
                if (m.Success)
                {
                    BackImage.Add(new MQOBackImage(
                        m.Groups[1].Value, m.Groups[2].Value,
                        Decimal.Parse(m.Groups[3].Value), Decimal.Parse(m.Groups[4].Value),
                        Decimal.Parse(m.Groups[5].Value), Decimal.Parse(m.Groups[6].Value)));
                    continue;
                }
                else if (str.EndsWith("}"))
                {
                    return true;
                }
                else return false; // ここに来るのは不正なデータのとき
            }
        }

        private bool parseMaterial(TextReader tr)
        {
            while (true)
            {
                string str = tr.ReadLine().Trim();
                Match m = MQORegex.Material.Match(str);
                if (m.Success)
                {
                    MQOMaterial mat = new MQOMaterial(m.Groups[1].Value);
                    if (!mat.parseParams(m.Groups[2].Value)) { mat.Dispose(); return false; }
                    Material.Add(mat);
                    continue;
                }
                else if (str.EndsWith("}"))
                {
                    return true;
                }
                else return false; // ここに来るのは不正なデータのとき
            }
        }

        internal static bool skipBlock(TextReader tr)
        {
            while (true)
            {
                string str = tr.ReadLine().Trim();
                if (str.EndsWith("{"))
                {
                    if (!skipBlock(tr)) return false;
                }
                else if (str.EndsWith("}"))
                {
                    return true;
                }
            }
        }
    }

    internal static class MQORegex
    {
        static string sDecimal = @"(-?\d+(?:\.\d+)?)";
        public static Regex Object = new Regex("^Object \"(.+)\" {$", RegexOptions.Compiled);
        public static Regex BackImage = new Regex("^(\\w+) \"(.*)\" " + sDecimal + " " + sDecimal + " " + sDecimal + " " + sDecimal, RegexOptions.Compiled);
        public static Regex Decimal = new Regex(sDecimal, RegexOptions.Compiled);
        public static Regex Material = new Regex("^\"(.*)\" (.+)$", RegexOptions.Compiled);
        public static Regex Attribute = new Regex(@"^(\w+) (.+)$", RegexOptions.Compiled);
        public static Regex Param = new Regex("(?<key>\\w+)\\((?:\"(?<val>.*)\"|(?<val>[^\\)]+))\\)", RegexOptions.Compiled);
        public static Regex Face = new Regex(@"([234]) (.+)$", RegexOptions.Compiled);
    }

    public partial class MQOScene : IDisposable
    {
        internal bool parse(TextReader tr)
        {
            int depth = 0;
            while (true)
            {
                if (depth < 0) return false;

                string str = tr.ReadLine().Trim();
                if (str.EndsWith("{")) // ライト設定などは読み飛ばす
                {
                    depth++;
                }
                else if (str.EndsWith("}"))
                {
                    if (depth > 0) depth--; else return true;
                }
                else if (depth == 0) // depth=0 の属性値だけ読み込む
                {
                    MQOAttribute ma = MQOAttribute.parse(str);
                    if (ma != null) Attribute.Add(ma);
                }
                else
                { }
            }
        }
    }
    public partial class MQOBackImage : IDisposable
    {
    }
    public partial class MQOMaterial : IDisposable
    {
        internal bool parseParams(string str)
        {
            MatchCollection mc = MQORegex.Param.Matches(str);
            foreach (Match m in mc)
            {
                string key = m.Groups["key"].Value;
                string val = m.Groups["val"].Value;
                switch (key)
                {
                    case "tex":
                        Tex = val;
                        break;
                    case "aplane":
                        Alpha = val;
                        break;
                    case "bump":
                        Bump = val;
                        break;
                    case "col":
                        MatchCollection c = MQORegex.Decimal.Matches(val);
                        if (c.Count != 4) return false;
                        Color = new MQOColor();
                        for (int i = 0; i < 4; i++) Color.SetValue(i, Decimal.Parse(c[i].Groups[0].Value));
                        break;
                    case "dif":
                        Diffuse = Decimal.Parse(val); break;
                    case "amb":
                        Ambient = Decimal.Parse(val); break;
                    case "emi":
                        Emission = Decimal.Parse(val); break;
                    case "spc":
                        Specular = Decimal.Parse(val); break;
                    case "power":
                        Power = Decimal.Parse(val); break;
                    default:
                        break;
                }
            }
            return true;
        }
    }
    public partial class MQOObject : IDisposable
    {
        internal bool parse(TextReader tr, bool triangle_only = false)
        {
            while (true)
            {
                string str = tr.ReadLine().Trim();
                if (str.StartsWith("uid "))
                {
                    if (!int.TryParse(str.Substring(4), out this.UID))
                    {
                        Dispose();
                        return false;
                    }
                    continue;
                }
                else if (str.StartsWith("vertex "))
                {
                    if (!parseVertex(tr))
                    {
                        Dispose();
                        return false;
                    }
                    continue;
                }
                else if (str.StartsWith("face "))
                {
                    if (!parseFace(tr, triangle_only))
                    {
                        Dispose();
                        return false;
                    }

                    continue;
                }
                else if (str.StartsWith("vertexattr "))
                {
                    if (!parseVertexID(tr))
                    {
                        Dispose();
                        return false;
                    }

                    continue;
                }
                else if (str.EndsWith("{"))
                {
                    if (!MQOFile.skipBlock(tr)) { Dispose(); return
                        false; }
                    continue;
                }
                else if (str.EndsWith("}"))
                {
                    return true;
                }
                else
                {
                    MQOAttribute ma = MQOAttribute.parse(str);
                    if (ma == null) { Dispose(); 
                        return false; }
                    Attribute.Add(ma);
                    continue;
                }
            }
        }
        internal bool parseVertex(TextReader tr)
        {
            while (true)
            {
                string str = tr.ReadLine().Trim();
                if (str.EndsWith("}"))
                {
                    return true;
                }
                else
                {
                    MatchCollection mc = MQORegex.Decimal.Matches(str);
                    if (mc.Count != 3) return false;
                    Vertex.Add(new MQOVertex(Decimal.Parse(mc[0].Groups[0].Value), Decimal.Parse(mc[1].Groups[0].Value), Decimal.Parse(mc[2].Groups[0].Value)));
                    continue;
                }
            }
        }
        internal bool parseFace(TextReader tr, bool triangle_only = false)
        {
            while (true)
            {
                string str = tr.ReadLine().Trim();
                if (str.EndsWith("}"))
                {
                    return true;
                }
                else
                {
                    MQOFace f = MQOFace.parse(this, str);
                    if (f == null) 
                        return false;
                    if (triangle_only)
                        Face.AddRange(f.triangle_divide());
                    else
                        Face.Add(f);
                    continue;
                }
            }
        }
        internal bool parseVertexID(TextReader tr)
        {
            bool inUID = false;
            int blockNest = 0;

            while (true)
            {
                string str = tr.ReadLine().Trim();
                if (str.StartsWith("uid {"))
                {
                    inUID = true;
                }
                else if (str.EndsWith("{"))
                {
                    blockNest++;
                }
                else if (str.EndsWith("}"))
                {
                    if (!inUID)
                    {
                        if (blockNest == 0)
                            return true;
                        blockNest--;
                    }
                    else
                        inUID = false;
                }
                else if (inUID)
                {
                    int uid;
                    if (!int.TryParse(str, out uid))
                        return false;

                    this.VertexIDs.Add(uid);
                    continue;
                }
                else
                    continue;
            }
        }
    }
    public partial class MQOAttribute
    {
        internal static MQOAttribute parse(string str)
        {
            Match m = MQORegex.Attribute.Match(str);
            if (!m.Success) return null;
            MQOAttribute ma = new MQOAttribute();
            ma.Name = m.Groups[1].Value;
            MatchCollection mc = MQORegex.Decimal.Matches(m.Groups[2].Value);
            ma.Values = new Decimal[mc.Count];
            for (int i = 0; i < mc.Count; i++) ma.Values[i] = Decimal.Parse(mc[i].Value);
            return ma;
        }
    }
    public partial class MQOFace : IDisposable
    {
        internal static MQOFace parse(MQOObject mobj, string str)
        {
            MatchCollection mc;
            Match m = MQORegex.Face.Match(str);
            if (!m.Success) return null;
            int n = int.Parse(m.Groups[1].Value);
            MQOFace f = new MQOFace();
            f.VertexID = new int[n];
            f.UVID = new int[n];
            f.MatID = -1;
            bool noUV = true;
            foreach(Match p in MQORegex.Param.Matches(m.Groups[2].Value))
            {
                switch (p.Groups["key"].Value)
                {
                    case "M":
                        f.MatID = int.Parse(p.Groups["val"].Value);
                        break;
                    case "V":
                        mc = MQORegex.Decimal.Matches(p.Groups["val"].Value);
                        if (mc.Count != n) { f.Dispose(); 
                            return null; }
                        for (int i = 0; i < n; i++) f.VertexID[i] = int.Parse(mc[i].Value);
                        break;
                    case "UV":
                        mc = MQORegex.Decimal.Matches(p.Groups["val"].Value);
                        if (mc.Count != 2*n) { f.Dispose(); 
                            return null; }
                        noUV = false;
                        for (int i = 0; i < n; i++) f.UVID[i] = mobj.getUVIndex(Decimal.Parse(mc[2*i].Value), Decimal.Parse(mc[2*i+1].Value));
                        break;
                }
            }
            // UVがない場合は(0,0)を割り当てる
            if (noUV) for (int i = 0; i < n; i++) f.UVID[i] = mobj.getUVIndex(0, 0);
            return f;
        }
        internal List<MQOFace> triangle_divide()
        {
            var tri = new List<MQOFace>();
            switch (VertexID.Length)
            {
                case 3:
                    tri.Add(this);
                    break;
                case 4:
                    var f = new MQOFace();
                    f.MatID = MatID;
                    f.VertexID = new int[]{VertexID[0], VertexID[1], VertexID[2]};
                    f.UVID = new int[] { UVID[0], UVID[1], UVID[2] };
                    tri.Add(f);
                    f = new MQOFace();
                    f.MatID = MatID;
                    f.VertexID = new int[]{VertexID[0], VertexID[2], VertexID[3]};
                    f.UVID = new int[] { UVID[0], UVID[2], UVID[3] };
                    tri.Add(f);
                    break;
            }
            return tri;
        }
    }
}
