using System.Windows;
using System.Windows.Input;
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

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.A)
                ViewModel.ToggleAnswerAllowedCommand.Execute(default);
        }

        private void EditBetAmountButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SettingBetAmountForPlayer = true;
        }

        private void BetAmountBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                ViewModel.SettingBetAmountForPlayer = false;
                e.Handled = true;
            }
        }
    }
}
