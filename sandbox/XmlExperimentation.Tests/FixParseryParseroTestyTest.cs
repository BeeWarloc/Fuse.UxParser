using NUnit.Framework;
using XmlExperimentation;

namespace XmlPatchStreams.Tests
{
	[TestFixture]
	public class FixParseryParseroTestyTest
	{
		[Test]
		public void BackAndForthIsJustAsFar()
		{
			var original = "<Monkey    Foo=\"Bar&#xE343;\"     \r\n     Moo=\"Cow\"></Monkey>";
			var doc = FixParseryParsero.Parse(original);
			Assert.That(FixParseryParsero.ToString(doc), Is.EqualTo(original));
		}
	}
}