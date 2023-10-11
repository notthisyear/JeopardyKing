using System.Collections;
using System.Windows;
using System.Windows.Controls;
using JeopardyKing.ViewModels;

namespace JeopardyKing.WpfComponents
{
    public partial class DetailedQuestionVisualizer : UserControl
    {
        public IEnumerable Questions
        {
            get { return (IEnumerable)GetValue(QuestionsProperty); }
            set { SetValue(QuestionsProperty, value); }
        }

        public static readonly DependencyProperty QuestionsProperty = DependencyProperty.Register(
            nameof(Questions),
            typeof(IEnumerable),
            typeof(DetailedQuestionVisualizer),
            new FrameworkPropertyMetadata(null));

        public CategoryViewViewModel ViewModel
        {
            get => (CategoryViewViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CategoryViewViewModel),
            typeof(DetailedQuestionVisualizer),
            new FrameworkPropertyMetadata(null));

        public double ValueFontSize
        {
            get => (double)GetValue(ValueFontSizeProperty);
            set => SetValue(ValueFontSizeProperty, value);
        }
        public static readonly DependencyProperty ValueFontSizeProperty = DependencyProperty.Register(
            nameof(ValueFontSize),
            typeof(double),
            typeof(DetailedQuestionVisualizer),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public DetailedQuestionVisualizer()
        {
            InitializeComponent();
        }
    }
}
