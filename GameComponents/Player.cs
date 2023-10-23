using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyKing.GameComponents
{
    public class Player : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _name = string.Empty;
        private decimal _cash = decimal.Zero;
        private decimal _currentBet = decimal.Zero;
        private bool _isPressingKey = false;
        private bool _isBetting = false;
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

        public decimal CurrentBet
        {
            get => _currentBet;
            set => SetProperty(ref _currentBet, decimal.Round(value));
        }

        public bool IsPressingKey
        {
            get => _isPressingKey;
            set => SetProperty(ref _isPressingKey, value);
        }

        public bool IsBetting
        {
            get => _isBetting;
            set => SetProperty(ref _isBetting, value);
        }
        #endregion

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }


        public void AddCashForQuestion(Question q)
        {
            decimal valueToAdd;
            if (q.IsBonus)
                valueToAdd = 2 * q.Value;
            else if (q.IsGamble)
                valueToAdd = CurrentBet;
            else
                valueToAdd = q.Value;

            Cash += valueToAdd;
            CurrentBet = 0;
        }

        public void SubtractCashForQuestion(Question q)
        {
            decimal valueToSubtract;
            if (q.IsBonus)
                valueToSubtract = q.Value;
            else if (q.IsGamble)
                valueToSubtract = CurrentBet;
            else
                valueToSubtract = q.Value;

            Cash -= valueToSubtract;
            CurrentBet = 0;
        }
    }
}
