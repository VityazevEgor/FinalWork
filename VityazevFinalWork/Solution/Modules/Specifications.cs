using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VityazevFinalWork.Solution.Models;

namespace VityazevFinalWork.Solution.Modules
{
    internal class Specifications
    {
        private readonly List<TData> _data;
        private readonly CartesianChart? _chart;

        public Specifications(List<TData> data, CartesianChart? chart)
        {
            _data = data;
            _chart = chart;
        }

        // срденее
        public double Mean()
        {
            return _data.Average(d => d.amount);
        }

        // медиана
        public double Median()
        {
            var sortedAmounts = _data.Select(d => d.amount).OrderBy(a => a).ToList();
            int count = sortedAmounts.Count;
            if (count % 2 == 0)
            {
                return (sortedAmounts[count / 2 - 1] + sortedAmounts[count / 2]) / 2;
            }
            else
            {
                return sortedAmounts[count / 2];
            }
        }

        // стандартное отклонение
        public double StandardDeviation()
        {
            double mean = Mean();
            double sumOfSquaresOfDifferences = _data.Select(d => Math.Pow(d.amount - mean, 2)).Sum();
            double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / _data.Count);
            return standardDeviation;
        }

        // мода
        public double Mode()
        {
            return _data.GroupBy(n => n.amount)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key).FirstOrDefault();
        }

        // дисперсия
        public double Variance()
        {
            double mean = Mean();
            double variance = _data.Average(d => Math.Pow(d.amount - mean, 2));
            return variance;
        }


        public (double, double) MinMax()
        {
            return (_data.MinBy(n => n.amount).amount, _data.MaxBy(n => n.amount).amount);
        }

        // скос
        public double Skewness()
        {
            double mean = Mean();
            double standardDeviation = StandardDeviation();
            double skewness = _data.Average(d => Math.Pow(d.amount - mean, 3)) / Math.Pow(standardDeviation, 3);
            return skewness;
        }

        // эксцесс
        public double Kurtosis()
        {
            double mean = Mean();
            double standardDeviation = StandardDeviation();
            double kurtosis = _data.Average(d => Math.Pow(d.amount - mean, 4)) / Math.Pow(standardDeviation, 4);
            return kurtosis;
        }

        // Для всяких тестов на нормальнео распрделение
        public double[] GetNormolizedData()
        {
            double mean = Mean();
            double stdDev = StandardDeviation();

            return _data.Select(d => (d.amount - mean) / stdDev).ToArray();
        }

        public void BuildGraph()
        {
            if (_chart != null)
            {
                _chart.Dispatcher.Invoke(() =>
                {
                    LineSeries lineSeries = new LineSeries
                    {
                        Title = "Заявки на кредит",
                        Values = new ChartValues<double>(),
                        PointGeometry = null // Это убирает маркеры точек на линии
                    };
                    // Заполняем LineSeries данными
                    foreach (var data in _data)
                    {
                        lineSeries.Values.Add(data.amount);
                    }
                    // Добавляем LineSeries на график
                    _chart.Series.Add(lineSeries);

                    // Устанавливаем метки для оси X
                    _chart.AxisX.Add(new Axis
                    {
                        Title = "Время",
                        Labels = _data.Select(d => d.date.ToString()).ToList(),
                        Separator = new Separator { Step = 10, IsEnabled = false } // Настраиваем шаг меток
                    });

                    // Устанавливаем метки для оси Y
                    _chart.AxisY.Add(new Axis
                    {
                        Title = "Количество заявок",
                        LabelFormatter = value => value.ToString("N")
                    });
                });
            }
            else
            {
                MessageBox.Show("_chart is null");
            }
        }
    }
}
