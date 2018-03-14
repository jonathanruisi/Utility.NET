// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       XmlViewModel.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:47 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using JLR.Utility.NET.ChangeNotification;
using JLR.Utility.NET.IO;

namespace JLR.Utility.NET.Xml
{
	public abstract class XmlViewModel<T> : XmlViewModelElement<T>
		where T : IPropertyChangeNotification, IXNode<XElement>, new()
	{
		#region Fields
		public event EventHandler<FileActionEventArgs> Loaded, Saved;
		private string                                 _filePath;
		private XDeclaration                           _fileDeclaration;
		#endregion

		#region Constructors
		protected XmlViewModel()
		{
			SuspendChildCollectionChangeNotification = true;
			_filePath                                = null;
			_fileDeclaration                         = null;
		}
		#endregion

		#region Public Methods
		public bool Load(string filePath, bool clearExistingData = true)
		{
			if (!File.Exists(filePath))
				return false;

			// Load document into an XDocument object
			XDocument doc;
			try
			{
				doc = XDocument.Load(filePath);
			}
			catch (FormatException)
			{
				return false;
			}

			if (doc.Root == null)
				return false;

			// File is a valid XML file
			_filePath        = filePath;
			_fileDeclaration = doc.Declaration;

			// Extract the first element matching the name of this database
			var element = doc.Descendants(NodeName).Single();
			if (element == null)
				return false;

			// Clear existing data, then load new data
			if (clearExistingData)
				Clear();
			FromXNode(element);
			OnLoaded();
			return true;
		}

		public void Save(string filePath = default(string))
		{
			if (!String.IsNullOrEmpty(filePath))
				_filePath = filePath;
			if (String.IsNullOrEmpty(_filePath))
				throw new ArgumentException("No file path has been specified for this instance", nameof(filePath));

			if (_fileDeclaration == null)
				_fileDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
			var doc = new XDocument(_fileDeclaration, ToXNode());
			doc.Save(_filePath);
			OnSaved();
		}
		#endregion

		#region Protected Methods
		protected virtual void OnLoaded()
		{
			var handler = Loaded;
			handler?.Invoke(this, new FileActionEventArgs(FileAction.Load, _filePath));
		}

		protected virtual void OnSaved()
		{
			var handler = Saved;
			handler?.Invoke(this, new FileActionEventArgs(FileAction.Save, _filePath));
		}
		#endregion
	}
}