using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JeopardyKing.GameComponents;
using JeopardyKing.ViewModels;

namespace JeopardyKing.WpfComponents
{
    public partial class CategoryViewEditable : UserControl
    {
        #region Dependency properties
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

        public CategoryViewEditableViewModel ViewModel
        {
            get => (CategoryViewEditableViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CategoryViewEditableViewModel),
            typeof(CategoryViewEditable),
            new FrameworkPropertyMetadata(default));

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
        #endregion
        public CategoryViewEditable()
        {
            InitializeComponent();
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
    }
}
