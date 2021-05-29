using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace LinearNetwork
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Points = new ObservableCollection<Point>();
            DeletePointCommand = new RelayCommand(o =>
            {
                Points.Remove((Point) o);
                OnPropertyChanged(nameof(Points));
            });
        }

        public ICommand DeletePointCommand { get; }

        public ObservableCollection<Point> Points { get; set; }
        public InitialParams InitialParams { get; set; } = new InitialParams();
    }

    class InitialParams : ViewModelBase
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

        private double _weight1;
        private double _weight2;
        private double _bias;
        private double _learningRate = 0.01;
        private int _maxIterations = 50;
    }
}
