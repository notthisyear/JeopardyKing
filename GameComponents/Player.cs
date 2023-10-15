using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyKing.GameComponents
{
    public class Player : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _name = string.Empty;
        private decimal _cash = decimal.Zero;
        private bool _isPressingKey = false;
        #endregion

        public int Id { get; }
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

        public bool IsPressingKey
        {
            get => _isPressingKey;
            set => SetProperty(ref _isPressingKey, value);
        }
        #endregion

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
