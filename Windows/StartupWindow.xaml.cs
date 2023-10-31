using System;
using System.IO;
using System.Windows;
using JeopardyKing.ViewModels;

namespace JeopardyKing.Windows
{
    public partial class StartupWindow : Window
    {
        private const string ResourceFolderName = "Resources";
        private const string IntroSoundFileName = "intro.wav";
        private const string PressedSoundFileName = "pressed.wav";
        private const string CorrectAnswerSoundFileName = "correct.wav";
        private const string IncorrectAnswerOrAbandonSoundFileName = "incorrect_or_abandon.wav";

        public StartupWindow()
        {
            InitializeComponent();
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            var introSoundPath = Path.Combine(AppContext.BaseDirectory, ResourceFolderName, IntroSoundFileName);
            var pressedSoundPath = Path.Combine(AppContext.BaseDirectory, ResourceFolderName, PressedSoundFileName);
            var correctSoundPath = Path.Combine(AppContext.BaseDirectory, ResourceFolderName, CorrectAnswerSoundFileName);
            var incorrectOrAbandonSoundPath = Path.Combine(AppContext.BaseDirectory, ResourceFolderName, IncorrectAnswerOrAbandonSoundFileName);
            var playWindowViewModel = new PlayWindowViewModel(introSoundPath, pressedSoundPath, correctSoundPath, incorrectOrAbandonSoundPath);
            var playWindow = new PlayWindow { ViewModel = playWindowViewModel };
            var gameManagerWindow = new GameManagerWindow(playWindow) { ViewModel = new(playWindowViewModel) };

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
