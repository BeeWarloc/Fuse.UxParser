using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxReplaceNodeChange : UxChange
	{
		public UxReplaceNodeChange(UxNodePath nodePath, NodeSyntax oldNode, NodeSyntax newNode)
		{
			NodePath = nodePath;
			OldNode = oldNode;
			NewNode = newNode;
		}

		public UxNodePath NodePath { get; }
		public NodeSyntax OldNode { get; }
		public NodeSyntax NewNode { get; }

		public override UxChange Invert()
		{
			return new UxReplaceNodeChange(NodePath, NewNode, OldNode);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			if (!NodePath.TryFind(document, out UxNode node))
				return false;

			if (!node.Syntax.Equals(OldNode))
				return false;

			node.ReplaceWith(UxNode.FromSyntax(NewNode));

			return true;
		}
	}
}