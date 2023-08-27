using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyKing.ViewModels
{
    public class PlayWindowViewModel : ObservableObject
    {
        #region Public properties
        public string ProgramDescription { get; init; } = "Play";
        #endregion

        public PlayWindowViewModel()
        {
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
