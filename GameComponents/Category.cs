using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace JeopardyKing.GameComponents
{
    public class Category : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _title = string.Empty;
        private bool _allQuestionsAnswered = false;
        #endregion

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool AllQuestionsAnswered
        {
            get => _allQuestionsAnswered;
            private set => SetProperty(ref _allQuestionsAnswered, value);
        }

        public int Id { get; }

        public ObservableCollection<Question> Questions { get; }

        public const int MaxNumberOfQuestions = 5;
        #endregion

        #region Commands
        private RelayCommand? _addQuestionCommand;

        [JsonIgnore]
        public ICommand AddQuestionCommand
        {
            get
            {
                _addQuestionCommand ??= new RelayCommand(AddQuestion);
                return _addQuestionCommand;
            }
        }
        #endregion

        private readonly object _questionsLock = new();
        private int _questionIdCounter = 0;
        private readonly Board _board;
        private readonly decimal _defaultIncreasePerQuestion;

        public Category(int id, Board board, string title, decimal defaultIncreasePerQuestion)
        {
            Id = id;
            _board = board;
            Title = title;

            _defaultIncreasePerQuestion = defaultIncreasePerQuestion;
            Questions = new();
            BindingOperations.EnableCollectionSynchronization(Questions, _questionsLock);
        }

        #region Public methods
        public void CheckIfAllQuestionsAnswered()
        {
            lock (_questionsLock)
                AllQuestionsAnswered = !Questions.Where(x => !x.IsAnswered).Any();
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
            AddQuestion(QuestionType.Text, newValue, _board.Currency);
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

        public void SetParametersForAllYoutubeQuestions(bool autoplay, bool showControls)
        {
            lock (_questionsLock)
            {
                var youtubeQuestions = Questions.Where(x => x.Type == QuestionType.YoutubeVideo);
                foreach (var youtubeQuestion in youtubeQuestions)
                    youtubeQuestion.RefreshYoutubeVideoUrl(autoplay, showControls);
            }
        }
        #endregion
    }
}
