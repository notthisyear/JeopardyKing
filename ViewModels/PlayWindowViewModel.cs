using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.GameComponents;

namespace JeopardyKing.ViewModels
{
    public enum PlayWindowState
    {
        None,
        GameStarting,
        RevealCategories,
        ShowBoard,
        ShowQuestion,
        PlayerAnswering,
        AnswerIncorrect,
        AnswerCorrect,
        Done
    }

    public class PlayWindowViewModel : ObservableObject
    {
        #region Public properties

        #region Private fields
        private PlayWindowState _windowState = PlayWindowState.None;
        private Category? _categoryBeingRevealed = default;
        private bool _categoryChanging = true;
        private Board? _gameBoard = default;
        #endregion

        public PlayWindowState WindowState
        {
            get => _windowState;
            private set => SetProperty(ref _windowState, value);
        }

        public Category? CategoryBeingRevealed
        {
            get => _categoryBeingRevealed;
            private set => SetProperty(ref _categoryBeingRevealed, value);
        }

        public bool CategoryChanging
        {
            get => _categoryChanging;
            private set => SetProperty(ref _categoryChanging, value);
        }

        public Board? GameBoard
        {
            get => _gameBoard;
            private set => SetProperty(ref _gameBoard, value);
        }

        public ObservableCollection<Player> Players { get; }
        #endregion

        #region Private fields
        private readonly object _playersLock = new();
        private int _currentCategoryIdx = 0;
        #endregion

        public PlayWindowViewModel()
        {
            Players = new ObservableCollection<Player>();
            BindingOperations.EnableCollectionSynchronization(Players, _playersLock);
        }

        public void StartGame(Board board, ObservableCollection<Player> players)
        {
            if (WindowState == PlayWindowState.None)
            {
                GameBoard = board;
                lock (_playersLock)
                {
                    Players.Clear();
                    foreach (var p in players)
                        Players.Add(p);
                }

                WindowState = PlayWindowState.GameStarting;
            }
        }

        public void RevealNextCategory()
        {
            if (GameBoard == default)
                return;

            if (CategoryBeingRevealed == default && WindowState == PlayWindowState.GameStarting)
            {
                CategoryBeingRevealed = GameBoard.Categories.First();
                _currentCategoryIdx = 0;
                WindowState = PlayWindowState.RevealCategories;
                CategoryChanging = false;
            }
            else if (CategoryBeingRevealed != default && WindowState == PlayWindowState.RevealCategories)
            {
                _currentCategoryIdx++;
                CategoryChanging = true;

                Task.Run(() =>
                {
                    // Give eventual animations some time to run
                    Thread.Sleep(750);
                    if (_currentCategoryIdx < GameBoard.Categories.Count)
                    {
                        CategoryBeingRevealed = GameBoard.Categories[_currentCategoryIdx];
                    }
                    else
                    {
                        _currentCategoryIdx = 0;
                        WindowState = PlayWindowState.ShowBoard;
                    }
                    CategoryChanging = false;
                });
            }
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
