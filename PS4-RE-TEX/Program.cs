using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using DirectXTex;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* just a reminder, this is a remake of the orginal tool, this is NOT orginal source code. */
namespace PS4_RE_TEX
{
    class Program
    {
        public static List<Texture.TEX> g_entires = new List<Texture.TEX>();

        [STAThread] // needed due to use using MessageBox's
        static void Main(string[] args)
        {
            // parse arguments
            Settings.ParseSettings(args);

            // start the "UI".
            UI.StartUIThread();

            // [out] varables, needed for parsing.
            DirectXTexUtility.DDSHeader     header      = new DirectXTexUtility.DDSHeader();
            DirectXTexUtility.DX10Header    dx10Header  = new DirectXTexUtility.DX10Header();
            Texture.TEX                     tex         = new Texture.TEX();

            // illterate all .tex's, parse and write them to a DDS.
            foreach (string textures in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "natives", "*", SearchOption.AllDirectories).Where(s => s.Contains(".tex") && !s.Contains(".dds")))
            {
                if (!Texture.Parse(textures, out tex))
                {
                    MessageBox.Show($"Failed to Parse texture -> {textures}", "[Texture Processing Error", MessageBoxButtons.OK);
                }
                else
                {
                    DirectXTexUtility.GenerateDDSHeader(DirectXTexUtility.GenerateMataData((int)tex.Width, (int)tex.Height, (int)tex.MipCount, (DirectXTexUtility.DXGIFormat)tex.Format, false), DirectXTexUtility.DDSFlags.NONE, out header, out dx10Header);
                    DDS.WriteDDS(DirectXTexUtility.EncodeDDSHeader(header, dx10Header), tex.data, textures + ".dds");
                    g_entires.Add(tex);
                }
            }

            // convert all those DDS's to GNF.
            GNF.Textures.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "natives", "*.dds", SearchOption.AllDirectories));
            GNF.ConvertAll();

            // rebuild all those GNF's into the a new .tex
            foreach (var parsedTextures in g_entires)
            {
                if (!Texture.Export(parsedTextures.texpath, parsedTextures))
                {
                    MessageBox.Show($"Failed to Export texture -> {parsedTextures.texpath}", "[Texture Processing Error", MessageBoxButtons.OK);
                }
                else
                {
                    if (!Settings.KeepGNF)
                        File.Delete(parsedTextures.ddspath);
                }
            }
        }
    }
}
