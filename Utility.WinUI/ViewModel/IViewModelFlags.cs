using System;
using System.Collections.Generic;
using System.Text;

namespace JLR.Utility.WinUI.ViewModel
{
    public interface IViewModelFlags
    {
        /// <inheritdoc cref="ViewModelElement.Flags"/>
        ulong Flags { get; internal protected set; }

        /// <inheritdoc cref="ViewModelElement.CheckFlag"/>
        bool CheckFlag(int flag);

        /// <inheritdoc cref="ViewModelElement.SetFlag"/>
        void SetFlag(int flag);

        /// <inheritdoc cref="ViewModelElement.ClearFlag"/>
        void ClearFlag(int flag);

        /// <inheritdoc cref="ViewModelElement.ToggleFlag"/>
        void ToggleFlag(int flag);
    }
}