using System;

namespace Fuse.UxParser.Syntax
{
	public abstract class AttributeSyntaxBase : SyntaxBase
	{
		protected AttributeSyntaxBase(NameToken name)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		[NodeChild(0)]
		public NameToken Name { get; }

		/// <summary>
		///     The decoded attribute value
		/// </summary>
		public abstract string Value { get; }

		public abstract AttributeSyntaxBase With(string newName = null, string newValue = null);
	}
}