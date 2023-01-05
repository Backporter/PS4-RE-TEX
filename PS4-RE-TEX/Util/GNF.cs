using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4_RE_TEX
{
    class GNF
    {
        public static StreamWriter sw = new StreamWriter(File.Open(AppDomain.CurrentDomain.BaseDirectory + "Data\\log.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
        public static List<string> Textures = new List<string>();
        
        public static bool IsGNF(string path)
        {
            using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                if (br.ReadUInt32() == 0x20464E47)
                    return true;
                else
                    return false;
            }
        }

        public static void Thread1()
        {
            int count = Textures.Count / 2;
            foreach (string tex in Textures.Take(count))
            {
                string newpath = tex.Replace(Path.GetExtension(tex), ".gnf0");

                Process process = new Process();
                process.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Data", "orbis-image2gnf.exe");
                process.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Data");
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.Arguments = $"-i \"{tex}\" -o \"{newpath}\" -f auto";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                while (!process.StandardOutput.EndOfStream)
                {
                    sw.Write("=================================================================================\n");
                    sw.WriteLine(process.StandardOutput.ReadToEnd());
                }

                File.Delete(tex);
                File.Move(newpath, tex);
            }
        }

        public static void Thread2()
        {
            int count = Textures.Count / 2;
            foreach (string tex in Textures.Skip(count))
            {
                string newpath = tex.Replace(Path.GetExtension(tex), ".gnf1");

                Process process = new Process();
                process.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Data", "orbis-image2gnf.exe");
                process.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Data");
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.Arguments = $"-i \"{tex}\" -o \"{newpath}\" -f auto";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                
                while (!process.StandardOutput.EndOfStream)
                {
                    sw.Write("=================================================================================\n");
                    sw.WriteLine(process.StandardOutput.ReadToEnd());
                }

                File.Delete(tex);
                File.Move(newpath, tex);
            }
        }

        public static void ConvertAll()
        {
            Task thread1 = new Task(new Action(Thread1));
            Task thread2 = new Task(new Action(Thread2));
            
            thread1.Start();
            thread2.Start();
            
            Task.WaitAll(new Task[] { thread1, thread2});
        }
    }
}
