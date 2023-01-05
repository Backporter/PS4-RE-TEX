using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4_RE_TEX
{
    class Settings
    {
        // settings
        public static bool UseMutiThreading = false; // -EMT
        public static int  numThreadCount = 1; //-NTC

        public static bool UseGNFExtension = false; //-EGNF
        public static bool UseMIPS = false; // -UMM
        public static bool KeepGNF = false; // -KGNF
        // 

        public static void ParseSettings(string[] settings)
        {
            for (int i = 0; i < settings.Length; i++)
            {
                switch (settings[i])
                {
                    case "-EMT":
                        UseMutiThreading = true;
                        break;
                    case "-NTC":
                        numThreadCount = int.Parse(settings[i++]);
                        break;
                    case "-EGNF":
                        UseGNFExtension = true;
                        break;
                    case "-UMM":
                        UseMIPS = true;
                        break;
                    case "-KGNF":
                        KeepGNF = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
