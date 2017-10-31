using System;
using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class CDataSyntax : NodeSyntax
	{
		CDataSyntax(CDataStartToken start, EncodedTextToken value, CDataEndToken end)
		{
			Start = start ?? throw new ArgumentNullException(nameof(start));
			Value = value ?? throw new ArgumentNullException(nameof(value));
			End = end ?? throw new ArgumentNullException(nameof(end));
		}

		public static CDataSyntax Create(CDataStartToken start, EncodedTextToken value, CDataEndToken end)
		{
			return new CDataSyntax(start, value, end);
		}

		[NodeChild(0)]
		public CDataStartToken Start { get; }

		[NodeChild(1)]
		public EncodedTextToken Value { get; }

		[NodeChild(2)]
		public CDataEndToken End { get; }

		public override int FullSpan => Start.FullSpan + Value.FullSpan + End.FullSpan;

		public override TriviaSyntax LeadingTrivia => Start.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => End.TrailingTrivia;

		public override void Write(TextWriter writer)
		{
			Start.Write(writer);
			Value.Write(writer);
			End.Write(writer);
		}
	}
}