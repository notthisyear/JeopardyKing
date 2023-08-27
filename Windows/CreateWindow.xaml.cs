using System.Windows;
using JeopardyKing.ViewModels;
using JeopardyKing.WpfComponents;

namespace JeopardyKing.Windows
{
    public partial class CreateWindow : Window
    {
        public CreateWindowViewModel ViewModel
        {
            get => (CreateWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CreateWindowViewModel),
            typeof(CreateWindow),
            new FrameworkPropertyMetadata(null));

        public CreateWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            
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

        private void QuestionDeletionRequest(object sender, RoutedEventArgs e)
        {
            var categoryId = ViewModel.ModeManager.CurrentlySelectedQuestion?.CategoryId;
            var questionId = ViewModel.ModeManager.CurrentlySelectedQuestion?.Id;
            if (categoryId != null && questionId != null)
                ViewModel.GameBoard.DeleteQuestion((int)categoryId, (int)questionId);
        }
    }
}
