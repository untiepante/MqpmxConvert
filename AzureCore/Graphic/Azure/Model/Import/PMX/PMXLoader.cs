using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AzureCore.Graphic.Azure.Model.Import.PMX
{
    #region 要素
    static class PMXLoadHelper
    {
        public static int ReadInteger(Stream s,int length)
        {
            switch (length)
            {
                case 1:
                    return BinaryAccess.ReadByte(s);
                case 2:
                    return (int)BinaryAccess.ReadUInt16(s);
                case 4:
                    return (int)BinaryAccess.ReadUInt32(s);
                default:
                    return -1;
            }
        }
        public static string ReadText(Stream s, byte textEncoding)
        {
            if (textEncoding == 0)
                return BinaryAccess.ReadString(s, System.Text.Encoding.Unicode);
            else
                return BinaryAccess.ReadString(s, System.Text.Encoding.UTF8);
        }
        public static void WriteInteger(Stream s, int val, int length)
        {
            switch (length)
            {
                case 1:
                    BinaryAccess.Write(s, (byte)val);
                    break;
                case 2:
                    BinaryAccess.Write(s, (ushort)val);
                    break;
                case 4:
                    BinaryAccess.Write(s, (uint)val);
                    break;
                default:
                    break;
            }
        }
        public static void WriteText(Stream s, string text, byte textEncoding)
        {
            if (textEncoding == 0)
                BinaryAccess.Write(s, text, System.Text.Encoding.Unicode);
            else
                BinaryAccess.Write(s, text, System.Text.Encoding.UTF8);
        }
    }
    public class PMXHeader
    {
        public byte[] HeadCode = new byte[4];
        public float Version = 0.0f;
        public byte AddInfoLength = 0;
        public byte[] AddInfo = new byte[0];
        public byte EncodingStyle
        {
            get
            {
                return AddInfo[0];
            }
        }
        public byte AddTextureCoordCount
        {
            get
            {
                return AddInfo[1];
            }
        }
        public byte VertexIndexSize
        {
            get
            {
                return AddInfo[2];
            }
        }
        public byte TextureIndexSize
        {
            get
            {
                return AddInfo[3];
            }
        }
        public byte MaterialIndexSize
        {
            get
            {
                return AddInfo[4];
            }
        }
        public byte BoneIndexSize
        {
            get
            {
                return AddInfo[5];
            }
        }
        public byte MorphIndexSize
        {
            get
            {
                return AddInfo[6];
            }
        }
        public byte RigidIndexSize
        {
            get
            {
                return AddInfo[7];
            }
        }

        public PMXHeader()
        {
        }

        public void Read(Stream s)
        {
            //識別子
            HeadCode = new byte[4];
            s.Read(HeadCode, 0, 4);

            //バージョン
            Version = BinaryAccess.ReadFloat(s);

            //後続する追加情報のバイト長
            AddInfoLength = BinaryAccess.ReadByte(s);

            //後続する追加情報
            AddInfo = new byte[AddInfoLength];
            s.Read(AddInfo, 0, AddInfoLength);
        }

        public void Write(Stream s)
        {
            s.Write(HeadCode, 0, 4);

            BinaryAccess.Write(s, Version);

            BinaryAccess.Write(s, AddInfoLength);

            s.Write(AddInfo, 0, AddInfoLength);
        }
    }
    public class PMXInfo
    {
        public string ModelName;
        public string ModelEnglishName;
        public string Comments;
        public string EnglishComments;

        public PMXInfo()
        {
            ModelName = "";
            ModelEnglishName = "";
            Comments = "";
            EnglishComments = "";
        }

        public void Read(Stream s,PMXHeader header)
        {
            //モデル名
            ModelName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //英モデル名
            ModelEnglishName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //コメント
            Comments = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //英コメント
            EnglishComments = PMXLoadHelper.ReadText(s, header.EncodingStyle);
        }

        public void Write(Stream s, PMXHeader header)
        {
            PMXLoadHelper.WriteText(s, ModelName, header.EncodingStyle);
            PMXLoadHelper.WriteText(s, ModelEnglishName, header.EncodingStyle);
            PMXLoadHelper.WriteText(s, Comments, header.EncodingStyle);
            PMXLoadHelper.WriteText(s, EnglishComments, header.EncodingStyle);
        }
    }
    public class PMXVertex
    {
        public float[] Position;
        public float[] Normal;
        public float[] TextureCoord;
        public float[][] AddTextureCoord;
        
        public enum enumSkinningMethod
        {
            BDEF1 = 0, BDEF2 = 1, BDEF4 = 2, SDEF = 3
        }
        public enumSkinningMethod SkinningMethod;
        public int[] BoneIndices;
        public float[] BoneWeights;
        public float EdgeRaito;

        public PMXVertex()
        {
            Position = new float[3];
            Normal = new float[3];
            TextureCoord = new float[3];
            AddTextureCoord = new float[0][];
            SkinningMethod = enumSkinningMethod.BDEF1;
            BoneIndices = new int[2];
            BoneWeights = new float[2];
            EdgeRaito = 0.0f;
        }

        public void Read(PMXHeader header, Stream s, float coordinate,float scale)
        {
            //位置
            Position = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            Position[0] *= scale; Position[1] *= scale; Position[2] *= scale * coordinate;
            
            //法線
            Normal = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            Normal[2] *= coordinate;

            //テクスチャ座標
            TextureCoord = new float[2] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };

            //追加テクスチャ座標
            AddTextureCoord = new float[header.AddTextureCoordCount][];
            for (int i = 0; i < header.AddTextureCoordCount; i++)
            {
                AddTextureCoord[i] = new float[4] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            }

            //スキニング方法
            SkinningMethod = (enumSkinningMethod)BinaryAccess.ReadByte(s);

            //スキニングパラメータ
            switch (SkinningMethod)
            {
                case enumSkinningMethod.BDEF1:
                    BoneIndices = new int[1] { PMXLoadHelper.ReadInteger(s, header.BoneIndexSize) };
                    BoneWeights = new float[1] { 1.0f };
                    break;
                case enumSkinningMethod.BDEF2:
                    BoneIndices = new int[2] { PMXLoadHelper.ReadInteger(s, header.BoneIndexSize), PMXLoadHelper.ReadInteger(s, header.BoneIndexSize) };
                    BoneWeights = new float[2] { BinaryAccess.ReadFloat(s), 1.0f };
                    BoneWeights[1] -= BoneWeights[0];
                    break;
                case enumSkinningMethod.BDEF4:
                    BoneIndices = new int[4] { PMXLoadHelper.ReadInteger(s, header.BoneIndexSize), PMXLoadHelper.ReadInteger(s, header.BoneIndexSize), PMXLoadHelper.ReadInteger(s, header.BoneIndexSize), PMXLoadHelper.ReadInteger(s, header.BoneIndexSize) };
                    BoneWeights = new float[4] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
                    break;
                case enumSkinningMethod.SDEF:
                    BoneIndices = new int[2] { PMXLoadHelper.ReadInteger(s, header.BoneIndexSize), PMXLoadHelper.ReadInteger(s, header.BoneIndexSize) };
                    BoneWeights = new float[2 + 9] { BinaryAccess.ReadFloat(s), 1.0f,
                                                     BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s),
                                                     BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s),
                                                     BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s)};
                    BoneWeights[1] -= BoneWeights[0];
                    break;
            }

            //エッジ
            EdgeRaito = BinaryAccess.ReadFloat(s);
        }

        public void Write(PMXHeader header, Stream s, float coordinate, float scale)
        {
            //位置
            BinaryAccess.Write(s, Position[0] * scale);
            BinaryAccess.Write(s, Position[1] * scale);
            BinaryAccess.Write(s, Position[2] * scale * coordinate);

            //法線
            BinaryAccess.Write(s, Normal[0]);
            BinaryAccess.Write(s, Normal[1]);
            BinaryAccess.Write(s, Normal[2] * coordinate);

            //テクスチャ座標
            BinaryAccess.Write(s, TextureCoord[0]);
            BinaryAccess.Write(s, TextureCoord[1]);

            //追加テクスチャ座標
            for (int i = 0; i < header.AddTextureCoordCount; i++)
            {
                BinaryAccess.Write(s, AddTextureCoord[i][0]);
                BinaryAccess.Write(s, AddTextureCoord[i][1]);
                BinaryAccess.Write(s, AddTextureCoord[i][2]);
                BinaryAccess.Write(s, AddTextureCoord[i][3]);
            }

            //スキニング方法
            BinaryAccess.Write(s, (byte)SkinningMethod);

            //スキニングパラメータ
            switch (SkinningMethod)
            {
                case enumSkinningMethod.BDEF1:
                    PMXLoadHelper.WriteInteger(s, BoneIndices[0], header.BoneIndexSize);
                    break;
                case enumSkinningMethod.BDEF2:
                    PMXLoadHelper.WriteInteger(s, BoneIndices[0], header.BoneIndexSize);
                    PMXLoadHelper.WriteInteger(s, BoneIndices[1], header.BoneIndexSize);
                    BinaryAccess.Write(s, BoneWeights[0]);
                    break;
                case enumSkinningMethod.BDEF4:
                    PMXLoadHelper.WriteInteger(s, BoneIndices[0], header.BoneIndexSize);
                    PMXLoadHelper.WriteInteger(s, BoneIndices[1], header.BoneIndexSize);
                    PMXLoadHelper.WriteInteger(s, BoneIndices[2], header.BoneIndexSize);
                    PMXLoadHelper.WriteInteger(s, BoneIndices[3], header.BoneIndexSize);
                    BinaryAccess.Write(s, BoneWeights[0]);
                    BinaryAccess.Write(s, BoneWeights[1]);
                    BinaryAccess.Write(s, BoneWeights[2]);
                    BinaryAccess.Write(s, BoneWeights[3]);
                    break;
                case enumSkinningMethod.SDEF:
                    PMXLoadHelper.WriteInteger(s, BoneIndices[0], header.BoneIndexSize);
                    PMXLoadHelper.WriteInteger(s, BoneIndices[1], header.BoneIndexSize);
                    BinaryAccess.Write(s, BoneWeights[0]);
                    BinaryAccess.Write(s, BoneWeights[2]);
                    BinaryAccess.Write(s, BoneWeights[3]);
                    BinaryAccess.Write(s, BoneWeights[4]);
                    BinaryAccess.Write(s, BoneWeights[5]);
                    BinaryAccess.Write(s, BoneWeights[6]);
                    BinaryAccess.Write(s, BoneWeights[7]);
                    BinaryAccess.Write(s, BoneWeights[8]);
                    BinaryAccess.Write(s, BoneWeights[9]);
                    BinaryAccess.Write(s, BoneWeights[10]);
                    break;
            }

            //エッジ
            BinaryAccess.Write(s, EdgeRaito);
        }
    }
    public class PMXMaterial
    {
        public string MaterialName;
        public string MaterialEnglishName;
        public float[] Diffuse;
        public float[] Specular;
        public float SpecularPower;
        public float[] Ambient;
        public byte AttributeFlags;
        public bool CullFlag
        {
            get
            {
                return (AttributeFlags & 1) == 0;
            }
        }
        public bool PlaneShadowedFlag
        {
            get
            {
                return ((AttributeFlags >> 1) & 1) == 1;
            }
        }
        public bool ShadowCasterFlag
        {
            get
            {
                return ((AttributeFlags >> 2) & 1) == 1;
            }
        }
        public bool ShadowReceiverFlag
        {
            get
            {
                return ((AttributeFlags >> 3) & 1) == 1;
            }
        }
        public bool EdgeFlag
        {
            get
            {
                return ((AttributeFlags >> 4) & 1) == 1;
            }
        }
        public float[] EdgeColor;
        public float EdgeWidth;
        public int DiffuseTextureIndex;
        public int SphereMapIndex;
        public enum enumSphereMappingOption
        {
            Disable = 0, Multiply = 1, Add = 2, Other = 3
        }
        public enumSphereMappingOption SphereMappingOption;
        public bool UseInternalToon;
        public int ToonTextureIndex;
        public string Memo;
        public int VertexCount;


        public PMXMaterial()
        {
            MaterialName = "";
            MaterialEnglishName = "";
            Diffuse = new float[4];
            Specular = new float[3];
            SpecularPower = 0.0f;
            Ambient = new float[3];
            AttributeFlags = 0;
            EdgeColor = new float[4];
            EdgeWidth = 0.0f;
            DiffuseTextureIndex = -1;
            SphereMapIndex = -1;
            SphereMappingOption = enumSphereMappingOption.Disable;
            UseInternalToon = true;
            ToonTextureIndex = 0;
            Memo = "";
            VertexCount = 0;
        }

        public void Read(PMXHeader header, Stream s)
        {
            //材質名
            MaterialName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //材質名（英語）
            MaterialEnglishName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //拡散色
            Diffuse = new float[4] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };

            //鏡面反射色
            Specular = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            SpecularPower = BinaryAccess.ReadFloat(s);

            //環境色
            Ambient = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };

            //属性フラグ
            AttributeFlags = BinaryAccess.ReadByte(s);

            //エッジ色
            EdgeColor = new float[4] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };

            //エッジ幅
            EdgeWidth = BinaryAccess.ReadFloat(s);

            //拡散色テクスチャ
            DiffuseTextureIndex = PMXLoadHelper.ReadInteger(s, header.TextureIndexSize);

            //スフィアマップ
            SphereMapIndex = PMXLoadHelper.ReadInteger(s, header.TextureIndexSize);

            //スフィアマップモード
            SphereMappingOption = (enumSphereMappingOption)BinaryAccess.ReadByte(s);

            //トゥーンテクスチャ参照先
            UseInternalToon = BinaryAccess.ReadBoolean(s);
            if (!UseInternalToon)
            {
                ToonTextureIndex = PMXLoadHelper.ReadInteger(s, header.TextureIndexSize);
            }
            else
            {
                ToonTextureIndex = BinaryAccess.ReadByte(s);
            }

            //メモ
            Memo = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //頂点数
            VertexCount = BinaryAccess.ReadInt32(s);
        }

        public void Write(PMXHeader header, Stream s)
        {
            //材質名
            PMXLoadHelper.WriteText(s, MaterialName, header.EncodingStyle);

            //材質名（英語）
            PMXLoadHelper.WriteText(s, MaterialEnglishName, header.EncodingStyle);

            //拡散色
            BinaryAccess.Write(s, Diffuse[0]);
            BinaryAccess.Write(s, Diffuse[1]);
            BinaryAccess.Write(s, Diffuse[2]);
            BinaryAccess.Write(s, Diffuse[3]);

            //鏡面反射色
            BinaryAccess.Write(s, Specular[0]);
            BinaryAccess.Write(s, Specular[1]);
            BinaryAccess.Write(s, Specular[2]);
            BinaryAccess.Write(s, SpecularPower);

            //環境色
            BinaryAccess.Write(s, Ambient[0]);
            BinaryAccess.Write(s, Ambient[1]);
            BinaryAccess.Write(s, Ambient[2]);

            //属性フラグ
            BinaryAccess.Write(s, AttributeFlags);

            //エッジ色
            BinaryAccess.Write(s, EdgeColor[0]);
            BinaryAccess.Write(s, EdgeColor[1]);
            BinaryAccess.Write(s, EdgeColor[2]);
            BinaryAccess.Write(s, EdgeColor[3]);

            //エッジ幅
            BinaryAccess.Write(s, EdgeWidth);

            //拡散色テクスチャ
            PMXLoadHelper.WriteInteger(s, DiffuseTextureIndex, header.TextureIndexSize);

            //スフィアマップ
            PMXLoadHelper.WriteInteger(s, SphereMapIndex, header.TextureIndexSize);

            //スフィアマップモード
            BinaryAccess.Write(s, (byte)SphereMappingOption);

            //トゥーンテクスチャ参照先
            BinaryAccess.Write(s, UseInternalToon);
            if (!UseInternalToon)
            {
                PMXLoadHelper.WriteInteger(s, ToonTextureIndex, header.TextureIndexSize);
            }
            else
            {
                BinaryAccess.Write(s, (byte)ToonTextureIndex);
            }

            //メモ
            PMXLoadHelper.WriteText(s, Memo, header.EncodingStyle);

            //頂点数
            BinaryAccess.Write(s, (int)VertexCount);
        }
    }
    public class PMXBone
    {
        public string BoneName;
        public string BoneEnglishName;
        public float[] Position;
        public int ParentBoneIndex;
        public int TransformLevel;
        public ushort AttributeFlag;
        public bool ViewBindedIsBone
        {
            get
            {
                return (AttributeFlag & 1) == 1;
            }
        }
        public bool CanRotate
        {
            get
            {
                return ((AttributeFlag >> 1) & 1) == 1;
            }
        }
        public bool CanTranslate
        {
            get
            {
                return ((AttributeFlag >> 2) & 1) == 1;
            }
        }
        public bool Visible
        {
            get
            {
                return ((AttributeFlag >> 3) & 1) == 1;
            }
        }
        public bool CanManipulate
        {
            get
            {
                return ((AttributeFlag >> 4) & 1) == 1;
            }
        }
        public bool IKEnabled
        {
            get
            {
                return ((AttributeFlag >> 5) & 1) == 1;
            }
        }
        public bool RotatePlus
        {
            get
            {
                return ((AttributeFlag >> 8) & 1) == 1;
            }
        }
        public bool TransformPlus
        {
            get
            {
                return ((AttributeFlag >> 9) & 1) == 1;
            }
        }
        public bool AxisFixed
        {
            get
            {
                return ((AttributeFlag >> 10) & 1) == 1;
            }
        }
        public bool AxisLocalized
        {
            get
            {
                return ((AttributeFlag >> 11) & 1) == 1;
            }
        }
        public bool TransformAfterPhysic
        {
            get
            {
                return ((AttributeFlag >> 12) & 1) == 1;
            }
        }
        public bool ExternalTransform
        {
            get
            {
                return ((AttributeFlag >> 13) & 1) == 1;
            }
        }
        public float[] BoneViewOffset;
        public int ViewBoneIndex;
        public int RotatePlusIndex;
        public float RotatPlusRaito;
        public float[] FixedAxisVector;
        public float[] LocalAxisVectorX;
        public float[] LocalAxisVectorZ;
        public int ExternalTransformBone;
        public int IKBone;
        public int IKIteration;
        public float IKLimitDegree;
        public class IKChain
        {
            public int BoneIndex;
            public bool AngleLimitEnabled;
            public float[] AngleMax;
            public float[] AngleMin;
        }
        public List<IKChain> IKChains;

        public PMXBone()
        {
        }

        public void Read(PMXHeader header, Stream s, float coord, float scale)
        {
            //ボーン名
            BoneName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //ボーン名（英）
            BoneEnglishName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            //位置
            Position = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            Position[0] *= scale; Position[1] *= scale; Position[2] *= scale;
            Position[2] *= coord;

            //親ボーンのインデックス
            ParentBoneIndex = PMXLoadHelper.ReadInteger(s, header.BoneIndexSize);

            //変形階層
            TransformLevel = BinaryAccess.ReadInt32(s);

            //性質フラグ
            AttributeFlag = BinaryAccess.ReadUInt16(s);

            //画面に表示するボーン
            if (ViewBindedIsBone)
                ViewBoneIndex = PMXLoadHelper.ReadInteger(s, header.BoneIndexSize);
            else
            {
                BoneViewOffset = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
                BoneViewOffset[0] *= scale; BoneViewOffset[1] *= scale; BoneViewOffset[2] *= scale;
                BoneViewOffset[2] *= coord;
            }

            //回転付与
            if (RotatePlus || TransformPlus)
            {
                //付与先ボーンインデックス
                RotatePlusIndex = PMXLoadHelper.ReadInteger(s, header.BoneIndexSize);

                //付与率
                RotatPlusRaito = BinaryAccess.ReadFloat(s);
            }

            //軸固定
            if (AxisFixed)
            {
                //軸の方向ベクトル
                FixedAxisVector = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            }

            //ローカル軸
            if (AxisLocalized)
            {
                //ローカルX軸の方向ベクトル
                LocalAxisVectorX = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };

                //ローカルZ軸の方向ベクトル
                LocalAxisVectorZ = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
            }

            //外部親変形
            if (ExternalTransform)
            {
                //外部親のKey値？
                ExternalTransformBone = BinaryAccess.ReadInt32(s);
            }

            if (IKEnabled)
            {
                //IKボーン
                IKBone = PMXLoadHelper.ReadInteger(s, header.BoneIndexSize);

                //IK試行回数
                IKIteration = BinaryAccess.ReadInt32(s);

                //IK制限角
                IKLimitDegree = BinaryAccess.ReadFloat(s);

                //IKチェイン数
                int ikCount = BinaryAccess.ReadInt32(s);
                IKChains = new List<IKChain>();
                for (int i = 0; i < ikCount; i++)
                {
                    IKChain ik = new IKChain();

                    //連結ボーンインデックス
                    ik.BoneIndex = PMXLoadHelper.ReadInteger(s, header.BoneIndexSize);

                    //角度制限
                    ik.AngleLimitEnabled = BinaryAccess.ReadBoolean(s);

                    if (ik.AngleLimitEnabled)
                    {
                        //角度制限の下限
                        ik.AngleMin = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };

                        //角度制限の上限
                        ik.AngleMax = new float[3] { BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s), BinaryAccess.ReadFloat(s) };
                    }

                    IKChains.Add(ik);
                }
            }
        }

        public void Write(PMXHeader header, Stream s, float coord, float scale)
        {
            //ボーン名
            PMXLoadHelper.WriteText(s, BoneName, header.EncodingStyle);

            //ボーン名（英）
            PMXLoadHelper.WriteText(s, BoneEnglishName, header.EncodingStyle);

            //位置
            BinaryAccess.Write(s, Position[0] * scale);
            BinaryAccess.Write(s, Position[1] * scale);
            BinaryAccess.Write(s, Position[2] * scale * coord);

            //親ボーンのインデックス
            PMXLoadHelper.WriteInteger(s, ParentBoneIndex, header.BoneIndexSize);

            //変形階層
            BinaryAccess.Write(s, (int)TransformLevel);

            //性質フラグ
            BinaryAccess.Write(s, (ushort)AttributeFlag);

            //画面に表示するボーン
            if (ViewBindedIsBone)
                PMXLoadHelper.WriteInteger(s, ViewBoneIndex, header.BoneIndexSize);
            else
            {
                BinaryAccess.Write(s, BoneViewOffset[0] * scale);
                BinaryAccess.Write(s, BoneViewOffset[1] * scale);
                BinaryAccess.Write(s, BoneViewOffset[2] * scale * coord);
            }

            //回転付与
            if (RotatePlus || TransformPlus)
            {
                //付与先ボーンインデックス
                PMXLoadHelper.WriteInteger(s, RotatePlusIndex, header.BoneIndexSize);

                //付与率
                BinaryAccess.Write(s, RotatPlusRaito);
            }

            //軸固定
            if (AxisFixed)
            {
                //軸の方向ベクトル
                BinaryAccess.Write(s, FixedAxisVector[0]);
                BinaryAccess.Write(s, FixedAxisVector[1]);
                BinaryAccess.Write(s, FixedAxisVector[2]);
            }

            //ローカル軸
            if (AxisLocalized)
            {
                //ローカルX軸の方向ベクトル
                BinaryAccess.Write(s, LocalAxisVectorX[0]);
                BinaryAccess.Write(s, LocalAxisVectorX[1]);
                BinaryAccess.Write(s, LocalAxisVectorX[2]);

                //ローカルZ軸の方向ベクトル
                BinaryAccess.Write(s, LocalAxisVectorZ[0]);
                BinaryAccess.Write(s, LocalAxisVectorZ[1]);
                BinaryAccess.Write(s, LocalAxisVectorZ[2]);
            }

            //外部親変形
            if (ExternalTransform)
            {
                //外部親のKey値？
                BinaryAccess.Write(s, (int)ExternalTransformBone);
            }

            if (IKEnabled)
            {
                //IKボーン
                PMXLoadHelper.WriteInteger(s, IKBone, header.BoneIndexSize);

                //IK試行回数
                BinaryAccess.Write(s, (int)IKIteration);

                //IK制限角
                BinaryAccess.Write(s, IKLimitDegree);

                //IKチェイン数
                BinaryAccess.Write(s, IKChains.Count);
                for (int i = 0; i < IKChains.Count; i++)
                {
                    IKChain ik = IKChains[i];

                    //連結ボーンインデックス
                    PMXLoadHelper.WriteInteger(s, ik.BoneIndex, header.BoneIndexSize);

                    //角度制限
                    BinaryAccess.Write(s, ik.AngleLimitEnabled);

                    if (ik.AngleLimitEnabled)
                    {
                        //角度制限の下限
                        BinaryAccess.Write(s, ik.AngleMin[0]);
                        BinaryAccess.Write(s, ik.AngleMin[1]);
                        BinaryAccess.Write(s, ik.AngleMin[2]);

                        //角度制限の上限
                        BinaryAccess.Write(s, ik.AngleMax[0]);
                        BinaryAccess.Write(s, ik.AngleMax[1]);
                        BinaryAccess.Write(s, ik.AngleMax[2]);
                    }
                }
            }
        }
    }
    public class PMXVertexMorph
    {

    }
    public class PMXDisplay
    {
        public string Name;
        public string EnglishName;
        public byte AttributeFlags;
        public bool SpecialFlag
        {
            get
            {
                return (AttributeFlags & 1) == 1;
            }
        }
        public class PMXDisplayElement
        {
            public byte KindFlags;
            public bool IsBone
        {
            get
            {
                return KindFlags  == 0;
            }
        }
            public bool IsMorph
        {
            get
            {
                return KindFlags  == 1;
            }
        }
            public int IndexData;


            public PMXDisplayElement()
	        {
                KindFlags = 0;
                IndexData = 0;
            }

            public void Read(PMXHeader header, Stream s)
            {
                KindFlags = BinaryAccess.ReadByte(s);

                if (IsBone)
                {
                    IndexData = PMXLoadHelper.ReadInteger(s, header.BoneIndexSize);
                }
                else if (IsMorph)
                {
                    IndexData = PMXLoadHelper.ReadInteger(s, header.MorphIndexSize);
                }
            }

            public void Write(PMXHeader header, Stream s)
            {
                BinaryAccess.Write(s, KindFlags);

                if (IsBone)
                {
                    PMXLoadHelper.WriteInteger(s, IndexData, header.BoneIndexSize);
                }
                else if (IsMorph)
                {
                    PMXLoadHelper.WriteInteger(s, IndexData, header.MorphIndexSize);
                }
            }
        }
        public PMXDisplayElement[] Elements;


        public PMXDisplay()
        {
            Name = "";
            EnglishName = "";
            AttributeFlags = 0;
            Elements = new PMXDisplayElement[0];
        }

        public void Read(PMXHeader header, Stream s)
        {
            Name = PMXLoadHelper.ReadText(s, header.EncodingStyle);
            
            EnglishName = PMXLoadHelper.ReadText(s, header.EncodingStyle);

            AttributeFlags = BinaryAccess.ReadByte(s);

            int length = BinaryAccess.ReadInt32(s);
            Elements = new PMXDisplayElement[length];
            for (int i = 0; i < length; i++)
            {
                Elements[i] = new PMXDisplayElement();

                Elements[i].Read(header, s);
            }
        }

        public void Write(PMXHeader header, Stream s)
        {
            PMXLoadHelper.WriteText(s, Name, header.EncodingStyle);

            PMXLoadHelper.WriteText(s, EnglishName, header.EncodingStyle);

            BinaryAccess.Write(s, AttributeFlags);

            BinaryAccess.Write(s, Elements.Length);
            for (int i = 0; i < Elements.Length; i++)
            {
                Elements[i].Write(header, s);
            }
        }
    }
    #endregion

    public class PMXModel
    {
        public PMXHeader Header;
        public PMXInfo Info;
        public PMXVertex[] Vertexes { get; set; }
        public int[] FaceVertexes { get; set; }
        public List<string> TextureList { get; set; }
        public PMXMaterial[] Materials { get; set; }
        public PMXBone[] Bones { get; set; }
        public PMXVertexMorph[] VMorphes { get; set; }
        public PMXDisplay[] Displays { get; set; }
        
        public float Coordinate { get; private set; } //座標系。左手なら+,右手なら-にする。

        public PMXModel()
        {
            Header = new PMXHeader();
            Vertexes = new PMXVertex[0];
            TextureList = new List<string>();
        }

        /// <summary>
        /// ストリームからPMDデータを読み込む。左手座標系のときは+,右手座標系で読み込みたいときは-を指定する。
        /// </summary>
        public void Read(Stream s, float scale = 1.0f, float coord = 1.0f)
        {
            //座標系変換補正
            if (coord == 0) coord = 1.0f; coord /= (float)Math.Abs(coord);

            //ヘッダ
            Header = new PMXHeader();
            Header.Read(s);

            //モデル情報
            Info = new PMXInfo();
            Info.Read(s, Header);
            
            //頂点リスト
            int vertex_length = BinaryAccess.ReadInt32(s);
            Vertexes = new PMXVertex[vertex_length];
            for (uint i = 0; i < vertex_length; i++)
            {
                Vertexes[i] = new PMXVertex();
                Vertexes[i].Read(Header, s, coord, scale);
            }
            
            //面リスト
            int face_length = BinaryAccess.ReadInt32(s);
            FaceVertexes = new int[face_length];
            for (int i = 0; i < face_length; i++)
            {
                FaceVertexes[i] = PMXLoadHelper.ReadInteger(s, Header.VertexIndexSize);
            }

            //テクスチャリスト
            int texture_length = BinaryAccess.ReadInt32(s);
            TextureList = new List<string>();
            for (int i = 0; i < texture_length; i++)
            {
                TextureList.Add(PMXLoadHelper.ReadText(s, Header.EncodingStyle));
            }

            //材質リスト
            int material_length = BinaryAccess.ReadInt32(s);
            Materials = new PMXMaterial[material_length];
            for (uint i = 0; i < material_length; i++)
            {
                Materials[i] = new PMXMaterial();
                Materials[i].Read(Header, s);
            }

            //ボーンリスト読み込み
            int bone_length = BinaryAccess.ReadInt32(s);
            Bones = new PMXBone[bone_length];
            for (int i = 0; i < bone_length; i++)
            {
                Bones[i] = new PMXBone();
                Bones[i].Read(Header, s, coord, scale);
            }

            //頂点モーフ読み込み
            int morph_length = BinaryAccess.ReadInt32(s);
            VMorphes = new PMXVertexMorph[0]; //とりあえずは頂点モーフ無しで登録

            //表示枠読み込み
            /*int display_length = BinaryAccess.ReadInt32(s);
            Displays = new PMXDisplay[display_length];
            for (int i = 0; i < display_length; i++)
            {
                Displays[i] = new PMXDisplay();
                Displays[i].Read(Header, s);
            }*/
            Displays = new PMXDisplay[0];
            return;
            //剛体読み込み
            int rigid_length = BinaryAccess.ReadInt32(s);

            //ジョイント読み込み
            int joint_length = BinaryAccess.ReadInt32(s);
        }

        public void Write(Stream s, float scale = 1.0f, float coord = 1.0f)
        {
            //座標系変換補正
            if (coord == 0) coord = 1.0f; coord /= (float)Math.Abs(coord);

            //ヘッダ
            Header.Write(s);

            //モデル情報
            Info.Write(s, Header);

            //頂点リスト
            BinaryAccess.Write(s, Vertexes.Length);
            for (uint i = 0; i < Vertexes.Length; i++)
            {
                Vertexes[i].Write(Header, s, coord, scale);
            }

            //面リスト
            BinaryAccess.Write(s, FaceVertexes.Length);
            for (int i = 0; i < FaceVertexes.Length; i++)
            {
                PMXLoadHelper.WriteInteger(s, FaceVertexes[i], Header.VertexIndexSize);
            }

            //テクスチャリスト
            BinaryAccess.Write(s, TextureList.Count);
            for (int i = 0; i < TextureList.Count; i++)
            {
                PMXLoadHelper.WriteText(s, TextureList[i], Header.EncodingStyle);
            }

            //材質リスト
            BinaryAccess.Write(s, Materials.Length);
            for (uint i = 0; i < Materials.Length; i++)
            {
                Materials[i].Write(Header, s);
            }

            //ボーンリスト読み込み
            BinaryAccess.Write(s, Bones.Length);
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].Write(Header, s, coord, scale);
            }

            //頂点モーフ読み込み
            BinaryAccess.Write(s, (int)0);

            //表示枠読み込み
            BinaryAccess.Write(s, Displays.Length);
            for (int i = 0; i < Displays.Length; i++)
            {
                Displays[i].Write(Header, s);
            }

            //剛体読み込み
            BinaryAccess.Write(s, (int)0);

            //ジョイント読み込み
            BinaryAccess.Write(s, (int)0);
        }
    }
}
