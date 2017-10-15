using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxInsertAttributeChange : UxChange
	{
		public UxInsertAttributeChange(
			UxNodePath nodePath,
			int attributeIndex,
			AttributeSyntaxBase attribute)
		{
			NodePath = nodePath;
			AttributeIndex = attributeIndex;
			Attribute = attribute;
		}

		public UxNodePath NodePath { get; }
		public int AttributeIndex { get; }
		public AttributeSyntaxBase Attribute { get; }

		protected override UxChange Invert()
		{
			return new UxRemoveAttributeChange(NodePath, AttributeIndex, Attribute);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			UxElement node;
			if (!NodePath.TryFind(document, out node))
				return false;

			if (node.Attributes.Count > AttributeIndex)
				return false;

			node.Attributes.Insert(AttributeIndex, new UxAttribute(Attribute));
			return true;
		}
	}
}