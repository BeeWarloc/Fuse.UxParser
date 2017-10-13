using System;
using System.IO;
using System.Xml;
using Castle.DynamicProxy;

namespace XmlExperimentation
{
	public class XmlReaderInterceptor : IInterceptor
	{
		static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

		int _disableInterception;

		public void Intercept(IInvocation invocation)
		{
			if (_disableInterception > 0)
			{
				invocation.Proceed();
				return;
			}

			var reader = (XmlTextReader) invocation.InvocationTarget;
			var preState = NoIntercept(() => reader.NodeType);
			var preLineInfo = string.Format("({0},{1})", reader.LineNumber, reader.LinePosition);
			invocation.Proceed();
			Console.WriteLine(
				"{0}     before {1} {2}   after {3} {4}",
				invocation.Method.Name,
				preState,
				preLineInfo,
				NoIntercept(() => reader.NodeType),
				string.Format("({0},{1})", reader.LineNumber, reader.LinePosition));
		}

		public static XmlReader CreateReader(TextReader reader)
		{
			reader = new LoggingTextReader(reader);
			return (XmlReader) _proxyGenerator.CreateClassProxy(
				typeof(XmlTextReader),
				new object[] { reader },
				new XmlReaderInterceptor());
		}

		public T NoIntercept<T>(Func<T> func)
		{
			try
			{
				_disableInterception++;
				return func();
			}
			finally
			{
				_disableInterception--;
			}
		}

		class LoggingTextReader : TextReader
		{
			readonly TextReader _inner;

			public LoggingTextReader(TextReader inner)
			{
				_inner = inner;
			}

			public override int Peek()
			{
				var retval = _inner.Peek();
				Console.WriteLine("Peek() -> '{0}'", retval == -1 ? "-1" : ((char) retval).ToString());
				return retval;
			}

			public override int Read()
			{
				var retval = _inner.Read();
				Console.WriteLine("Read() -> '{0}'", retval == -1 ? "-1" : ((char) retval).ToString());
				return retval;
			}

			public override int Read(char[] buffer, int index, int count)
			{
				var actualReadCount = _inner.Read(buffer, index, count);
				Console.WriteLine(
					"Read(char[{0}], {1}, {2}) -> \"{3}\"",
					buffer.Length,
					index,
					count,
					new string(buffer, index, actualReadCount));
				return actualReadCount;
			}
		}
	}
}