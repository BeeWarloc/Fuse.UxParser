using System;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public static class XmlDiffExtensions
	{
		public static IObservable<XmlChange> Listen(XDocument doc)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Conditionally inserts a node into a document at specified node path.
		/// </summary>
		/// <param name="doc">The document to insert to</param>
		/// <param name="nodePath">The path the node will have after being inserted</param>
		/// <param name="inserted">The inserted node</param>
		/// <returns>True if node was inserted, false if parent of path could not be found</returns>
		public static bool TryInsert(this XDocument doc, XmlNodePath nodePath, XNode inserted)
		{
			return nodePath.TryInsertToDocument(doc, inserted);
		}

		/// <summary>
		///     Conditionally removes a node from a document at specified node path.
		///     If <paramref name="comparedNode" /> is provided, the node also has to
		///     be equal to this to be removed.
		/// </summary>
		/// <param name="doc">The document to remove from</param>
		/// <param name="nodePath">The path of the node to remove</param>
		/// <param name="comparedNode">Node to compare with before removing. Can be null.</param>
		/// <returns>True if node was removed, false if not found or not equal to comparedNode.</returns>
		public static bool TryRemove(this XDocument doc, XmlNodePath nodePath, XNode comparedNode = null)
		{
			XNode removed;
			nodePath.TryFind(doc, out removed);
			if (removed == null || comparedNode != null && !comparedNode.DeepEquals(removed))
				return false;

			removed.Remove();
			return true;
		}
	}
}