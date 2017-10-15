using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Fuse.UxParser.Syntax
{
	public class ElementSyntax : ElementSyntaxBase
	{
		// Cache FullSpan of elements to avoid very deep shit.
		int _fullSpan = -1;

		public ElementSyntax(ElementStartTagSyntax startTag, IImmutableList<NodeSyntax> nodes, ElementEndTagSyntax endTag)
		{
			StartTag = startTag;
			Nodes = nodes;
			EndTag = endTag;
			DescendantElementCount = nodes.OfType<ElementSyntaxBase>().Sum(x => x.DescendantElementCount + 1);
		}

		[NodeChild(0)]
		public ElementStartTagSyntax StartTag { get; }

		[NodeChild(1)]
		public override IImmutableList<NodeSyntax> Nodes { get; }

		[NodeChild(2)]
		public ElementEndTagSyntax EndTag { get; }


		public override NameToken Name => StartTag.Name;
		public override IImmutableList<AttributeSyntaxBase> Attributes => StartTag.Attributes;
		public override int DescendantElementCount { get; }

		public override bool IsEmpty => false;

		public override int FullSpan
		{
			get
			{
				return _fullSpan == -1
					? (_fullSpan = StartTag.FullSpan + Nodes.Sum(x => x.FullSpan) + EndTag.FullSpan)
					: _fullSpan;
			}
		}

		public override TriviaSyntax LeadingTrivia => StartTag.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => EndTag.TrailingTrivia;

		public override ElementSyntaxBase With(
			string name = null,
			IImmutableList<AttributeSyntaxBase> attributes = null,
			IImmutableList<NodeSyntax> nodes = null,
			bool? isEmpty = null)
		{
			name = name ?? Name.Text;
			nodes = nodes ?? Nodes;
			attributes = attributes ?? Attributes;

			if ((isEmpty ?? false) && nodes.Count == 0)
				return new EmptyElementSyntax(
					StartTag.LessThan,
					StartTag.Name.With(name),
					attributes,
					SlashToken.Default,
					EndTag.GreaterThan);

			if (name.Equals(Name.Text) &&
				(attributes.Equals(Attributes) || attributes.SequenceEqual(Attributes)) &&
				(nodes.Equals(Nodes) || nodes.SequenceEqual(Nodes)))
				return this;

			return new ElementSyntax(
				StartTag.With(StartTag.Name.With(name), attributes),
				nodes,
				EndTag.With(EndTag.Name.With(name)));
		}

		public override void Write(TextWriter writer)
		{
			StartTag.Write(writer);
			foreach (var node in Nodes)
				node.Write(writer);
			EndTag.Write(writer);
		}
	}
}