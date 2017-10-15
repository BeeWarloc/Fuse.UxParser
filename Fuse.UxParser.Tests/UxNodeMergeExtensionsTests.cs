using System;
using System.Linq;
using System.Xml.Linq;
using Fuse.UxParser.Syntax;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	[TestFixture]
	public class UxNodeMergeExtensionsTests
	{
		static UxDocument ParseXml(string xml)
		{
			return UxDocument.Parse(xml);
		}

		static string EventToString(UxChange change)
		{
			throw new NotImplementedException();
		}

		class SyntaxConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				writer.WriteValue(value.ToString());
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotSupportedException();
			public override bool CanRead => false;
			public override bool CanConvert(Type objectType) =>
				typeof(SyntaxBase).IsAssignableFrom(objectType) ||
				objectType == typeof(UxNodePath);
		}

		// TODO: This test is in progress of being ported from using XElement to UxElement
		[TestCase("<TheRoot />", "<!-- comment before root --><TheRoot />")]
		[TestCase("<A/>", "<A/>")]
		[TestCase("<A/>", "<A></A>", "Value Element")]
		[TestCase("<A></A>", "<A/>", "Value Element")]
		[TestCase("<A/>", "<A><B/></A>", "Add Element")]
		[TestCase("<A><B/></A>", "<A><C/></A>", "Name Element")]
		[TestCase(
			"<A/>",
			"<A>        <B/>    <!-- Bad indentation!! -->    </A>",
			"Add Text, Add Element, Add Text, Add Comment, Add Text")]
		[TestCase(
			"<Root><A><B Foo=\"Bar\" /><B Foo=\"deadbeef\" /></A></Root>",
			"<Root><A><B Foo=\"Bar\" /><B Foo=\"MidInsert\" /><B Foo=\"deadbeef\" /></A></Root>",
			"Add Element")]
		[TestCase("<A Foo=\"Bar\" />", "<A Foo=\"Bar\" Chicken=\"Cow\" />", "Add Attribute")]
		[TestCase("<A Foo=\"Bar\" />", "<A     Foo=\"Bar\"    Chicken=\"Cow\"     />", "Add Attribute")]
		[TestCase("<A Foo=\"Bar\" />", "<A Foo=\"Moo\" Chicken=\"Cow\" />", "Value Attribute, Add Attribute")]
		[TestCase("<A Foo=\"Bar\" />", "<A Foo=\"Moo Moo\" />", "Value Attribute")]
		[TestCase("<A></A>", "<A xmlns:foo=\"urn:foo\"><foo:Bar /></A>", "Add Attribute, Add Element")]
		[TestCase("<Foo></Foo>", "<Bar></Bar>", "Name Element")]
		[Test]
		public void Merge_then_verify_equality_and_events(string before, string after, string expectedEventsString = "")
		{

			var source = ParseXml(after);
			var destination = ParseXml(before);
			var patchDestination = ParseXml(before);

			var sourceBefore = source.ToString();

			destination.Changed += change =>
			{
				Console.WriteLine(
					"Got change: {0}",
					JsonConvert.SerializeObject(
						change,
						Formatting.Indented,
						new JsonSerializerSettings
						{
							TypeNameHandling = TypeNameHandling.Objects,
							Converters = { new SyntaxConverter() }
						}));
				Assert.That(change.TryApply(patchDestination), Is.True, "Could not apply change to patch destination");
				Assert.That(patchDestination.Syntax, Is.EqualTo(destination.Syntax));
			};

			var expectedEvents = expectedEventsString
				.Split(',')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToList();

			var actualEventCount = 0;

			// Adding this check to make sure we're not changing the source by accident
			//source.Changing += (s, ev) => Assert.Fail("Source xml should not be changing.");

			//destination.Changed += (s, ev) =>
			//{
			//	var actualEvent = EventToString(s, ev);
			//	var expectedEvent = expectedEvents.ElementAtOrDefault(actualEventCount);
			//	Assert.That(expectedEvent, Is.Not.Null, "Got \"{0}\" event, but no more events were expected", actualEvent);
			//	Assert.That(actualEvent, Is.EqualTo(expectedEvent), string.Format("Expected event nr {0} to be \"{1}\", was \"{2}\"", actualEventCount, expectedEvent, actualEvent));
			//	actualEventCount++;
			//};

			Console.WriteLine("Destination:\r\n{0}\r\nSource:\r\n{1}", destination, source);
			destination.Merge(source);
			Console.WriteLine("Destination:\r\n{0}\r\nSource:\r\n{1}", destination, source);
			Assert.That(source.ToString(), Is.EqualTo(sourceBefore));
			Assert.That(destination.ToString(), Is.EqualTo(source.ToString()));
			Assert.That(source.DeepEquals(destination), Is.True);
			//Assert.That(actualEventCount, Is.EqualTo(expectedEvents.Count),
			//	"Fewer events than expected emitted by Changed event:\r\n" +
			//	string.Concat(expectedEvents.Select(x => string.Format("    {0}\r\n", x))));
		}
	}
}