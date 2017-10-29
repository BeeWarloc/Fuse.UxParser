using System.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxReplaceAttributeChange : UxChange
	{
		public UxReplaceAttributeChange(
			UxNodePath nodePath,
			int attributeIndex,
			AttributeSyntaxBase oldAttribute,
			AttributeSyntaxBase newAttribute)
		{
			NodePath = nodePath;
			AttributeIndex = attributeIndex;
			OldAttribute = oldAttribute;
			NewAttribute = newAttribute;
		}

		public UxNodePath NodePath { get; }
		public int AttributeIndex { get; }
		public AttributeSyntaxBase OldAttribute { get; }
		public AttributeSyntaxBase NewAttribute { get; }

		public override UxChange Invert()
		{
			return new UxReplaceAttributeChange(NodePath, AttributeIndex, NewAttribute, OldAttribute);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			if (!NodePath.TryFind(document, out UxElement node))
				return false;

			var attr = node.Attributes.ElementAtOrDefault(AttributeIndex);
			if (attr == null || !attr.Syntax.Equals(OldAttribute))
				return false;

			if (attr.Name == NewAttribute.Name.Text)
			{
				attr.ReplaceSyntax(NewAttribute);
			}
			else
			{
				if (!attr.Remove())
					return false;
				node.Attributes.Insert(AttributeIndex, new UxAttribute(NewAttribute));
			}
			return true;
		}
	}
}