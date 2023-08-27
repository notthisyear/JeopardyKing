using System;
using System.Windows;
using System.Windows.Input;
using JeopardyKing.WpfComponents;

namespace JeopardyKing.Windows
{
    public enum ModalWindowButton
    {
        OK,
        Cancel
    }

    public partial class PopupWindowModal : Window
    {
        public string WindowHeadline { get; }

        public string WindowInstruction { get; }

        private readonly Action<ModalWindowButton> _windowClosedAction;
        
        public PopupWindowModal(Window parentWindow,
                                string windowHeadline,
                                Action<ModalWindowButton> windowClosedAction,
                                string windowInstruction = "")
        {
            InitializeComponent();
            Owner = parentWindow;
            WindowHeadline = windowHeadline;
            _windowClosedAction = windowClosedAction ?? throw new ArgumentNullException(nameof(windowClosedAction));
            WindowInstruction = windowInstruction;
        }

        private void CloseModalWindow(ModalWindowButton buttonClicked)
        {
            _windowClosedAction?.Invoke(buttonClicked);
            Close();
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                if (e.Key == Key.Escape)
                    CloseModalWindow(ModalWindowButton.Cancel);
                if (e.Key == Key.Enter)
                    CloseModalWindow(ModalWindowButton.OK);
            }
        }

        private void TitleBarButtonPressed(object sender, RoutedEventArgs e)
        {
            if (e is TitleBarButtonClickedEventArgs eventArgs && eventArgs.ButtonClicked == TitleBarButton.Close)
                CloseModalWindow(ModalWindowButton.Cancel);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
            => CloseModalWindow(ModalWindowButton.Cancel);
        
        private void OkButtonClick(object sender, RoutedEventArgs e)
            => CloseModalWindow(ModalWindowButton.OK);

    }
}
