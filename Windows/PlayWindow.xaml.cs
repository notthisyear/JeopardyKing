using System.Windows;
using JeopardyKing.ViewModels;
using JeopardyKing.WpfComponents;

namespace JeopardyKing.Windows
{
    public partial class PlayWindow : Window
    {
        public PlayWindowViewModel ViewModel
        {
            get => (PlayWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(PlayWindowViewModel),
            typeof(PlayWindow),
            new FrameworkPropertyMetadata(null));


        public PlayWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            var verticalMargin = MaxHeight - Height;
            Top = verticalMargin / 4;
        }

        private void TitleBarButtonPressed(object sender, RoutedEventArgs e)
        {
            if (e is TitleBarButtonClickedEventArgs eventArgs)
            {
                switch (eventArgs.ButtonClicked)
                {
                    case TitleBarButton.Minimize:
                        WindowState = WindowState.Minimized;
                        break;
                    case TitleBarButton.Maximize:
                        WindowState = WindowState.Maximized;
                        break;
                    case TitleBarButton.Restore:
                        WindowState = WindowState.Normal;
                        break;
                    case TitleBarButton.Close:
                        ViewModel.NotifyWindowClosed();
                        Close();
                        break;
                };
            }
        }
    }
}
