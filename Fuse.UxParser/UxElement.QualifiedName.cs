using System;
using System.Linq;

namespace Fuse.UxParser
{
	public partial class UxElement
	{
		// Might want to cache the qualified name, but have to remember to invalidate this if parent ns declarations changes
		public string QualifiedName => GetQualifiedName(Name, true);

		internal string GetQualifiedName(string name, bool useDefaultNamespace)
		{
			var prefixEnd = name.IndexOf(':');
			string ns = null;
			string localName = name;
			if (prefixEnd == -1)
			{
				if (useDefaultNamespace)
					ns = PrefixToNamespace(null);

				if (ns == null)
					return name;
			}
			else
			{
				var prefix = name.Substring(0, prefixEnd);
				localName = name.Substring(prefixEnd + 1);
				ns = PrefixToNamespace(prefix);

				if (ns == null)
					throw new InvalidOperationException("Unable to resolve namespace from prefix " + prefix);
			}

			return string.Format("{{{0}}}{1}", ns, localName);
		}

		string IUxContainerInternals.PrefixToNamespace(string prefix)
		{
			return PrefixToNamespace(prefix);
		}

		string PrefixToNamespace(string prefix)
		{
			if (string.IsNullOrEmpty(prefix))
				return Attributes.Where(x => x.Name == "xmlns").Select(x => x.Value).FirstOrDefault() ??
					Container?.PrefixToNamespace(prefix);

			var xmlnsAttrKey = "xmlns:" + prefix;
			return Attributes
					.Where(x => x.Name.Equals(xmlnsAttrKey))
					.Select(x => x.Value)
					.FirstOrDefault()
				?? Container?.PrefixToNamespace(prefix);
		}
	}
}