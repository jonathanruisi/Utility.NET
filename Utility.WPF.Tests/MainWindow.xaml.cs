using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using JLR.Utility.NET;
using JLR.Utility.NET.Math;
using JLR.Utility.WPF.Elements;

namespace Utility.WPF.Tests
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded      += MainWindow_Loaded;
			SizeChanged += MainWindow_SizeChanged;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var binding = new Binding
			{
				Path         = new PropertyPath("Minimum"),
				Source       = TickBarTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("Maximum"),
				Source       = TickBarTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxA2.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("MajorTickFrequency"),
				Source       = TickBarTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxD0.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path         = new PropertyPath("MinorTickFrequency"),
				Source       = TickBarTest,
				Mode         = BindingMode.TwoWay,
				StringFormat = "0.###"
			};
			TextBoxD1.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding
			{
				Path      = new PropertyPath("Ticks"),
				Source    = TickBarTest,
				Mode      = BindingMode.TwoWay,
				Converter = new TickHashSetConverter()
			};
			TextBoxD2.SetBinding(TextBox.TextProperty, binding);
		}

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) { }


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button btn)) return;

			var rnd = new Random(DateTime.Now.Millisecond);

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