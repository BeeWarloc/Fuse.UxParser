using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public class InternalTagWhitespace
	{
		// TODODODODO
		static readonly Regex attributeRegex = new Regex(
			"<[^/>\\s]+(\\s+(([^/>]+=\"[^\"]*\")|([^/>]+=\'[^\']*\')))*?(?<endws>\\s*)/?>",
			RegexOptions.Compiled);

		public InternalTagWhitespace(string value)
		{
			Value = value;
		}

		public string Value { get; }

		public static bool TryInjectToElements(string source, XElement root)
		{
			var actions = new List<Action>();
			var lineMap = new LineMap(source);

			var success = true;
			foreach (var node in root.DescendantNodesAndSelf().OfType<XElement>())
			{
				foreach (var attr in node.Attributes())
				{
					int offset;
					if (lineMap.TryGetOffset(attr, out offset))
					{
						success = false;
						break;
					}

					// Line info now at beginning of attribute
					// Assertion below to check that assumption
					// Search backwards until match.
					var end = offset;
					while (offset > 0 && char.IsWhiteSpace(source[offset - 1]))
						offset--;

					var len = end - offset;
					if (len == 0)
						throw new InvalidOperationException(
							"Assumption failed: should always be at least one whitespace char before attribute!");

					// If it's just one whitespace character that will be implicit
					if (len == 1 && source[offset] == ' ')
						continue;

					actions.Add(
						() =>
						{
							attr.RemoveAnnotations<InternalTagWhitespace>();
							attr.AddAnnotation(new InternalTagWhitespace(source.Substring(offset, len)));
						});
				}

				if (!success)
					break;

				// TODO add any remaining whitespace annotation not preceding attribute.
				int elementStartOffset;
				if (!lineMap.TryGetOffset(node, out elementStartOffset))
				{
					var match = attributeRegex.Match(source, elementStartOffset);
					if (!match.Success)
					{
						success = false;
						break;
					}
					var endws = match.Groups["endws"];
					if (endws.Length > 0)
					{
						node.RemoveAnnotations<InternalTagWhitespace>();
						node.AddAnnotation(new InternalTagWhitespace(endws.Value));
					}
				}
			}
			foreach (var action in actions)
				action();
			return true;
		}

		class LineMap
		{
			readonly List<int> _lineToOffsetMap;
			readonly int _totalLength;

			public LineMap(string source)
			{
				_lineToOffsetMap = new List<int>();
				var sourceOffset = 0;
				do
				{
					_lineToOffsetMap.Add(sourceOffset);
					sourceOffset = source.IndexOf('\n', sourceOffset) + 1;
				} while (sourceOffset != 0 /* not -1, since we add 1 to the result of indexof */);
				_totalLength = source.Length;
			}

			public bool TryGetOffset(IXmlLineInfo lineInfo, out int offset)
			{
				offset = 0;
				if (!lineInfo.HasLineInfo())
					return false;

				var line = lineInfo.LineNumber - 1;
				var pos = lineInfo.LinePosition - 1;
				if (line == -1 || pos == -1 || line >= _lineToOffsetMap.Count)
					return false;

				offset = _lineToOffsetMap[line] + pos;
				if (offset >= _totalLength)
					return false;
				return true;
			}
		}
	}
}