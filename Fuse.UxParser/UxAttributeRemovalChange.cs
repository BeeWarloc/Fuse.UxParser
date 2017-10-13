using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxAttributeRemovalChange : UxChange
	{
		public UxAttributeRemovalChange(
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
	}
}