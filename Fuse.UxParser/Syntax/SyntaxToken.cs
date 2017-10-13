using System.IO;

namespace Fuse.UxParser.Syntax
{
	// TODO: it's possible that the tokens should be structs
	public abstract class SyntaxToken
	{
		protected SyntaxToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
		{
			LeadingTrivia = leadingTrivia;
			TrailingTrivia = trailingTrivia;
		}

		public TriviaSyntax LeadingTrivia { get; }
		public abstract string Text { get; }
		public TriviaSyntax TrailingTrivia { get; }

		public int FullSpan => LeadingTrivia.Whitespace.Length + Text.Length + TrailingTrivia.Whitespace.Length;

		protected bool Equals(SyntaxToken other)
		{
			return LeadingTrivia.Equals(other.LeadingTrivia) && Text.Equals(other.Text) &&
				TrailingTrivia.Equals(other.TrailingTrivia);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((SyntaxToken) obj);
		}

		public override int GetHashCode()
		{
			var hashCode = -28710936;
			hashCode = hashCode * -1521134295 + LeadingTrivia.GetHashCode();
			hashCode = hashCode * -1521134295 + Text.GetHashCode();
			hashCode = hashCode * -1521134295 + TrailingTrivia.GetHashCode();
			return hashCode;
		}

		public void Write(TextWriter writer)
		{
			writer.Write(LeadingTrivia.Whitespace);
			writer.Write(Text);
			writer.Write(TrailingTrivia.Whitespace);
		}

		public override string ToString()
		{
			using (var sw = new StringWriter())
			{
				Write(sw);
				sw.Flush();
				return sw.ToString();
			}
		}
	}
}