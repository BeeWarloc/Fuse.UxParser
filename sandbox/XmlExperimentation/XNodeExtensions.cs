using System;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public static class XNodeExtensions
	{

		public static bool DeepEquals(this IImmutableXNode a, XNode b)
		{
			// NOCOMMIT! BLARGH DIRTY POO
			var xa = a as XNode;
			if (xa == null)
				throw new NotSupportedException("Don't know how to check deep equality when node is not XNode");
			return xa.DeepEquals(b);
		}

		
		public static int NodeIndex(this XNode node)
		{
			if (node.Parent == null)
				throw new InvalidOperationException("Can't get index of a root node.");
			var index = 0;
			while (node.PreviousNode != null)
			{
				index++;
				node = node.PreviousNode;
			}
			return index;
		}

		public static XmlNodePath NodePath(this XNode node)
		{
			return XmlNodePath.From(node);
		}
	}
}