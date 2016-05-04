using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using AzureCore.Graphic.Azure.Model.Import.PMX;
using System.Text;

namespace AzureCore.Graphic.Azure.Model.Import.MQO
{
    public class PMXImporter
    {
        public void LoadPmx(string pmxpath, string outpathWithoutExt)
        {
            using (System.IO.Stream input = System.IO.File.Open(pmxpath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                Console.Write("PMXファイルを読込中...");
                PMX.PMXModel pmx = new PMXModel();
                pmx.Read(input, 1.0f, 1.0f);
                Console.WriteLine("完了");

                MQOFile mqo = new MQOFile();
                MqoBone[] bones = new MqoBone[pmx.Bones.Length];

                Console.WriteLine("材質を登録中...");
                mqo.Material = new List<MQOMaterial>();
                for (int i = 0; i < pmx.Materials.Length; i++)
                {
                    PMX.PMXMaterial pmat = pmx.Materials[i];
                    Console.Write("'" + pmat.MaterialName + "'...");

                    MQOMaterial mmat = new MQOMaterial(pmat.MaterialName);

                    //色の平均を取得する
                    mmat.Color.R = (decimal)((pmat.Ambient[0] + pmat.Diffuse[0] + pmat.Specular[0]) / 3f);
                    mmat.Color.G = (decimal)((pmat.Ambient[1] + pmat.Diffuse[1] + pmat.Specular[1]) / 3);
                    mmat.Color.B = (decimal)((pmat.Ambient[2] + pmat.Diffuse[2] + pmat.Specular[2]) / 3);
                    mmat.Color.A = (decimal)pmat.Diffuse[3];

                    if (mmat.Color.R <= (decimal)1.0e-6f) mmat.Color.R = (decimal)0.1;
                    if (mmat.Color.G <= (decimal)1.0e-6f) mmat.Color.G = (decimal)0.1;
                    if (mmat.Color.B <= (decimal)1.0e-6f) mmat.Color.B = (decimal)0.1;

                    mmat.Diffuse = (decimal)((pmat.Diffuse[0] / (float)mmat.Color.R + pmat.Diffuse[1] / (float)mmat.Color.G + pmat.Diffuse[2] / (float)mmat.Color.B) / 3f);
                    mmat.Ambient = (decimal)((pmat.Ambient[0] / (float)mmat.Color.R + pmat.Ambient[1] / (float)mmat.Color.G + pmat.Ambient[2] / (float)mmat.Color.B) / 3f);
                    mmat.Specular = (decimal)((pmat.Specular[0] / (float)mmat.Color.R + pmat.Specular[1] / (float)mmat.Color.G + pmat.Specular[2] / (float)mmat.Color.B) / 3f);
                    mmat.Power = (decimal)pmat.SpecularPower;

                    mmat.Tex = (pmat.DiffuseTextureIndex < 0 || pmat.DiffuseTextureIndex >= pmx.TextureList.Count) ? "" : pmx.TextureList[pmat.DiffuseTextureIndex];
                    mmat.Emission = 0;

                    mqo.Material.Add(mmat);
                }
                Console.WriteLine("完了");

                Console.WriteLine("ボーンを登録中...");

                {
                    for (int bi = 0; bi < pmx.Bones.Length; bi++)
                    {
                        bones[bi] = new MqoBone(bi); //まず作成
                    }

                    for (int bi = 0; bi < pmx.Bones.Length; bi++)
                    {
                        MqoBone mbone = bones[bi];
                        PMX.PMXBone pbone = pmx.Bones[bi];
                        Console.Write("'" + pbone.BoneName + "'...");

                        mbone.id = bi + 1;
                        mbone.name = pbone.BoneName;
                        mbone.world_position = new SharpDX.Vector3(pbone.Position);
                        mbone.world_position.Z *= -1f;
                        mbone.q = mbone.world_position;
                        mbone.parent = (pbone.ParentBoneIndex < 0 || pbone.ParentBoneIndex >= bones.Length) ? null : bones[pbone.ParentBoneIndex]; //前にある前提

                        //ウェイトを登録
                        List<MqoWeit> weights = new List<MqoWeit>();
                        for (int vi = 0; vi < pmx.Vertexes.Length; vi++)
                        {
                            PMX.PMXVertex vt = pmx.Vertexes[vi];

                            int checkCount = 0;
                            switch (vt.SkinningMethod)
                            {
                                case PMXVertex.enumSkinningMethod.BDEF1:
                                    checkCount = 1;
                                    break;
                                case PMXVertex.enumSkinningMethod.BDEF2:
                                    checkCount = 2;
                                    break;
                                case PMXVertex.enumSkinningMethod.BDEF4:
                                    checkCount = 4;
                                    break;
                                case PMXVertex.enumSkinningMethod.SDEF:
                                    checkCount = 2; //考慮しない
                                    break;
                            }

                            for (int checkIdx = 0; checkIdx < checkCount; checkIdx++)
                            {
                                if (vt.BoneIndices[checkIdx] == bi && vt.BoneWeights[checkIdx] > 0f)
                                {
                                    weights.Add(new MqoWeit() { node_id = bi, object_id = 1, vertex_id = vi + 1, weit = vt.BoneWeights[checkIdx] });
                                }
                            }
                        }
                        mbone.weits = weights;
                    }

                    Console.WriteLine("親子関係を確認しています...");
                    for (int bi = 0; bi < bones.Length; bi++)
                    {
                        MqoBone bone = bones[bi];

                        bone.pid = bone.parent == null ? 0 : bone.parent.id;
                        MqoBone[] childs = Array.FindAll(bones, (b) => b.parent == bone);
                        bone.cids = new List<int>();
                        for (int i = 0; i < childs.Length; i++)
                        {
                            bone.cids.Add(childs[i].id);
                        }
                        bone.p = childs.Length == 0 ? bone.q : childs[0].q;

                        MqoBone[] brothers = Array.FindAll(bones, (b) => b.parent == bone.parent && b != bone);
                        bone.bids = new List<int>();
                        for (int i = 0; i < brothers.Length; i++)
                        {
                            bone.bids.Add(brothers[i].id);
                        }
                    }
                }
                Console.WriteLine("完了");

                Console.WriteLine("オブジェクトを登録中...");
                {
                    Console.WriteLine("すべての頂点・面は１つオブジェクトにまとまります...");
                    MQOObject mobj = new MQOObject("pmx");
                    mobj.UID = 1;
                    for (int vi = 0; vi < pmx.Vertexes.Length; vi++)
                    { //1からVIDを割り当て
                        PMX.PMXVertex vx = pmx.Vertexes[vi];

                        mobj.Vertex.Add(new MQOVertex((decimal)vx.Position[0], (decimal)vx.Position[1], (decimal)-vx.Position[2]));
                        mobj.UV.Add(new MQOUV((decimal)vx.TextureCoord[0], (decimal)vx.TextureCoord[1]));
                        mobj.VertexIDs.Add(vi + 1);
                    }

                    int wholeFaceIdx = 0;
                    for (int mi = 0; mi < pmx.Materials.Length; mi++)
                    {
                        PMX.PMXMaterial mt = pmx.Materials[mi];
                        for (int vxcount = 0; vxcount < mt.VertexCount / 3; vxcount++)
                        {
                            mobj.Face.Add(new MQOFace()
                            {
                                MatID = mi,
                                VertexID = new int[3]{
                                 pmx.FaceVertexes[wholeFaceIdx + 0],
                                 pmx.FaceVertexes[wholeFaceIdx + 1],
                                 pmx.FaceVertexes[wholeFaceIdx + 2]
                             },
                                UVID = new int[3]{
                                 pmx.FaceVertexes[wholeFaceIdx + 0],
                                 pmx.FaceVertexes[wholeFaceIdx + 1],
                                 pmx.FaceVertexes[wholeFaceIdx + 2]
                             }
                            });
                            wholeFaceIdx += 3;
                        }
                    }
                    mobj.Attribute.Add(new MQOAttribute() { Name = "uid", Values = new decimal[1] { 1 } });
                    mqo.Object.Add(1, mobj);
                }
                Console.WriteLine("完了");


                string mqoOutPath = outpathWithoutExt + ".mqo";
                string mqxOutPath = outpathWithoutExt + ".mqx";
                if (System.IO.File.Exists(mqoOutPath))
                    System.IO.File.Delete(mqoOutPath);
                if (System.IO.File.Exists(mqxOutPath))
                    System.IO.File.Delete(mqxOutPath);

                using (System.IO.Stream ws = System.IO.File.OpenWrite(mqoOutPath))
                {
                    System.IO.TextWriter tw = new System.IO.StreamWriter(ws, Encoding.Default);
                    mqo.WriteTo(tw, mqxOutPath);
                    tw.Flush();
                }

                MqxWriter mqx = new MqxWriter();
                mqx.MqoFile = System.IO.Path.GetFileName(mqoOutPath);
                mqx.Write(mqxOutPath, bones, mqo.Object.Count);
            }
        }
        private static void PrintProgress(int cur, int max)
        {
            if (max == 0 || (cur * 10) % max > 0)
                return;

            Console.Write((cur * 10 / max).ToString() + "%...");
        }
    }
}
