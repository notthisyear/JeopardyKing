using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyKing.GameComponents
{
    public class Board : ObservableObject
    {
        #region Backing fields
        private string _gameName = string.Empty;
        #endregion

        public string GameName
        {
            get => _gameName;
            set => SetProperty(ref _gameName, value);
        }

        public ObservableCollection<Category> Categories { get; }

        public const int MaxNumberOfCategories = 6;

        private const CurrencyType DefaultCurrencyType = CurrencyType.SwedishKrona;
        private const decimal DefaultStartingValue = 200;
        private const decimal DefaultIncreasePerQuestion = 200;
        private const int DefaultNumberOfCategories = 6;
        private const int DefaultNumberOfQuestionPerCategory = 5;

        private int _categoryIdCounter = 0;
        private readonly object _categoriesLock = new();

        public Board(bool createDefault = false)
        {
            Categories = new();
            BindingOperations.EnableCollectionSynchronization(Categories, _categoriesLock);

            if (createDefault)
            {
                for (var i = 0; i < DefaultNumberOfCategories; i++)
                {
                    var category = CreateNewCategory();
                    for (var j = 0; j < DefaultNumberOfQuestionPerCategory; j++)
                        category.AddQuestion(QuestionType.Text, DefaultStartingValue + (j * DefaultIncreasePerQuestion), DefaultCurrencyType);
                    AddCategory(category);
                }
            }
        }

        public void AddNewCategory()
            => AddCategory(CreateNewCategory());

        public void AddCategory(Category category)
        {
            lock (_categoriesLock)
            {
                if (Categories.Count < MaxNumberOfCategories)
                    Categories.Add(category);
            }
        }

        public void DeleteCategory(int categoryId)
        {
            lock (_categoriesLock)
            {
                var c = Categories.FirstOrDefault(x => x.Id == categoryId);
                if (c != default)
                    Categories.Remove(c);
            }
        }

        public void DeleteQuestion(int categoryId, int id)
        {
            lock (_categoriesLock)
            {
                var c = Categories.FirstOrDefault(x => x.Id == categoryId);
                if (c != default)
                    c.DeleteQuestion(id);
            }
        }

        private Category CreateNewCategory(string name = "")
        {
            Category? c = default;
            lock (_categoriesLock)
            {
                if (string.IsNullOrEmpty(name))
                    name = $"Category {Categories.Count + 1}";
                c = new(_categoryIdCounter++, this, name, DefaultIncreasePerQuestion, DefaultCurrencyType);
            }
            return c!;
        }
    }
}
