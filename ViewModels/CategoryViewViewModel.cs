using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;

namespace JeopardyKing.ViewModels
{
    public class CategoryViewViewModel
    {
        #region Commands
        private RelayCommand<Question>? _mouseEnterQuestionCardCommand;
        private RelayCommand? _mouseLeaveQuestionCardCommand;
        private RelayCommand<Question>? _mouseClickQuestionCardCommand;

        public ICommand MouseEnterQuestionCardCommand
        {
            get
            {
                _mouseEnterQuestionCardCommand ??= new RelayCommand<Question>(q =>
                {
                    if (ModeManager.CurrentState != QuestionVisualState.NothingSelected)
                        return;

                    ModeManager.SetQuestionHighlightedStatus(true, q);
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
                    if (ModeManager.CurrentState != QuestionVisualState.QuestionSelected)
                        ModeManager.SetQuestionHighlightedStatus(false);
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
                    if (ModeManager.CurrentState != QuestionVisualState.QuestionSelected)
                    {
                        ModeManager.SetQuestionSelectedStatus(true);
                    }
                    else
                    {
                        if (ModeManager.CurrentlySelectedQuestion == q)
                        {
                            ModeManager.SetQuestionSelectedStatus(false);
                            ModeManager.SetQuestionHighlightedStatus(true, q);
                        }
                        else
                        {
                            if (ModeManager.CurrentlySelectedQuestion != default)
                                ModeManager.SetQuestionSelectedStatus(false);

                            // Setting the status to highlight in between ensures that eventual transistions run
                            ModeManager.SetQuestionHighlightedStatus(true, q);
                            ModeManager.SetQuestionSelectedStatus(true);
                        }
                    }
                });
                return _mouseClickQuestionCardCommand;
            }
        }
        #endregion

        #region Protected fields        
        protected QuestionModeManager ModeManager { get; }
        #endregion

        public CategoryViewViewModel(QuestionModeManager modeManager)
        {
            ModeManager = modeManager;
        }
    }
}
