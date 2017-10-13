using System.IO;
using System.Text;

namespace XmlExperimentation
{
	public class VeryStrangeWriterWrapper : TextWriter
	{
		readonly TextWriter _inner;
		string _nextWhitespace;

		public VeryStrangeWriterWrapper(TextWriter inner)
		{
			_inner = inner;
		}

		public override Encoding Encoding => _inner.Encoding;

		public void SetNextWhitespace(string ws)
		{
			_nextWhitespace = ws;
		}

		public override void Write(char value)
		{
			if (_nextWhitespace != null && value == ' ' || value == '/' || value == '>')
			{
				_inner.Write(_nextWhitespace);
				_nextWhitespace = null;
				return;
			}

			_inner.Write(value);
		}
	}
}