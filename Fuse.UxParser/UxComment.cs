using System;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxComment : UxNode
	{
		CommentSyntax _syntax;

		public UxComment(CommentSyntax syntax)
		{
			_syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public string Value
		{
			get => _syntax.Value.Text;
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
					_syntax = _syntax.With(value);
					SetDirty();
				}
			}
		}

		public override NodeSyntax Syntax => _syntax;
		public override XmlNodeType NodeType => XmlNodeType.Comment;

		internal override UxNode DetachedNodeClone()
		{
			return new UxComment(_syntax);
		}
	}
}