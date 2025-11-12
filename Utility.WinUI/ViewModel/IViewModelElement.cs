using System;
using System.Collections.Generic;
using System.Text;

namespace JLR.Utility.WinUI.ViewModel
{
    public interface IViewModelElement
    {
        /// <inheritdoc cref="ViewModelElement.Name"/>
        string Name { get; set; }

        /// <inheritdoc cref="ViewModelElement.IsSelected"/>
        bool IsSelected { get; set; }

        /// <inheritdoc cref="ViewModelElement.Parent"/>
        ViewModelNode Parent { get; internal protected set; }
    }
}