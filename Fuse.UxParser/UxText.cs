using System;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxText : UxNode
	{
		TextSyntax _syntax;
		string _unescapedText;

		public UxText(TextSyntax syntax)
		{
			_syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public string Value
		{
			get => _unescapedText ?? (_unescapedText = UxTextEncoding.Unescape(_syntax.Value.Text));
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				// TODO: Validate that text is valid for comment, ie does not contain "-->"
				// throw an exception if not.
				if (value.Contains("-->"))
					throw new ArgumentException("Value must not contain comment end indicator");

				if (Value != value)
				{
					_syntax = new TextSyntax(new EncodedTextToken(UxTextEncoding.EncodeText(value)));
					_unescapedText = value;
					SetDirty();
				}
			}
		}

		public override NodeSyntax Syntax => _syntax;
		public override XmlNodeType NodeType => XmlNodeType.Text;

		internal override UxNode DetachedNodeClone()
		{
			return new UxText(_syntax);
		}
	}
}