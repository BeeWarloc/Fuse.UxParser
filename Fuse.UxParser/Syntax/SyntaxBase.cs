using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fuse.UxParser.Syntax
{
	public abstract class SyntaxBase
	{
		static readonly ConcurrentDictionary<Type, IList<Func<SyntaxBase, object>>> _childNodePropertyMaps =
			new ConcurrentDictionary<Type, IList<Func<SyntaxBase, object>>>();

		int? _cachedHashCode;

		public abstract TriviaSyntax LeadingTrivia { get; }

		public abstract TriviaSyntax TrailingTrivia { get; }

		public abstract int FullSpan { get; }

		internal IList<Func<SyntaxBase, object>> ChildNodePropertyGetters => _childNodePropertyMaps.GetOrAdd(
			GetType(),
			t => t.GetProperties()
				.Select(prop => new { nodeChildAttr = prop.GetCustomAttribute<NodeChildAttribute>(), prop })
				.Where(x => x.nodeChildAttr != null).OrderBy(x => x.nodeChildAttr.OrderIndex)
				.Select(x => new Func<SyntaxBase, object>(x.prop.GetValue)).ToList());

		internal IEnumerable<object> ChildNodePropertyValues => ChildNodePropertyGetters.Select(getter => getter(this));

		public IEnumerable<SyntaxToken> AllTokens
		{
			get
			{
				foreach (var getter in ChildNodePropertyGetters)
				{
					var child = getter(this);
					switch (child)
					{
						case SyntaxBase syntax:
							foreach (var innerToken in syntax.AllTokens)
								yield return innerToken;
							break;
						case SyntaxToken token:
							yield return token;
							break;
						case IEnumerable<SyntaxBase> syntaxList:
							foreach (var syntax in syntaxList)
							foreach (var innerToken in syntax.AllTokens)
								yield return innerToken;
							break;
						case IEnumerable<SyntaxToken> tokenList:
							foreach (var token in tokenList)
								yield return token;
							break;
						default:
							throw new InvalidOperationException("Unexpected type for child node property");
					}
				}
			}
		}

		public abstract void Write(TextWriter writer);

		public override bool Equals(object other)
		{
			if (ReferenceEquals(other, this))
				return true;

			if (other == null)
				return false;

			if (other.GetType() != GetType())
				return false;

			foreach (var getter in ChildNodePropertyGetters)
			{
				var thisChild = getter(this);
				var otherChild = getter((SyntaxBase) other);

				if (ReferenceEquals(thisChild, otherChild))
					continue;

				if (thisChild is IEnumerable<SyntaxBase> thisChildAsSyntaxList &&
					otherChild is IEnumerable<SyntaxBase> otherChildAsSyntaxList &&
					thisChildAsSyntaxList.SequenceEqual(otherChildAsSyntaxList))
					continue;

				if (thisChild is IEnumerable<SyntaxToken> thisChildAsTokenList &&
					otherChild is IEnumerable<SyntaxToken> otherChildAsTokenList &&
					thisChildAsTokenList.SequenceEqual(otherChildAsTokenList))
					continue;

				if (!thisChild.Equals(otherChild))
					return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (_cachedHashCode.HasValue)
				return _cachedHashCode.Value;

			var hashCode = -811868959;
			foreach (var getter in ChildNodePropertyGetters)
				hashCode = hashCode * -1521134295 + getter(this).GetHashCode();
			_cachedHashCode = hashCode;
			return hashCode;
		}

		public override string ToString()
		{
			using (var sw = new StringWriter())
			{
				Write(sw);
				sw.Flush();
				return sw.ToString();
			}
		}
	}
}