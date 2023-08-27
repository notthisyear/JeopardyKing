using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.Common;

namespace JeopardyKing.GameComponents
{
    public enum QuestionType
    {
        Text,
        Image,
        Audio,
        Video
    }

    public class Question : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private bool _isAnswered = false;
        private bool _isBeingEdited = false;
        private string _categoryName = string.Empty;
        private decimal _value;
        private QuestionType _type;
        private CurrencyType _currency;
        private bool _isBonus;
        private string _content = string.Empty;
        private string _multimediaContentLink = string.Empty;
        #endregion

        public int Id { get; }

        public int CategoryId { get; }

        public bool IsAnswered
        {
            get => _isAnswered;
            private set => SetProperty(ref _isAnswered, value);
        }

        public bool IsBeingEdited
        {
            get => _isBeingEdited;
            set => SetProperty(ref _isBeingEdited, value);
        }

        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public QuestionType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public CurrencyType Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }

        public bool IsBonus
        {
            get => _isBonus;
            set => SetProperty(ref _isBonus, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string MultimediaContentLink
        {
            get => _multimediaContentLink;
            set => SetProperty(ref _multimediaContentLink, value);
        }
        #endregion

        public Question(int id, int categoryId, string categoryName, QuestionType type, decimal value, CurrencyType currency)
        {
            Id = id;
            CategoryId = categoryId;
            CategoryName = categoryName;
            Type = type;
            Value = value;
            Currency = currency;
            IsBonus = false;
            Content = string.Empty;
        }
    }
}
