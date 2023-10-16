using System;
using System.Windows;
using JeopardyKing.InputRaw;
using JeopardyKing.WpfComponents;
using static JeopardyKing.InputRaw.Enumerations;

namespace JeopardyKing.Windows
{
    public partial class SelectButtonPopupModal : Window
    {
        #region Dependency properties
        public string LastKeyPressed
        {
            get { return (string)GetValue(LastKeyPressedProperty); }
            set { SetValue(LastKeyPressedProperty, value); }
        }
        public static readonly DependencyProperty LastKeyPressedProperty = DependencyProperty.Register(
            nameof(LastKeyPressed),
            typeof(string),
            typeof(SelectButtonPopupModal),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public string LastPressedSource
        {
            get { return (string)GetValue(LastPressedSourceProperty); }
            set { SetValue(LastPressedSourceProperty, value); }
        }
        public static readonly DependencyProperty LastPressedSourceProperty = DependencyProperty.Register(
            nameof(LastPressedSource),
            typeof(string),
            typeof(SelectButtonPopupModal),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public string PlayerName { get; }

        internal InputManager.KeyboardEvent? LastEvent { get; set; }

        private readonly Action<long, KeyboardScanCode> _windowClosedAction;

        public SelectButtonPopupModal(Window parentWindow,
                                string playerName,
                                Action<long, KeyboardScanCode> windowClosedAction)
        {
            InitializeComponent();
            Owner = parentWindow;
            PlayerName = playerName;
            _windowClosedAction = windowClosedAction;
        }

        private void CloseModalWindow(ModalWindowButton buttonClicked)
        {
            if (buttonClicked == ModalWindowButton.OK && LastEvent != default)
                _windowClosedAction.Invoke(LastEvent.SourceId, LastEvent.Key);
            Close();
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
