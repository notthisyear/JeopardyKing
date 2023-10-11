using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;

namespace JeopardyKing.ViewModels
{
    public class CategoryViewEditableViewModel : CategoryViewViewModel
    {
        #region Commands
        private RelayCommand<Category>? _deleteCategoryCommand;

        public ICommand DeleteCategoryCommand
        {
            get
            {
                _deleteCategoryCommand ??= new RelayCommand<Category>(c =>
                {
                    PopupWindowModal confirmationDialog = new(
                        Application.Current.MainWindow,
                        "Are you sure?",
                        "Delete category",
                        x =>
                        {
                            if (x == ModalWindowButton.OK && c != default)
                            {
                                if (ModeManager.CurrentlySelectedQuestion != default &&
                                ModeManager.CurrentlySelectedQuestion.CategoryId == c.Id)
                                {
                                    ModeManager.SetQuestionSelectedStatus(false);
                                    ModeManager.DeselectQuestion();
                                }
                                c?.DeleteCategory();
                            }
                        },
                    $"You are about to delete category '{c?.Title}'. This will delete all questions in the category.\n\nAre you sure?");
                    _ = confirmationDialog.ShowDialog();
                });
                return _deleteCategoryCommand;
            }
        }
        #endregion

        public CategoryViewEditableViewModel(QuestionModeManager modeManager) : base(modeManager)
        { }
    }
}
