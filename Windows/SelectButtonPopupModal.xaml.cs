using System;
using System.Windows;
using JeopardyKing.Communication;
using JeopardyKing.WpfComponents;
using WindowsNativeRawInputWrapper.Types;

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

        public bool LastKeyAvailable
        {
            get { return (bool)GetValue(LastKeyAvailableProperty); }
            set { SetValue(LastKeyAvailableProperty, value); }
        }
        public static readonly DependencyProperty LastKeyAvailableProperty = DependencyProperty.Register(
            nameof(LastKeyAvailable),
            typeof(bool),
            typeof(SelectButtonPopupModal),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public string PlayerName { get; }

        internal InputManager.KeyboardEvent? LastEvent { get; set; }

        private readonly Action<bool, long, RawKeyboardInput.KeyboardScanCode> _okButtonClickAction;

        public SelectButtonPopupModal(Window parentWindow, string playerName, Action<bool, long, RawKeyboardInput.KeyboardScanCode> okButtonClickAction)
        {
            InitializeComponent();
            Owner = parentWindow;
            PlayerName = playerName;
            _okButtonClickAction = okButtonClickAction;
        }

        private void CloseModalWindow(ModalWindowButton buttonClicked)
        {
            if (buttonClicked == ModalWindowButton.OK)
                _okButtonClickAction.Invoke(LastEvent != default, LastEvent?.SourceId ?? long.MinValue, LastEvent?.Key ?? RawKeyboardInput.KeyboardScanCode.UnknownScanCode);
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
