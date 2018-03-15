using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Utility.WPF.Tests
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
			SizeChanged += MainWindow_SizeChanged;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var binding = new Binding
			{
				Path         = new PropertyPath("Minimum"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("Maximum"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA2.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("Position"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA1.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("VisibleRangeStart"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxB0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("VisibleRangeEnd"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxB2.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("SelectionStart"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxC0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("SelectionEnd"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxC2.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("MouseDist"),
				Source       = MediaSliderTest,
				Mode         = BindingMode.OneWay,
				StringFormat = "0.###"
			};
			TextBoxE0.SetBinding(TextBlock.TextProperty, binding);
			TextBoxE0.IsReadOnly = true;

			MediaSliderTest.PositionChanged += MediaSliderTest_PositionChanged;
		}

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
		}

		private void MediaSliderTest_PositionChanged(object sender, RoutedEventArgs e)
		{
			
		}
	}
}