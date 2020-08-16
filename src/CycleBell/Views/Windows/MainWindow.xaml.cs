using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using CycleBell.ViewModels.TimePointViewModels;

namespace CycleBell.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ActiveTimePointProperty =
            DependencyProperty.RegisterAttached(
                "ActiveTimePoint",
                typeof( TimePointViewModelBase ),
                typeof( MainWindow ),
                new FrameworkPropertyMetadata( 
                    null, 
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    new PropertyChangedCallback( OnActiveTimePointChanged ))
                );

        private static void OnActiveTimePointChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( d is Visual child ) 
            {
                DependencyObject parent = VisualTreeHelper.GetParent( child );
                MainWindow mv = parent as MainWindow;

                while ( parent != null && mv == null ) {
                    parent = VisualTreeHelper.GetParent( parent );
                    mv = parent as MainWindow;
                }

                if ( mv != null ) 
                {
                    TaskbarItemInfo tb = mv.m_TaskbarItemInfo;
                    tb.ProgressState = TaskbarItemProgressState.Error;
                    tb.ProgressValue = 1.0;
                }
            }
            
        }

        public static void SetActiveTimePoint( DependencyObject d, TimePointViewModelBase value )
        {
            d.SetValue( ActiveTimePointProperty, value );
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
