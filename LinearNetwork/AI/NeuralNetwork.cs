using System;
using System.Threading;
using System.Windows;

namespace LinearNetwork.AI
{
    class NeuralNetwork
    {
        public NeuralNetwork(InitialParams @params, Action<string> logger, Action<LinearFunction> callback)
        {
            _params = @params;
            _logger = logger;
            _callback = callback;

            _w1 = @params.Weight1;
            _w2 = @params.Weight2;
            _b = @params.Bias;
        }

        public (int iterations, double totalError, double w1, double w2, double b, bool isOverflow) Train(Point[] trainingData)
        {
            var i = 0;
            var rate = _params.LearningRate;
            var targetError = _params.TargetError;
            var maxIterations = _params.MaxIterations;

            var totalError = double.MaxValue;

            var isOverflow = false;

            while (i < maxIterations && totalError > targetError)
            {
                foreach (var point in trainingData)
                {
                    const int desired = 0;
                    var actual = ComputeOutput(point, _w1, _w2, _b);
                    var err = desired - actual;

                    _w1 = _w1 + rate * err * point.X;
                    _w2 = _w2 + rate * err * point.Y;

                    _b = _b + rate * err;
                }

                totalError = TotalError(trainingData, _w1, _w2, _b);

                if (double.IsNaN(_w1) || double.IsNaN(_w2) || double.IsNaN(_b) ||  double.IsNaN(totalError) ||
                    double.IsInfinity(_w1) || double.IsInfinity(_w2) || double.IsInfinity(_b) ||  double.IsInfinity(totalError))
                {
                    isOverflow = true;
                    _logger?.Invoke($"Обнаружено переполнение");
                    break;
                }

                if (maxIterations < 3_000 || i % 100 == 0)
                {
                    _logger?.Invoke($"Эп. {i + 1}, w1={_w1:e2}, w2={_w2:e2}, b={_b:e2}, ош={totalError:F2}");
                }

                _callback?.Invoke(new LinearFunction {Weight1 = _w1, Weight2 = _w2, Bias = _b});

                i++;

                if (_callback != null && i % 2 == 0)
                {
                    Thread.Sleep(1);
                }
            }

            return (i, totalError, _w1, _w2, _b, isOverflow);
        }

        private static double ComputeOutput(Point point, double w1, double w2, double b)
        {
            return w1 * point.X + w2 * point.Y + b;
        }
        
        private static double TotalError(Point[] points, double w1, double w2, double b)
        {
            double sum = 0;

            foreach (var point in points)
            {
                var res = ComputeOutput(point, w1, w2, b);
                sum += res * res;
            }

            return sum / 2;
        }
        

        private double _w1;
        private double _w2;
        private double _b;

        private readonly InitialParams _params;
        private readonly Action<string> _logger;
        private readonly Action<LinearFunction> _callback;
    }

}
