using System;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using XmlPatchStreams.Tests;

namespace XmlExperimentation.Tests
{
	[TestFixture]
	public class Tests
	{
		static string ToJson(XmlChange change)
		{
			return JsonConvert.SerializeObject(
				change,
				Formatting.Indented,
				new StringEnumConverter(),
				new ToStringJsonConverter<XmlNodePath>());
		}

		[Test]
		public void DiffTestNocommit()
		{
			var sourceDoc = XDocument.Parse("<Foo />");
			var sourceRoot = sourceDoc.Root;
			var destDoc = XDocument.Parse("<Foo />");
			var destRoot = destDoc.Root;

			var listener = new XmlChangeListener(sourceDoc);
			listener.Changes.Subscribe(
				change =>
				{
					var destBefore = destDoc.ToString(SaveOptions.DisableFormatting);
					Func<string, string> failMessage = failReason =>
						string.Format(
							"{0}\r\nPatch:\r\n{1}\r\nSource:\r\n{2}\r\nDest before\r\n{3}\r\nDest after\r\n{4}",
							failReason,
							ToJson(change),
							sourceDoc.ToString(SaveOptions.DisableFormatting),
							destBefore,
							destDoc.ToString(SaveOptions.DisableFormatting));

					Assert.That(change.TryApply(destDoc), Is.True, () => failMessage("Could not apply patch:"));
					Assert.That(
						destDoc.DeepEquals(sourceDoc),
						Is.True,
						() => failMessage("TryApply returned true, however dest is not equal to source"));
				});

			var bar = new XElement("Bar");
			sourceRoot.Add(bar);
			bar.Add(new XAttribute("Moo", "kkk"));
			bar.Add(new XText("Floook"));
			bar.Add(new XComment("WAAWWW A COMMENT"));
			bar.Add(new XCData("Some literal textest <><><>< ><><....????...<<<"));
			bar.Add(new XElement("Inner"));

			Assert.That(destDoc.DeepEquals(sourceDoc));
		}
	}
}