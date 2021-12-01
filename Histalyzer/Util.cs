using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Histalyzer
{
	class Util
	{
        public static string numericLabel(double label)
        {
            string labels;
            double test;
            int maxDecimals = 0;
            if (label != 0)
            {
                maxDecimals = (int)Math.Ceiling(1 - Math.Log10(Math.Abs(label)));
            }

            if (Math.Abs(maxDecimals) <= 3) {
                if (maxDecimals < 0)
                {
                    maxDecimals = 0;
                }
                labels = label.ToString("F" + string.Format("{0}", maxDecimals), CultureInfo.InvariantCulture);
            }
            else
            {
                labels = label.ToString("0.##E0", CultureInfo.InvariantCulture);
            }
			double.TryParse(labels, out test);
			return test.ToString();
		}

		public static string[] getFiles(string path, string pattern)
		{
			string[] files = new string[0];

			if (Directory.Exists(path))
			{
				files = Directory.GetFiles(path, pattern);
				Array.Sort(files);
			}
			return files;
		}

		public static string getFileTitle(string path)
		{
			string title;
			string[] parts;
			
			parts = path.Split(new Char[] { '\\' });
			title = parts[parts.Length - 1];
			parts = title.Split(new Char[] { '.' });
			title = parts[0];

			return title;
		}

		public static string[] getFileTitles(string[] paths)
		{
			List<string> files = new List<string>();
			foreach (string path in paths)
			{
				files.Add(getFileTitle(path));
			}
			return files.ToArray();
		}

		public static List<double> parseLine(string line)
		{
			List<double> values = new List<double>();
			double x;
			string[] parts = line.Split(new string[] { "," }, StringSplitOptions.None);
			foreach (string part in parts)
			{
				x = 0;
				double.TryParse(part.Replace("\"", ""), out x);
				values.Add(x);
			}
			return values;
		}

	}
}
