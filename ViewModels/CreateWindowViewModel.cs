using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;

namespace JeopardyKing.ViewModels
{
    public class CreateWindowViewModel : ObservableObject
    {
        #region Public properties
        public Board GameBoard { get; }

        public CreateWindowModeManager ModeManager { get; }

        public EditQuestionBoxViewModel EditQuestionViewModel{ get; }

        public string ProgramDescription => "Jeopardy game creator";
        #endregion

        public CreateWindowViewModel()
        {
            ModeManager = new();
            GameBoard = new(createDefault: true);
            EditQuestionViewModel = new(this);
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
