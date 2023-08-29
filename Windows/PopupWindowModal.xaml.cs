using System;
using System.Windows;
using System.Windows.Controls;
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
        #region Dependency properties
        public bool InputIsValid
        {
            get { return (bool)GetValue(InputIsValidProperty); }
            set { SetValue(InputIsValidProperty, value); }
        }
        public static readonly DependencyProperty InputIsValidProperty = DependencyProperty.Register(
            nameof(InputIsValid),
            typeof(bool),
            typeof(PopupWindowModal),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public string InputErrorMessage
        {
            get { return (string)GetValue(InputErrorMessageProperty); }
            set { SetValue(InputErrorMessageProperty, value); }
        }
        public static readonly DependencyProperty InputErrorMessageProperty = DependencyProperty.Register(
            nameof(InputErrorMessage),
            typeof(string),
            typeof(PopupWindowModal),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public string WindowHeadline { get; }

        public string WindowInstruction { get; }

        public bool HasInputField { get; }

        public bool ShowCancelButton { get; }

        private readonly Func<string, (bool isValid, string errorMessage)>? _inputValidator;
        private readonly Action<ModalWindowButton>? _windowClosedActionWithoutReturn;
        private readonly Action<ModalWindowButton, string>? _windowClosedActionWithReturn;

        public PopupWindowModal(Window parentWindow,
                                string windowTitle,
                                string windowHeadline,
                                Action<ModalWindowButton, string> windowClosedAction,
                                string preFilledInput = "",
                                Func<string, (bool isValid, string errorMessage)>? inputValidator = null)
        {
            InitializeComponent();
            Owner = parentWindow;
            Title = windowTitle;
            WindowHeadline = windowHeadline;
            HasInputField = true;
            ShowCancelButton = false;
            WindowInstruction = string.Empty;
            inputField.Text = preFilledInput;
            _inputValidator = inputValidator;
            _windowClosedActionWithReturn = windowClosedAction ?? throw new ArgumentNullException(nameof(windowClosedAction));

            if (_inputValidator != default)
                (InputIsValid, InputErrorMessage) = _inputValidator.Invoke(inputField.Text);

            inputField.Focus();
        }

        public PopupWindowModal(Window parentWindow,
                                string windowTitle,
                                string windowHeadline,
                                Action<ModalWindowButton> windowClosedAction,
                                string windowInstruction = "")
        {
            InitializeComponent();
            Owner = parentWindow;
            Title = windowTitle;
            WindowHeadline = windowHeadline;
            HasInputField = false;
            ShowCancelButton = true;
            _windowClosedActionWithoutReturn = windowClosedAction ?? throw new ArgumentNullException(nameof(windowClosedAction));
            WindowInstruction = windowInstruction;
            InputIsValid = true;
        }

        private void CloseModalWindow(ModalWindowButton buttonClicked)
        {
            if (_windowClosedActionWithoutReturn != default)
                _windowClosedActionWithoutReturn.Invoke(buttonClicked);
            else if (_windowClosedActionWithReturn != default)
                _windowClosedActionWithReturn.Invoke(buttonClicked, inputField.Text);
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

        private void InputFieldTextChanged(object sender, TextChangedEventArgs e)
        {
            var (isValid, errorMessage) = (true, string.Empty);
            if (sender is TextBox inputBox && _inputValidator != default)
                (isValid, errorMessage) = _inputValidator.Invoke(inputBox.Text);
            InputIsValid = isValid;
            InputErrorMessage = errorMessage;
        }
    }
}
