using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using VityazevFinalWork.Solution.Misc;
using VityazevFinalWork.Solution.Models;
using VityazevFinalWork.Solution.Modules;

namespace VityazevFinalWork.Solution
{
    internal class Main
    {
        private readonly RichTextBox _logsBox;
        private readonly List<TData> _data;
        private readonly CartesianChart _chart, _korChart, _modelChart;

        public Main(RichTextBox richTextBox, CartesianChart chart, CartesianChart korChart, CartesianChart modelChart)
        {
            _logsBox = richTextBox;
            _data = ExcelReader.ReadBigExcel();
            _chart = chart;
            _korChart = korChart;
            _modelChart = modelChart;
            _data = Shared.SmoothData(_data, 11);
        }

        public void Run(bool async = false)
        {
            if (async)
            {
                Task.Run(MainWorker);
            }
            else
            {
                MainWorker();
            }
        }

        private void MainWorker()
        {
            print("1. Анализ характеристик объекта исследования", true);
            
            var sp = new Specifications(_data, _chart);
            print($"Среднее = {sp.Mean()}");
            print($"Медиана = {sp.Median()}");
            print($"Стандартное отклонение = {sp.StandardDeviation()}");
            print($"Диспесрсия = {sp.Variance()}");
            print($"Минимум = {sp.MinMax().Item1}");
            print($"Максимум = {sp.MinMax().Item2}");
            print($"Скос = {sp.Skewness()}");
            print($"Эксцесс = {sp.Kurtosis()}");
            sp.BuildGraph();


            print("2. Моделирование статистических зависимостей", true);
            var dp = new Dependencies(_data);
            print($"Шапиро-Вилк тест PValue: {dp.lib_ShapiroWilkTest().p}");
            print($"Андерсон-Дарлинг тест Pvalue: {dp.lib_AndersonDarlingTest().p}");
            dp.lib_AutoCorelation(_korChart);

            
            var ssa = new SSAmodel(_data);
            ssa.Train();
            var ssaPredict =  ssa.Predict();
            var mae = ssa.TestModel(_modelChart);

            var rm = new RegressionModel(_data);
            rm.TrainAll();
            rm.PredictAll(_data[0].date);
            rm.PredictAll(_data[_data.Count - 1].date);
            rm.TestModels(_modelChart);

            print("4. Модель SSA", true);
            print($"Предсказанные значения на 11 месяцев: {string.Join(' ', ssaPredict)}");
            print($"Реальные значения на 11 месяцев: {string.Join(' ', _data.TakeLast(11).ToArray().Select(d=>Math.Round( d.amount)).ToArray())}");
            print($"MAE = {mae}");
        }


        private void print(string text, bool isTitel = false)
        {
            _logsBox.Dispatcher.Invoke(() =>
            {
                Paragraph p1 = new Paragraph();
                p1.Inlines.Add(new Run(text)
                {
                    FontSize = isTitel ? 20 : 15,
                    FontWeight = isTitel ? FontWeights.Bold : FontWeights.Light
                });
                _logsBox.Document.Blocks.Add(p1);
            });
        }
    }
}
