// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       XmlViewModelElement.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:48 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Xml.Linq;

using JLR.Utility.NETFramework.ChangeNotification;
using JLR.Utility.NETFramework.Collections;

namespace JLR.Utility.NETFramework.Xml
{
	/// <summary>
	/// Represents a collection of objects that can be easily bound to user interface controls,
	/// as well as serialized to and deserialized from XML.
	/// The serialization process can be easily customized in derived classes,
	/// and is based on XElement containers.
	/// Supports property and collection change notification for the entire depth of the tree,
	/// allowing for smooth two-way data binding.
	/// </summary>
	/// <typeparam name="T">The type of child elements in this collection</typeparam>
	public abstract class XmlViewModelElement<T> : DeepObservableCollection<T>, IXNode<XElement>
		where T : IPropertyChangeNotification, IXNode<XElement>, new()
	{
		#region Properties
		public abstract string NodeName { get; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Serializes a <see cref="XmlViewModelElement{T}"/> as an XElement.
		/// When overridden in derived classes, additional members can be added to the XElement.
		/// </summary>
		/// <returns>This object's XML representation as an XElement</returns>
		public virtual XElement ToXNode()
		{
			var element = new XElement(NodeName);
			foreach (var child in this)
			{
				element.Add(child.ToXNode());
			}

			return element;
		}

		/// <summary>
		/// Deserializes an <see cref="XmlViewModelElement{T}"/> from an XElement.
		/// </summary>
		/// <param name="element">The XElement containing this object's data</param>
		public virtual void FromXNode(XElement element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));
			if (element.Name != NodeName)
				throw new ArgumentException($"The name of the specified element must be {NodeName}");

			var childNodeName = (new T() as IXNode<XElement>).NodeName;
			foreach (var childElement in element.Descendants(childNodeName))
			{
				var child = new T();
				child.FromXNode(childElement);
				Add(child);
			}
		}
		#endregion
	}
}