using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.GameComponents;

namespace JeopardyKing.Windows
{
    public enum CreateWindowState
    {
        NothingSelected,
        QuestionHighlighted,
        EditingQuestion
    }

    public class CreateWindowModeManager : ObservableObject
    {
        private CreateWindowState _currentState = CreateWindowState.NothingSelected;
        private Question? _currentlySelectedQuestion = default;

        public CreateWindowState CurrentState
        {
            get => _currentState;
            private set => SetProperty(ref _currentState, value);
        }

        public Question? CurrentlySelectedQuestion
        {
            get => _currentlySelectedQuestion;
            set => SetProperty(ref _currentlySelectedQuestion, value);
        }

        public void SetQuestionHighlightedStatus(bool isBeingHighlighted, Question? q = default)
        {
            CurrentState = isBeingHighlighted ? CreateWindowState.QuestionHighlighted : CreateWindowState.NothingSelected;
            CurrentlySelectedQuestion = q;
        }

        public void SetSelectedQuestionEditStatus(bool isBeingEdited)
        {
            if (CurrentlySelectedQuestion != default)
                CurrentlySelectedQuestion.IsBeingEdited = isBeingEdited;

            if (isBeingEdited)
                CurrentState = CreateWindowState.EditingQuestion;
        }

        public void DeselectQuestion()
        {
            CurrentlySelectedQuestion = null;
            CurrentState = CreateWindowState.NothingSelected;
        }
    }
}
