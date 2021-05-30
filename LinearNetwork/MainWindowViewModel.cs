using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LinearNetwork.AI;
using LinearNetwork.Graph;
using LinearNetwork.Util;

namespace LinearNetwork
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            DeletePointCommand = new RelayCommand(o => GraphModel.Points.Remove((Point) o));
            ClearPointsCommand = new RelayCommand(o => GraphModel.Points.Clear());
            StartCommand = new AsyncCommand(StartLearning);
            AddCommand = new RelayCommand(o => AddPoint(), o => double.TryParse(NewPointX, out var a) && double.TryParse(NewPointY, out var b));
        }

        public ICommand DeletePointCommand { get; }
        public ICommand ClearPointsCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand AddCommand { get; }

        public InitialParams InitialParams { get; set; } = new InitialParams();

        public bool IsLearning { get; set; }

        public string NewPointX { get; set; }
        public string NewPointY { get; set; }

        public string Messages { get; set; } = string.Empty;

        public GraphModel GraphModel { get; set; } = new GraphModel();

        private async Task StartLearning()
        {
            IsLearning = true;
            OnPropertyChanged(nameof(IsLearning));

            var initialParams = InitialParams.Clone();
            var net = new NeuralNetwork(initialParams, 
            s =>
            {
                Messages += s + Environment.NewLine;
                OnPropertyChanged(nameof(Messages));
            }, 
            f =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action) (() => GraphModel.Function = f));
            });

            Messages = string.Empty;
            OnPropertyChanged(nameof(Messages));

            var iterations = 0;
            double totalError = -1;
            double w1 = 0;
            double w2 = 0;
            double b = 0;
            
            await Task.Run(() => { (iterations, totalError, w1, w2, b) = net.Train(GraphModel.Points.ToArray()); });

            Messages += "Обучение окончено!" + Environment.NewLine + Environment.NewLine;
            Messages += $"Итераций: {iterations}/{initialParams.MaxIterations} {Environment.NewLine}{Environment.NewLine}";
            Messages += $"w₁: {w1} {Environment.NewLine}";
            Messages += $"w₂: {w2} {Environment.NewLine}";
            Messages += $"bₖ: {b} {Environment.NewLine}{Environment.NewLine}";
            Messages += $"Ср. кв. ошибка: {totalError} {Environment.NewLine}";
            Messages += $"Целевая ошибка: {initialParams.TargetError} {Environment.NewLine}";
            Messages += $"Точность: {Math.Abs(totalError - initialParams.TargetError)} {Environment.NewLine}";

            OnPropertyChanged(nameof(Messages));

            IsLearning = false;
            OnPropertyChanged(nameof(IsLearning));
        }

        private void AddPoint()
        {
            var x = double.Parse(NewPointX);
            var y = double.Parse(NewPointY);

            NewPointX = NewPointY = string.Empty;
            OnPropertyChanged(nameof(NewPointX));
            OnPropertyChanged(nameof(NewPointY));

            GraphModel.Points.Add(new Point(x, y));
        }
    }

    internal class LinearFunction : ViewModelBase
    {
        public double Weight1
        {
            get => _weight1;
            set { _weight1 = value; OnPropertyChanged(); }
        }

        public double Weight2
        {
            get => _weight2;
            set { _weight2 = value; OnPropertyChanged(); }
        }

        public double Bias
        {
            get => _bias;
            set { _bias = value; OnPropertyChanged(); }
        }

        public double Calc(double x)
        {
            return (-Bias - Weight1 * x) / Weight2;
        }

        private double _weight1 = 3;
        private double _weight2 = 3;
        private double _bias = 1;
    }

    class InitialParams : LinearFunction
    {
        public double LearningRate
        {
            get => _learningRate;
            set { _learningRate = value; OnPropertyChanged(); }
        }

        public int MaxIterations
        {
            get => _maxIterations;
            set { _maxIterations = value; OnPropertyChanged(); }
        }

        public double TargetError
        {
            get => _targetError;
            set { _targetError = value; OnPropertyChanged(); }
        }

        private double _learningRate = 0.01;
        private int _maxIterations = 50;
        private double _targetError = 0.001;

        public InitialParams Clone()
        {
            return new InitialParams
            {
                Weight1 = Weight1,
                Weight2 = Weight2,
                Bias = Bias,
                LearningRate = LearningRate,
                MaxIterations = MaxIterations,
                TargetError = TargetError,
            };
        }
    }
}
