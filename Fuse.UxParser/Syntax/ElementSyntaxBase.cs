using System.Collections.Immutable;

namespace Fuse.UxParser.Syntax
{
	public abstract class ElementSyntaxBase : NodeSyntax
	{
		public abstract bool IsEmpty { get; }
		public abstract NameToken Name { get; }
		public abstract IImmutableList<AttributeSyntaxBase> Attributes { get; }
		public abstract IImmutableList<NodeSyntax> Nodes { get; }
		public abstract int DescendantElementCount { get; }

		public abstract ElementSyntaxBase With(
			string name = null,
			IImmutableList<AttributeSyntaxBase> attributes = null,
			IImmutableList<NodeSyntax> nodes = null,
			bool? isEmpty = null);
	}
}