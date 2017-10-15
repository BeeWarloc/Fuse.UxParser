using System.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxRemoveAttributeChange : UxChange
	{
		public UxRemoveAttributeChange(
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
			return new UxInsertAttributeChange(NodePath, AttributeIndex, Attribute);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			UxElement node;
			if (!NodePath.TryFind(document, out node))
				return false;

			var attr = node.Attributes.ElementAtOrDefault(AttributeIndex);
			if (attr == null || !attr.Syntax.Equals(Attribute))
				return false;

			return attr.Remove();
		}
	}
}