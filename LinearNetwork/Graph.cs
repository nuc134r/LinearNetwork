using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearNetwork
{
    class Graph : FrameworkElement
    {
        public static readonly DependencyProperty PointsProperty 
            = DependencyProperty.Register("Points", typeof(ObservableCollection<Point>), typeof(Graph), 
                new FrameworkPropertyMetadata(default(ObservableCollection<Point>), FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Graph graph)
            {
                ((ObservableCollection<Point>) e.NewValue).CollectionChanged += (sender, args) => graph.InvalidateVisual();
            }
        }

        public ObservableCollection<Point> Points
        {
            get => (ObservableCollection<Point>) GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        public static readonly DependencyProperty LineProperty 
            = DependencyProperty.Register("Line", typeof(InitialParams), typeof(Graph), 
                new FrameworkPropertyMetadata(default(InitialParams), FrameworkPropertyMetadataOptions.AffectsRender));
        public InitialParams Line
        {
            get => (InitialParams) GetValue(LineProperty);
            set => SetValue(LineProperty, value);
        }

        public Graph()
        {
            Loaded += (sender, args) =>
            {
                _x = ScreenToGraph(-ActualWidth / 2, 0);
                _y = ScreenToGraph(-ActualHeight / 2, 0);

                InvalidateVisual();
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            var actualSize = new Rect(new Size(ActualWidth, ActualHeight));

            dc.PushClip(new RectangleGeometry(actualSize));
            dc.DrawRectangle(Brushes.White, null, actualSize);
            
            // координатные линии
            for (var i = (int) Math.Floor(ScreenToGraph(0, _x)); i < (int) Math.Floor(ScreenToGraph(ActualWidth, _x)); i++)
            {
                dc.DrawLine(_coordPen, new Point(GraphToScreen(i, _x), 0), new Point(GraphToScreen(i, _x), ActualHeight));
            }
            for (var i = (int) Math.Floor(ScreenToGraph(0, _y)); i < (int) Math.Floor(ScreenToGraph(ActualHeight, _y)); i++)
            {
                dc.DrawLine(_coordPen, new Point(0, GraphToScreen(i, _y)), new Point(ActualWidth, GraphToScreen(i, _y)));
            }

            // координатные оси
            dc.DrawLine(_axisPen, new Point(GraphToScreen(0, _x), 0), new Point(GraphToScreen(0, _x), ActualHeight));
            dc.DrawLine(_axisPen, new Point(0, GraphToScreen(0, _y)), new Point(ActualWidth, GraphToScreen(0, _y)));

            if (Points != null)
            {
                foreach (var point in Points)
                {
                    var x = Math.Round(GraphToScreen(point.X, _x), 1);
                    var y = Math.Round(GraphToScreen(point.Y, _y), 1);
                    dc.DrawEllipse(Brushes.IndianRed, null, new Point(x, y), 4, 4);
                }
            }

            dc.Pop();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (Points == null) return;

            var position = e.GetPosition(this);

            Points.Add(new Point(ScreenToGraph(position.X, _x), ScreenToGraph(position.Y, _y)));

            InvalidateVisual();
        }

        private double GraphWidth => ActualWidth / _zoom;
        private double GraphHeight => ActualHeight / _zoom;

        private double ScreenToGraph(double value, double offset)
        {
            return value / _zoom + offset;
        }

        private double GraphToScreen(double value, double offset)
        {
            return (value - offset) * _zoom;
        }

        private double _zoom = 10;
        private double _x = 0;
        private double _y = 0;

        private readonly Pen _axisPen = new Pen(new SolidColorBrush(Color.FromRgb(123, 123, 123)).ToFrozen(), 1.5).ToFrozen();
        private readonly Pen _coordPen = new Pen(new SolidColorBrush(Color.FromRgb(234, 234, 234)).ToFrozen(), 1).ToFrozen();
    }

    public static class Extensions
    {
        public static T ToFrozen<T>(this T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }

    }
}
