using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace JLR.Utility.WinUI.Controls
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(CanvasControl))]
    public sealed class LogElement : Control
    {
        #region Fields
        private CanvasControl _canvas;
        #endregion

        #region Constructor
        public LogElement()
        {
            DefaultStyleKey = typeof(LogElement);
        }
        #endregion

        #region Properties

        #endregion

        #region Events

        #endregion

        #region Public Methods

        #endregion

        #region Dependency Property Callbacks

        #endregion

        #region Layout Method Overrides
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Canvas") is CanvasControl canvas)
            {
                _canvas = canvas;
            }
        }
        #endregion

        #region Event Handlers

        #endregion

        #region Rendering

        #endregion

        #region Private Methods

        #endregion
    }
}