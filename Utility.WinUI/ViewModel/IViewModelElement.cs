using System;
using System.Collections.Generic;
using System.Text;

namespace JLR.Utility.WinUI.ViewModel
{
    public interface IViewModelElement
    {
        /// <summary>
        /// Gets or sets the name of this element.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this element.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this element
        /// is currently selected somewhere in the user interface.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets a reference to this element's parent node.
        /// </summary>
        ViewModelNode Parent { get; internal protected set; }
    }
}