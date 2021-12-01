using System;
using System.Collections.Generic;

namespace Histalyzer
{
	class StatData
	{
		public List<double> data = new List<double>();

		public int[] bins = new int[0];
		public double min, max;
		public double maxRange;
		public double mean, median;
		public double peak, peak2;
		public double otsu, otsuMax;
		public int maxVal;
		public double stdDev, stdDevPos, stdDevNeg;
		public double ssstdDev;

		public void add(double x)
		{
			data.Add(x);
		}

		public bool calcStats(int nbins, double partition)
		{
			if (data.Count > 0)
			{
				data.Sort();	// for median etc
				calcMean();
				calcMedian();
				calcStdDev();
				calcSSStdDev();
				calcRange();
				calcHistogram(nbins, partition);
				calcMax();
				peak = calcPeak(0);
				calcOtsu();
				if (otsu != 0)
				{
					peak2 = calcPeak(otsu);
					otsuMax = peak2 + (peak2 - otsu);
				}
				return true;
			}
			return false;
		}

		public void calcMean()
		{
			double tot = 0;

			if (data.Count > 0)
			{
				foreach (double x in data)
				{
					tot += x;
				}
				mean = tot / data.Count;
			}
		}

		public void calcMedian()
		{
			median = calcPartition(0.5f);
		}

		public void calcRange()
		{
			if (data.Count > 0)
			{
				min = data[0];
				max = data[0];

				foreach (double x in data)
				{
					if (x < min)
					{
						min = x;
					}
					if (x > max)
					{
						max = x;
					}
				}
			}
		}

		public double calcPartition(double partition)
		{
			int index;
			int max = data.Count;

			if (max > 0)
			{
				index = (int)(max * partition);
				if (index >= max)
                {
					index = max - 1;
                }
				return data[index];
			}
			return 0;
		}

		public double getBinCenter(int bini)
        {
			double min0 = 0;
			if (min < 0)
            {
				min0 = min;
            }
			return (bini + 0.5f) / bins.Length * maxRange + min0;
		}

		public void calcStdDev()
		{
			double totdif = 0;
			double totdifpos = 0;
			double totdifneg = 0;
			int npos = 0;
			int nneg = 0;
			double dif;

			foreach (double x in data)
			{
				dif = x - mean;
				totdif += dif * dif;
				if (dif > 0)
				{
					totdifpos += dif * dif;
					npos++;
				}
				else
				{
					totdifneg += dif * dif;
					nneg++;
				}
			}

			if (data.Count != 0)
			{
				stdDev = Math.Sqrt(totdif / data.Count);
			}
			else
			{
				stdDev = 0;
			}

			if (npos != 0)
			{
				stdDevPos = Math.Sqrt(totdifpos / npos);
			}
			else
			{
				stdDevPos = 0;
			}

			if (nneg != 0)
			{
				stdDevNeg = Math.Sqrt(totdifneg / nneg);
			}
			else
			{
				stdDevNeg = 0;
			}
		}

		public void calcSSStdDev()
		{
			double totdif = 0;
			double dif;

			foreach (double x in data)
			{
				dif = x - 0;
				totdif += dif * dif;
			}
			ssstdDev = Math.Sqrt(totdif / data.Count);
		}

		public void calcHistogram(int nbins, double partition)
		{
			// Optimal bin size: https://en.wikipedia.org/wiki/Freedman%E2%80%93Diaconis_rule

//			double q1, q3, iqr, binsize;
//			int n = data.Count;
			int i;
			double min0 = 0;

			data.Sort();

			if (min < 0)
			{
				min0 = min;
			}
			maxRange = calcPartition(partition) - min0;

			if (maxRange != 0)
			{
//				q1 = calcPartition(0.25);
//				q3 = calcPartition(0.75);
//				iqr = q3 - q1;
//				binsize = 2 * iqr / Math.Pow(n, (double)1 / 3);
//				nbins = (int)Math.Ceiling(maxRange / binsize);

//              nbins = (int)Math.Pow(n, (double)1 / 3);

//              nbins = 20;

                bins = new int[nbins];

				foreach (double x in data)
				{
					i = (int)((x - min0) / maxRange * nbins);
					if (i >= nbins)
					{
						i = nbins - 1;
					}
					if (i < 0)
                    {
						i = 0;
                    }
					bins[i]++;
				}
			}
			else
			{
				bins = new int[0];
				maxRange = 1;
			}
		}

		public double calcPeak(double start)
		{
			double peak = 0;
			double binval;
			double peakval = 0;
			int mini = (int)(start / maxRange * bins.Length);
			int maxi = bins.Length - 1;

			if (maxi < 1)
			{
				maxi = 1;
			}

			if (data.Count > 0)
			{
				// skip last bin
				for (int i = maxi - 1; i > mini; i--)
				{
					if (i >= 0 && i < bins.Length)
					{
						binval = bins[i];
						if (binval > peakval)
						{
							peakval = binval;
							peak = getBinCenter(i);
						}
					}
				}
			}
			return peak;
		}

		public void calcMax()
		{
			int maxval = 0;

			if (data.Count > 0)
			{
				foreach (int x in bins)
				{
					if (x > maxval)
					{
						maxval = x;
					}
				}
				maxVal = maxval;
			}
		}

		public void calcOtsu()
		{
			double wF, mF, between;
			double sumB = 0;
			double wB = 0;
			double maximum = 0;
			double total = 0;
			double sum1 = 0;
			int nbins = bins.Length - 1;	// skip last bin

			otsu = 0;

			for (int i = 0; i < nbins; i++)
			{
				total += bins[i];
				sum1 += i * bins[i];
			}

			for (int i = 0; i < nbins; i++)
			{
				wB += bins[i];
				wF = total - wB;

				if (wB != 0 && wF != 0)
				{
					sumB = sumB + i * bins[i];
					mF = (sum1 - sumB) / wF;
					between = wB * wF * ((sumB / wB) - mF) * ((sumB / wB) - mF);
					if (between >= maximum)
					{
						otsu = getBinCenter(i);
						maximum = between;
					}
				}
			}
		}

		public override string ToString()
		{
			string output = "";

			output += string.Format("N: {0}\n", data.Count);
			output += string.Format("Range: {0} - {1}\n", Util.numericLabel(min), Util.numericLabel(max));
			output += string.Format("Mean: {0}\n", Util.numericLabel(mean));
			output += string.Format("Median: {0}\n", Util.numericLabel(median));
			output += string.Format("Peak: {0}\n", Util.numericLabel(peak));
			if (otsu != 0)
			{
				output += string.Format("Otsu: {0}\n", Util.numericLabel(otsu));
				output += string.Format("Secondary peak: {0}\n", Util.numericLabel(peak2));
				output += string.Format("Otsu maximum range: {0}\n", Util.numericLabel(otsuMax));
			}
			output += string.Format("StdDev: {0} (pos: {1} neg: {2})\n", Util.numericLabel(stdDev), Util.numericLabel(stdDevPos), Util.numericLabel(stdDevNeg));
			output += string.Format("Single-sided StdDev: {0}", Util.numericLabel(ssstdDev));

			return output;
		}

	}
}
