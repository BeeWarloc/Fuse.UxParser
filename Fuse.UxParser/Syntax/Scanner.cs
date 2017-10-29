using System;
using System.Collections.Generic;

namespace Fuse.UxParser.Syntax
{
	/// <summary>
	///     NOTE: Right now implemented over a string, could be reimplemented to work with a stream if needed
	/// </summary>
	public class Scanner
	{
		readonly Dictionary<string, string> _parseDict = new Dictionary<string, string>();

		readonly string _str;
		int _pos;

		public Scanner(string str)
		{
			_str = str;
		}

		void BackTrack(int commitPos)
		{
			_pos = commitPos;
		}

		public ParseScope Scope()
		{
			return new ParseScope(this);
		}

		public int Peek()
		{
			return _pos < _str.Length ? _str[_pos] : -1;
		}

		public bool ScanUntilMatch(string match, out string result)
		{
			var matchPos = _str.IndexOf(match, _pos, StringComparison.Ordinal);
			if (matchPos == -1)
			{
				result = null;
				return false;
			}
			result = _str.Substring(_pos, matchPos - _pos);
			_pos = matchPos;

			Deduplicate(ref result);

			return true;
		}

		public bool ScanOneOrMore(Func<char, bool> predicate, out string result)
		{
			// TODO: make it buffer!
			var endPos = _pos;
			while (endPos < _str.Length && predicate(_str[endPos]))
				endPos++;
			if (_pos == endPos)
			{
				result = null;
				return false;
			}

			result = _str.Substring(_pos, endPos - _pos);
			_pos = endPos;

			Deduplicate(ref result);

			// Always succeeds, there's always zero available
			return true;
		}

		public bool ScanZeroOrMore(Func<char, bool> predicate, out string result)
		{
			// TODO: make it buffer!
			var endPos = _pos;
			while (endPos < _str.Length && predicate(_str[endPos]))
				endPos++;
			if (_pos == endPos)
			{
				result = string.Empty;
				return true;
			}
			result = _str.Substring(_pos, endPos - _pos);
			_pos = endPos;

			Deduplicate(ref result);

			// Always succeeds, there's always zero available
			return true;
		}

		public bool Scan(string match)
		{
			if (string.CompareOrdinal(match, 0, _str, _pos, match.Length) == 0)
			{
				_pos += match.Length;
				return true;
			}
			return false;
		}

		public bool Scan(char match)
		{
			if (_pos < _str.Length && _str[_pos] == match)
			{
				_pos++;
				return true;
			}
			return false;
		}

		public bool ScanOne(Func<char, bool> predicate, out char c)
		{
			if (_pos < _str.Length && predicate(_str[_pos]))
			{
				c = _str[_pos++];
				return true;
			}
			c = default(char);
			return false;
		}

		public void ThrowError()
		{
			throw new NotImplementedException("TODO: Handle syntax error: " + _str.Substring(_pos) + "(EOF)");
		}


		public class ParseScope : IDisposable
		{
			readonly Scanner _scanner;
			int _rollbackPos;

			internal ParseScope(Scanner scanner)
			{
				_scanner = scanner;
				_rollbackPos = _scanner._pos;
			}

			public void Dispose()
			{
				if (_rollbackPos >= 0)
					_scanner.BackTrack(_rollbackPos);
			}

			public void Commit()
			{
				_rollbackPos = -1;
			}
		}

		void Deduplicate(ref string result)
		{
			if (result.Length < 42)
				if (_parseDict.TryGetValue(result, out var dedupedResult))
					result = dedupedResult;
				else
					_parseDict[result] = result;
		}
	}
}