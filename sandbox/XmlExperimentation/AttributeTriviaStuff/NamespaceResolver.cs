using System.Xml.Linq;

namespace XmlExperimentation
{
	internal struct NamespaceResolver
	{
		class NamespaceDeclaration
		{
			public XNamespace ns;
			public string prefix;
			public NamespaceDeclaration prev;
			public int scope;
		}

		int _scope;
		NamespaceDeclaration _declaration;
		NamespaceDeclaration _rover;

		public void PushScope()
		{
			_scope++;
		}

		public void PopScope()
		{
			var d = _declaration;
			if (d != null)
				do
				{
					d = d.prev;
					if (d.scope != _scope) break;
					if (d == _declaration)
						_declaration = null;
					else
						_declaration.prev = d.prev;
					_rover = null;
				} while (d != _declaration && _declaration != null);
			_scope--;
		}

		public void Add(string prefix, XNamespace ns)
		{
			var d = new NamespaceDeclaration();
			d.prefix = prefix;
			d.ns = ns;
			d.scope = _scope;
			if (_declaration == null)
				_declaration = d;
			else
				d.prev = _declaration.prev;
			_declaration.prev = d;
			_rover = null;
		}

		public void AddFirst(string prefix, XNamespace ns)
		{
			var d = new NamespaceDeclaration();
			d.prefix = prefix;
			d.ns = ns;
			d.scope = _scope;
			if (_declaration == null)
			{
				d.prev = d;
			}
			else
			{
				d.prev = _declaration.prev;
				_declaration.prev = d;
			}
			_declaration = d;
			_rover = null;
		}

		// Only elements allow default namespace declarations. The rover 
		// caches the last namespace declaration used by an element.
		public string GetPrefixOfNamespace(XNamespace ns, bool allowDefaultNamespace)
		{
			if (_rover != null && _rover.ns == ns && (allowDefaultNamespace || _rover.prefix.Length > 0)) return _rover.prefix;
			var d = _declaration;
			if (d != null)
				do
				{
					d = d.prev;
					if (d.ns == ns)
					{
						var x = _declaration.prev;
						while (x != d && x.prefix != d.prefix)
							x = x.prev;
						if (x == d)
							if (allowDefaultNamespace)
							{
								_rover = d;
								return d.prefix;
							}
							else if (d.prefix.Length > 0)
							{
								return d.prefix;
							}
					}
				} while (d != _declaration);
			return null;
		}
	}
}