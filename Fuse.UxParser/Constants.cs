using System.Collections.Generic;

namespace Fuse.UxParser
{
	internal class Constants
	{
		public static IEnumerable<KeyValuePair<string, string>> DefaultNamespacePrefixes { get; } = new[]
		{
			new KeyValuePair<string, string>("ux", "http://schemas.fusetools.com/ux"),
			new KeyValuePair<string, string>("dep", "http://schemas.fusetools.com/dep")
		};

		public const string DefaultFuseNamespaceList =
				"Fuse, Fuse.Reactive, Fuse.Selection, Fuse.Animations, Fuse.Drawing, Fuse.Entities, Fuse.Controls, Fuse.Layouts, Fuse.Elements, Fuse.Effects, Fuse.Triggers, Fuse.Navigation, Fuse.Triggers.Actions, Fuse.Gestures, Fuse.Resources, Fuse.Native, Fuse.Physics, Fuse.Vibration, Fuse.Motion, Fuse.Testing, Uno.UX"
			;
	}
}