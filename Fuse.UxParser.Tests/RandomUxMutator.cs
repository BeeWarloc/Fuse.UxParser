using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuse.UxParser.Tests
{
	public class RandomUxMutator
	{
		readonly Random _rng;
		readonly int _maUxElements = 200;

		readonly string[] _colors =
			{ "#c66029", "#e27043", "#ed830c", "#eba41e", "#eede83", "#91e11c", "#53bfdc", "#3963d3", "#e76b8f" };

		public RandomUxMutator(Random rng = null)
		{
			_rng = rng ?? new Random();
		}

		public static int Depth(UxElement element)
		{
			return element.Parent == null ? 0 : Depth(element.Parent) + 1;
		}

		public void Mutate(UxDocument doc)
		{
			var element = doc.Root;
			var descendants = element.Descendants().ToList();
			if ((descendants.Count < _maUxElements / 2 || _rng.NextDouble() > descendants.Count / (double) _maUxElements) &&
				descendants.Count < _maUxElements)
				AddRandomItem(element, descendants);
			else
				RandomItem(descendants).Remove();
		}

		void AddRandomItem(UxElement element, List<UxElement> descendants)
		{
			var containers = descendants.Count > 0 ? descendants.Where(x => x.Name == "Grid") : new[] { element };
			var container = RandomItem(containers);
			var existingChildCount = container.Elements.Count();
			var insertPoint = _rng.Next(0, existingChildCount + 1);

			var insertElement = UxElement.Parse(
				$"<Grid ChildOrder=\"{(Depth(container) % 2 == 0 ? "RowMajor" : "ColumnMajor")}\" Background=\"{RandomItem(_colors)}\" />");

			if (insertPoint == existingChildCount)
			{
				container.Add(insertElement);
				;
			}
			else
			{
				container.Elements.ElementAt(insertPoint).AddBeforeSelf(insertElement);
			}
		}

		T RandomItem<T>(IEnumerable<T> items)
		{
			var list = items as IList<T> ?? items.ToArray();
			if (list.Count == 0)
				throw new InvalidOperationException("Need at least one item");

			return list[_rng.Next(0, list.Count)];
		}
	}
}