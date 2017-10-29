using System;
using System.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	/// <summary>
	/// Represents an attribute with an (immutable) name.
	/// </summary>
	public class UxAttribute : UxObject
	{
		int _index;
		string _cachedUnescapedValue;

		public UxAttribute(AttributeSyntaxBase syntax)
		{
			Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public int AttributeIndex
		{
			get
			{
				// TODO: This is for debugging until tests for index updates are done
				var calculatedIndex = Parent?.Attributes.IndexOf(this) ?? -1;
				if (calculatedIndex != _index)
					throw new InvalidOperationException("Index updates have failed");
				// END

				return _index;
			}
			internal set => _index = value;
		}

		public UxElement Parent { get; internal set; }

		public AttributeSyntaxBase Syntax { get; private set; }

		public string Value
		{
			get => _cachedUnescapedValue ?? (_cachedUnescapedValue = Syntax.Value);
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (value != Value)
				{
					ReplaceSyntax(Syntax.With(newValue: value));
					_cachedUnescapedValue = value;
				}
			}
		}

		public string QualifiedName => Parent?.GetQualifiedName(Name, false) ??
			throw new InvalidOperationException("Can only provide qualified name when attached to an element.");

		public string Name => Syntax.Name.Text;
		public UxAttribute NextAttribute => Parent?.Attributes.ElementAtOrDefault(AttributeIndex + 1);
		public UxAttribute PreviousAttribute => AttributeIndex > 0 ? Parent?.Attributes[AttributeIndex - 1] : null;

		public override string ToString()
		{
			return Syntax.ToString();
		}

		internal void ReplaceSyntax(AttributeSyntaxBase updatedSyntax)
		{
			if (updatedSyntax == null) throw new ArgumentNullException(nameof(updatedSyntax));

			// Name is seen as an invariant of UxAttribute.
			if (Syntax.Name.Text != updatedSyntax.Name.Text)
				throw new InvalidOperationException("Only accepts updates from syntax with same name");

			var oldSyntax = Syntax;
			_cachedUnescapedValue = null;
			Syntax = updatedSyntax;
			Parent.SetDirty();
			(Parent as IUxContainerInternals)?.Changed?
				.Invoke(new UxReplaceAttributeChange(Parent.NodePath, AttributeIndex, oldSyntax, Syntax));
		}

		public bool Remove()
		{
			if (Parent == null)
				return false;

			Parent.Attributes.RemoveAt(AttributeIndex);
			return true;
		}
	}
}