using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace SplineVisualizationWPF
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool debug = false;
		bool colorful = true;

		string windowTitle = "Визуализация сплайнов";
		SolidColorBrush color1;
		SolidColorBrush color2;
		List<double> xValues;
		List<double> yValues;
		List<double> xStarValues;
		List<string> functionsNames;
		Spline spline1;
		Spline spline2;
		double Sa;
		double Sb;
		int pointsAmount;
		int minPointsAmount = 5;
		string sinus = "sin(x)";
		string ln = "ln(x)";
		string testFileName = "";
		string tableFileName = "";

		public MainWindow()
		{
			InitializeComponent();

			this.Title = windowTitle;
			if (colorful)
			{
				color1 = new SolidColorBrush(Colors.Blue);
				color2 = new SolidColorBrush(Colors.Red);
			}
			else
			{
				color1 = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				color2 = new SolidColorBrush(Color.FromRgb(160, 160, 160));
			}

			XValuesTextBox.TextChanged += (s, e) =>
			{
				try
				{
					int count = StringToListOfDoubles(XValuesTextBox.Text, "").Count;
					if (count < minPointsAmount)
					{
						ShowError($"Необходимо минимум {minPointsAmount} точек");
					}
					List<int> values = new List<int>();
					for (int i = minPointsAmount; i <= count; i++)
					{
						values.Add(i);
					}
					PointsAmountComboBox.ItemsSource = null;
					PointsAmountComboBox.ItemsSource = values;

				}
				catch { }
			};

			functionsNames = new List<string> { sinus, ln };
			TestFunctionsComboBox.ItemsSource = functionsNames;
			TestFunctionsComboBox.SelectedItem = functionsNames[0];


			FunctionsTab.SelectionChanged += (s, e) =>
			{
				var tab = FunctionsTab.SelectedItem;
				var tabTest = TestFunctionTab;
				var tabTable = TableFunctionTab;
				if (tab == tabTest)
				{
					if (testFileName != "")
						this.Title = $"{windowTitle} - {testFileName}";
					else this.Title = windowTitle;
				}
				else if (tab == tabTable)
				{
					if (tableFileName != "")
						this.Title = $"{windowTitle} - {tableFileName}";
					else this.Title = windowTitle;
				}
			};
		}


		private List<ObservablePoint> GetPoints(List<double> x, List<double> y)
		{
			List<ObservablePoint> result = new List<ObservablePoint>();

			for (int i = 0; i < x.Count; i++)
				result.Add(new ObservablePoint(x[i], y[i]));
			return result;
		}

		/// <summary>
		/// Заполнение чарта сплайнами
		/// </summary>
		/// <param name="splineChart"></param>
		/// <param name="splineVisuals"></param>
		private void ShowChart(ref CartesianChart splineChart, params SplineVisual[] splineVisuals)
		{
			var seriesCollection = new SeriesCollection();

			foreach (var splineVisual in splineVisuals)
			{
				var points = GetPoints(splineVisual.spline.X, splineVisual.spline.Y);
				if (splineVisual.isLineVisible)
				{
					LineSeries lineSeries = new LineSeries()
					{
						Stroke = splineVisual.brush,
						Title = splineVisual.title,
						Fill = Brushes.Transparent,
						Values = new ChartValues<ObservablePoint>(points)
					};
					seriesCollection.Add(lineSeries);
				}
				else
				{
					ScatterSeries scatterSeries = new ScatterSeries()
					{
						Stroke = splineVisual.brush,
						Title = splineVisual.title,
						Fill = splineVisual.brush,
						Values = new ChartValues<ObservablePoint>(points)
					};
					seriesCollection.Add(scatterSeries);
				}
			}

			splineChart.Series = seriesCollection;
		}


		private List<double> GetValues(List<double> values, int valuesAmount)
		{
			List<double> result = new List<double>();
			int firstPoints = (int)Math.Ceiling((float)valuesAmount / 2);
			int lastPoints = (int)Math.Floor((float)valuesAmount / 2);

			for (int i = 0; i < firstPoints; i++)
				result.Add(values[i]);

			for (int i = values.Count - lastPoints; i < values.Count; i++)
				result.Add(values[i]);

			return result;
		}


		private List<double> StringToListOfDoubles(string text, string valueIdentify)
		{
			try
			{
				List<double> result = new List<double>();
				var tempStr = text.Split(';');
				foreach (var item in tempStr)
					result.Add(Convert.ToDouble(item));
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"{valueIdentify}: {ex.Message}");
			}
		}


		private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (FunctionsTab.SelectedItem == TableFunctionTab)
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "Файл конфигурации (*.spl)|*.spl";
				saveFileDialog.AddExtension = true;
				saveFileDialog.DefaultExt = ".spl";
				saveFileDialog.ShowDialog();
				var path = saveFileDialog.FileName;
				if (path != "")
				{
					try
					{
						var fileName = System.IO.Path.GetFileName(path);
						File.WriteAllText(path,
							XValuesTextBox.Text + "\n" +
							YValuesTextBox.Text + "\n" +
							XStarValuesTextBox.Text + "\n" +
							SaTextBox.Text + "\n" +
							SbTextBox.Text);
						this.Title = $"{windowTitle} - {fileName}";
						tableFileName = fileName;
					}
					catch (Exception ex)
					{
						ShowError(ex, debug);
					}
				}
			}
			else if (FunctionsTab.SelectedItem == TestFunctionTab)
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "Файл конфигурации (*.splt)|*.splt";
				saveFileDialog.AddExtension = true;
				saveFileDialog.DefaultExt = ".splt";
				saveFileDialog.ShowDialog();
				var path = saveFileDialog.FileName;

				if (path != "")
				{
					try
					{
						var fileName = Path.GetFileName(path);
						File.WriteAllText(path,
							TestFunctionsComboBox.SelectedItem + "\n" +
							StartTextBoxTestTab.Text + "\n" +
							EndTextBoxTestTab.Text + "\n" +
							PointsAmountTextBoxTestTab.Text + "\n" +
							XStarValuesTextBoxTestTab.Text + "\n" +
							SaTextBoxTestTab.Text + "\n" +
							SbTextBoxTestTab.Text);
						this.Title = $"{windowTitle} - {fileName}";
						testFileName = fileName;
					}
					catch (Exception ex)
					{
						ShowError(ex, debug);
					}
				}
			}
		}


		private void ImportMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (FunctionsTab.SelectedItem == TableFunctionTab)
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Файл конфигурации (*.spl)|*.spl";
				openFileDialog.AddExtension = true;
				openFileDialog.DefaultExt = ".spl";
				openFileDialog.ShowDialog();
				var path = openFileDialog.FileName;
				var fileName = System.IO.Path.GetFileName(path);
				if (path != "")
				{
					try
					{
						var values = File.ReadAllText(path).Split('\n');
						XValuesTextBox.Text = values[0];
						YValuesTextBox.Text = values[1];
						XStarValuesTextBox.Text = values[2];
						SaTextBox.Text = values[3];
						SbTextBox.Text = values[4];

						this.Title = $"{windowTitle} - {fileName}";
						tableFileName = fileName;
						if (!debug) DrawSplines();
					}
					catch (Exception ex)
					{
						ShowError(ex, debug);
					}
					if (debug) DrawSplines();
				}
			}
			else if (FunctionsTab.SelectedItem == TestFunctionTab)
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Файл конфигурации (*.splt)|*.splt";
				openFileDialog.AddExtension = true;
				openFileDialog.DefaultExt = ".splt";
				openFileDialog.ShowDialog();
				var path = openFileDialog.FileName;
				var fileName = Path.GetFileName(path);
				if (path != "")
				{
					try
					{
						var values = File.ReadAllText(path).Split('\n');

						TestFunctionsComboBox.SelectedItem = values[0];
						StartTextBoxTestTab.Text = values[1];
						EndTextBoxTestTab.Text = values[2];
						PointsAmountTextBoxTestTab.Text = values[3];
						XStarValuesTextBoxTestTab.Text = values[4];
						SaTextBoxTestTab.Text = values[5];
						SbTextBoxTestTab.Text = values[6];

						this.Title = $"{windowTitle} - {fileName}";
						testFileName = fileName;

						if (!debug) DrawSplines();
					}
					catch (Exception ex)
					{
						ShowError(ex, debug);
					}
					if (debug) DrawSplines();
				}
			}
		}


		private void DrawSplines()
		{
			if (FunctionsTab.SelectedItem == TestFunctionTab)
			{
				xValues = new List<double>();
				yValues = new List<double>();
				xStarValues = null;

				if (XStarValuesTextBoxTestTab.Text.Length != 0)
					xStarValues = StringToListOfDoubles(XStarValuesTextBoxTestTab.Text.Replace(" ", ""), "x*");


				List<int> values = new List<int>();

				if (StringToListOfDoubles(StartTextBoxTestTab.Text, "a").Count > 1) throw new Exception("Для поля \"a\" допустимо только одно значение");
				if (StringToListOfDoubles(EndTextBoxTestTab.Text, "b").Count > 1) throw new Exception("Для поля \"b\" допустимо только одно значение");


				int pointsAmount;
				try
				{
					pointsAmount = int.Parse(PointsAmountTextBoxTestTab.Text);
				}
				catch 
				{
					throw new Exception("Для поля \"Количество точек\" допустимо только одно целочисленное значение");
				}

				var a = StringToListOfDoubles(StartTextBoxTestTab.Text, "a")[0];
				var b = StringToListOfDoubles(EndTextBoxTestTab.Text, "b")[0];

				var step = (b - a) / (pointsAmount - 1);

				//Табулирование тестовой функции
				for (var x = a; x <= b + step * 0.2; x += step)
				{
					xValues.Add(x);
					if (TestFunctionsComboBox.SelectedItem.ToString() == sinus)
					{
						yValues.Add(Math.Sin(x));
					}
					else if (TestFunctionsComboBox.SelectedItem.ToString() == ln)
					{
						var value = Math.Log(x);
						if (double.IsInfinity(value))
							throw new Exception($"Недопустимое значение x = {x} для функции {ln}");
						else yValues.Add(value);
					}
				}

				//xValues = GetValues(xValues, pointsAmount);
				//yValues = GetValues(yValues, pointsAmount);

				var SaValues = StringToListOfDoubles(SaTextBoxTestTab.Text.Replace(" ", ""), "S'(a)");
				var SbValues = StringToListOfDoubles(SbTextBoxTestTab.Text.Replace(" ", ""), "S'(b)");
				if (SaValues.Count != 1) throw new Exception("Допустимо не более одного значений S'(a)");
				if (SbValues.Count != 1) throw new Exception("Допустимо не более одного значений S'(b)");

				//Sa = SaValues[0];
				//Sb = SbValues[0];
				if (TestFunctionsComboBox.SelectedItem.ToString() == sinus)
				{
					Sa = Math.Cos(xValues[0]);
					Sb = Math.Cos(xValues.Last());
				}
				else if (TestFunctionsComboBox.SelectedItem.ToString() == ln)
				{
					Sa = Math.Pow(xValues[0], -1);
					Sb = Math.Pow(xValues.Last(), -1);
				}

				SaTextBoxTestTab.Text = Sa.ToString();
				SbTextBoxTestTab.Text = Sb.ToString();

				var xSpline2 = new List<double>();
				var ySpline2 = new List<double>();

				var fXStar = new List<double>();
				var phiXStar = new List<double>();
				var epsilons = new List<double>();
				var eValues = new List<double>();

				spline1 = new Spline(xValues, yValues, Sa, Sb);

				foreach (var x in xValues)
					xSpline2.Add(x);
				if (xStarValues != null)
				{
					foreach (var xStar in xStarValues)
					{
						if (xStar >= xValues.First() && xStar <= xValues.Last())
						{
							xSpline2.Add(xStar);
						}
					}
				}

				foreach (var x in xSpline2)
				{
					var phiXStarValue = spline1.GetYCoordinateOnSpline(x);
					
					ySpline2.Add(phiXStarValue);
				}

				foreach (var x in xStarValues)
				{
					var phiXStarValue = spline1.GetYCoordinateOnSpline(x);
					double fXStarValue = -1;

					if (TestFunctionsComboBox.SelectedItem.ToString() == sinus)
						fXStarValue = Math.Sin(x);
					else if (TestFunctionsComboBox.SelectedItem.ToString() == ln)
						fXStarValue = Math.Log(x);

					double epsilon;
					epsilon = Math.Abs(fXStarValue - phiXStarValue);
					fXStar.Add(fXStarValue);
					epsilons.Add(epsilon);
					phiXStar.Add(phiXStarValue);
					//var k = 1.0 / 12;
					var k = 3.0 / 128;

					eValues.Add(k * Math.Pow(step, 3) * fXStarValue);
				}

				for (int i = 0; i < xSpline2.Count; i++)
				{
					for (int j = i + 1; j < xSpline2.Count; j++)
					{
						if (xSpline2[i] > xSpline2[j])
						{
							/*
							var xTemp = xSpline2[i];
							xSpline2[i] = xSpline2[j];
							xSpline2[j] = xTemp;

							var yTemp = ySpline2[i];
							ySpline2[i] = ySpline2[j];
							ySpline2[j] = yTemp;
							*/

							//кортежи
							//меняются местами i и j значения
							(xSpline2[i], xSpline2[j]) = (xSpline2[j], xSpline2[i]);
							(ySpline2[i], ySpline2[j]) = (ySpline2[j], ySpline2[i]);
						}
					}
				}

				string result = "";
				for (int i = 0; i < fXStar.Count; i++)
				{
					result += $"x* = {xStarValues[i]}\nf(x*) = {fXStar[i]}\nφ(x*) = {phiXStar[i]}\nE(x*) = {eValues[i]}\nε = {epsilons[i]}\n\n";
				}
				ResultsTextBlockTestTab.Text = result;

				spline2 = new Spline(xSpline2, ySpline2, Sa, Sb);

				SplineVisual splineVisual1 = new SplineVisual(spline1, color1, "Исходный сплайн", true);
				SplineVisual splineVisual2 = new SplineVisual(spline2, color2, "Построенный сплайн", true);

				ShowChart(ref Spline1ChartTestTab, splineVisual1);
				ShowChart(ref Spline2ChartTestTab, splineVisual2);
				ShowChart(ref SplinesBothChartTestTab, splineVisual1, splineVisual2);
			}
			else if (FunctionsTab.SelectedItem == TableFunctionTab)
			{
				xValues = null;
				yValues = null;
				xStarValues = null;

				if (XValuesTextBox.Text.Replace(" ", "").Length == 0 ||
					YValuesTextBox.Text.Replace(" ", "").Length == 0 ||
					SaTextBox.Text.Replace(" ", "").Length == 0 ||
					SbTextBox.Text.Replace(" ", "").Length == 0)
					throw new Exception("Поля X, Y, S'(a) и S'(b) должны быть заполнены");

				xValues = StringToListOfDoubles(XValuesTextBox.Text.Replace(" ", ""), "X");
				yValues = StringToListOfDoubles(YValuesTextBox.Text.Replace(" ", ""), "Y");
				if (xValues.Count != yValues.Count) throw new Exception("Количество координат X и Y должно совпадать!");

				var SaValues = StringToListOfDoubles(SaTextBox.Text.Replace(" ", ""), "S'(a)");
				var SbValues = StringToListOfDoubles(SbTextBox.Text.Replace(" ", ""), "S'(b)");

				//Задание количества точек для комбобокса
				try
				{
					var item = PointsAmountComboBox.SelectedItem;
					if (item == null) throw new Exception();
					pointsAmount = int.Parse(item.ToString());
				}
				catch
				{
					pointsAmount = xValues.Count;
					PointsAmountComboBox.SelectedItem = pointsAmount;
				}

				xValues = GetValues(xValues, pointsAmount);
				yValues = GetValues(yValues, pointsAmount);

				if (XStarValuesTextBox.Text.Length != 0)
					xStarValues = StringToListOfDoubles(XStarValuesTextBox.Text.Replace(" ", ""), "x*");

				if (SaValues.Count != 1) throw new Exception("Допустимо не более одного значений S'(a)");
				if (SbValues.Count != 1) throw new Exception("Допустимо не более одного значений S'(b)");
				Sa = SaValues[0];
				Sb = SbValues[0];

				var xSpline2 = new List<double>();
				var ySpline2 = new List<double>();
				var yStar = new List<double>();

				spline1 = new Spline(xValues, yValues, Sa, Sb);

				foreach (var x in xValues)
					xSpline2.Add(x);
				if (xStarValues != null)
				{
					foreach (var xStar in xStarValues)
					{
						if (xStar >= xValues.First() && xStar <= xValues.Last())
						{
							xSpline2.Add(xStar);
						}
					}
				}

				foreach (var x in xSpline2)
					ySpline2.Add(spline1.GetYCoordinateOnSpline(x));

				string result = "";
				foreach (var xStar in xStarValues)
				{
					double phiXStarValue = 0;

					try
					{
						phiXStarValue = spline1.GetYCoordinateOnSpline(xStar);
						result += $"φ({xStar}) = {phiXStarValue}\n";
					}
					catch
					{
						result += $"φ({xStar}) = ...\n";
					}
				}
				ResultsTextBlock.Text = result;


				for (int i = 0; i < xSpline2.Count; i++)
				{
					for (int j = i + 1; j < xSpline2.Count; j++)
					{
						if (xSpline2[i] > xSpline2[j])
						{
							/*
							var xTemp = xSpline2[i];
							xSpline2[i] = xSpline2[j];
							xSpline2[j] = xTemp;

							var yTemp = ySpline2[i];
							ySpline2[i] = ySpline2[j];
							ySpline2[j] = yTemp;
							*/

							//кортежи
							//меняются местами i и j значения
							(xSpline2[i], xSpline2[j]) = (xSpline2[j], xSpline2[i]);
							(ySpline2[i], ySpline2[j]) = (ySpline2[j], ySpline2[i]);
						}
					}
				}

				
				spline2 = new Spline(xSpline2, ySpline2, Sa, Sb);

				SplineVisual mainSpline = new SplineVisual(spline1, color1, "Исходный сплайн", true);
				SplineVisual mainSplineDots = new SplineVisual(spline1, color1, "Исходный сплайн", true);
				SplineVisual builtSpline = new SplineVisual(spline2, color2, "Построенный сплайн", true);

				ShowChart(ref Spline1Chart, mainSpline);
				ShowChart(ref Spline2Chart, builtSpline);
				ShowChart(ref SplinesBothChart, builtSpline, mainSplineDots);
			}
		}


		private void RunMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (debug)
			{
				DrawSplines();
			}
			else
			{
				try
				{
					DrawSplines();
				}
				catch (Exception ex)
				{
					ShowError(ex, debug);
				}
			}
		}


		private void ShowError(Exception ex, bool debug)
		{
			if (debug)
				MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
			else
				MessageBox.Show($"{ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
		}


		private void ShowError(string message)
		{
			MessageBox.Show($"{message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
