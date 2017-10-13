using System;

namespace Fuse.UxParser.Syntax
{
	public class NodeChildAttribute : Attribute
	{
		public NodeChildAttribute(int orderIndex)
		{
			OrderIndex = orderIndex;
		}


		public int OrderIndex { get; }
	}
}