using Accord.Statistics.Models.Regression.Linear;
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
        private readonly MLContext _context = new MLContext();
        private Dictionary<string, ITransformer> _models = new Dictionary<string, ITransformer>();

        public RegressionModel(List<TData> data)
        {
            _data = new List<InputData>();
            data.ForEach(d =>
            {
                _data.Add(d.ToInpData());
            });
        }

        public void TrainPoisson()
        {
            var dataView = _context.Data.LoadFromEnumerable(_data);
            var poissonPipeline = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: "amount"));

            var model = poissonPipeline.Fit(dataView);
            Debug.WriteLine($"Bias = {model.LastTransformer.Model.Bias}");
            Debug.WriteLine($"Weights = {string.Join(" ", model.LastTransformer.Model.Weights)}");
            //model.LastTransformer.Model.We
                
        }


        public void TrainAll()
        {
            var dataView = _context.Data.LoadFromEnumerable(_data);

            var sdcaPipeline = _context.Transforms.Concatenate("Features", "date")
                //.Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.Sdca(labelColumnName: "amount", maximumNumberOfIterations: 100));

            var onlineGradientDescentPipeline = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.OnlineGradientDescent(labelColumnName: "amount"));

            var poissonPipeline = _context.Transforms.Concatenate("Features", "date")
                .Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: "amount"));

            var fastTree = _context.Transforms.Concatenate("Features", "date")
                //.Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.FastTree(labelColumnName: "amount"));

            var fastForest = _context.Transforms.Concatenate("Features", "date")
                //.Append(_context.Transforms.NormalizeMinMax("Features"))
                .Append(_context.Regression.Trainers.FastForest(labelColumnName: "amount"));

            // Обучите модели
            _models["SDCA"] = sdcaPipeline.Fit(dataView);
            _models["OnlineGradientDescent"] = onlineGradientDescentPipeline.Fit(dataView);
            _models["Poisson"] = poissonPipeline.Fit(dataView);
            _models["fastTree"] = fastTree.Fit(dataView);
            _models["fastForest"] = fastForest.Fit(dataView);

            var fT = fastTree.Fit(dataView);
            

            var s = new SimpleLinearRegression();
            Debug.WriteLine($"Аккорд = {s.Regress(_data.Select(d => Convert.ToDouble(d.date)).ToArray(), _data.Select(d => Convert.ToDouble(d.amount)).ToArray())}");



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
            var testList = new List<InputData>();
            ExcelReader.ReadTestData().ForEach(d =>
            {
                testList.Add(d.ToInpData());
            });

            var testData = _context.Data.LoadFromEnumerable(testList);
            foreach (var model in _models)
            {
                IDataView predictions = _models[model.Key].Transform(testData);
                var metrics = _context.Regression.Evaluate(predictions, labelColumnName: "amount");
                Debug.WriteLine($"---{model.Key}---");
                //Debug.WriteLine($"R^2: {metrics.RSquared:0.##}");
                Debug.WriteLine($"MAE: {metrics.MeanAbsoluteError:#.##}");
                Debug.WriteLine($"MSE: {metrics.MeanSquaredError:#.##}");
                Debug.WriteLine($"RMSE: {metrics.RootMeanSquaredError:#.##}");

                if (chart != null)
                {
                    var predictionEngine = _context.Model.CreatePredictionEngine<InputData, PredictData>(model.Value);
                    chart.Dispatcher.Invoke(() =>
                    {
                        var predictedValues = new ChartValues<double>();
                        testList.ForEach(t =>
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
