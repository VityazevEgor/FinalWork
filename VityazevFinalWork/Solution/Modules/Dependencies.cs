using System.Collections.Generic;
using System.Linq;
using VityazevFinalWork.Solution.Models;
using Accord.Statistics.Testing;
using MathNet.Numerics.Statistics;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace VityazevFinalWork.Solution.Modules
{
    internal class Dependencies
    {
        private readonly List<TData> _data;

        public Dependencies(List<TData> data)
        {
            _data = data;
        }

        public (double w, double p) lib_ShapiroWilkTest()
        {
            var test = new ShapiroWilkTest(_data.Select(d => d.amount).ToArray());
            return (test.Statistic, test.PValue);
        }

        public (double w, double p) lib_AndersonDarlingTest()
        {
            var sp = new Specifications(_data, null);

            var adTest = new AndersonDarlingTest(sp.GetNormolizedData(), new Accord.Statistics.Distributions.Univariate.NormalDistribution());

            return (adTest.Statistic, adTest.PValue);
        }

        public double[] lib_AutoCorelation(CartesianChart? chart = null)
        {
            double[] acf = Correlation.Auto(_data.Select(d=>d.amount).ToArray());
            if (chart != null)
            {
                chart.Dispatcher.Invoke(() =>
                {
                    LineSeries lineSeries = new LineSeries
                    {
                        Title = "Автокорреляционной функция",
                        Values = new ChartValues<ObservablePoint>(),
                        Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                        Fill = new SolidColorBrush(Color.FromArgb(20, 255, 0, 0)),
                        PointGeometry = null
                    };
                    for (int i=0; i < acf.Length; i++)
                    {
                        lineSeries.Values.Add(new ObservablePoint(i, Math.Round( acf[i],4)));
                    }
                    chart.Series = new SeriesCollection() { lineSeries };

                    chart.AxisX.Clear();
                    chart.AxisY.Clear();
                    chart.AxisX.Add(new Axis { Title = "Лаг", FontSize=15 });
                    chart.AxisY.Add(new Axis { Title = "Значение АФК", FontSize=15 });
                });
            }
            return acf;
        }

    }
}
