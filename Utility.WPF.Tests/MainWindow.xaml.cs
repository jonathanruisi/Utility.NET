using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using JLR.Utility.NET.Math;

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

		private void MediaSliderTest_PositionChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
		{
			
		}

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button btn)) return;

			switch (btn.Content)
			{
				case "1":
					var nMin      = int.Parse(TextBoxD0.Text);
					var nMax      = int.Parse(TextBoxD1.Text);
					var timer     = new Stopwatch();
					var str       = new StringBuilder();
					var totalTime = new TimeSpan();

					const string filePath = "C:\\Users\\Jonathan\\Desktop\\Results.txt";
					if (File.Exists(filePath))
						File.Delete(filePath);
					var file = new StreamWriter(filePath);
					file.WriteLine("{0,-20}{1,-20}{2,-30}{3}", "Number", "Time (ms)", "Factorization", "Prime Count");

					for (var num = nMin; num <= nMax; num++)
					{
						timer.Start();
						var result = MathHelper.PrimeFactors(num);
						timer.Stop();

						str.Clear();
						str.Append('{');
						for (var i = 0; i < result.Count; i++)
						{
							if (i > 0)
								str.Append(',');
							str.Append($"{result[i].factor}^{result[i].power}");
						}

						str.Append('}');
						file.WriteLine("{0,-20:D}{1,-20:0.0000}{2,-30}{3}", num, timer.Elapsed.Ticks / 10000M, str, result.Count);
						totalTime = totalTime.Add(timer.Elapsed);
						timer.Reset();
					}

					file.WriteLine($"Total computation time: {totalTime.TotalMilliseconds:0.###} ms\n");
					file.WriteLine($"Total number of primes in cache: {MathHelper._primes.Count}");

					file.Flush();
					file.Close();

					MessageBox.Show($"Total computation time: {totalTime.TotalMilliseconds:0.###} ms\n");
					Process.Start(filePath);
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