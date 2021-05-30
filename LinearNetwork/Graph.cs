using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearNetwork
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

    class Graph : FrameworkElement
    {
        public static readonly DependencyProperty ModelProperty 
            = DependencyProperty.Register("Model", typeof(GraphModel), typeof(Graph), 
                new PropertyMetadata(default(GraphModel), ModelChangedCallback));

        private static void ModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Graph view)
            {
                ((GraphModel) e.NewValue).RequestInvalidate += () => view.InvalidateVisual();
            }
        }

        public GraphModel Model
        {
            get => (GraphModel) GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
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

            if (Model?.Points != null)
            {
                foreach (var point in Model.Points)
                {
                    var x = Math.Round(GraphToScreen(point.X, _x), 1);
                    var y = Math.Round(GraphToScreen(point.Y, _y), 1);
                    dc.DrawEllipse(Brushes.IndianRed, null, new Point(x, y), 4, 4);
                }
            }

            if (Model?.Function != null)
            {
                var x1 = ScreenToGraph(0, _x);
                var x2 = ScreenToGraph(ActualWidth, _x);

                var y1 = Model.Function.Calc(x1);
                var y2 = Model.Function.Calc(x2);

                dc.DrawLine(_funcPen, new Point(0, GraphToScreen(y1, _y)), new Point(ActualWidth, GraphToScreen(y2, _y)));
            }

            dc.Pop();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);

            if (e.ChangedButton == MouseButton.Left)
            {
                if (Model?.Points == null) return;
                if (IsEnabled == false) return;

                Model.Points.Add(new Point(ScreenToGraph(position.X, _x), ScreenToGraph(position.Y, _y)));

                InvalidateVisual();
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                Mouse.Capture(this);

                _dragStartLocation = position;
                _dragStartPosition = new Point(_x, _y);
                _isDragging = true;

                Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            if (_isDragging)
            {
                _x = _dragStartPosition.X + ScreenToGraph(_dragStartLocation.X, 0) - ScreenToGraph(position.X, 0);
                _y = _dragStartPosition.Y + ScreenToGraph(_dragStartLocation.Y, 0) - ScreenToGraph(position.Y, 0);

                InvalidateVisual();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_isDragging && e.ChangedButton == MouseButton.Right)
            {
                _isDragging = false;
                Cursor = Cursors.Arrow;
                Mouse.Capture(null);
            }
        }

        private double ScreenToGraph(double value, double offset)
        {
            return value / _zoom + offset;
        }

        private double GraphToScreen(double value, double offset)
        {
            return (value - offset) * _zoom;
        }

        private double _zoom = 20;
        private double _x = 0;
        private double _y = 0;

        private Point _dragStartLocation;
        private Point _dragStartPosition;
        private bool _isDragging;

        private readonly Pen _funcPen = new Pen(new SolidColorBrush(Color.FromRgb(95, 232, 86)).ToFrozen(), 1.5).ToFrozen();
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
