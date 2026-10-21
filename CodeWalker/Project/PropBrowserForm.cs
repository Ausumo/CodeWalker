using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CodeWalker.Project
{
    // Searchable list of all known archetypes; double-click (or Place) adds the prop to the current ymap.
    public class PropBrowserForm : Form
    {
        private readonly ProjectForm project;
        private readonly TextBox search = new TextBox { Dock = DockStyle.Top };
        private readonly ListBox list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        private readonly Label info = new Label { Dock = DockStyle.Bottom, Height = 22 };
        private readonly Button place = new Button { Dock = DockStyle.Bottom, Text = "Place in current ymap", Height = 28 };
        private List<string> names;

        public PropBrowserForm(ProjectForm project)
        {
            this.project = project;
            Text = "Prop browser";
            Width = 360; Height = 560;
            Controls.Add(list); Controls.Add(place); Controls.Add(info); Controls.Add(search);
            names = (project.GameFileCache?.GetAllArchetypes() ?? new List<Archetype>())
                .Select(a => a.Name).Where(n => !string.IsNullOrEmpty(n)).Distinct().OrderBy(n => n).ToList();
            search.TextChanged += (s, e) => Filter();
            list.DoubleClick += (s, e) => Place();
            place.Click += (s, e) => Place();
            Filter();
        }

        private void Filter()
        {
            var terms = search.Text.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var res = names.Where(n => terms.All(t => n.ToLowerInvariant().Contains(t))).Take(2000).ToArray();
            list.BeginUpdate();
            list.Items.Clear();
            list.Items.AddRange(res);
            list.EndUpdate();
            info.Text = res.Length + " of " + names.Count + " archetypes" + (res.Length == 2000 ? " (showing first 2000)" : "");
        }

        private void Place()
        {
            var name = list.SelectedItem as string;
            if (name == null) return;
            if (project.NewEntity(null, false, true, name) == null)
                MessageBox.Show(this, "Open or create a ymap in the project first.", "Prop browser");
        }
    }
}
