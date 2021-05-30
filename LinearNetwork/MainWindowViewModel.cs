using LinearNetwork.AI;
using LinearNetwork.Graph;
using LinearNetwork.Util;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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

        public bool IsLearning
        {
            get => _isLearning;
            set
            {
                if (value == _isLearning) return;
                _isLearning = value;
                OnPropertyChanged();
            }
        }

        public bool IsLearningRateConfigurable
        {
            get => _isLearningRateConfigurable;
            set
            {
                if (value == _isLearningRateConfigurable) return;
                _isLearningRateConfigurable = value;
                OnPropertyChanged();
            }
        }

        public string NewPointX { get; set; }
        public string NewPointY { get; set; }

        public int CurrentTabIndex
        {
            get => _currentTabIndex;
            set
            {
                if (value == _currentTabIndex) return;
                _currentTabIndex = value;
                IsLearningRateConfigurable = _currentTabIndex == 0;
                OnPropertyChanged();
            }
        }

        public string Messages
        {
            get => _messages;
            set
            {
                if (value == _messages) return;
                _messages = value;
                OnPropertyChanged();
            }
        }

        public LinearFunctionGraphModel GraphModel { get; set; } = new LinearFunctionGraphModel();
        public RatioGraphModel EpochRatioGraphModel { get; set; } = new RatioGraphModel();
        public RatioGraphModel AccuracyRatioGraphModel { get; set; } = new RatioGraphModel();

        private async Task StartLearning()
        {
            if (CurrentTabIndex == 0)
            {
                await Learn();
            }
            else
            {
                await Benchmark();
            }
        }

        private async Task Learn()
        {
            IsLearning = true;

            var initialParams = InitialParams.Clone();
            var net = new NeuralNetwork(initialParams,
                s => { Messages += s + Environment.NewLine; },
                f =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (Action) (() => GraphModel.Function = f));
                });

            Messages = string.Empty;

            var iterations = 0;
            var totalError = -1.0;
            var w1 = 0.0;
            var w2 = 0.0;
            var b = 0.0;

            await Task.Run(() => { (iterations, totalError, w1, w2, b, _) = net.Train(GraphModel.Points.ToArray()); });

            Messages += "Обучение окончено!" + Environment.NewLine + Environment.NewLine;
            Messages += $"Итераций: {iterations}/{initialParams.MaxIterations} {Environment.NewLine}{Environment.NewLine}";

            Messages += $"w₁: {w1} {Environment.NewLine}";
            Messages += $"w₂: {w2} {Environment.NewLine}";
            Messages += $"bₖ: {b} {Environment.NewLine}{Environment.NewLine}";

            Messages += $"Ср. кв. ошибка: {totalError} {Environment.NewLine}";
            Messages += $"Целевая ошибка: {initialParams.TargetError} {Environment.NewLine}";
            Messages += $"Точность: {Math.Abs(totalError - initialParams.TargetError)} {Environment.NewLine}";

            IsLearning = false;
        }

        private async Task Benchmark()
        {
            IsLearning = true;

            var initialParams = InitialParams.Clone();
            var points = GraphModel.Points.ToArray();
            EpochRatioGraphModel.Items.Clear();
            AccuracyRatioGraphModel.Items.Clear();

            Messages = string.Empty;

            for (var i = 0; i < Consts.LearningRates.Length; i++)
            {
                var rate = Consts.LearningRates[i];
                Messages += $"{i}/{Consts.LearningRates.Length}... ";

                var net = new NeuralNetwork(initialParams.Clone(withRate: rate), null, null);
                
                var iterations = 0;
                var totalError = -1.0;
                var w1 = 0.0;
                var w2 = 0.0;
                var b = 0.0;
                var overflow = false;

                await Task.Run(() => (iterations, totalError, w1, w2, b, overflow) = net.Train(points));
                
                var accuracy = Math.Abs(totalError - initialParams.TargetError);

                if (overflow)
                {
                    Messages += $"{iterations} эпох, переполнение{Environment.NewLine}";
                    EpochRatioGraphModel.Items.Add(0);
                    AccuracyRatioGraphModel.Items.Add(0);
                }
                else
                {
                    Messages += $"{iterations} эпох, точность = {accuracy:0.##################################################}{Environment.NewLine}";
                    EpochRatioGraphModel.Items.Add(iterations);
                    AccuracyRatioGraphModel.Items.Add(accuracy);
                }

                
            }

            Messages += $"Подбор окончен! {Environment.NewLine}";

            IsLearning = false;
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

        private string _messages = string.Empty;
        private int _currentTabIndex = 0;
        private bool _isLearningRateConfigurable = true;
        private bool _isLearning;
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

        public InitialParams Clone(double? withRate = null)
        {
            return new InitialParams
            {
                Weight1 = Weight1,
                Weight2 = Weight2,
                Bias = Bias,
                LearningRate = withRate ?? LearningRate,
                MaxIterations = MaxIterations,
                TargetError = TargetError,
            };
        }
    }
}
