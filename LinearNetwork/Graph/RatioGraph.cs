using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LinearNetwork.AI;
using LinearNetwork.Util;

namespace LinearNetwork.Graph
{
    class RatioGraph : FrameworkElement
    {
        public static readonly DependencyProperty ModelProperty 
            = DependencyProperty.Register("Model", typeof(RatioGraphModel), typeof(RatioGraph), 
                new PropertyMetadata(default(RatioGraphModel), ModelChangedCallback));
        public RatioGraphModel Model
        {
            get => (RatioGraphModel) GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public RatioGraph()
        {
            Loaded += (sender, args) => { InvalidateVisual(); };
        }

        private static void ModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RatioGraph view)
            {
                ((RatioGraphModel) e.NewValue).RequestInvalidate += () => view.InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawLine(_axisPen, new Point(MarginOffset, MarginOffset), new Point(MarginOffset, ActualHeight - MarginOffset));
            dc.DrawLine(_axisPen, new Point(MarginOffset, ActualHeight - MarginOffset), new Point(ActualWidth - MarginOffset, ActualHeight - MarginOffset));

            var ratesCount = Consts.LearningRates.Length;

            var spacing = (ActualWidth - MarginOffset * 2) / (ratesCount + 1);

            for (var i = 0; i < ratesCount; i++)
            {
                var x = MarginOffset * 2 + spacing * i;
                var y = ActualHeight - MarginOffset;

                dc.DrawLine(_axisPen, new Point(x, y), new Point(x, y - 10));

                var text = new FormattedText(Consts.LearningRates[i].ToString("0.#####################"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.Black, 96);
                dc.DrawText(text, new Point(x - text.WidthIncludingTrailingWhitespace / 2, y - 28));
            }

            if (Model != null && Model.Items.Any())
            {
                var graphEnd = MarginOffset;
                var graphStart = ActualHeight - MarginOffset - MarginOffset;
                var pixels = graphEnd - graphStart;
                var maxVal = Model.Items.Max() + 1;

                var pixelsPerVal = pixels / maxVal;

                Point? lastPoint = null;

                for (var i = 0; i < Model.Items.Count; i++)
                {
                    var value = Model.Items[i];
                    
                    var x = MarginOffset * 2 + spacing * i;
                    var y = MarginOffset - (pixels - (pixelsPerVal * value));
                    var point = new Point(x, y);

                    if (lastPoint != null)
                    {
                        dc.DrawLine(_graphPen, lastPoint.Value, point);
                    }
                    lastPoint = point;
                    
                    dc.DrawEllipse(Brushes.CornflowerBlue, null, point, 3, 3);
                    
                    var text = new FormattedText(value.ToString("0.###"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.Black, 96);
                    dc.DrawText(text, new Point(x - text.WidthIncludingTrailingWhitespace / 2, y - 28));
                }
            }
        }

        private const double MarginOffset = 35;

        private readonly Pen _graphPen = new Pen(new SolidColorBrush(Colors.DeepSkyBlue).ToFrozen(), 1).ToFrozen();
        private readonly Pen _axisPen = new Pen(new SolidColorBrush(Color.FromRgb(123, 123, 123)).ToFrozen(), 2).ToFrozen();
        private readonly Pen _coordPen = new Pen(new SolidColorBrush(Color.FromRgb(234, 234, 234)).ToFrozen(), 1).ToFrozen();
    }

    class RatioGraphModel
    {
        public RatioGraphModel()
        {
            Items = new ObservableCollection<double>();
            Items.CollectionChanged += (sender, args) => RequestInvalidate?.Invoke();
        }

        public ObservableCollection<double> Items { get; set; }

        public event Action RequestInvalidate;
    }
}
