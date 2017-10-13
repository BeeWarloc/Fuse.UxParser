using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class CommentSyntax : NodeSyntax
	{
		public CommentSyntax(CommentStartToken start, EncodedTextToken value, CommentEndToken end)
		{
			Start = start;
			Value = value;
			End = end;
		}

		[NodeChild(0)]
		public CommentStartToken Start { get; }

		// TODO: Use another token
		[NodeChild(1)]
		public EncodedTextToken Value { get; }

		[NodeChild(2)]
		public CommentEndToken End { get; }

		public override int FullSpan => Start.FullSpan + Value.FullSpan + End.FullSpan;

		public override TriviaSyntax LeadingTrivia => Start.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => End.TrailingTrivia;

		public override void Write(TextWriter writer)
		{
			Start.Write(writer);
			Value.Write(writer);
			End.Write(writer);
		}

		public CommentSyntax With(string value)
		{
			return new CommentSyntax(Start, new EncodedTextToken(value), End);
		}
	}
}