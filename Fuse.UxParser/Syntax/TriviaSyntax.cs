using System;

namespace Fuse.UxParser.Syntax
{
	public struct TriviaSyntax
	{
		readonly string _whitespace;

		public TriviaSyntax(string whitespace)
		{
			_whitespace = whitespace ?? throw new ArgumentNullException(nameof(whitespace));
		}

		public string Whitespace => _whitespace ?? string.Empty;

		public static TriviaSyntax Empty { get; } = new TriviaSyntax();
		public static TriviaSyntax Space { get; } = new TriviaSyntax(" ");

		public bool Equals(TriviaSyntax other)
		{
			return string.Equals(_whitespace, other._whitespace);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TriviaSyntax && Equals((TriviaSyntax) obj);
		}

		public override int GetHashCode()
		{
			return Whitespace != null ? Whitespace.GetHashCode() : 0;
		}

		public static bool operator ==(TriviaSyntax left, TriviaSyntax right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TriviaSyntax left, TriviaSyntax right)
		{
			return !left.Equals(right);
		}
	}
}