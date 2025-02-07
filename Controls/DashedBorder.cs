using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFModernVerticalMenu.Controls
{
    public class DashedBorder : Border
    {
        public static readonly DependencyProperty UseDashedBorderProperty =
            DependencyProperty.Register(nameof(UseDashedBorder), typeof(bool), typeof(DashedBorder), new PropertyMetadata(false));

        public static readonly DependencyProperty DashedBorderBrushProperty =
            DependencyProperty.Register(nameof(DashedBorderBrush), typeof(Brush), typeof(DashedBorder), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyProperty.Register(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(DashedBorder), new PropertyMetadata(new DoubleCollection { 4, 4 }));

        public bool UseDashedBorder
        {
            get => (bool)GetValue(UseDashedBorderProperty);
            set => SetValue(UseDashedBorderProperty, value);
        }

        public Brush DashedBorderBrush
        {
            get => (Brush)GetValue(DashedBorderBrushProperty);
            set => SetValue(DashedBorderBrushProperty, value);
        }

        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (UseDashedBorder)
            {
                Pen pen = new Pen(DashedBorderBrush, BorderThickness.Left) { DashStyle = new DashStyle(StrokeDashArray, 0) };
                dc.DrawRectangle(null, pen, new Rect(0, 0, ActualWidth, ActualHeight));
            }
            else
            {
                base.OnRender(dc);
            }
        }
    }
}
