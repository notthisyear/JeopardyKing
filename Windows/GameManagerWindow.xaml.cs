using System;
using System.Windows;
using System.Windows.Interop;
using JeopardyKing.ViewModels;
using JeopardyKing.WpfComponents;

namespace JeopardyKing.Windows
{
    public partial class GameManagerWindow : Window
    {
        public GameManagerViewModel ViewModel
        {
            get => (GameManagerViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(GameManagerViewModel),
            typeof(GameManagerWindow),
            new FrameworkPropertyMetadata(null));

        private readonly Window _playWindow;

        public GameManagerWindow(Window playWindow)
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            _playWindow = playWindow;

            Loaded += WindowLoaded;
        }

        private void WindowLoaded(object? sender, EventArgs e)
        {
            Loaded -= WindowLoaded;
            var interopHelper = new WindowInteropHelper(this);
            ViewModel.SetApplicationWindowHandle(interopHelper.Handle);
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
                        _playWindow.Close();
                        Close();
                        break;
                };
            }
        }
    }
}
