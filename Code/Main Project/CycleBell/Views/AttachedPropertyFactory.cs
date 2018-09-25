/*
 *  StrokeColor is setted in BaseButton style and used in Path style
 */
using System.Windows;
using System.Windows.Media;

namespace CycleBell.Views
{
    public static class AttachedPropertyFactory
    {

        #region BorderColor

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.RegisterAttached("BorderColor", typeof(Color), typeof(AttachedPropertyFactory)                                                                                                    
                                                                                                            , new FrameworkPropertyMetadata(Colors.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetBorderColor(DependencyObject d, Color value)
        {
            d.SetValue(AttachedPropertyFactory.BorderColorProperty, value);
        }

        public static Color GetBorderColor(DependencyObject d)
        {
            return (Color) d.GetValue(AttachedPropertyFactory.BorderColorProperty);
        }

        #endregion

        #region StrokeColor

        public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.RegisterAttached("StrokeColor", typeof(Color), typeof(AttachedPropertyFactory)
                                                                                                            , new FrameworkPropertyMetadata(Colors.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStrokeColor(DependencyObject d, Color value) => d.SetValue(AttachedPropertyFactory.StrokeColorProperty, value);

        public static Color GetStrokeColor(DependencyObject d) => (Color)d.GetValue(AttachedPropertyFactory.StrokeColorProperty);

        #endregion

        #region FillColor1

        public static readonly DependencyProperty FillBrush1Property = DependencyProperty.RegisterAttached("FillBrush1", typeof(Brush), typeof(AttachedPropertyFactory)
                                                                                                            , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetFillBrush1(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.FillBrush1Property, value);

        public static Brush GetFillBrush1(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.FillBrush1Property);

        #endregion

        #region FillColor2

        public static readonly DependencyProperty FillBrush2Property = DependencyProperty.RegisterAttached("FillBrush2", typeof(Brush), typeof(AttachedPropertyFactory)
                                                                                                           , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetFillBrush2(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.FillBrush2Property, value);

        public static Brush GetFillBrush2(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.FillBrush2Property);

        #endregion

        #region FillColor1

        public static readonly DependencyProperty StrokeBrush1Property = DependencyProperty.RegisterAttached("StrokeBrush1", typeof(Brush), typeof(AttachedPropertyFactory)
            , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStrokeBrush1(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.StrokeBrush1Property, value);

        public static Brush GetStrokeBrush1(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.StrokeBrush1Property);

        #endregion

        #region FillColor2

        public static readonly DependencyProperty StrokeBrush2Property = DependencyProperty.RegisterAttached("StrokeBrush2", typeof(Brush), typeof(AttachedPropertyFactory)
            , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetStrokeBrush2(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.StrokeBrush2Property, value);

        public static Brush GetStrokeBrush2(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.StrokeBrush2Property);

        #endregion
    }
}
