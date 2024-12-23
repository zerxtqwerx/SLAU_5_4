using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace MatrixSolver
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            /*// Чтение размерности
              int n;
              if (!int.TryParse(txtSize.Text, out n) || n <= 0)
              {
                  MessageBox.Show("Введите корректное значение размерности.");
                  return;
              }*/
            int n = 3;
            double[][] A = new double[n][];
            A[0] = new double[] { 1, 2, 4, -10 };
            A[1] = new double[] { 2, 13, 23, -50 };
            A[2] = new double[] { 4, 23, 77, -150 };

            double[] b = { 2.222, 0.555, 1.666 };

            double[] x0_1 = new double[n];
            double[] x0_2 = Enumerable.Repeat(1.0, n).ToArray();

            // Решение с методом Якоби
            var (solutionJacobi1, normJacobi1) = Jacobi(A, b, x0_1);
            var (solutionJacobi2, normJacobi2) = Jacobi(A, b, x0_2);

            // Решение с методом минимальных невязок
            var (solutionMinRes1, normMinRes1) = MinResidualMethod(A, b, x0_1);
            var (solutionMinRes2, normMinRes2) = MinResidualMethod(A, b, x0_2);

           /* MessageBox.Show($"Решение методом Якоби (начальное приближение 1): ", Convert.ToString(solutionJacobi1));
            MessageBox.Show($"Решение методом Якоби (начальное приближение 2): ", Convert.ToString(solutionJacobi2));
            MessageBox.Show($"Решение методом минимальных невязок (начальное приближение 1): ", Convert.ToString(solutionMinRes1));
            MessageBox.Show($"Решение методом минимальных невязок (начальное приближение 2): ", Convert.ToString(solutionMinRes2));*/

            PlotGraph(normJacobi1, normJacobi2, normMinRes1, normMinRes2);
        }

        private double DotProduct(double[] v1, double[] v2)
        {
            return v1.Zip(v2, (a, b) => a * b).Sum();
        }

        private double Norm(double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => x * x));
        }

        private double[] MatrixVectorProduct(double[][] A, double[] x)
        {
            return A.Select(row => DotProduct(row, x)).ToArray();
        }

        private double[] VectorSubtract(double[] v1, double[] v2)
        {
            return v1.Zip(v2, (a, b) => a - b).ToArray();
        }

        private double[] VectorAdd(double[] v1, double[] v2)
        {
            return v1.Zip(v2, (a, b) => a + b).ToArray();
        }

        private double[] VectorScale(double[] v, double scalar)
        {
            return v.Select(a => a * scalar).ToArray();
        }

        private (double[], List<double>) Jacobi(double[][] A, double[] b, double[] x0, double tol = 0.0001, int maxIterations = 100)
        {
            int n = b.Length;
            double[] x = (double[])x0.Clone();
            List<double> norms = new List<double>();

            for (int k = 0; k < maxIterations; k++)
            {
                double[] x_new = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double sum1 = DotProduct(A[i].Take(i).ToArray(), x.Take(i).ToArray());
                    double sum2 = DotProduct(A[i].Skip(i + 1).ToArray(), x.Skip(i + 1).ToArray());
                    x_new[i] = (b[i] - sum1 - sum2) / A[i][i];
                }

                double[] diff_vector = VectorSubtract(x_new, x);
                norms.Add(Norm(diff_vector));
                if (Norm(diff_vector) < tol) break;

                x = x_new;
            }
            return (x, norms);
        }

        private (double[], List<double>) MinResidualMethod(double[][] A, double[] b, double[] x0, double tol = 0.0001, int maxIterations = 100)
        {
            double[] x = (double[])x0.Clone();
            List<double> norms = new List<double>();

            for (int k = 0; k < maxIterations; k++)
            {
                double[] r = VectorSubtract(b, MatrixVectorProduct(A, x));
                double alpha = DotProduct(r, r) / DotProduct(r, MatrixVectorProduct(A, r));
                x = VectorAdd(x, VectorScale(r, alpha));

                double current_norm = Norm(r);
                norms.Add(current_norm);
                if (current_norm < tol) break;
            }
            return (x, norms);
        }

        private void PlotGraph(List<double> normJacobi1, List<double> normJacobi2, List<double> normMinRes1, List<double> normMinRes2)
        {
            GraphPane pane = new GraphPane();
            pane.Title.Text = "Норма невязки";
            pane.XAxis.Title.Text = "Итерации";
            pane.YAxis.Title.Text = "Норма невязки";

            pane.AddCurve("Якоби, нач. прибл. 0", Enumerable.Range(0, normJacobi1.Count).Select(i => (double)i).ToArray(), normJacobi1.ToArray(), System.Drawing.Color.Blue);
            pane.AddCurve("Якоби, нач. прибл. 1", Enumerable.Range(0, normJacobi2.Count).Select(i => (double)i).ToArray(), normJacobi2.ToArray(), System.Drawing.Color.Red);
            pane.AddCurve("Мин. невязок, нач. прибл. 0", Enumerable.Range(0, normMinRes1.Count).Select(i => (double)i).ToArray(), normMinRes1.ToArray(), System.Drawing.Color.Green);
            pane.AddCurve("Мин. невязок, нач. прибл. 1", Enumerable.Range(0, normMinRes2.Count).Select(i => (double)i).ToArray(), normMinRes2.ToArray(), System.Drawing.Color.Orange);

            pane.YAxis.Scale.Min = 1e-10; // Установка минимального значения для логарифмической шкалы
            pane.YAxis.Scale.Max = Math.Max(normJacobi1.Max(), Math.Max(normJacobi2.Max(), Math.Max(normMinRes1.Max(), normMinRes2.Max()))) * 10;
            pane.YAxis.Type = AxisType.Log; // Логарифмическая шкала

            var graphControl = new ZedGraphControl
            {
                GraphPane = pane,
                Dock = DockStyle.Fill
            };

            var form = new Form
            {
                Text = "График",
                WindowState = FormWindowState.Maximized
            };
            form.Controls.Add(graphControl);
            form.Load += (s, e) => graphControl.AxisChange();
            form.ShowDialog();
        }
    }
}