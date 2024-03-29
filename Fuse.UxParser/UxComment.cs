﻿using System;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxComment : UxNode
	{
		public UxComment(CommentSyntax syntax)
		{
			Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public string Value
		{
			get => Syntax.Value.Text;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				// TODO: Validate that text is valid for comment, ie does not contain "-->"
				// throw an exception if not.
				if (value.Contains("-->"))
					throw new ArgumentException("Value must not contain comment end indicator");

				if (Value != value)
					ReplaceSyntax(Syntax.With(value));
			}
		}

		protected override NodeSyntax NodeSyntax => Syntax;
		public new CommentSyntax Syntax { get; set; }

		public override XmlNodeType NodeType => XmlNodeType.Comment;

		internal override UxNode DetachedNodeClone()
		{
			return new UxComment(Syntax);
		}

		public override void ReplaceSyntax(NodeSyntax newSyntax)
		{
			if (newSyntax == null) throw new ArgumentNullException(nameof(newSyntax));
			if (!(newSyntax is CommentSyntax newCommentSyntax))
				throw new ArgumentException("UxText can only have its syntax replaced by CommentSyntax object");

			var oldSyntax = Syntax;
			Syntax = newCommentSyntax;
			SetDirty();

			Container?.Changed?.Invoke(new UxReplaceNodeChange(NodePath, oldSyntax, newCommentSyntax));
		}
	}
}