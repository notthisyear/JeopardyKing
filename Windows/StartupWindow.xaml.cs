using System;
using System.Windows;

namespace JeopardyKing.Windows
{
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
            PlayButtonClick(this, new());
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            var playWindow = new PlayWindow { ViewModel = new() };
            var gameManagerWindow = new GameManagerWindow(playWindow) { ViewModel = new() };
            Application.Current.MainWindow = gameManagerWindow;
            gameManagerWindow.Show();
            playWindow.Show();
            Close();
        }

        private void CreateButtonClick(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateWindow { ViewModel = new() };
            Application.Current.MainWindow = createWindow;
            createWindow.Show();
            Close();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
