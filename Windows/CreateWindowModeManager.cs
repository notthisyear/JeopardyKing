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
            set => SetProperty(ref _currentState, value);
        }

        public Question? CurrentlySelectedQuestion
        {
            get => _currentlySelectedQuestion;
            set => SetProperty(ref _currentlySelectedQuestion, value);
        }

    }
}
