using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using JLR.Utility.WPF.Controls;
using JLR.Utility.WPF.Elements;

namespace Utility.WPF.Tests
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var binding = new Binding
			{
				Source       = TickBarAdvancedTest,
				Path         = new PropertyPath("Minimum"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Source       = TickBarAdvancedTest,
				Path         = new PropertyPath("Maximum"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA2.SetBinding(TextBox.TextProperty, binding);

			/*binding = new Binding
			{
				Source       = MediaSliderTest,
				Path         = new PropertyPath("Position"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA1.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Source       = MediaSliderTest,
				Path         = new PropertyPath("SelectionStart"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxC0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Source       = MediaSliderTest,
				Path         = new PropertyPath("SelectionEnd"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxC2.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Source       = MediaSliderTest,
				Path         = new PropertyPath("ZoomStart"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxB0.SetBinding(TextBox.TextProperty, binding);
			
			binding = new Binding
			{
				Source       = MediaSliderTest,
				Path         = new PropertyPath("ZoomEnd"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxB2.SetBinding(TextBox.TextProperty, binding);*/

			binding = new Binding
			{
				Source       = TickBarAdvancedTest,
				Path         = new PropertyPath("MajorTickFrequency"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxD0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Source       = TickBarAdvancedTest,
				Path         = new PropertyPath("MinorTickFrequency"),
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxD1.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Source    = TickBarAdvancedTest,
				Path      = new PropertyPath("Ticks"),
				Mode      = BindingMode.TwoWay,
				Converter = new TickListConverter()
			};
			TextBoxD2.SetBinding(TextBox.TextProperty, binding);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button btn)) return;

			switch (btn.Tag)
			{
				case "1":
					break;
				case "2":
					break;
				case "3":
					break;
				case "4":
					break;
				case "5":
					break;
				case "6":
					break;
				case "7":
					break;
				case "8":
					break;
				case "9":
					break;
				case "10":
					break;
				case "11":
					break;
				case "12":
					break;
				case "13":
					break;
				case "14":
					break;
				case "15":
					break;
			}
		}
	}
}