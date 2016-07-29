using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace AzureCore.Graphic.Azure.Model.Import.MQO
{
    public class MqoBone
    {
        public int id; //id in mqx file
        public int node_id; //id in bonearray used by your program
        public string name;
        public string path;
        public bool tail;
        //親id
        //なければ0
        public int pid;
        public MqoBone parent;
        //子id
        public List<int> cids = new List<int>();
        public List<int> bids = new List<int>();

        //根本position
        public Vector3 q;

        //先端position
        public Vector3 p;

        public List<MqoWeit> weits;

        public Vector3 world_position;
        public Vector3 local_position;
        public bool turned;
        public bool world_turned;
        public Matrix matrix
        {
            get
            {
                Matrix m = Matrix.Identity;
                if (turned)
                {
                    m.M11 = -1.0f;
                    m.M33 = -1.0f;
                }
                m.M41 = local_position.Z;
                m.M42 = local_position.Y;
                m.M43 = local_position.Z;
                return m;
            }
        }

        public MqoBone(int node_id)
        {
            this.node_id = node_id;
            weits = new List<MqoWeit>(2048 * 3 * 4);
        }

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement("Bone");
            writer.WriteAttributeString("id", id.ToString());

            writer.WriteAttributeString("rtX", q.X.ToString());
            writer.WriteAttributeString("rtY", q.Y.ToString());
            writer.WriteAttributeString("rtZ", q.Z.ToString());

            writer.WriteAttributeString("tpX", p.X.ToString());
            writer.WriteAttributeString("tpY", p.Y.ToString());
            writer.WriteAttributeString("tpZ", p.Z.ToString());

            writer.WriteAttributeString("rotB", "0.0");
            writer.WriteAttributeString("rotH", "0.0");
            writer.WriteAttributeString("rotP", "0.0");

            writer.WriteAttributeString("mvX", "0.0");
            writer.WriteAttributeString("mvY", "0.0");
            writer.WriteAttributeString("mvZ", "0.0");

            writer.WriteAttributeString("sc", "1.0");

            writer.WriteAttributeString("maxAngB", "90.0");
            writer.WriteAttributeString("maxAngH", "180.0");
            writer.WriteAttributeString("maxAngP", "180.0");

            writer.WriteAttributeString("minAngB", "-90.0");
            writer.WriteAttributeString("minAngH", "-180.0");
            writer.WriteAttributeString("minAngP", "-180.0");

            writer.WriteAttributeString("isDummy", tail ? "1" : "0");
            writer.WriteAttributeString("name", name);

            writer.WriteStartElement("P");
            writer.WriteAttributeString("id", pid.ToString());
            writer.WriteEndElement();

            foreach (int cid in cids)
            {
                writer.WriteStartElement("C");
                writer.WriteAttributeString("id", cid.ToString());
                writer.WriteEndElement();
            }
            foreach (int bid in bids)
            {
                writer.WriteStartElement("B");
                writer.WriteAttributeString("id", bid.ToString());
                writer.WriteEndElement();
            }
            foreach (MqoWeit weit in weits)
            {
                weit.Write(writer);
            }

            writer.WriteEndElement();
        }

        public void Read(XmlReader reader)
        {
            this.id = int.Parse(reader.GetAttribute("id"));
            this.name = reader.GetAttribute("name");
            //maxAngB="180.00000" maxAngH="180.00000" maxAngP="180.00000" minAngB="-180.00000" minAngH="-180.00000" minAngP="-180.00000" IKChain="0" isIK="0" IKParent="0" IKParentIsIK="0" isDummy="0" isLock="0" isHide="0" isMirror="0" isMovable="0" name="upper body" tip_name="upper body_end" ik_name="upper body_IK" ik_tip_name="upper body_IK_end"

            float rtX = float.Parse(reader.GetAttribute("rtX"));
            float rtY = float.Parse(reader.GetAttribute("rtY"));
            float rtZ = float.Parse(reader.GetAttribute("rtZ"));
            this.world_position = new Vector3(rtX, rtY, rtZ);

            reader.Read();//Bone

            if (reader.IsStartElement("P"))
            {
                int id = int.Parse(reader.GetAttribute("id"));
                reader.Read();//P
                this.pid = id;
            }

            while (reader.IsStartElement("C"))
            {
                int id = int.Parse(reader.GetAttribute("id"));
                reader.Read();//C
                this.cids.Add(id);
            }

            while (reader.IsStartElement("B"))
            {
                reader.Read();
             //   int id = int.Parse(reader.GetAttribute("id"));
               // reader.Read();//C
                //this.cids.Add(id);
            }

            while (reader.IsStartElement("L"))
            {
                reader.Read();
                //   int id = int.Parse(reader.GetAttribute("id"));
                // reader.Read();//C
                //this.cids.Add(id);
            }

            while (reader.IsStartElement("W"))
            {
                MqoWeit weit = new MqoWeit();
                weit.Read(reader);
                weit.node_id = this.node_id;
                this.weits.Add(weit);
            }

            reader.ReadEndElement();//Bone
        }

        public void Dump()
        {
            Console.WriteLine("Bone");

            Console.WriteLine("  id:{0}", this.id);
            Console.WriteLine("  name:{0}", this.name);

            Console.WriteLine("P");
            Console.WriteLine("  id:{0}", pid);

            foreach (int cid in cids)
            {
                Console.WriteLine("C");
                Console.WriteLine("  id:{0}", cid);
            }
        }
    }

    public class MqoWeit
    {
        public int object_id;
        public int vertex_id;
        public int node_id; //idx of bones
        public float weit;

        public void Write(XmlWriter writer)
        {
            float weit_percent = weit * 100.0f;

            writer.WriteStartElement("W");
            writer.WriteAttributeString("oi", object_id.ToString());
            writer.WriteAttributeString("vi", vertex_id.ToString());
            writer.WriteAttributeString("w", weit_percent.ToString());
            writer.WriteEndElement();
        }

        public void Read(XmlReader reader)
        {
            this.object_id = int.Parse(reader.GetAttribute("oi"));
            this.vertex_id = int.Parse(reader.GetAttribute("vi"));
            this.weit = float.Parse(reader.GetAttribute("w")) * 0.01f;

            reader.Read();//W
        }

        public void Dump()
        {
            Console.WriteLine("W");

            Console.WriteLine("  oi:{0}", this.object_id);
            Console.WriteLine("  vi:{0}", this.vertex_id);
            Console.WriteLine("  w:{0}", this.weit);
        }
    }

    public class MqxReader
    {
        // mqo path
        //todo: rename to MqoPath
        public string MqoFile;

        string GetMqxPath()
        {
            return Path.ChangeExtension(MqoFile, ".mqx");
        }

        // ボーン配列 [out]
        public MqoBone[] bones;

        //頂点ウェイト配列 [out]
        public MqoWeit[] weits;

        public void CreateWeits()
        {
            weits = new MqoWeit[4];
            for (int i = 0; i < 4; ++i)
            {
                weits[i] = new MqoWeit();
            }
        }

        // MqxFileを読み込む。
        public bool Load(string mqo_file)
        {
            MqoFile = mqo_file;
            string mqx_path = GetMqxPath();

            if (!File.Exists(mqx_path))
                return false;

            XmlReader reader = XmlReader.Create(mqx_path);
            Read(reader);
            reader.Close();

            return true;
        }

        Dictionary<int, MqoBone> bone_idmap;

        // create bone_idmap
        // map id to bone
        void CreateBoneMap()
        {
            bone_idmap = new Dictionary<int, MqoBone>();
            foreach (MqoBone bone in bones)
            {
                bone_idmap[bone.id] = bone;
            }
        }

        Dictionary<string, bool> bone_turnedmap;

        static string GetTurnNodesPath()
        {
            return Path.Combine(Application.StartupPath, @"turnnodes.txt");
        }
        void CreateBoneTurnedMap()
        {
            bone_turnedmap = new Dictionary<string, bool>();
            return;
            using (StreamReader reader = new StreamReader(GetTurnNodesPath()))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                        break;

                    bone_turnedmap[line] = true;
                }
            }
        }
        void UpdateBones()
        {
            // assign node.parent
            foreach (MqoBone bone in bones)
            {
                if (bone.pid != 0)
                {
                    bone.parent = bone_idmap[bone.pid];
                }
                else
                    bone.parent = null;
            }
            // assign node.path
            foreach (MqoBone bone in bones)
            {
                if (bone.parent != null)
                {
                    bone.path = bone.parent.path + "|" + bone.name;
                }
                else
                    bone.path = "|" + bone.name;
            }
            // assign node.turned
            foreach (MqoBone bone in bones)
            {
                bone.turned = bone_turnedmap.ContainsKey(bone.name);
            }
            // assign node.world_turned
            foreach (MqoBone bone in bones)
            {
                if (bone.parent != null)
                {
                    bone.world_turned = bone.parent.world_turned ^ bone.turned;
                }
                else
                    bone.world_turned = bone.turned;
            }
            // assign node.local_position
            foreach (MqoBone bone in bones)
            {
                if (bone.parent != null)
                {
                    bone.local_position = bone.world_position - bone.parent.world_position;
                    if (bone.parent.world_turned)
                    {
                        bone.local_position.X = -bone.local_position.X;
                        bone.local_position.Z = -bone.local_position.Z;
                    }
                }
                else
                    bone.local_position = bone.world_position;
            }
        }

        public void Read(XmlReader reader)
        {
            reader.Read();

            reader.ReadStartElement("MetasequoiaDocument");

            reader.ReadStartElement("IncludedBy");
            string mqo_file = reader.ReadString();
            Console.WriteLine(mqo_file);
            reader.ReadEndElement();//IncludedBy

            try
            {
                reader.ReadStartElement("Plugin.56A31D20.71F282AB");
            }
            catch
            {
                this.bones = new MqoBone[0];
                return;
            }

            reader.ReadStartElement("BoneSet");
            List<MqoBone> boneList = new List<MqoBone>();
            int len = 512;
            //bones = new MqoBone[len];
            int i = 0;
            while (reader.IsStartElement("Group"))
            {
                reader.Read();
            }
            while (reader.IsStartElement("Bone"))
            {
                MqoBone bone = new MqoBone(i);
                bone.Read(reader);
                boneList.Add(bone);
                i++;
            }
            reader.ReadEndElement();//BoneSet

            len = boneList.Count;
            //len = i;
            //Array.Resize(ref bones, len);
            bones = boneList.ToArray();

            CreateBoneMap();// return;
            CreateBoneTurnedMap();
            UpdateBones();

            while (reader.IsStartElement("Obj"))
            {
                //Console.WriteLine("Obj");
                //Console.WriteLine("  id:{0}", reader.GetAttribute("id"));
                reader.Read();//Obj
            }

            while (reader.IsStartElement("Poses"))
            {
                //Console.WriteLine("Poses");
                //Console.WriteLine("  isExist:{0}", reader.GetAttribute("isExist"));
                bool empty = reader.IsEmptyElement;
                reader.Read();//Poses
                if (empty)
                    continue;
                while (reader.IsStartElement("Pose"))
                {
                    //Console.WriteLine("Pose");
                    //Console.WriteLine("  id:{0}", reader.GetAttribute("id"));
                    reader.Read();//Pose
                }
                reader.ReadEndElement();//Poses
            }
            reader.ReadEndElement();//Plugin.56A31D20.71F282AB
            reader.ReadEndElement();//MetasequoiaDocument

            CreateWeitMap();
            CreateWeits();
        }

        List<Dictionary<int, List<MqoWeit>>> weitmap; //[obj][vertex_id]

        public void CreateWeitMap()
        {
            int maxobjects = 255;
            weitmap = new List<Dictionary<int, List<MqoWeit>>>(maxobjects);
            for (int i = 0; i < maxobjects; i++)
            {
                weitmap.Add(new Dictionary<int, List<MqoWeit>>(2048));
            }
            foreach (MqoBone bone in bones)
            {
                foreach (MqoWeit weit in bone.weits)
                {
                    Dictionary<int, List<MqoWeit>> map = weitmap[weit.object_id];
                    List<MqoWeit> weits;
                    if (!map.TryGetValue(weit.vertex_id, out weits))
                    {
                        weits = map[weit.vertex_id] = new List<MqoWeit>(4);
                    }
                    weits.Add(weit);
                }
            }
        }

        public void UpdateWeits(int object_id, int vertex_id)
        {
            List<MqoWeit> weits = weitmap[object_id][vertex_id];
            int len = weits.Count;
            if (len > 4)
                len = 4;

            //todo: sort

            for (int i = 0; i < len; ++i)
            {
                this.weits[i].node_id = weits[i].node_id;
                this.weits[i].weit = weits[i].weit;
            }
            for (int i = len; i < 4; ++i)
            {
                this.weits[i].node_id = 0;
                this.weits[i].weit = 0.0f;
            }
        }
    }

    public class MqxWriter
    {
        // mqo path
        //todo: rename to MqoPath
        public string MqoFile;

        string GetMqxPath()
        {
            return Path.ChangeExtension(MqoFile, ".mqx");
        }

        // MqxFileを書き出す。
        //
        // bones:
        // ボーン配列
        // numobjects:
        // オブジェクトの総数
        // スキン設定の対象オブジェクトは全てのオブジェクトとする。
        //
        // todo: oids
        public void Write(string outputPath, MqoBone[] bones, int numobjects)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = new String(' ', 4);
            XmlWriter writer = XmlWriter.Create(outputPath, settings);
            writer.WriteStartElement("MetasequoiaDocument");
            writer.WriteElementString("IncludedBy", Path.GetFileName(MqoFile));

            writer.WriteStartElement("Plugin.56A31D20.71F282AB");
            writer.WriteAttributeString("name", "ボーン");
            writer.WriteStartElement("BoneSet");

            foreach (MqoBone bone in bones)
                bone.Write(writer);

            writer.WriteEndElement();//BoneSet

            for (int i = 0; i < numobjects; i++)
            {
                writer.WriteStartElement("Obj");
                writer.WriteAttributeString("id", (i + 1).ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();//Plugin.56A31D20.71F282AB

            writer.WriteEndElement();//MetasequoiaDocument
            writer.Close();
        }
    }
}
