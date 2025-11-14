using System;
using System.Collections.Generic;
using System.Text;

namespace JLR.Utility.WinUI.ViewModel
{
    public interface IViewModelFlags
    {
        /// <summary>
        /// Gets or sets the value of a 64-bit mask.
        /// </summary>
        /// <remarks>
        /// From the perspective of <see cref="ViewModelElement"/>, the meaning of each flag bit is arbitrary.
        /// It is up to derived classes to define the purpose of each flag bit.
        /// </remarks>
        ulong Flags { get; internal protected set; }

        /// <summary>
        /// Returns <b><c>true</c></b> if <paramref name="flag"/> is set; otherwise, <b><c>false</c></b>.
        /// </summary>
        /// <param name="flag">The specific flag to check.</param>
        /// <returns></returns>
        bool CheckFlag(int flag);

        /// <summary>
        /// Sets (enables) the specified <paramref name="flag"/>.
        /// </summary>
        /// <param name="flag">The flag to set.</param>
        void SetFlag(int flag);

        /// <summary>
        /// Clears (disables) the specified <paramref name="flag"/>.
        /// </summary>
        /// <param name="flag">The flag to clear.</param>
        void ClearFlag(int flag);

        /// <summary>
        /// Toggles the specified <paramref name="flag"/>.
        /// </summary>
        /// <param name="flag">The flag to toggle.</param>
        void ToggleFlag(int flag);
    }
}