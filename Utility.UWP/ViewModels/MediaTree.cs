using JLR.Utility.NET.Collections.Tree;
using JLR.Utility.UWP.Models;

namespace JLR.Utility.UWP.ViewModels
{
	public abstract class MediaTreeNode : ObservableTreeNode
	{
		#region Fields
		private string _name;
		#endregion

		#region Properties
		public string Name
		{
			get => _name;
			set => Set(ref _name, value);
		}
		#endregion

		#region Constructors
		protected MediaTreeNode(string name)
		{
			_name = name;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return _name;
		}
		#endregion
	}

	public sealed class MediaTreeFolder : MediaTreeNode
	{
		public MediaTreeFolder(string name) : base(name)
		{
		}
	}

	public sealed class MediaTreeFile : MediaTreeNode
	{
		public MediaFile File { get; }

		public MediaTreeFile(MediaFile file) : this(file.ToString(), file)
		{
		}

		public MediaTreeFile(string name, MediaFile file) : base(name)
		{
			File = file;
		}
	}
}