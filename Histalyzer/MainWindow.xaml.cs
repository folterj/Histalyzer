using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Histalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		StatData stats = new StatData();
		HistGraph histGraph = new HistGraph();
		RenderTargetBitmap bitmap;
		int width, height;
		string path;
		string[] files;
		string file;
		bool hasHeader = false;
		bool hasData = false;

		string[] columnLabels = new string[0] { };
		int selectedColumn = 0;

		int nbins = 20;
		double partition = 0.95;
		bool init = false;


		public MainWindow()
		{
			InitializeComponent();

			binsSlider.Value = nbins;
			partitionSlider.Value = partition;

			updateList();

			init = true;
		}

		void openFile(bool resetSelectedColumn = true)
		{
			StreamReader reader;
			List<string> columnNumbers = new List<string>();
			string line;
			string[] parts;
			double x;
			int nums = 0;
			int nonnums = 0;

			try
			{
				reader = new StreamReader(file);
				line = reader.ReadLine();
				reader.Close();

				if (line != null)
				{
					parts = line.Split(new string[] { "," }, StringSplitOptions.None);
					foreach (string part in parts)
					{
						if (double.TryParse(part, out x))
						{
							nums++;
						}
						else
						{
							nonnums++;
						}
					}

					hasHeader = (nonnums > nums);

					if (hasHeader)
					{
						columnLabels = parts;
					}
					else
					{
						for (int i = 0; i < parts.Length; i++)
						{
							columnNumbers.Add(i.ToString());
						}
						columnLabels = columnNumbers.ToArray();
					}

					hasData = true;
				}
				else
				{
					hasHeader = false;
					hasData = false;
					columnLabels = new string[] { };
				}

				columnCombo.ItemsSource = columnLabels;
				if (resetSelectedColumn)
				{
					selectedColumn = 0;
				}
				if (selectedColumn < columnLabels.Length)
				{
					columnCombo.SelectedIndex = selectedColumn;
				}

				readFile();
				redraw();
			}
			catch (Exception e)
			{
				MessageBox.Show("File error: " + e.Message, "Error");
			}
		}

		void readFile()
		{
			StreamReader reader = new StreamReader(file);
			string line;
			List<double> values;
			bool first = true;

			stats.data.Clear();

			while ((line = reader.ReadLine()) != null)
			{
				if (!first || !hasHeader)
				{
					values = Util.parseLine(line);
					if (selectedColumn < values.Count)
					{
						stats.data.Add(values[selectedColumn]);
					}
				}
				first = false;
			}
			reader.Close();
			stats.calcStats(nbins, partition);
		}

		void redraw()
		{
			string s = "File: " + Path.GetFileName(file) + "\n";
			if (columnLabels.Length != 0)
			{
				s += "Column: " + columnLabels[selectedColumn] + "\n";
			}
			s += stats.ToString();
			outText.Text = s;

			if (histGraph.inited)
			{
				bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
				createHistImage();
				histImage.Source = bitmap;
			}
		}

		void createHistImage()
		{
			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			drawHist(drawingContext, width, height);
			drawingContext.Close();

			bitmap.Render(drawingVisual);
		}

		void drawHist(DrawingContext drawingContext, double width, double height)
		{
			double vx1, vx2, vy1, vy2;
			double min0 = 0;
			int nbins = stats.bins.Length;
			int scaleStep;
			int i = 0;

			if (stats.min < 0)
			{
				min0 = stats.min;
			}

			scaleStep = (int)Math.Pow(10, (int)Math.Round(Math.Log10(nbins) - 1));
			if (scaleStep < 1)
			{
				scaleStep = 1;
			}

			if (stats != null)
			{
				foreach (int count in stats.bins)
				{
					vx1 = (double)i / nbins;
					vx2 = (double)(i + 1) / nbins;
					vy1 = (double)count / stats.maxVal;
					vy2 = 0;

					histGraph.drawBar(drawingContext, vx1, vy1, vx2, vy2);

					if (nbins < 50 || i % scaleStep == 0)
					{
						histGraph.drawText(drawingContext, vx1, vy2, Util.numericLabel((double)i / nbins * stats.maxRange + min0));
					}
					i++;
				}
			}
		}

		void updateList()
		{
			path = folderText.Text;
			if (path != "")
			{
				files = Util.getFiles(path, "*.csv");
				fileListBox.ItemsSource = Util.getFileTitles(files);
			}
		}

		private void folderButton_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();

			folderDialog.SelectedPath = path;
			if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				path = folderDialog.SelectedPath;
				folderText.Text = path;
			}
		}

		private void folderText_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (folderText.Text != "")
			{
				updateList();
			}
		}

		private void fileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			file = files[fileListBox.SelectedIndex];
			openFile(false);
		}

		private void openMenuItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			if ((bool)openFileDialog.ShowDialog())
			{
				file = openFileDialog.FileName;
				openFile();
			}
		}

		private void columnCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			selectedColumn = columnCombo.SelectedIndex;
			if (selectedColumn < 0)
            {
				selectedColumn = 0;
			}
			readFile();
			redraw();
		}

		private void binsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (init)
			{
				nbins = (int)binsSlider.Value;
				stats.calcStats(nbins, partition);
				redraw();
			}
		}

		private void partitionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (init)
			{
				partition = partitionSlider.Value;
				stats.calcStats(nbins, partition);
				redraw();
			}
		}

		private void histImage_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			width = (int)e.NewSize.Width;
			height = (int)e.NewSize.Height;
			histGraph.init(width, height);
			redraw();
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			string text = "";

			if (histGraph.inited && hasData)
			{
				Point point = e.GetPosition(histImage);
				double dataPosition = histGraph.getDataPosition(point);
				int nbins = stats.bins.Length;
				int bin = (int)(dataPosition * nbins);

				double range1 = (double)bin / nbins * stats.maxRange;
				double range2 = (double)(bin + 1) / nbins * stats.maxRange;
				if (bin >= 0 && bin < nbins)
				{
					int count = stats.bins[bin];

					if (bin + 1 == nbins)
					{
						text = string.Format("Bin: {0}-", Util.numericLabel(range1));
					}
					else
					{
						text = string.Format("Bin: {0}-{1}", Util.numericLabel(range1), Util.numericLabel(range2));
					}
					text += string.Format(" Count: {0} ({1:P1}/Max) ({2:P1}/Tot)",
										count, (double)count / stats.maxVal, (double)count / stats.data.Count);
				}
			}
			statusText.Content = text;
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e)
		{
			statusText.Content = "";
		}

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			AboutBox about = new AboutBox();
			about.ShowDialog();
		}

	}
}
