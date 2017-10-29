using System;
using System.Collections.Immutable;
using System.IO;

namespace Fuse.UxParser.Syntax
{
	// Not very happy how this turned out... Should def change this
	public class InlineScriptElementSyntax : ElementSyntaxBase
	{
		public InlineScriptElementSyntax(ElementStartTagSyntax startTag, ScriptToken script, ElementEndTagSyntax endTag)
		{
			StartTag = startTag;
			Script = script;
			EndTag = endTag;
		}

		[NodeChild(0)]
		public ElementStartTagSyntax StartTag { get; }

		[NodeChild(1)]
		public ScriptToken Script { get; }

		[NodeChild(2)]
		public ElementEndTagSyntax EndTag { get; }


		public override bool IsEmpty => false;
		public override NameToken Name => StartTag.Name;
		public override IImmutableList<AttributeSyntaxBase> Attributes => StartTag.Attributes;
		public override IImmutableList<NodeSyntax> Nodes => ImmutableList<NodeSyntax>.Empty;
		public override int DescendantElementCount => 0;

		public override int FullSpan => StartTag.FullSpan + Script.FullSpan + EndTag.FullSpan;

		public override TriviaSyntax LeadingTrivia => StartTag.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => EndTag.TrailingTrivia;

		public override ElementSyntaxBase With(
			NameToken name = null,
			IImmutableList<AttributeSyntaxBase> attributes = null,
			IImmutableList<NodeSyntax> nodes = null,
			bool? isEmpty = null)
		{
			throw new NotImplementedException("Can't turn JavaScript tag into anything else");
		}

		public override void Write(TextWriter writer)
		{
			StartTag.Write(writer);
			Script.Write(writer);
			EndTag.Write(writer);
		}
	}
}