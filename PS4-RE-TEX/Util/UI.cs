using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4_RE_TEX
{
    class UI
    {
        public static bool g_stop = false;
        public static Task g_uiTask;

        public static void UIThread()
        {
            Console.CursorVisible = false;
            while (!g_stop)
            {
                Console.Write("\rWorking     ");
                Thread.Sleep(700);
                Console.Write("\rWorking.");
                Thread.Sleep(700);
                Console.Write("\rWorking..");
                Thread.Sleep(700);
                Console.Write("\rWorking...");
                Thread.Sleep(700);
            }
        }


        public static void StartUIThread()
        {
            g_uiTask = new Task(new Action(UIThread));
            g_uiTask.Start();
        }
        
        public static void StopUIThread()
        {
            g_stop = true;
        }
    }
}
