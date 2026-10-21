using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeWalker.Project
{
    public static class FiveMExport
    {
        // Writes <folder>/<resource>/fxmanifest.lua and <folder>/<resource>/stream/* for all project files.
        public static string Export(ProjectFile proj, string folder, string resname)
        {
            var res = Path.Combine(folder, resname);
            var stream = Path.Combine(res, "stream");
            Directory.CreateDirectory(stream);
            int count = 0;

            void Put(string name, byte[] data) { File.WriteAllBytes(Path.Combine(stream, name), data); count++; }
            void Copy(GameFile f)
            {
                if (f == null || string.IsNullOrEmpty(f.FilePath) || !File.Exists(f.FilePath)) return;
                Put(Path.GetFileName(f.FilePath), File.ReadAllBytes(f.FilePath));
            }

            foreach (var f in proj.YmapFiles) { var d = f.Save(); if (d != null) Put(Path.GetFileName(f.FilePath ?? (f.Name + ".ymap")), d); }
            foreach (var f in proj.YtypFiles) { var d = f.Save(); if (d != null) Put(Path.GetFileName(f.FilePath ?? (f.Name + ".ytyp")), d); }
            foreach (var f in proj.YbnFiles) { var d = f.Save(); if (d != null) Put(Path.GetFileName(f.FilePath ?? (f.Name + ".ybn")), d); }
            foreach (var f in proj.YdrFiles) Copy(f);
            foreach (var f in proj.YddFiles) Copy(f);
            foreach (var f in proj.YftFiles) Copy(f);
            foreach (var f in proj.YtdFiles) Copy(f);

            var sb = new StringBuilder();
            sb.AppendLine("fx_version 'cerulean'");
            sb.AppendLine("game 'gta5'");
            sb.AppendLine();
            sb.AppendLine("author 'CodeWalker export'");
            sb.AppendLine("this_is_a_map 'yes'");
            if (proj.YtypFiles.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("files {");
                foreach (var f in proj.YtypFiles) sb.AppendLine("    'stream/" + Path.GetFileName(f.FilePath ?? (f.Name + ".ytyp")) + "',");
                sb.AppendLine("}");
                sb.AppendLine();
                foreach (var f in proj.YtypFiles) sb.AppendLine("data_file 'DLC_ITYP_REQUEST' 'stream/" + Path.GetFileName(f.FilePath ?? (f.Name + ".ytyp")) + "'");
            }
            File.WriteAllText(Path.Combine(res, "fxmanifest.lua"), sb.ToString());
            return res + " (" + count + " files)";
        }

        public static void Run(ProjectFile proj, IWin32Window owner)
        {
            if (proj == null) { MessageBox.Show(owner, "No project open."); return; }
            using (var dlg = new FolderBrowserDialog { Description = "Choose the FiveM resources folder to export into" })
            {
                if (dlg.ShowDialog(owner) != DialogResult.OK) return;
                var name = new string((proj.Name ?? "map").ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
                try { MessageBox.Show(owner, "Exported to " + Export(proj, dlg.SelectedPath, name), "FiveM export"); }
                catch (Exception ex) { MessageBox.Show(owner, "Export failed: " + ex.Message, "FiveM export"); }
            }
        }
    }
}
