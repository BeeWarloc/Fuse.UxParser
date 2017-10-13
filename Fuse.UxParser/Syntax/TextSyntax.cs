using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class TextSyntax : NodeSyntax
	{
		public TextSyntax(EncodedTextToken value)
		{
			Value = value;
		}

		[NodeChild(0)]
		public EncodedTextToken Value { get; }

		public override int FullSpan => Value.FullSpan;

		public override TriviaSyntax LeadingTrivia => Value.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => Value.TrailingTrivia;

		public override void Write(TextWriter writer)
		{
			Value.Write(writer);
		}
	}
}