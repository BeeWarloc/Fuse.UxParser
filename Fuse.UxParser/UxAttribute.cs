using System;
using System.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxMutAttribute : UxMutObject
	{
		int _index;
		string _unescapedValue;

		public UxMutAttribute(AttributeSyntaxBase syntax)
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
			get => _unescapedValue ?? (_unescapedValue = Syntax.Value);
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (value != Value)
				{
					Syntax = Syntax.With(newValue: value);
					_unescapedValue = value;
					Parent.SetDirty();
				}
			}
		}

		public string Name => Syntax.Name.Text;
		public UxMutAttribute NextAttribute => Parent?.Attributes.ElementAtOrDefault(AttributeIndex + 1);
		public UxMutAttribute PreviousAttribute => AttributeIndex > 0 ? Parent?.Attributes[AttributeIndex - 1] : null;

		public override string ToString()
		{
			return Syntax.ToString();
		}

		public void Remove()
		{
			EnsureAttached();
			Parent.Attributes.RemoveAt(AttributeIndex);
		}

		void EnsureAttached()
		{
			if (Parent == null)
				throw new InvalidOperationException("TODO PROPER ERROR");
		}
	}
}