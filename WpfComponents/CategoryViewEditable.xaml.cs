using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;

namespace JeopardyKing.WpfComponents
{
    public partial class CategoryViewEditable : UserControl
    {
        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category),
            typeof(Category),
            typeof(CategoryViewEditable),
            new FrameworkPropertyMetadata(null));

        public CreateWindowModeManager ModeManager
        {
            get => (CreateWindowModeManager)GetValue(ModeManagerProperty);
            set => SetValue(ModeManagerProperty, value);
        }
        public static readonly DependencyProperty ModeManagerProperty = DependencyProperty.Register(
            nameof(ModeManager),
            typeof(CreateWindowModeManager),
            typeof(CategoryViewEditable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool TitleIsBeingEdited
        {
            get => (bool)GetValue(TitleIsBeingEditedProperty);
            set => SetValue(TitleIsBeingEditedProperty, value);
        }
        public static readonly DependencyProperty TitleIsBeingEditedProperty = DependencyProperty.Register(
            nameof(TitleIsBeingEdited),
            typeof(bool),
            typeof(CategoryViewEditable),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public CategoryViewEditable()
        {
            InitializeComponent();
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            Category.AddQuestion();
        }

        private void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            PopupWindowModal confirmationDialog = new(Application.Current.MainWindow, "Are you sure?", "Delete category", x =>
            {
                if (x == ModalWindowButton.OK)
                {
                    if (ModeManager.CurrentlySelectedQuestion != default &&
                        ModeManager.CurrentlySelectedQuestion.CategoryId == Category.Id)
                    {
                        ModeManager.SetSelectedQuestionEditStatus(false);
                        ModeManager.DeselectQuestion();
                    }
                    Category.DeleteCategory();
                }
            },
            $"You are about to delete category '{Category.Title}'. This will delete all questions in the category.\n\nAre you sure?");
            _ = confirmationDialog.ShowDialog();
        }

        private void EditNameButtonClicked(object sender, RoutedEventArgs e)
        {
            TitleIsBeingEdited = true;
            editNameBox.Focus();
            editNameBox.CaretIndex = Category.Title.Length;
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
            TitleIsBeingEdited = false;
            Category.UpdateQuestionCategory();
        }

        private void MouseEnterQuestionCard(object sender, MouseEventArgs e)
        {
            if (sender is not Border b || b.DataContext is not Question q)
                return;

            if (ModeManager.CurrentState != CreateWindowState.NothingSelected)
                return;

            ModeManager.SetQuestionHighlightedStatus(true, q);
        }

        private void MouseLeaveQuestionCard(object sender, MouseEventArgs e)
        {
            if (ModeManager.CurrentState != CreateWindowState.EditingQuestion)
                ModeManager.SetQuestionHighlightedStatus(false);
        }

        private void MouseClickQuestionCard(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border b || b.DataContext is not Question q)
                return;

            if (ModeManager.CurrentState != CreateWindowState.EditingQuestion)
            {
                ModeManager.SetSelectedQuestionEditStatus(true);
            }
            else
            {
                if (ModeManager.CurrentlySelectedQuestion == q)
                {
                    ModeManager.SetSelectedQuestionEditStatus(false);
                    ModeManager.SetQuestionHighlightedStatus(true, q);
                }
                else
                {
                    if (ModeManager.CurrentlySelectedQuestion != default)
                        ModeManager.SetSelectedQuestionEditStatus(false);

                    // Setting it to highlight in between ensures that the edit box moves to the correct place
                    ModeManager.SetQuestionHighlightedStatus(true, q);
                    ModeManager.SetSelectedQuestionEditStatus(true);
                }
            }
        }
    }
}
