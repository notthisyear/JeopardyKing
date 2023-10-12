using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyKing.GameComponents
{
    public class Player : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _name = string.Empty;
        private decimal _cash = decimal.Zero;
        #endregion

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Cash
        {
            get => _cash;
            set => SetProperty(ref _cash, value);
        }
        #endregion

        public Player(string name)
        {
            Name = name;
        }
    }
}
