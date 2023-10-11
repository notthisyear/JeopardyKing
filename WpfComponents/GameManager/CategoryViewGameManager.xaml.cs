using System.Windows;
using System.Windows.Controls;
using JeopardyKing.GameComponents;
using JeopardyKing.ViewModels;

namespace JeopardyKing.WpfComponents
{
    public partial class CategoryViewGameManager : UserControl
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
            typeof(CategoryViewGameManager),
            new FrameworkPropertyMetadata(null));

        public CategoryViewViewModel ViewModel
        {
            get => (CategoryViewViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CategoryViewViewModel),
            typeof(CategoryViewGameManager),
            new FrameworkPropertyMetadata(null));
        #endregion

        public CategoryViewGameManager()
        {
            InitializeComponent();
        }
    }
}
