using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LinearNetwork.Util;

namespace LinearNetwork.Graph
{
    class Graph : FrameworkElement
    {
        public static readonly DependencyProperty ModelProperty 
            = DependencyProperty.Register("Model", typeof(GraphModel), typeof(Graph), 
                new PropertyMetadata(default(GraphModel), ModelChangedCallback));
        public GraphModel Model
        {
            get => (GraphModel) GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public Graph()
        {
            Loaded += (sender, args) => { ResetTransform(); };
        }
        
        protected override void OnRender(DrawingContext dc)
        {
            var actualSize = new Rect(new Size(ActualWidth, ActualHeight));

            dc.PushClip(new RectangleGeometry(actualSize));
            dc.DrawRectangle(Brushes.White, null, actualSize);
            
            // координатные линии
            for (var i = (int) Math.Floor(ScreenToGraph(0, _x, Axis.X)); i < (int) Math.Floor(ScreenToGraph(ActualWidth, _x, Axis.X)); i++)
            {
                dc.DrawLine(_coordPen, new Point(GraphToScreen(i, _x, Axis.X), 0), new Point(GraphToScreen(i, _x, Axis.X), ActualHeight));
            }
            for (var i = (int) Math.Floor(ScreenToGraph(0, _y, Axis.Y)); i < (int) Math.Floor(ScreenToGraph(ActualHeight, _y, Axis.Y)); i++)
            {
                dc.DrawLine(_coordPen, new Point(0, GraphToScreen(i, _y, Axis.Y)), new Point(ActualWidth, GraphToScreen(i, _y, Axis.Y)));
            }

            // координатные оси
            dc.DrawLine(_axisPen, new Point(GraphToScreen(0, _x, Axis.X), 0), new Point(GraphToScreen(0, _x, Axis.X), ActualHeight));
            dc.DrawLine(_axisPen, new Point(0, GraphToScreen(0, _y, Axis.Y)), new Point(ActualWidth, GraphToScreen(0, _y, Axis.Y)));

            // точки
            if (Model?.Points != null)
            {
                foreach (var point in Model.Points)
                {
                    var x = Math.Round(GraphToScreen(point.X, _x, Axis.X), 1);
                    var y = Math.Round(GraphToScreen(point.Y, _y, Axis.Y), 1);
                    dc.DrawEllipse(Brushes.IndianRed, null, new Point(x, y), 4, 4);

                    var text = new FormattedText($"{point.X:0.0}; {point.Y:0.0}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.Black, 96);
                    dc.DrawText(text, new Point(x, y));
                }
            }

            // линейная функция 
            if (Model?.Function != null)
            {
                var x1 = ScreenToGraph(0, _x, Axis.X);
                var x2 = ScreenToGraph(ActualWidth, _x, Axis.X);

                var y1 = Model.Function.Calc(x1);
                var y2 = Model.Function.Calc(x2);

                dc.DrawLine(_funcPen, new Point(0, GraphToScreen(y1, _y, Axis.Y)), new Point(ActualWidth, GraphToScreen(y2, _y, Axis.Y)));
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

                Model.Points.Add(new Point(ScreenToGraph(position.X, _x, Axis.X), ScreenToGraph(position.Y, _y, Axis.Y)));

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
                _x = _dragStartPosition.X + ScreenToGraph(_dragStartLocation.X, 0, Axis.X) - ScreenToGraph(position.X, 0, Axis.X);
                _y = _dragStartPosition.Y + ScreenToGraph(_dragStartLocation.Y, 0, Axis.Y) - ScreenToGraph(position.Y, 0, Axis.Y);

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

        private static void ModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Graph view)
            {
                ((GraphModel) e.NewValue).RequestInvalidate += () => view.InvalidateVisual();
            }
        }
        
        private void ResetTransform()
        {
            _x = ScreenToGraph(-ActualWidth / 2, 0, Axis.X);
            _y = ScreenToGraph(-ActualHeight / 2, 0, Axis.Y);
            _zoom = ZoomDefault;

            InvalidateVisual();
        }

        private double ScreenToGraph(double value, double offset, Axis axis)
        {
            return value / _zoom + offset;
        }

        private double GraphToScreen(double value, double offset, Axis axis)
        {
            return (value - offset) * _zoom;
        }

        private double _zoom = 20;
        private double _x;
        private double _y;

        private const double ZoomDefault = 20;

        private Point _dragStartLocation;
        private Point _dragStartPosition;
        private bool _isDragging;

        private readonly Pen _funcPen = new Pen(new SolidColorBrush(Color.FromRgb(95, 232, 86)).ToFrozen(), 1.5).ToFrozen();
        private readonly Pen _axisPen = new Pen(new SolidColorBrush(Color.FromRgb(123, 123, 123)).ToFrozen(), 1.5).ToFrozen();
        private readonly Pen _coordPen = new Pen(new SolidColorBrush(Color.FromRgb(234, 234, 234)).ToFrozen(), 1).ToFrozen();

        enum Axis
        {
            X,
            Y
        }
    }
}