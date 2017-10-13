using System.Xml;
using System.Xml.Linq;

namespace XmlExperimentation
{
	internal struct ElementWriter
	{
		readonly XmlWriter _writer;
		readonly VeryStrangeWriterWrapper _textWriter;
		NamespaceResolver _resolver;

		public ElementWriter(XmlWriter writer, VeryStrangeWriterWrapper textWriter)
		{
			_writer = writer;
			_textWriter = textWriter;
			_resolver = new NamespaceResolver();
		}

		public void WriteElement(XElement element)
		{
			PushAncestors(element);

			WriteElementInner(element);
		}

		void WriteElementInner(XElement element)
		{
			WriteStartElement(element);
			if (element.IsEmpty)
			{
				// Closed tag
				WriteEndElement();
				return;
			}

			var child = element.FirstNode;
			if (child == null)
			{
				// Open tag with no (0 length) content
				_writer.WriteString(string.Empty);
				WriteFullEndElement();
				return;
			}

			while (child != null)
			{
				var childElement = child as XElement;
				if (childElement != null)
					WriteElement(childElement);
				else
					child.WriteTo(_writer);
				child = child.NextNode;
			}
		}

		const string xmlPrefixNamespace = "http://www.w3.org/XML/1998/namespace";
		const string xmlnsPrefixNamespace = "http://www.w3.org/2000/xmlns/";


		string GetPrefixOfNamespace(XNamespace ns, bool allowDefaultNamespace)
		{
			var namespaceName = ns.NamespaceName;
			if (namespaceName.Length == 0) return string.Empty;
			var prefix = _resolver.GetPrefixOfNamespace(ns, allowDefaultNamespace);
			if (prefix != null) return prefix;
			if (namespaceName == xmlPrefixNamespace) return "xml";
			if (namespaceName == xmlnsPrefixNamespace) return "xmlns";
			return null;
		}

		void PushAncestors(XElement e)
		{
			foreach (var ancestor in e.Ancestors())
				AddNamespaces(ancestor);
		}

		void PushElement(XElement e)
		{
			_resolver.PushScope();
			AddNamespaces(e);
		}

		void AddNamespaces(XElement e)
		{
			foreach (var a in e.Attributes())
				if (a.IsNamespaceDeclaration)
					_resolver.Add(a.Name.NamespaceName.Length == 0 ? string.Empty : a.Name.LocalName, XNamespace.Get(a.Value));
		}

		void WriteEndElement()
		{
			_writer.WriteEndElement();
			_resolver.PopScope();
		}

		void WriteFullEndElement()
		{
			_writer.WriteFullEndElement();
			_resolver.PopScope();
		}

		void WriteStartElement(XElement e)
		{
			PushElement(e);
			var ns = e.Name.Namespace;
			_writer.WriteStartElement(GetPrefixOfNamespace(ns, true), e.Name.LocalName, ns.NamespaceName);
			// Insert element trivia here
			for (var a = e.FirstAttribute; a != null; a = a.NextAttribute)
			{
				ns = a.Name.Namespace;
				var localName = a.Name.LocalName;
				var namespaceName = ns.NamespaceName;
				// Insert attribute trivia here
				_writer.Flush();
				//Console.WriteLine(_writer.WriteState);
				var precedingWhitespace = a.Annotation<InternalTagWhitespace>();
				if (precedingWhitespace != null)
					_textWriter.SetNextWhitespace(precedingWhitespace.Value);
				//Console.WriteLine(_writer.WriteState);
				_writer.WriteAttributeString(
					GetPrefixOfNamespace(ns, false),
					localName,
					namespaceName.Length == 0 && localName == "xmlns" ? xmlnsPrefixNamespace : namespaceName,
					a.Value);
			}
		}
	}
}