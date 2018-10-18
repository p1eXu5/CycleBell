/*
 *  StrokeColor is setted in BaseButton style and used in Path style
 */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CycleBell.Controls;

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

        #region BorderBrush

        public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.RegisterAttached("PressedBorderBrush", typeof(Brush), typeof(AttachedPropertyFactory)
            , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetPressedBorderBrush(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.PressedBorderBrushProperty, value);

        public static Brush GetPressedBorderBrush(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.PressedBorderBrushProperty);

        #endregion


        #region PathStrokeBrush

        public static readonly DependencyProperty PathStrokeBrushProperty = DependencyProperty.RegisterAttached("PathStrokeBrush", typeof(Brush), typeof(AttachedPropertyFactory)
                                                                                                            , new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetPathStrokeBrush(DependencyObject d, Brush value) => d.SetValue(AttachedPropertyFactory.PathStrokeBrushProperty, value);

        public static Brush GetPathStrokeBrush(DependencyObject d) => (Brush)d.GetValue(AttachedPropertyFactory.PathStrokeBrushProperty);

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

        #region IsFocused

        public static readonly DependencyProperty IsPresetNameFocusedProperty = DependencyProperty.RegisterAttached("IsPresetNameFocused", typeof(bool), typeof(AttachedPropertyFactory)
            , new FrameworkPropertyMetadata(false, ChangeFocus) {BindsTwoWayByDefault = true});

        public static void SetIsPresetNameFocused(DependencyObject d, bool value) => d.SetValue(IsPresetNameFocusedProperty, value);

        public static bool GetIsPresetNameFocused(DependencyObject d) => (bool)d.GetValue(IsPresetNameFocusedProperty);

        public static void ChangeFocus (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement elem) {

                elem.MoveFocus (new TraversalRequest (FocusNavigationDirection.Right));
                //elem.Focus();
            }
        }

        #endregion

        #region IsFocused

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(AttachedPropertyFactory)
                                                                                                          , new FrameworkPropertyMetadata(false, ChangeTimeFocus));

        public static void SetIsFocused(DependencyObject d, bool value) => d.SetValue(IsFocusedProperty, value);

        public static bool GetIsFocused(DependencyObject d) => (bool)d.GetValue(IsFocusedProperty);

        public static void ChangeTimeFocus(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement elem) {

                if (!elem.IsFocused)
                    elem.Focus();

                //if (elem is TimerBox tb)
                //    tb.CaretIndex = 2;
            }
        }

        #endregion

        #region IsFocused

        public static readonly DependencyProperty IsTimePopintsProperty = DependencyProperty.RegisterAttached("IsTimePopints", typeof(bool), typeof(AttachedPropertyFactory)
                                                                                                          , new FrameworkPropertyMetadata(false));

        public static void SetIsTimePopints(DependencyObject d, bool value) => d.SetValue(IsTimePopintsProperty, value);

        public static bool GetIsTimePopints(DependencyObject d) => (bool)d.GetValue(IsTimePopintsProperty);

        
        #endregion


    }
}
