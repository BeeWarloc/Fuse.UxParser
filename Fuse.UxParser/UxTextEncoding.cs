using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace Fuse.UxParser
{
	// http://cs.lmu.edu/~ray/notes/xmlgrammar/
	// TODO: Equality override

	public static class UxTextEncoding
	{
		static readonly Dictionary<string, char> _entityToChar;
		static readonly Dictionary<char, string> _charToEntity;

		static UxTextEncoding()
		{
			_entityToChar = new Dictionary<string, char>
			{
				{ "quot", '"' },
				{ "amp", '&' },
				{ "apos", '\'' },
				{ "lt", '<' },
				{ "gt", '>' }
			};

			_charToEntity = _entityToChar.ToDictionary(x => x.Value, x => x.Key);
		}

		static string Encode(string str, string encodedChars)
		{
			StringBuilder sb = null;
			var i = 0;

			Action<string> append = c =>
			{
				if (sb == null)
				{
					// Avoid creating StringBuilder until we have to
					sb = new StringBuilder(str.Length * 2);
					if (i > 0)
						sb.Append(str, 0, i);
				}
				sb.Append(c);
			};

			while (i < str.Length)
			{
				var c = str[i];
				if (encodedChars.IndexOf(c) != -1)
				{
					string entity;
					if (!_charToEntity.TryGetValue(c, out entity))
						entity = string.Format("x{0}", (int) c);
					append("&");
					append(entity);
					append(";");
				}
				else if (sb != null)
				{
					sb.Append(c);
				}
				i++;
			}

			return sb != null ? sb.ToString() : str;
		}

		public static string Unescape(string encoded)
		{
			return Unescape(encoded, 0, encoded.Length);
		}

		public static string Unescape(string encoded, int startOffset, int length)
		{
			if (startOffset < 0)
				throw new ArgumentOutOfRangeException(nameof(startOffset));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length));
			if (startOffset > encoded.Length)
				throw new ArgumentOutOfRangeException(nameof(startOffset), "Specified start offset is beyond string range");
			var endOffset = startOffset + length;
			if (endOffset > encoded.Length)
				throw new ArgumentOutOfRangeException(nameof(length), "Length goes beyond end of string");

			// If there's no & characters in string there's nothing to decode
			if (startOffset == 0 && length == encoded.Length && encoded.IndexOf('&') == -1)
				return encoded;
			var sb = new StringBuilder(length * 2);

			var i = startOffset;
			while (i < endOffset)
			{
				var c = encoded[i];
				int entityEnd;
				if (c == '&' && (entityEnd = encoded.IndexOf(';', i)) != -1)
					if (encoded[i + 1] == '#')
					{
						var numstart = i + 2;
						var numlen = entityEnd - (i + 2);
						int code;
						if (numlen > 0 && int.TryParse(encoded.Substring(numstart, numlen), out code))
						{
							sb.Append(char.ConvertFromUtf32(code));
							i = entityEnd + 1;
							continue;
						}
					}
					else if (encoded[i + 1] == 'x')
					{
						var numstart = i + 2;
						var numlen = entityEnd - (i + 2);
						int code;
						if (numlen > 0 && int.TryParse(
							encoded.Substring(numstart, numlen),
							NumberStyles.HexNumber,
							CultureInfo.InvariantCulture,
							out code))
						{
							sb.Append(char.ConvertFromUtf32(code));
							i = entityEnd + 1;
							continue;
						}
					}
					else
					{
						var inner = encoded.Substring(i + 1, entityEnd - (i + 1));
						char decodedChar;
						if (_entityToChar.TryGetValue(inner, out decodedChar))
						{
							sb.Append(decodedChar.ToString());
							i = entityEnd + 1;
							continue;
						}
					}

				sb.Append(c);
				i++;
			}

			return sb.ToString();
		}

		public static string EncodeAttribute(string value, char quoteChar)
		{
			// TODO: Handling of quotechar
			switch (quoteChar)
			{
				case '\'':
					return Encode(value, "&'");
				case '"':
					return Encode(value, "&\"");
				default:
					throw new ArgumentException("Not a valid quote character", nameof(quoteChar));
			}
		}

		public static string EncodeText(string value)
		{
			return Encode(value, "&<");
		}
	}
}