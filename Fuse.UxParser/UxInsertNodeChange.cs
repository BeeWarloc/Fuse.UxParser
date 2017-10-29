using System;
using System.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxInsertNodeChange : UxChange
	{
		public UxInsertNodeChange(UxNodePath path, NodeSyntax node)
		{
			Node = node ?? throw new ArgumentNullException(nameof(node));
			NodePath = path ?? throw new ArgumentNullException(nameof(path));
		}

		public NodeSyntax Node { get; }

		public UxNodePath NodePath { get; }

		public override UxChange Invert()
		{
			return new UxRemoveNodeChange(NodePath, Node);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			IUxContainer parent;
			if (!NodePath.TryFindPathParent(document, out parent))
				return false;

			var index = NodePath.Indexes.LastOrDefault();
			if (index > parent.Nodes.Count)
				return false;

			parent.Nodes.Insert(index, UxNode.FromSyntax(Node));
			return true;
		}
	}
}