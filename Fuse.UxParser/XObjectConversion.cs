using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public static class XObjectConversion
	{
		//XmlNamespaceManager nsMgr = new XmlNamespaceManager((XmlNameTable)new SimpleNameTable());
		//nsMgr.AddNamespace("ux", "http://schemas.fusetools.com/ux");
		//nsMgr.AddNamespace("dep", "http://schemas.fusetools.com/dep");
		//nsMgr.AddNamespace("", "Fuse, Fuse.Reactive, Fuse.Selection, Fuse.Animations, Fuse.Drawing, Fuse.Entities, Fuse.Controls, Fuse.Layouts, Fuse.Elements, Fuse.Effects, Fuse.Triggers, Fuse.Navigation, Fuse.Triggers.Actions, Fuse.Gestures, Fuse.Resources, Fuse.Native, Fuse.Physics, Fuse.Vibration, Fuse.Motion, Fuse.Testing, Uno.UX");
		//return new XmlParserContext((XmlNameTable) null, nsMgr, (string) null, XmlSpace.Default);

		static Dictionary<string, XNamespace> DefaultPrefixToNamespaceMap { get; } =
			Constants.DefaultNamespacePrefixes
				.Select(x => new { prefix = x.Key, ns = XNamespace.Get(x.Value) })
				.Concat(new[] { new { prefix = "xmlns", ns = XNamespace.Xmlns } })
				.Concat(new[] { new { prefix = "xml", ns = XNamespace.Xml } })
				.ToDictionary(x => x.prefix, x => x.ns);

		static XNamespace DefaultFuseNamespace { get; } = XNamespace.Get(Constants.DefaultFuseNamespaceList);

		public static XDocument ToXml(this DocumentSyntax syntax, bool annotateWithSyntax = false)
		{
			if (annotateWithSyntax)
				throw new NotImplementedException();
			var xDocument = Convert(
				syntax,
				prefix =>
				{
					if (string.IsNullOrEmpty(prefix))
						return DefaultFuseNamespace;

					if (!DefaultPrefixToNamespaceMap.TryGetValue(prefix, out var ns))
						throw new InvalidOperationException("Unrecognized namespace prefix");
					return ns;
				});
			return xDocument;
		}

		static XDocument Convert(DocumentSyntax syntax, Func<string, XNamespace> prefixToNamespace)
		{
			var xdoc = new XDocument();
			//xdoc.GetType()
			//	.GetMethod("SetBaseUri", BindingFlags.NonPublic | BindingFlags.Instance)
			//	.Invoke(xdoc, new object[] { DefaultFuseNamespace.NamespaceName });
			foreach (var nodeSyntax in syntax.Nodes)
				ConvertAndAddNode(nodeSyntax, prefixToNamespace, xdoc);
			return xdoc;
		}

		static void ConvertAndAddNodes(
			XContainer container,
			IImmutableList<NodeSyntax> nodeSyntaxList,
			Func<string, XNamespace> prefixToNamespace)
		{
			foreach (var nodeSyntax in nodeSyntaxList)
				ConvertAndAddNode(nodeSyntax, prefixToNamespace, container);
		}

		static void ConvertAndAddNode(NodeSyntax syntax, Func<string, XNamespace> prefixToNamespace, XContainer container)
		{
			switch (syntax)
			{
				case InlineScriptElementSyntax script:
					container.Add(new XCData(script.Script.Text));
					break;
				case ElementSyntaxBase element:
					ConvertAndAddElement(element, prefixToNamespace, container);
					break;
				case CDataSyntax cdata:
					container.Add(new XCData(cdata.Value.Text));
					break;
				case TextSyntax text:
					container.Add(new XText(text.Value.Text));
					break;
				case CommentSyntax comment:
					container.Add(new XComment(comment.Value.Text));
					break;
				default:
					throw new InvalidOperationException("Syntax " + syntax.GetType() + " not recognized");
			}
		}

		static bool TryExtractPrefixAndLocalName(string name, out string prefix, out string localName)
		{
			var prefixEnd = name.IndexOf(':');
			if (prefixEnd == -1)
			{
				prefix = null;
				localName = null;
				return false;
			}
			prefix = name.Substring(0, prefixEnd);
			localName = name.Substring(prefixEnd + 1);
			return true;
		}

		static void ConvertAndAddElement(
			ElementSyntaxBase syntax,
			Func<string, XNamespace> prefixToNamespace,
			XContainer parent)
		{
			prefixToNamespace = AddNamespaceDeclarations(syntax, prefixToNamespace);

			var element = new XElement(ResolveName(prefixToNamespace, syntax.Name.Text, true));

			foreach (var attrSyntax in syntax.Attributes)
				element.Add(new XAttribute(ResolveName(prefixToNamespace, attrSyntax.Name.Text, false), attrSyntax.Value));

			ConvertAndAddNodes(
				element,
				syntax.Nodes,
				prefixToNamespace);

			parent?.Add(element);
		}

		static XName ResolveName(Func<string, XNamespace> prefixToNamespace, string name, bool searchDefaultNamespaces)
		{
			if (TryExtractPrefixAndLocalName(name, out var prefix, out var localName))
				return prefixToNamespace(prefix).GetName(localName);

			var ns = searchDefaultNamespaces ? (prefixToNamespace(null) ?? XNamespace.None) : XNamespace.None;
			return ns.GetName(name);
		}

		static Func<string, XNamespace> AddNamespaceDeclarations(
			ElementSyntaxBase syntax,
			Func<string, XNamespace> prefixToNamespace)
		{
			foreach (var attrSyntax in syntax.Attributes)
			{
				var attrKey = attrSyntax.Name.Text;
				var attrNsDeclStart = "xmlns:";
				if (attrKey.StartsWith(attrNsDeclStart))
				{
					var nsPrefix = attrKey.Substring(attrNsDeclStart.Length);
					var ns = XNamespace.Get(attrSyntax.Value);
					var next = prefixToNamespace;
					prefixToNamespace = p => p == nsPrefix ? ns : next(p);
				}
				else if (attrKey == "xmlns")
				{
					var ns = XNamespace.Get(attrSyntax.Value);
					var next = prefixToNamespace;
					prefixToNamespace = p => string.IsNullOrEmpty(p) ? ns : next(p);
				}
			}
			return prefixToNamespace;
		}
	}
}