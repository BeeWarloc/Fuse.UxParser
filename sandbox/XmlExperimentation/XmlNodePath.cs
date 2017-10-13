using System;
using System.Linq;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlNodePath
	{
		readonly int[] _indexes;


		XmlNodePath(int[] indexes)
		{
			if (indexes == null)
				throw new ArgumentNullException("indexes");
			_indexes = indexes;
		}


		public bool TryFind<TNode>(XDocument doc, out TNode node)
			where TNode : XNode
		{
			XElement parent;
			if (TryFindPathParent(doc.Root, out parent))
			{
				var leafPos = _indexes.Last();
				node = parent.Nodes().ElementAtOrDefault(leafPos) as TNode;
				return node != null;
			}
			node = null;
			return false;
		}

		public override string ToString()
		{
			return "/" + string.Join("/", _indexes);
		}

		public static XmlNodePath From(XNode node)
		{
			var depth = GetDepth(node);
			var indexes = new int[depth];
			for (var i = depth - 1; i >= 0; i--)
			{
				indexes[i] = node.NodeIndex();
				node = node.Parent;
			}
			return new XmlNodePath(indexes);
		}


		// depth 0 means a single element
		static int GetDepth(XNode element)
		{
			return element.Parent != null ? GetDepth(element.Parent) + 1 : 0;
		}

		internal bool TryRemoveFromDocument(XDocument doc)
		{
			if (_indexes.Length == 0)
				throw new InvalidOperationException("Can't remove root position. TODO: Maybe allow this?");

			XNode removed;
			if (TryFind(doc, out removed))
			{
				removed.Remove();
				return true;
			}
			return false;
		}

		internal bool TryInsertToDocument(XDocument doc, XNode inserted)
		{
			if (_indexes.Length == 0)
				throw new InvalidOperationException("Can't insert at root position. TODO: Maybe allow this?");

			XElement parent;
			if (TryFindPathParent(doc.Root, out parent))
			{
				var insertPos = _indexes.Last();
				if (insertPos == 0)
				{
					parent.AddFirst(inserted);
					return true;
				}

				var before = parent.Nodes().ElementAtOrDefault(insertPos - 1);
				if (before != null)
				{
					before.AddAfterSelf(inserted);
					return true;
				}
			}
			return false;
		}

		bool TryFindPathParent(XElement descendant, out XElement parent, int pathPos = 0)
		{
			if (pathPos >= _indexes.Length)
				throw new InvalidOperationException("Unable to locate parent");

			if (pathPos == _indexes.Length - 1)
			{
				parent = descendant;
				return true;
			}

			var el = descendant.Nodes().ElementAtOrDefault(_indexes[pathPos]) as XElement;
			if (el == null)
			{
				parent = null;
				return false;
			}
			return TryFindPathParent(el, out parent, pathPos + 1);
		}

		public static XmlNodePath Parse(string str)
		{
			XmlNodePath path;
			if (!TryParse(str, out path))
				throw new FormatException("Unable to parse XmlNodePath because provided string is not a valid path");
			return path;
		}

		public static bool TryParse(string str, out XmlNodePath path)
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
			path = new XmlNodePath(indexes);
			return true;
		}
	}
}