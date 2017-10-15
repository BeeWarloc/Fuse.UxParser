using System;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxRemoveNodeChange : UxChange
	{
		public UxRemoveNodeChange(UxNodePath path, NodeSyntax node)
		{
			Node = node ?? throw new ArgumentNullException(nameof(node));
			NodePath = path ?? throw new ArgumentNullException(nameof(path));
		}

		/// <summary>
		/// This node will be compared with to check if the right node is removed.
		/// It's also used when inverting the change.
		/// </summary>
		public NodeSyntax Node { get; }

		public UxNodePath NodePath { get; }

		protected override UxChange Invert()
		{
			return new UxInsertNodeChange(NodePath, Node);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			UxNode removed;
			if (!NodePath.TryFind(document, out removed))
				return false;

			// We might want to loosen up the requirements for this to apply
			// To start with however we expect the removed node to be _exactly_
			// equal to Node.
			if (!removed.Syntax.Equals(Node))
				return false;

			return removed.Remove();
		}
	}
}