using System;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxNodeInsertionChange : UxChange
	{
		public UxNodeInsertionChange(UxNodePath path, NodeSyntax node)
		{
			Node = node ?? throw new ArgumentNullException(nameof(node));
			NodePath = path ?? throw new ArgumentNullException(nameof(path));
		}

		public NodeSyntax Node { get; }

		public UxNodePath NodePath { get; }
	}
}