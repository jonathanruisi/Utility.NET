using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JLR.Utility.UWP.Dialogs
{
	public sealed partial class SingleStringPromptDialog : ContentDialog
	{
		public SingleStringPromptDialog()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty PromptTextProperty =
			DependencyProperty.Register("PromptText",
			                            typeof(string),
			                            typeof(SingleStringPromptDialog),
			                            new PropertyMetadata("Default text"));

		public string PromptText
		{
			get => (string) GetValue(PromptTextProperty);
			set => SetValue(PromptTextProperty, value);
		}

		public static readonly DependencyProperty EnteredTextProperty =
			DependencyProperty.Register("EnteredText",
			                            typeof(string),
			                            typeof(SingleStringPromptDialog),
			                            new PropertyMetadata(string.Empty));

		public string EnteredText
		{
			get => (string) GetValue(EnteredTextProperty);
			set => SetValue(EnteredTextProperty, value);
		}
	}
}