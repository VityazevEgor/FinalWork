using LiveCharts.Wpf;
using System.Collections.Generic;
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

        public Main(RichTextBox richTextBox, string excelFilePath, CartesianChart chart, CartesianChart korChart, CartesianChart modelChart)
        {
            _logsBox = richTextBox;
            //_data = ExcelReader.ReadExcel(excelFilePath);
            //_data = ExcelReader.MakeLegitData(_data);
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
            //ExcelReader.ReadBigExcel();
            print("1. Анализ характеристик объекта исследования", true);
            
            var sp = new Specifications(_data, _chart);
            print($"Среднее = {sp.Mean()}");
            print($"Медиана = {sp.Median()}");
            print($"Стандартное отклонение = {sp.StandardDeviation()}");
            print($"Мода = {sp.Mode()}");
            print($"Диспесрсия = {sp.Variance()}");
            print($"Минимум = {sp.MinMax().Item1}");
            print($"Максимум = {sp.MinMax().Item2}");
            print($"Скос = {sp.Skewness()}");
            print($"Эксцесс = {sp.Kurtosis()}");
            sp.BuildGraph();


            print("2. Моделирование статистических зависимостей", true);
            var dp = new Dependencies(_data);
            print($"Шапиро-Вилк тест PValue: {dp.lib_ShapiroWilkTest().p}");
            //print($"Колмогорова-Смирнова тест PValue: {dp.lib_KolmogorovSmirnovTest().p}");
            print($"Андерсон-Дарлинг тест Pvalue: {dp.lib_AndersonDarlingTest().p}");
            dp.lib_AutoCorelation(_korChart);

            var ml = new SSAmodel(_data);
            ml.Train();
            ml.Predict();
            ml.TestModel(_modelChart);

            var rm = new RegressionModel(_data);
            rm.TrainAll();
            rm.PredictAll(_data[0].date);
            rm.PredictAll(_data[_data.Count - 1].date);
            rm.TestModels(_modelChart);
            rm.TrainPoisson();
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
