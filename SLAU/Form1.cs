using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace IterativeMethodsZedGraph
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            SolveAndPlot();
        }

        private void SolveAndPlot()
        {
            double[,] A = {
                { 1, 2, 4, -10 },
                { 2, 13, 23, -50 },
                { 4, 23, 77, -150 },
            };

            double[] b = { 15, 10, 10, 10 };
            double[] initialGuess = { 0, 0, 0, 0 };
            double epsilon = 1e-6;

            // Списки для хранения значений и остатков
            List<double> jacobiResiduals = SolveUsingJacobi(A, b, initialGuess, epsilon);
            List<double> seidelResiduals = SolveUsingSeidel(A, b, initialGuess, epsilon);

            // Создание графика
            GraphPane pane = zedGraphControl.GraphPane;
            pane.Title.Text = "Зависимость нормы невязки от номера итерации";
            pane.XAxis.Title.Text = "Номер итерации";
            pane.YAxis.Title.Text = "Норма невязки";

            // Добавление данных метода Якоби
            LineItem jacobiCurve = pane.AddCurve("Метод Якоби", Enumerable.Range(1, jacobiResiduals.Count).Select(x => (double)x).ToArray(), jacobiResiduals.ToArray(), System.Drawing.Color.Blue);

            // Добавление данных метода Зейделя
            LineItem seidelCurve = pane.AddCurve("Метод Зейделя", Enumerable.Range(1, seidelResiduals.Count).Select(x => (double)x).ToArray(), seidelResiduals.ToArray(), System.Drawing.Color.Red);

            // Отображение графика
            zedGraphControl.AxisChange();
            zedGraphControl.Refresh();
        }

        private List<double> SolveUsingJacobi(double[,] A, double[] b, double[] initialGuess, double epsilon)
        {
            int n = b.Length;
            double[] x = (double[])initialGuess.Clone();
            double[] xNew = new double[n];
            List<double> residuals = new List<double>();

            do
            {
                for (int i = 0; i < n; i++)
                {
                    xNew[i] = (b[i] - Enumerable.Range(0, n).Where(j => j != i).Sum(j => A[i, j] * x[j])) / A[i, i];
                }

                double norm = Math.Sqrt(Enumerable.Range(0, n).Sum(i => Math.Pow(xNew[i] - x[i], 2)));
                residuals.Add(norm);

                x = (double[])xNew.Clone();

            } while (Math.Max(residuals.Last(), Math.Abs((b[0] - A[0, 0] * x[0])) / b[0]) > epsilon);

            return residuals;
        }

        private List<double> SolveUsingSeidel(double[,] A, double[] b, double[] initialGuess, double epsilon)
        {
            int n = b.Length;
            double[] x = (double[])initialGuess.Clone();
            List<double> residuals = new List<double>();

            do
            {
                double maxDiff = 0;

                for (int i = 0; i < n; i++)
                {
                    double xOld = x[i];
                    x[i] = (b[i] - Enumerable.Range(0, n).Sum(j => A[i, j] * (j <= i ? x[j] : xOld))) / A[i, i];
                    maxDiff = Math.Max(maxDiff, Math.Abs(x[i] - xOld));
                }

                residuals.Add(maxDiff);

            } while (residuals.Last() > epsilon);

            return residuals;
        }

        private void zedGraphControl_Load(object sender, EventArgs e)
        {
            InitializeComponent();
            SolveAndPlot();
        }
    }
}
