using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;

namespace JeopardyKing.ViewModels
{
    public partial class CategoryViewEditableViewModel : ObservableObject
    {
        #region Commands
        private RelayCommand<Category>? _deleteCategoryCommand;
        private RelayCommand<Question>? _mouseEnterQuestionCardCommand;
        private RelayCommand? _mouseLeaveQuestionCardCommand;
        private RelayCommand<Question>? _mouseClickQuestionCardCommand;

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
                                if (_modeManager.CurrentlySelectedQuestion != default &&
                                _modeManager.CurrentlySelectedQuestion.CategoryId == c.Id)
                                {
                                    _modeManager.SetSelectedQuestionEditStatus(false);
                                    _modeManager.DeselectQuestion();
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

        public ICommand MouseEnterQuestionCardCommand
        {
            get
            {
                _mouseEnterQuestionCardCommand ??= new RelayCommand<Question>(q =>
                {
                    if (_modeManager.CurrentState != CreateWindowState.NothingSelected)
                        return;

                    _modeManager.SetQuestionHighlightedStatus(true, q);
                });
                return _mouseEnterQuestionCardCommand;
            }
        }

        public ICommand MouseLeaveQuestionCardCommand
        {
            get
            {
                _mouseLeaveQuestionCardCommand ??= new RelayCommand(() =>
                {
                    if (_modeManager.CurrentState != CreateWindowState.EditingQuestion)
                        _modeManager.SetQuestionHighlightedStatus(false);
                });
                return _mouseLeaveQuestionCardCommand;
            }
        }

        public ICommand MouseClickQuestionCardCommand
        {
            get
            {
                _mouseClickQuestionCardCommand ??= new RelayCommand<Question>(q =>
                {
                    if (_modeManager.CurrentState != CreateWindowState.EditingQuestion)
                    {
                        _modeManager.SetSelectedQuestionEditStatus(true);
                    }
                    else
                    {
                        if (_modeManager.CurrentlySelectedQuestion == q)
                        {
                            _modeManager.SetSelectedQuestionEditStatus(false);
                            _modeManager.SetQuestionHighlightedStatus(true, q);
                        }
                        else
                        {
                            if (_modeManager.CurrentlySelectedQuestion != default)
                                _modeManager.SetSelectedQuestionEditStatus(false);

                            // Setting it to highlight in between ensures that the edit box moves to the correct place
                            _modeManager.SetQuestionHighlightedStatus(true, q);
                            _modeManager.SetSelectedQuestionEditStatus(true);
                        }
                    }
                });
                return _mouseClickQuestionCardCommand;
            }
        }
        #endregion

        #region Private fields        
        private readonly CreateWindowModeManager _modeManager;
        #endregion

        public CategoryViewEditableViewModel(CreateWindowModeManager modeManager)
        {
            _modeManager = modeManager;
        }
    }
}
