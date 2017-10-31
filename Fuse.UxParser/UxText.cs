using System;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxText : UxNode
	{
		string _cachedUnescapedText;

		public UxText(TextSyntax syntax)
		{
			Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public string Value
		{
			get => _cachedUnescapedText ?? (_cachedUnescapedText = UxTextEncoding.Unescape(Syntax.Value.Text));
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
					var encodedText = UxTextEncoding.EncodeText(value);
					ReplaceSyntax(TextSyntax.Create(new EncodedTextToken(encodedText)), value);
				}
			}
		}

		protected override NodeSyntax NodeSyntax => Syntax;

		public new TextSyntax Syntax { get; set; }

		public override XmlNodeType NodeType => XmlNodeType.Text;

		internal override UxNode DetachedNodeClone()
		{
			return new UxText(Syntax);
		}

		public override void ReplaceSyntax(NodeSyntax newSyntax)
		{
			if (newSyntax == null) throw new ArgumentNullException(nameof(newSyntax));
			if (!(newSyntax is TextSyntax newTextSyntax))
				throw new ArgumentException("UxText can only have its syntax replaced by TextSyntax object");

			ReplaceSyntax(newTextSyntax, null);
		}

		void ReplaceSyntax(TextSyntax newSyntax, string unescapedText)
		{
			var oldSyntax = Syntax;
			_cachedUnescapedText = unescapedText;
			Syntax = newSyntax;
			SetDirty();

			Container?.Changed?.Invoke(new UxReplaceNodeChange(NodePath, oldSyntax, newSyntax));
		}
	}
}