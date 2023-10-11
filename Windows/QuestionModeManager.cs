using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.GameComponents;

namespace JeopardyKing.Windows
{
    public enum QuestionVisualState
    {
        NothingSelected,
        QuestionHighlighted,
        QuestionSelected
    }

    public class QuestionModeManager : ObservableObject
    {
        private QuestionVisualState _currentState = QuestionVisualState.NothingSelected;
        private Question? _currentlySelectedQuestion = default;

        public QuestionVisualState CurrentState
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
            CurrentState = isBeingHighlighted ? QuestionVisualState.QuestionHighlighted : QuestionVisualState.NothingSelected;
            CurrentlySelectedQuestion = q;
        }

        public void SetQuestionSelectedStatus(bool isSelected)
        {
            if (CurrentlySelectedQuestion != default)
                CurrentlySelectedQuestion.IsSelected = isSelected;

            if (isSelected)
                CurrentState = QuestionVisualState.QuestionSelected;
        }

        public void DeselectQuestion()
        {
            CurrentlySelectedQuestion = null;
            CurrentState = QuestionVisualState.NothingSelected;
        }
    }
}
