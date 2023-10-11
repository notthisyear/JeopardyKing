namespace JeopardyKing.ViewModels
{
    public class GameManagerViewModel
    {
        #region Public properties
        public string ProgramDescription { get; init; } = "Manage";
        #endregion

        public GameManagerViewModel()
        {
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
