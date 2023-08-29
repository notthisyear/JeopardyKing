using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.Common;

namespace JeopardyKing.GameComponents
{
    public class Category : ObservableObject
    {
        #region Public properties

        #region Backing fields
        public string _title = string.Empty;
        #endregion

        public ObservableCollection<Question> Questions { get; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public int Id { get; }
        public const int MaxNumberOfQuestions = 5;
        #endregion

        #region Commands
        #endregion

        private readonly object _questionsLock = new();
        private int _questionIdCounter = 0;
        private readonly Board _board;
        private readonly decimal _defaultIncreasePerQuestion;
        private readonly CurrencyType _defaultCurrencyType;

        public Category(int id, Board board, string title, decimal defaultIncreasePerQuestion, CurrencyType defaultCurrencyType)
        {
            Id = id;
            _board = board;
            Title = title;

            _defaultIncreasePerQuestion = defaultIncreasePerQuestion;
            _defaultCurrencyType = defaultCurrencyType;
            Questions = new();
            BindingOperations.EnableCollectionSynchronization(Questions, _questionsLock);
        }

        public void DeleteCategory()
        {
            lock (_questionsLock)
                Questions.Clear();
            _board.DeleteCategory(Id);
        }

        public void AddQuestion()
        {
            decimal newValue;
            lock (_questionsLock)
                newValue = Questions.Any() ? Questions.Last().Value + _defaultIncreasePerQuestion : _defaultIncreasePerQuestion;
            AddQuestion(QuestionType.Text, newValue, _defaultCurrencyType);
        }

        public void AddQuestion(QuestionType type, decimal value, CurrencyType currencyType)
        {
            lock (_questionsLock)
            {
                if (Questions.Count < MaxNumberOfQuestions)
                    Questions.Add(new(_questionIdCounter++, Id, Title, type, value, currencyType));
            }
        }

        public void DeleteQuestion(int id)
        {
            lock (_questionsLock)
            {
                var q = Questions.FirstOrDefault(x => x.Id == id);
                if (q != default)
                    Questions.Remove(q);
            }
        }

        public void UpdateQuestionCategory()
        {
            lock (_questionsLock)
            {
                foreach (var q in Questions)
                    q.CategoryName = Title;
            }
        }
    }
}
