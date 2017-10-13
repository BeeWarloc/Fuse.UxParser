using System;
using System.Linq;

namespace Fuse.UxParser
{
	public class UxNodePath
	{
		readonly int[] _indexes;


		UxNodePath(int[] indexes)
		{
			if (indexes == null)
				throw new ArgumentNullException("indexes");
			_indexes = indexes;
		}

		public static UxNodePath From(UxNode node)
		{
			var depth = node.Depth;
			var indexes = new int[depth];
			for (var i = depth - 1; i >= 0; i--)
			{
				indexes[i] = node.NodeIndex;
				node = node.Parent;
			}
			return new UxNodePath(indexes);
		}

		public bool TryFind<TNode>(UxDocument doc, out TNode node)
			where TNode : UxNode
		{
			UxElement parent;
			if (TryFindPathParent(doc.Root, out parent))
			{
				var leafPos = _indexes.Last();
				node = parent.Nodes.ElementAtOrDefault(leafPos) as TNode;
				return node != null;
			}
			node = null;
			return false;
		}

		bool TryFindPathParent(UxElement descendant, out UxElement parent, int pathPos = 0)
		{
			if (pathPos >= _indexes.Length)
				throw new InvalidOperationException("Unable to locate parent");

			if (pathPos == _indexes.Length - 1)
			{
				parent = descendant;
				return true;
			}

			var el = descendant.Nodes.ElementAtOrDefault(_indexes[pathPos]) as UxElement;
			if (el == null)
			{
				parent = null;
				return false;
			}
			return TryFindPathParent(el, out parent, pathPos + 1);
		}


		public override string ToString()
		{
			return "/" + string.Join("/", _indexes);
		}


		public static UxNodePath Parse(string str)
		{
			UxNodePath path;
			if (!TryParse(str, out path))
				throw new FormatException("Unable to parse XmlNodePath because provided string is not a valid path");
			return path;
		}

		public static bool TryParse(string str, out UxNodePath path)
		{
			path = null;
			var parts = str.Split(new[] { '/' }, StringSplitOptions.None);
			if (parts.Length < 1)
				return false;
			if (parts[0] != string.Empty)
				return false;
			var indexes = new int[parts.Length - 1];
			for (var i = 1; i < parts.Length; i++)
			{
				int index;
				if (int.TryParse(parts[i], out index))
					indexes[i - 1] = index;
			}
			path = new UxNodePath(indexes);
			return true;
		}
	}
}