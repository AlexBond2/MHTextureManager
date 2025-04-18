
namespace MHTextureManager
{
    public class TreeItem
    {
        public string Name { get; set; }
        public TextureEntry Entry { get; set; }
        public List<TreeItem> Children { get; set; } = new();
        public TreeItem Parent { get; set; }
        public bool HasChildren => Children != null && Children.Count > 0;

        public int Level
        {
            get
            {
                int depth = 0;
                TreeItem current = this;
                while (current.Parent != null)
                {
                    depth++;
                    current = current.Parent;
                }
                return depth;
            }
        }

        public TreeNode CreateTreeNode()
        {
            var node = new TreeNode(Name)
            {
                Tag = this
            };

            if (HasChildren)
            {
                node.Nodes.Add(new TreeNode());
            }

            return node;
        }
    }

    public class TreeItems {

        private List<TreeItem> rootItems = new();

        public List<TreeItem> Items { get => rootItems; }

        public void BuildTree(List<TextureEntry> entries, object itemLock)
        {
            rootItems.Clear();

            var groupedEntries = entries
                .GroupBy(entry => entry.Head.TextureName.Split('.')[0])
                .ToList();

            var tasks = new List<Task>();

            foreach (var group in groupedEntries)
            {
                var task = Task.Run(() =>
                {
                    var rootItem = new TreeItem { Name = group.Key };
                    AddChildItems(rootItem, group.ToList());

                    lock (itemLock)
                    {
                        rootItems.Add(rootItem);
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void AddChildItems(TreeItem parentItem, List<TextureEntry> entries)
        {
            if (entries.Count == 0) return;

            int currentDepth = parentItem.Level + 1;

            var groupedEntries = entries
                .GroupBy(entry =>
                {
                    var parts = entry.Head.TextureName.Split('.');
                    return parts.Length > currentDepth ? parts[currentDepth] : null;
                })
                .Where(g => g.Key != null)
                .ToList();

            foreach (var group in groupedEntries)
            {
                var childItem = new TreeItem
                {
                    Name = group.Key,
                    Parent = parentItem,
                    Entry = group.FirstOrDefault(e => e.Head.TextureName.Split('.').Length == currentDepth + 1)
                };

                parentItem.Children.Add(childItem);
                AddChildItems(childItem, group.ToList());
            }
        }

    }
}
