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

        public static readonly DependencyProperty FillColor1Property = DependencyProperty.RegisterAttached("FillColor1", typeof(Brush), typeof(AttachedPropertyFactory)
                                                                                                            , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetFillColor1(DependencyObject d, Color value) => d.SetValue(AttachedPropertyFactory.FillColor1Property, value);

        public static Color GetFillColor1(DependencyObject d) => (Color)d.GetValue(AttachedPropertyFactory.FillColor1Property);

        #endregion

        #region FillColor2

        public static readonly DependencyProperty FillColor2Property = DependencyProperty.RegisterAttached("FillColor2", typeof(Brush), typeof(AttachedPropertyFactory)
                                                                                                           , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetFillColor2(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.FillColor2Property, value);

        public static Brush GetFillColor2(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.FillColor2Property);

        #endregion
    }
}
