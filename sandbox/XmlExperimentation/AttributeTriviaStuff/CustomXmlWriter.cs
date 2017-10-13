using System.IO;
using System.Text;
using System.Xml;

// ReSharper disable UsePatternMatching

// ReSharper disable InconsistentNaming

namespace XmlExperimentation
{
	public class CustomXmlWriter : XmlTextWriter
	{
		public CustomXmlWriter(Stream w, Encoding encoding) : base(w, encoding) { }

		public CustomXmlWriter(TextWriter w) : base(w) { }

		public CustomXmlWriter(string filename, Encoding encoding) : base(filename, encoding) { }
	}


	// Try some dynamic proxy interception for figuring out how XmlReader is used when loading XElement


	// From dotnet sources
	// TODO: ADD LICENSING NOTICE IF USING THIS


	// From dotnet sources
	// With modifications to avoid using internal fields of XElement
	// TODO: ADD LICENSING NOTICE IF USING THIS
}