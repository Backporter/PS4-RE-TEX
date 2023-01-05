using System;
using System.IO;
using System.Collections.Generic;
using DirectXTex;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS4_RE_TEX
{
    class Texture
    {
        // 0x10
        public struct MIP
        {
            public Int64 Offset { get; set; }
            public Int32 Pitch  { get; set; }
            public Int32 Size   { get; set; }

            // internal
            public MIP(Int64 a_offset, Int32 a_Pitch, Int32 a_Size)
            {
                Offset = a_offset;
                Pitch  = a_Pitch;
                Size   = a_Size;
            }
        }

        // 0x20/0x28. | depending on the texture version mipcount can be mipcount * 16, or just the mip count, version 30 && 28 seem to do mipcount * 16 while others do not.
        // Unk0E & Unk0F are named with Unk, this is due to the different versions of .TEX swapping the POS of mips, we already know both these values are mip releated, just not in the same spot across versions, keeping them labled UNK makes the source cleaner.
        public struct TEX
        {
            public Int32 Magic                   { get; set; } // 'TEX\0'
            public Int32 Version                 { get; set; } // ????    
            public Int16 Height                  { get; set; } // ??
            public Int16 Width                   { get; set; } // ??
            public byte  Unk0C                   { get; set; } // ?
            public byte  Unk0D                   { get; set; } // ?
            public byte  Unk0E                   { get; set; } // ? 
            public byte  Unk0F                   { get; set; } // ?
            public Int32 Format                  { get; set; } // ????
            public Int64 Unk14                   { get; set; } // ????????
            public byte  StreamingTexture        { get; set; } // ?
            public byte  Unk1D                   { get; set; } // ?
            public byte  Unk1E                   { get; set; } // ?
            public byte  Unk1F                   { get; set; } // ?

            // 
            public Int64 Unk20                   { get; set; } // only on TEX version 30 && 28.

            // internal.
            public string    texpath             { get; set; }
            public string    ddspath             { get; set; }
            public List<MIP> mips                { get; set; }
            public int       MipCount            { get; set; }
            public byte[]    data                { get; set; }

            public int GetMipCount()
            {
                return mips.Count;
            }

            public int GetTextureSize()
            {
                int size = 0;
                foreach (var mip in mips)
                {
                    size += mip.Size;
                }

                return size;
            }
        }
        
        public static ulong GenerateMIPStartOffset(int a_texVersion, int a_mipCount)
        {
            if (a_texVersion == 30 || a_texVersion == 28)
            {
                return (ulong)(40 + (a_mipCount * 16));
            }
            else
            {
                return (ulong)(32 + (a_mipCount * 16));
            }
            
        }

        public static bool Parse(string a_texPath, out TEX a_tex)
        {
            // initialize the texture and it's list.
            TEX s_tex = new TEX();
            s_tex.mips = new List<MIP>();
            s_tex.texpath = a_texPath;
            s_tex.ddspath = a_texPath + ".dds";

            using (BinaryReader br = new BinaryReader(File.Open(a_texPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                Int32 magic = br.ReadInt32();
                if (magic != 0x584554)
                {
                    MessageBox.Show($"Invailed TEX File({a_texPath}) got(0x{magic.ToString("X2")}) expected(0x584554)", "[Texture Processing Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    a_tex = s_tex;
                    return false;
                }

                s_tex.Magic             = magic;
                s_tex.Version           = br.ReadInt32();
                s_tex.Width             = br.ReadInt16();
                s_tex.Height            = br.ReadInt16();
                s_tex.Unk0C             = br.ReadByte();
                s_tex.Unk0D             = br.ReadByte();
                s_tex.Unk0E             = br.ReadByte();
                s_tex.Unk0F             = br.ReadByte();
                s_tex.Format            = br.ReadInt32();

                s_tex.Unk14             = br.ReadInt64();
                s_tex.Unk14             = 13; // PC/PS4 do not share the same (unkown)data, overwrite PC data with defualt PS4 data

                s_tex.StreamingTexture  = br.ReadByte();

                if (s_tex.StreamingTexture == 0) 
                    s_tex.StreamingTexture  = 1; // .

                s_tex.Unk1D             = br.ReadByte();
                s_tex.Unk1E             = br.ReadByte();
                s_tex.Unk1F             = br.ReadByte();
                
                if (s_tex.Version == 30 || s_tex.Version == 28)
                {
                    s_tex.Unk20 = br.ReadInt64();
                    s_tex.MipCount    = (s_tex.Unk0F / 16);
                }
                else
                {
                    s_tex.MipCount = s_tex.Unk0E;
                }
                
                for (int i = 0; i < s_tex.MipCount; i++)
                {
                    s_tex.mips.Add(new MIP(br.ReadInt64(), br.ReadInt32(), br.ReadInt32()));
                }

                s_tex.MipCount = 1;

                s_tex.data = br.ReadBytes(s_tex.GetTextureSize());
            }

            a_tex = s_tex;
            return true;
        }

        public static bool Export(string a_dst, TEX a_tex)
        {
            File.Delete(a_tex.texpath);

            using (BinaryWriter bw = new BinaryWriter(File.Open(a_tex.texpath, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
                var gnfdata = File.ReadAllBytes(a_tex.ddspath).Skip(256).ToArray();

                bw.Write(a_tex.Magic);
                bw.Write(a_tex.Version);
                bw.Write(a_tex.Width);
                bw.Write(a_tex.Height);
                bw.Write(a_tex.Unk0C);
                bw.Write(a_tex.Unk0D);

                if (a_tex.Version == 30 || a_tex.Version == 28)
                {
                    bw.Write(a_tex.Unk0E);
                    bw.Write((byte)16); // 1 mipmap = 1 * sizeof(MIP)
                }
                else
                {
                    bw.Write((byte)1); // 1 mipmap = 1
                    bw.Write(a_tex.Unk0F);
                }
                
                bw.Write(a_tex.Format);
                bw.Write(a_tex.Unk14);
                bw.Write(a_tex.StreamingTexture);
                bw.Write(a_tex.Unk1D);
                bw.Write(a_tex.Unk1E);
                bw.Write(a_tex.Unk1F);

                if (a_tex.Version == 30 || a_tex.Version == 28)
                {
                    bw.Write(a_tex.Unk20);
                    bw.Write(GenerateMIPStartOffset(a_tex.Version, 1));
                }
                else
                {
                    bw.Write(GenerateMIPStartOffset(a_tex.Version, 1));
                }

                bw.Write(DDS.CaculatePitch((DDS.DXGI_FORMAT)a_tex.Format, (uint)a_tex.Width));
                bw.Write(gnfdata.Length);
                bw.Write(gnfdata);
            }

            return true;
        }
    }
}
