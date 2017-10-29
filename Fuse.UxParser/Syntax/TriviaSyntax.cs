using System;

namespace Fuse.UxParser.Syntax
{
	public struct TriviaSyntax : ISyntax
	{
		readonly string _whitespace;

		TriviaSyntax(string whitespace)
		{
			_whitespace = whitespace ?? throw new ArgumentNullException(nameof(whitespace));
		}

		public static TriviaSyntax Create(string whitespace)
		{
			if (string.IsNullOrEmpty(whitespace))
				return Empty;
			if (whitespace == " ")
				return Space;
			return new TriviaSyntax(whitespace);
		}

		public string Whitespace => _whitespace ?? string.Empty;

		public static TriviaSyntax Empty { get; } = new TriviaSyntax();
		public static TriviaSyntax Space { get; } = new TriviaSyntax(" ");
		public bool IsEmpty => Whitespace.Length == 0;

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

		public int FullSpan => Whitespace.Length;
	}
}