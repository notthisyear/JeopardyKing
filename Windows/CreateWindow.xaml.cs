using System.Windows;
using System.Windows.Input;
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
            if (e is not TitleBarButtonClickedEventArgs eventArgs)
                return;

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

        private void MenuItemButtonPressed(object sender, RoutedEventArgs e)
        {
            if (e is not MenuItemButtonClickedEventArgs eventArgs)
                return;
            ViewModel.MenuItemPressed(eventArgs.ItemClicked);
        }

        private void CreateWindowKeyDown(object sender, KeyEventArgs e)
        {
            // NOTE: If escape is pressed while the mouse is over the currently edited
            //       question, the question box will disappear while the question
            //       remains highlighted. This can perhaps be considered a bug, but
            //       it's so minor that we'll leave it for now.
            if (e.Key == Key.Escape && ViewModel.ModeManager.CurrentState == CreateWindowState.EditingQuestion)
            {
                ViewModel.ModeManager.SetSelectedQuestionEditStatus(false);
                ViewModel.ModeManager.SetQuestionHighlightedStatus(false);
            }
        }
    }
}
