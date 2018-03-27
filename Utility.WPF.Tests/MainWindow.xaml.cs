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
			Loaded      += MainWindow_Loaded;
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

			MediaSliderTest.PositionChanged       += MediaSliderTest_PositionChanged;
			MediaSliderTest.SelectionRangeChanged += MediaSliderTest_SelectionRangeChanged;
			MediaSliderTest.VisibleRangeChanged   += MediaSliderTest_VisibleRangeChanged;
		}

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) { }

		private void MediaSliderTest_PositionChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e) { }

		private void MediaSliderTest_SelectionRangeChanged(object sender,
														   RoutedPropertyChangedEventArgs<(decimal rangeStart, decimal rangeEnd)?> e)
		{
			TextBoxC1.Text = e.NewValue.HasValue
				? (e.NewValue.Value.rangeEnd - e.NewValue.Value.rangeStart).ToString("0.###")
				: "DISABLED";
		}

		private void MediaSliderTest_VisibleRangeChanged(object sender,
														 RoutedPropertyChangedEventArgs<(decimal rangeStart, decimal rangeEnd)> e)
		{
			TextBoxB1.Text = (e.NewValue.rangeEnd - e.NewValue.rangeStart).ToString("0.###");
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button btn)) return;

			switch (btn.Tag)
			{
				case "1": // Tests JLR.Utility.NET.MathHelper prime related methods
					var nMin       = uint.Parse(TextBoxD0.Text);
					var nMax       = uint.Parse(TextBoxD1.Text);
					var isSort     = bool.Parse(TextBoxD2.Text);
					var factorStr  = new StringBuilder();
					var divisorStr = new StringBuilder();
					var timerA     = new Stopwatch();
					var timerB     = new Stopwatch();
					var timerC     = new Stopwatch();
					var totalTimeA = new TimeSpan();
					var totalTimeB = new TimeSpan();
					var totalTimeC = new TimeSpan();

					MathHelper.IsPrimeCacheEnabled = false;

					const string filePath = "C:\\Users\\Jonathan\\Desktop\\MathHelperTest.txt";
					if (File.Exists(filePath))
						File.Delete(filePath);
					var file = new StreamWriter(filePath);
					file.WriteLine(
						"{0,-20}{5,-10}{6,-10}{7,-10}{1,-8}{2,-12}{3,-32}{4}",
						"Number",
						"Prime?",
						"# Divisors",
						"Prime Factors",
						"Divisors",
						"Time(IP)",
						"Time(PF)",
						"Time(D)");

					for (var num = nMin; num <= nMax; num++)
					{
						timerA.Start();
						var factors = MathHelper.PrimeFactors(num);
						timerA.Stop();

						timerB.Start();
						var isPrime = MathHelper.IsPrime(num);
						timerB.Stop();

						timerC.Start();
						var divisors = MathHelper.Divisors(num, isSort);
						timerC.Stop();

						factorStr.Clear();
						factorStr.Append('{');
						for (var i = 0; i < factors.Count; i++)
						{
							if (i > 0)
								factorStr.Append(',');
							factorStr.Append($"{factors[i].factor}^{factors[i].power}");
						}

						factorStr.Append('}');
						divisorStr.Clear();
						divisorStr.Append('{');
						for (var i = 0; i < divisors.Count; i++)
						{
							if (i > 0)
								divisorStr.Append(',');
							divisorStr.Append($"{divisors[i]}");
						}

						divisorStr.Append('}');

						file.WriteLine(
							"{0,-20:D}{5,-10:0.0000}{6,-10:0.0000}{7,-10:0.0000}{1,-8}{2,-12}{3,-32}{4}",
							num,
							isPrime,
							divisors.Count,
							factorStr,
							divisorStr,
							timerB.Elapsed.Ticks / 1000M,
							timerA.Elapsed.Ticks / 1000M,
							timerC.Elapsed.Ticks / 1000M);

						totalTimeA = totalTimeA.Add(timerA.Elapsed);
						totalTimeB = totalTimeB.Add(timerB.Elapsed);
						totalTimeC = totalTimeC.Add(timerC.Elapsed);

						timerA.Reset();
						timerB.Reset();
						timerC.Reset();
					}

					file.WriteLine();
					file.WriteLine(
						totalTimeB.TotalSeconds < 1
							? $"Total \"IsPrime?\" computation time   = {totalTimeB.TotalMilliseconds:0.###} ms"
							: $"Total \"IsPrime?\" computation time   = {totalTimeB.TotalSeconds:0.###} s");

					file.WriteLine(
						totalTimeA.TotalSeconds < 1
							? $"Total prime factor computation time = {totalTimeA.TotalMilliseconds:0.###} ms"
							: $"Total prime factor computation time = {totalTimeA.TotalSeconds:0.###} s");

					file.WriteLine(
						totalTimeC.TotalSeconds < 1
							? $"Total divisor computation time      = {totalTimeC.TotalMilliseconds:0.###} ms"
							: $"Total divisor computation time      = {totalTimeC.TotalSeconds:0.###} s");

					var totalTime = totalTimeA + totalTimeB + totalTimeC;
					file.WriteLine();
					file.WriteLine(
						totalTime.TotalSeconds < 1
							? $"Total computation time              = {totalTime.TotalMilliseconds:0.###} ms\n"
							: $"Total computation time              = {totalTime.TotalSeconds:0.###} s\n");

					file.Flush();
					file.Close();

					MessageBox.Show(
						totalTime.TotalSeconds < 1
							? $"Total computation time: {(totalTimeA + totalTimeB + totalTimeC).TotalMilliseconds:0.###} ms\n"
							: $"Total computation time: {(totalTimeA + totalTimeB + totalTimeC).TotalSeconds:0.###} s\n");
					Process.Start(filePath);
					break;
				case "2":
					break;
				case "3":
					break;
				case "4":
					MediaSliderTest.VisibleRangeEnd += (MediaSliderTest.Maximum - MediaSliderTest.Minimum) * 0.1M;
					break;
				case "5":
					MediaSliderTest.VisibleRangeStart -= (MediaSliderTest.Maximum - MediaSliderTest.Minimum) * 0.1M;
					break;
				case "6":
					break;
				case "7":
					break;
				case "8":
					break;
				case "9":
					MediaSliderTest.VisibleRangeEnd -= (MediaSliderTest.Maximum - MediaSliderTest.Minimum) * 0.1M;
					break;
				case "10":
					MediaSliderTest.VisibleRangeStart += (MediaSliderTest.Maximum - MediaSliderTest.Minimum) * 0.1M;
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