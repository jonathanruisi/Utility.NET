using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JLR.Utility.UWP.Dialogs
{
	public sealed partial class TextPromptDialog : ContentDialog
	{
		public string Text
		{
			get => (string) GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		private static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text",
										typeof(string),
										typeof(TextPromptDialog),
										new PropertyMetadata(null, OnTextChanged));

		public string PromptText
		{
			get => (string) GetValue(PromptTextProperty);
			set => SetValue(PromptTextProperty, value);
		}

		private static readonly DependencyProperty PromptTextProperty =
			DependencyProperty.Register("PromptText",
										typeof(string),
										typeof(TextPromptDialog),
										new PropertyMetadata(null));

		public TextPromptDialog()
		{
			InitializeComponent();
		}

		private void InputTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			// Explicitly set here because binding to the TextBox's Text property
			// does not trigger change notification after each keypress
			Text = InputTextBox.Text;
		}

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is TextPromptDialog dlg))
				return;

			dlg.IsPrimaryButtonEnabled = !string.IsNullOrEmpty(e.NewValue as string);
		}
	}
}