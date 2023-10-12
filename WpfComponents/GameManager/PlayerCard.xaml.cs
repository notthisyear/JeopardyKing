using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JeopardyKing.GameComponents;
using JeopardyKing.ViewModels;

namespace JeopardyKing.WpfComponents
{
    public partial class PlayerCard : UserControl
    {
        #region Dependency properties
        public Player CurrentPlayer
        {
            get => (Player)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register(
            nameof(CurrentPlayer),
            typeof(Player),
            typeof(PlayerCard),
            new FrameworkPropertyMetadata(default));

        public GameManagerViewModel ViewModel
        {
            get => (GameManagerViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(GameManagerViewModel),
            typeof(PlayerCard),
            new FrameworkPropertyMetadata(default));

        public bool NameIsBeingEdited
        {
            get => (bool)GetValue(NameIsBeingEditedProperty);
            set => SetValue(NameIsBeingEditedProperty, value);
        }
        public static readonly DependencyProperty NameIsBeingEditedProperty = DependencyProperty.Register(
            nameof(NameIsBeingEdited),
            typeof(bool),
            typeof(PlayerCard),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public PlayerCard()
        {
            InitializeComponent();
        }

        private void EditPlayerButtonClicked(object sender, RoutedEventArgs e)
        {
            NameIsBeingEdited = true;
            editNameBox.Focus();
            var length = CurrentPlayer.Name.Length;
            editNameBox.CaretIndex = length;
            editNameBox.LostFocus += CloseEditBox;
        }

        private void KeyPressedEditBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                CloseEditBox(sender, e);
                e.Handled = true;
            }
        }

        private void CloseEditBox(object sender, RoutedEventArgs e)
        {
            editNameBox.LostFocus -= CloseEditBox;
            NameIsBeingEdited = false;
        }
    }
}
