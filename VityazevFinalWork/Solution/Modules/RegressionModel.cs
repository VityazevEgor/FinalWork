using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VityazevFinalWork.Solution.Misc;
using VityazevFinalWork.Solution.Models;
using static VityazevFinalWork.Solution.Models.TrainModels;

namespace VityazevFinalWork.Solution.Modules
{
    internal class RegressionModel
    {
        private readonly List<InputData> _data;
        private readonly List<InputData> _testData;
        private readonly MLContext _context = new MLContext();
        private Dictionary<string, ITransformer> _models = new Dictionary<string, ITransformer>();

        public RegressionModel(List<TData> data)
        {
            _data = new List<InputData>();
            data.ForEach(d =>
            {
                _data.Add(d.ToInpData());
            });
            _testData = _data.TakeLast(11).ToList();
            _data = _data.Take(_data.Count - 11).ToList();
        }

        public void TrainAll()
        {
            var dataView = _context.Data.LoadFromEnumerable(_data);

            var sdcaPipeline = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Regression.Trainers.Sdca(labelColumnName: "amount", maximumNumberOfIterations: 100));

            var onlineGradientDescentPipeline = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.OnlineGradientDescent(labelColumnName: "amount"));

            var poissonPipeline = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: "amount"));

            var fastTree = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Regression.Trainers.FastTree(labelColumnName: "amount"));

            var fastForest = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Regression.Trainers.FastForest(labelColumnName: "amount"));

            // Обучение моделей
            _models["SDCA"] = sdcaPipeline.Fit(dataView);
            _models["OnlineGradientDescent"] = onlineGradientDescentPipeline.Fit(dataView);
            _models["Poisson"] = poissonPipeline.Fit(dataView);
            _models["fastTree"] = fastTree.Fit(dataView);
            _models["fastForest"] = fastForest.Fit(dataView);
        }

        public void PredictAll(DateTime date)
        {
            var data = new InputData() { date = Shared.ConvertToUnixTime(date) };

            foreach (var model in _models)
            {
                var predictionEngine = _context.Model.CreatePredictionEngine<InputData, PredictData>(model.Value);
                var result = predictionEngine.Predict(data);
                Debug.WriteLine($"Predicted amount for date: {data.date} using {model.Key} = {result.amount}");
            }
        }

        public void TestModels(CartesianChart? chart = null)
        {
            var testData = _context.Data.LoadFromEnumerable(_testData);
            foreach (var model in _models)
            {
                IDataView predictions = _models[model.Key].Transform(testData);
                var metrics = _context.Regression.Evaluate(predictions, labelColumnName: "amount");
                Debug.WriteLine($"---{model.Key}---");
                Debug.WriteLine($"MAE: {metrics.MeanAbsoluteError:#.##}");

                if (chart != null)
                {
                    var predictionEngine = _context.Model.CreatePredictionEngine<InputData, PredictData>(model.Value);
                    chart.Dispatcher.Invoke(() =>
                    {
                        var predictedValues = new ChartValues<double>();
                        _testData.ForEach(t =>
                        {
                            predictedValues.Add((double)predictionEngine.Predict(t).amount);
                        });
                        var predictedSeries = new LineSeries { Title = model.Key, Values = predictedValues };
                        chart.Series.Add(predictedSeries);
                    });
                }

            }
        }
    }
}
