using System;
using System.Collections.Generic;

namespace SplineVisualizationWPF
{
	internal class Spline
	{
		List<double> SaVariants = new List<double>();
		List<double> SbVariants = new List<double>();
		List<double> xStarVariants = new List<double>();
		double[][] yVariants = new double[26][];

		public List<double> X { get; set; } = new List<double>();
		public List<double> Y { get; set; } = new List<double>();
		public double Sa { get; private set; }
		public double Sb { get; private set; }
		public static List<int> Variants
		{
			get
			{
				List<int> variants = new List<int>();
				for (int i = 1; i < 26; i++)
				{
					variants.Add(i);
				}
				return variants;
			}

		}

		public Spline(List<double> x, List<double> y, double Sa, double Sb)
		{
			X = x;
			Y = y;
			this.Sa = Sa;
			this.Sb = Sb;
		}

		public double GetYCoordinateOnSpline(double x)
		{
			List<double> A = new List<double>();
			List<double> B = new List<double>();
			List<double> C = new List<double>();
			List<double> D = new List<double>();
			List<double> E = new List<double>();
			List<double> H = new List<double>();

			for (int l = 1; l < X.Count; l++)
			{
				var hi_1 = X[l] - X[l - 1];
				double hi;
				if (l == X.Count - 1) hi = hi_1;
				else hi = X[l + 1] - X[l];

				var minValue = -hi / 3;
				var maxValue = hi_1 / 3;
				var sigmai = (minValue + maxValue) / 2;

				var Ai = -2 / hi_1 + sigmai * (6 / (hi_1 * hi_1));
				var Bi = -4 / hi - 4 / hi_1 - sigmai * (6 / (hi * hi)) + sigmai * (6 / (hi_1 * hi_1));
				var Ci = -2 / hi - sigmai * (6 / (hi * hi));
				var Di = -6 / hi - sigmai * (12 / (hi * hi));
				var Ei = -6 / hi_1 + sigmai * (12 / (hi_1 * hi_1));

				A.Add(Ai);
				B.Add(Bi);
				C.Add(Ci);
				D.Add(Di);
				E.Add(Ei);
				H.Add(hi);
			}

			List<double> alpha = new List<double>();
			List<double> beta = new List<double>();
			alpha.Add(0.0);
			beta.Add(Sa);

			//прогонка вперед
			for (int l = 1; l < A.Count; l++)
			{
				var a = A[l];
				var b = B[l];
				var c = C[l];
				var d = D[l];
				var e = E[l];

				alpha.Add(-c / (a * alpha[l - 1] + b));

				var yip1 = Y[l + 1];
				var yi = Y[l];
				var yim1 = Y[l - 1];

				//числитель
				var betaNumerator =
						d * ((yip1 - yi) / H[l]) +
						e * ((yi - yim1) / H[l - 1]) - a * beta[l - 1];
				//знаменатель
				var betaDenominator = a * alpha[l - 1] + b;

				beta.Add(betaNumerator / betaDenominator);
			}

			//прогонка назад
			List<double> m = new List<double>();
			for (int n = 0; n < alpha.Count + 1; n++) m.Add(double.NaN);
			m[m.Count - 1] = Sb;

			for (int l = m.Count - 1; l > 0; l--)
			{
				var mi = alpha[l - 1] * m[l] + beta[l - 1];
				m[l - 1] = mi;
			}

			//Результат
			var i = GetIndexByNearest(X, x);
			var start = X[i];
			var end = X[i + 1];
			var tau = (x - X[i]) / H[i];
			var tau2 = Math.Pow(tau, 2);
			var tau3 = Math.Pow(tau, 3);


			var f = Y;
			var f_ = m;

			var result =
					f[i] * (1 - 3 * tau2 + 2 * tau3) +
					f[i + 1] * (3 * tau2 - 2 * tau3) +
					f_[i] * H[i] * (tau - 2 * tau2 + tau3) +
					f_[i + 1] * H[i] * (tau3 - tau2);

			return result;
		}


		public int GetIndexByNearest(List<double> x, double value)
		{
			for (int i = 0; i < x.Count - 1; i++)
			{
				if (x[i] <= value && value <= x[i + 1])
					return i;
			}
			return -1;
		}
	}
}
