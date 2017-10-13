using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public static class FixParseryParsero
	{
		public static XDocument Parse(string xml)
		{
			using (var textReader = new StringReader(xml))
			using (var xmlReader = XmlReaderInterceptor.CreateReader(textReader))
			{
				var xdoc = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
				if (!InternalTagWhitespace.TryInjectToElements(xml, xdoc.Root))
					throw new InvalidOperationException("Injection not working!");
				return xdoc;
			}
		}

		public static string ToString(XDocument doc)
		{
			var sb = new StringBuilder();
			using (var textWriter = new VeryStrangeWriterWrapper(new StringWriter(sb)))
			using (var xmlWriter = new XmlTextWriter(textWriter))
			{
				var elementWriter = new ElementWriter(xmlWriter, textWriter);
				elementWriter.WriteElement(doc.Root);
			}
			return sb.ToString();
		}
	}
}