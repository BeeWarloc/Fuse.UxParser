using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public static class XNodeMergeExtensions
	{
		public static void Merge(this XNode dst, XNode src)
		{
			if (dst.NodeType != src.NodeType)
				throw new InvalidOperationException("Node type of merge source must be same as destination.");
			switch (src.NodeType)
			{
				case XmlNodeType.Element:
					MergeElement((XElement)dst, (XElement)src);
					break;
				case XmlNodeType.Comment:
					MergeComment((XComment)dst, (XComment)src);
					break;
				case XmlNodeType.Text:
					MergeText((XText)dst, (XText)src);
					break;
				case XmlNodeType.CDATA:
					MergeCData((XCData)dst, (XCData)src);
					break;
				case XmlNodeType.Document:
					Merge(((XDocument)dst).Root, ((XDocument)src).Root);
					break;
				default:
					throw new NotImplementedException("Don't know how to merge ");
			}
		}

		static void MergeElement(XElement dst, XElement src)
		{
			if (dst.Name != src.Name)
				dst.Name = src.Name;

			MergeAttributes(dst, src);
			MergeChildNodes(dst, src);

			if (dst.IsEmpty != src.IsEmpty)
			{
				if (src.IsEmpty)
				{
					// Unintuitive way to turn <Foo></Foo> into <Foo />
					dst.RemoveNodes();
				}
				else
				{
					// Unintuitive way to turn <Foo /> into <Foo></Foo>
					dst.Value = "";
				}
			}
		}

		static void MergeChildNodes(XElement dst, XElement src)
		{
			var dstNodes = WorkList.Create(dst);
			var srcNodes = WorkList.Create(src);

			// First skip all _identical_ nodes
			// (doesn't actually merge anything, just removes identical nodes from work list)
			MergeNodesBothDirections(
				dstNodes,
				srcNodes,
				DeepEquals);

			// First merge all where name matches
			MergeNodesBothDirections(
				dstNodes,
				srcNodes,
				(dstNode, srcNode) =>
				{
					if (dstNode.NodeType != srcNode.NodeType)
						return false;
					if (dstNode.NodeType == XmlNodeType.Element)
					{
						if (((XElement)dstNode).Name != ((XElement)srcNode).Name)
							return false;
					}
					Merge(dstNode, srcNode);
					return true;
				});

			// Then merge all where only node type matches
			MergeNodesBothDirections(
				dstNodes,
				srcNodes,
				(dstNode, srcNode) =>
				{
					if (dstNode.NodeType != srcNode.NodeType)
						return false;
					Merge(dstNode, srcNode);
					return true;
				});

			while (!dstNodes.IsEmpty)
			{
				var node = dstNodes.Pop(Position.Tail);
				node.Remove();
			}

			XNode dstInsertPoint = null;
			while (!srcNodes.IsEmpty)
			{
				// We need to know where to insert this.
				var srcNode = srcNodes.Pop(Position.Head);
				var dstNode = CloneNode(srcNode);
				if (dstInsertPoint == null)
				{
					var srcIndex = srcNode.NodeIndex();
					if (srcIndex == 0)
					{
						dst.AddFirst(dstNode);
					}
					else
					{
						dstInsertPoint = dst.FirstNode;
						// Insert point will be element before srcNode index
						for (int dstIndex = 0; dstIndex < (srcIndex - 1); dstIndex++)
						{
							dstInsertPoint = dstInsertPoint.NextNode;
						}
						dstInsertPoint.AddAfterSelf(dstNode);
					}
				}
				else
				{
					dstInsertPoint.AddAfterSelf(dstNode);
				}

				dstInsertPoint = dstNode;
			}
		}

		static XNode CloneNode(XNode srcNode)
		{
			switch (srcNode.NodeType)
			{
				case XmlNodeType.CDATA:
					return new XCData((XCData)srcNode);
				case XmlNodeType.Text:
					return new XText((XText)srcNode);
				case XmlNodeType.Element:
					return new XElement((XElement)srcNode);
				case XmlNodeType.Comment:
					return new XComment((XComment)srcNode);
				default:
					throw new NotImplementedException("TODO: Find out if we've covered all required node types");
			}
		}

		static void MergeNodesBothDirections<T>(WorkList<T> dst, WorkList<T> src, Func<T, T, bool> merge) where T : class
		{
			// Merge as far as we can down using the given merge strategy
			MergeNodeSequenceUntilStrategyFails(dst, src, Position.Head, merge);

			// Then merge backwards..
			MergeNodeSequenceUntilStrategyFails(dst, src, Position.Tail, merge);
		}

		static void MergeNodeSequenceUntilStrategyFails<T>(
			WorkList<T> dst,
			WorkList<T> src,
			Position popPos,
			Func<T, T, bool> merge) where T : class
		{
			while (!dst.IsEmpty && !src.IsEmpty)
			{
				var srcNode = src.Peek(popPos);
				var dstNode = dst.Peek(popPos);
				if (merge(dstNode, srcNode))
				{
					dst.Pop(popPos);
					src.Pop(popPos);
				}
				else break;
			}
		}

		static void MergeCData(XCData dst, XCData src)
		{
			if (dst.Value != src.Value)
				dst.Value = src.Value;
		}

		static void MergeText(XText dst, XText src)
		{
			if (dst.Value != src.Value)
				dst.Value = src.Value;
		}

		static void MergeComment(XComment dst, XComment src)
		{
			if (dst.Value != src.Value)
				dst.Value = src.Value;
		}

		static void MergeAttributes(XElement dst, XElement src)
		{
			// Attributes
			// This will reorder the attributes when merging, right?

			int matchCount = 0;
			foreach (var pair in dst.Attributes().Zip(src.Attributes(), (dstAttr, srcAttr) => new { dstAttr, srcAttr }))
			{
				var dstAttr = pair.dstAttr;
				var srcAttr = pair.srcAttr;

				if (dstAttr.Name != srcAttr.Name)
					continue;
				if (dstAttr.Value != srcAttr.Value)
				{
					dstAttr.Value = srcAttr.Value;
				}

				matchCount++;
			}

			// Remove all attributes below the matching ones
			foreach (var dstAttr in dst.Attributes().Skip(matchCount).ToArray())
			{
				dstAttr.Remove();
			}

			foreach (var srcAttr in src.Attributes().Skip(matchCount))
			{
				dst.Add(new XAttribute(srcAttr));
			}
		}

		static bool DeepEquals(XElement a, XElement b)
		{
			if (a == null) throw new ArgumentNullException("a");
			if (b == null) throw new ArgumentNullException("b");

			if (a.Name != b.Name)
				return false;

			if (a.IsEmpty != b.IsEmpty)
				return false;

			var aAttr = a.FirstAttribute;
			var bAttr = b.FirstAttribute;
			while (aAttr != null && bAttr != null)
			{
				if (aAttr.Name != bAttr.Name || aAttr.Value != bAttr.Value)
					return false;

				aAttr = aAttr.NextAttribute;
				bAttr = bAttr.NextAttribute;
			}

			var aChild = a.FirstNode;
			var bChild = b.FirstNode;
			while (aChild != null && bChild != null)
			{
				if (!DeepEquals(aChild, bChild))
					return false;

				aChild = aChild.NextNode;
				bChild = bChild.NextNode;
			}

			// If both are null the element child count also match
			return aChild == null && bChild == null;
		}

		public static bool DeepEquals(this XNode a, XNode b)
		{
			if (a == null) throw new ArgumentNullException("a");
			if (b == null) throw new ArgumentNullException("b");

			if (a.NodeType != b.NodeType)
				return false;

			switch (a.NodeType)
			{
				case XmlNodeType.Element:
					return DeepEquals((XElement)a, (XElement)b);
				case XmlNodeType.CDATA:
					return ((XCData)a).Value == ((XCData)b).Value;
				case XmlNodeType.Text:
					return ((XText)a).Value == ((XText)b).Value;
				case XmlNodeType.Comment:
					return ((XComment)a).Value == ((XComment)b).Value;
				case XmlNodeType.Document:
					return DeepEquals(((XDocument)a).Root, ((XDocument)b).Root);
				default:
					throw new InvalidOperationException("TODO: Find out if this covers all relevant cases");
			}
		}

		enum Position
		{
			Head,
			Tail
		}

		static class WorkList
		{
			public static WorkList<XNode> Create(XContainer container)
			{
				return new WorkList<XNode>(container.FirstNode, container.LastNode, x => x.NextNode, x => x.PreviousNode);
			}
		}

		class WorkList<T>
			where T : class
		{
			readonly Func<T, T> _next;
			readonly Func<T, T> _prev;
			T _head;
			T _tail;

			public WorkList(T head, T tail, Func<T, T> next, Func<T, T> prev)
			{
				_head = head;
				_tail = tail;
				_next = next;
				_prev = prev;
			}

			public bool IsEmpty
			{
				get { return _head == null; }
			}

			public T Peek(Position position)
			{
				if (_head == null)
					throw new InvalidOperationException("List is empty.");
				return position == Position.Head ? _head : _tail;
			}

			public T Pop(Position position)
			{
				if (IsEmpty)
					throw new InvalidOperationException("Nothing left to remove");

				// When we only have one element left we're empty.
				T node;
				if (_head == _tail)
				{
					node = _head;
					_head = null;
					_tail = null;
				}
				else if (position == Position.Head)
				{
					node = _head;
					_head = _next(node);
					if (_head == null)
						throw new InvalidOperationException(
							"Inconsistency detected. There should be a next node at this point, as we've already checked that the list is not of size 0");
				}
				else
				{
					node = _tail;
					_tail = _prev(node);
					if (_tail == null)
						throw new InvalidOperationException(
							"Inconsistency detected. There should be a next node at this point, as we've already checked that the list is not of size 0");
				}
				return node;
			}
		}
	}
}