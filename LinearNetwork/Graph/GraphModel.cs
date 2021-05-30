using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace LinearNetwork.Graph
{
    class GraphModel
    {
        public GraphModel()
        {
            Function = new LinearFunction();
            Points = new ObservableCollection<Point>();
            Points.CollectionChanged += (sender, args) => RequestInvalidate?.Invoke();
        }

        public ObservableCollection<Point> Points { get; set; }

        public LinearFunction Function
        {
            get => _function;
            set
            {
                _function = value;
                RequestInvalidate?.Invoke();
            }
        }

        public event Action RequestInvalidate;

        private LinearFunction _function;
    }
}