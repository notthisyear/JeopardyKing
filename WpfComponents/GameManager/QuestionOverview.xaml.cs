using System.Windows;
using System.Windows.Controls;
using JeopardyKing.GameComponents;

namespace JeopardyKing.WpfComponents
{
    public partial class QuestionOverview : UserControl
    {
        #region Dependency properties
        public Question SelectedQuestion
        {
            get { return (Question)GetValue(SelectedQuestionProperty); }
            set { SetValue(SelectedQuestionProperty, value); }
        }
        public static readonly DependencyProperty SelectedQuestionProperty = DependencyProperty.Register(
            nameof(SelectedQuestion),
            typeof(Question),
            typeof(QuestionOverview),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public QuestionOverview()
        {
            InitializeComponent();
        }
    }
}
