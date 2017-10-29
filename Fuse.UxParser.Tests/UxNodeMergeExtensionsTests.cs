using System;
using System.Collections.Generic;
using Fuse.UxParser.Syntax;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	[TestFixture]
	public class UxNodeMergeExtensionsTests
	{
		[Test]
		public void Merge_repeatedly_with_random_permutations_to_ux_source_and_verify_tree_still_matches()
		{
			var source = ParseXml("<App />");
			var destination = ParseXml("<App />");

			var mutator = new RandomUxMutator(new Random(0x684bae7));
			for (int i = 0; i < 1000; i++)
			{
				mutator.Mutate(source);
				destination.Merge(source);
				Assert.That(destination.DeepEquals(source), Is.True);
			}
		}

		[TestCase("<A>Original text</A>", "<A>New text</A>")]
		[TestCase("<JavaScript>Original < script</JavaScript>", "<JavaScript>New script</JavaScript>")]
		[TestCase("<A><!-- Original comment --></A>", "<A><!-- New comment --></A>")]
		[TestCase("<A><![CDATA[ Original cdata ]]></A>", "<A><![CDATA[ New cdata ]]></A>")]
		[TestCase("<TheRoot />", "<!-- comment before root --><TheRoot />")]
		[TestCase("<A/>", "<A/>")]
		[TestCase("<A/>", "<B/>")]
		[TestCase("<A/>", "<A></A>")]
		[TestCase("<A></A>", "<A/>")]
		[TestCase("<A/>", "<A><B/></A>")]
		[TestCase("<A><B/></A>", "<A/>")]
		[TestCase("<A><B/></A>", "<A><C/></A>")]
		[TestCase(
			"<A/>",
			"<A>        <B/>    <!-- Bad indentation!! -->    </A>")]
		[TestCase(
			"<Root><A><B Foo=\"Bar\" /><B Foo=\"deadbeef\" /></A></Root>",
			"<Root><A><B Foo=\"Bar\" /><B Foo=\"MidInsert\" /><B Foo=\"deadbeef\" /></A></Root>")]
		[TestCase("<A Foo=\"Bar\" />", "<A Foo=\"Bar\" Chicken=\"Cow\" />")]
		[TestCase("<A Foo=\"Bar\" />", "<A     Foo=\"Bar\"    Chicken=\"Cow\"     />")]
		[TestCase("<A Foo=\"Bar\" />", "<A Foo=\"Moo\" Chicken=\"Cow\" />")]
		[TestCase("<A Foo=\"Bar\" />", "<A Foo=\"Moo Moo\" />")]
		[TestCase("<A></A>", "<A xmlns:foo=\"urn:foo\"><foo:Bar /></A>")]
		[TestCase("<Foo></Foo>", "<Bar></Bar>")]
		[Test]
		public void Merge_then_verify_equality_and_events(string before, string after)
		{
			var source = ParseXml(after);
			var destination = ParseXml(before);
			var patchDestination = ParseXml(before);
			var sourceBefore = source.ToString();
			var invertedChanges = new Stack<UxChange>();

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
				invertedChanges.Push(change.Invert());
			};

			Console.WriteLine("Destination:\r\n{0}\r\nSource:\r\n{1}", destination, source);
			destination.Merge(source);
			Console.WriteLine("Destination:\r\n{0}\r\nSource:\r\n{1}", destination, source);
			Assert.That(source.ToString(), Is.EqualTo(sourceBefore));
			Assert.That(destination.ToString(), Is.EqualTo(source.ToString()));
			Assert.That(source.DeepEquals(destination), Is.True);

			Assert.That(
				patchDestination.Syntax,
				Is.EqualTo(destination.Syntax),
				"Change stream did not recreate \"after\" syntax");

			var reversed = UxDocument.FromSyntax(patchDestination.Syntax);
			while (invertedChanges.Count > 0)
				invertedChanges.Pop().TryApply(reversed);

			Assert.That(
				reversed.Syntax,
				Is.EqualTo(ParseXml(before).Syntax),
				"Inverted change stream did not recreate \"before\" syntax");
			//Assert.That(actualEventCount, Is.EqualTo(expectedEvents.Count),
			//	"Fewer events than expected emitted by Changed event:\r\n" +
			//	string.Concat(expectedEvents.Select(x => string.Format("    {0}\r\n", x))));
		}

		static UxDocument ParseXml(string xml)
		{
			return UxDocument.Parse(xml);
		}

		class SyntaxConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				writer.WriteValue(value.ToString());
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				throw new NotSupportedException();
			}

			public override bool CanRead => false;

			public override bool CanConvert(Type objectType)
			{
				return typeof(SyntaxBase).IsAssignableFrom(objectType) ||
					objectType == typeof(UxNodePath);
			}
		}
	}
}