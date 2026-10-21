using System;
using System.IO;

namespace CodeWalker
{
    public static class SafeSave
    {
        // Copies an existing file to a timestamped backup before it is overwritten.
        public static void Backup(string path, int keep = 10)
        {
            try
            {
                if (!File.Exists(path)) return;
                var dir = Path.Combine(Path.GetDirectoryName(path) ?? ".", ".cw_backup");
                Directory.CreateDirectory(dir);
                var name = Path.GetFileName(path);
                File.Copy(path, Path.Combine(dir, name + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak"), true);
                var olds = Directory.GetFiles(dir, name + ".*.bak");
                Array.Sort(olds);
                for (int i = 0; i < olds.Length - keep; i++) File.Delete(olds[i]);
            }
            catch { }
        }

        public static void WriteAllBytes(string path, byte[] data)
        {
            Backup(path);
            File.WriteAllBytes(path, data);
        }
    }
}
