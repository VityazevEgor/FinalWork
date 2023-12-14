using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VityazevFinalWork.Solution.Misc;
using VityazevFinalWork.Solution.Models;

namespace VityazevFinalWork.Solution.Modules
{
    internal class SSAmodel
    {
        private readonly List<MTData> _data;
        private readonly MLContext _context;

        public SsaForecastingTransformer? _model;

        public class MTData
        {
            //public required DateTime date { get; set; }
            public float amount;
            public MTData(float value)
            {
                this.amount = value;
            }
        }

        public class TDataForecast
        {
            public float[]? ForecastedAmount { get; set; }
            public float[]? ConfidenceLowerBound { get; set; }
            public float[]? ConfidenceUpperBound { get; set; }
        }


        public SSAmodel(List<TData> data)
        {
            _data = new List<MTData>();
            data.ForEach(d =>
            {
                _data.Add(new MTData(Convert.ToSingle(d.amount)));
            });
            _context = new MLContext();
        }

        public void Train()
        {
            var dataView = _context.Data.LoadFromEnumerable(_data);

            // Создайте конвейер
            var pipeline = _context.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedAmount",
                inputColumnName: "amount",
                windowSize: 11, // количество месяцев 
                seriesLength: 11*2+1, // Общее количество месяцев 
                trainSize: _data.Count,
                horizon: 11,
                confidenceLevel: 0.9f,
                confidenceLowerBoundColumn: "ConfidenceLowerBound",
                confidenceUpperBoundColumn: "ConfidenceUpperBound"
                );

            // Обучите модель
            _model = pipeline.Fit(dataView);
        }

        public float[] Predict()
        {
            
            var forecastEngine = _model.CreateTimeSeriesEngine<MTData, TDataForecast>(_context);
            var forecast = forecastEngine.Predict();
            Debug.WriteLine($"Предикт на 11 месяцев: {string.Join(", ", forecast.ForecastedAmount)}");
            Debug.WriteLine($"Реальные данные на 11 месяцев: {string.Join(", ", ExcelReader.ReadTestData().Select(d => d.amount).ToArray())}");
            //Debug.WriteLine($"Нижняя граница: {string.Join(" ", forecast.ConfidenceLowerBound)}");
            return forecast.ForecastedAmount;
        }

        public void TestModel(CartesianChart? chart = null)
        {
            var testData = new List<MTData>();
            ExcelReader.ReadTestData().ForEach(data => { testData.Add(new MTData((float)data.amount)); });
            var forecastEngine = _model.CreateTimeSeriesEngine<MTData, TDataForecast>(_context);
            float[]? prediction = forecastEngine.Predict().ForecastedAmount;

            // Вычислите среднюю абсолютную ошибку (MAE)
            var mae = prediction.Zip(testData.Select(x => x.amount), (forecast, actual) => Math.Abs(forecast - actual)).Average();
            Debug.WriteLine($"MAE = {mae}");

            if (chart  != null)
            {
                chart.Dispatcher.Invoke(() => { 
                    var actualValues = new ChartValues<double>(testData.Select(x => (double)x.amount));
                    var predictedValues = new ChartValues<double>(prediction.Select(x => (double)x));
                    var lowerBoundValues = new ChartValues<double>(forecastEngine.Predict().ConfidenceLowerBound.Select(x => (double)x));

                    var actualSeries = new ScatterSeries { Title = "Actual", Values = actualValues, MinPointShapeDiameter = 20 };
                    var predictedSeries = new LineSeries { Title = "SSA", Values = predictedValues };
                    //var lowerBoundSeies = new LineSeries { Title = "Lower bound", Values = lowerBoundValues };

                    chart.Series = new SeriesCollection { actualSeries, predictedSeries };
                    chart.AxisX.Add(new Axis { Title = "Time" });
                    chart.AxisY.Add(new Axis { Title = "Value" });
                });
            }
        }



    }
}
