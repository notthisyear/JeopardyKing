using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private bool _inShowContent = false;
        private bool _inShowMediaContent = false;
        private bool _inPlayerAnswering = false;
        private bool _inPlayerHasAnswered = false;
        private Board? _gameBoard = default;
        private ReadOnlyObservableCollection<Player>? _players = default;
        private Question? _currentQuestion = default;
        private Player? _currentlyAnsweringPlayer = default;
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

        public bool InShowContent
        {
            get => _inShowContent;
            private set => SetProperty(ref _inShowContent, value);
        }

        public bool InShowMediaContent
        {
            get => _inShowMediaContent;
            private set => SetProperty(ref _inShowMediaContent, value);
        }

        public bool InPlayerAnswering
        {
            get => _inPlayerAnswering;
            private set => SetProperty(ref _inPlayerAnswering, value);
        }

        public bool InPlayerHasAnswered
        {
            get => _inPlayerHasAnswered;
            private set => SetProperty(ref _inPlayerHasAnswered, value);
        }

        public Board? GameBoard
        {
            get => _gameBoard;
            private set => SetProperty(ref _gameBoard, value);
        }

        public ReadOnlyObservableCollection<Player>? Players
        {
            get => _players;
            private set => SetProperty(ref _players, value);
        }

        public Question? CurrentQuestion
        {
            get => _currentQuestion;
            private set => SetProperty(ref _currentQuestion, value);
        }

        public Player? CurrentlyAnsweringPlayer
        {
            get => _currentlyAnsweringPlayer;
            private set => SetProperty(ref _currentlyAnsweringPlayer, value);
        }
        #endregion

        #region Private fields
        private int _currentCategoryIdx = 0;
        #endregion

        public void StartGame(Board board, ReadOnlyObservableCollection<Player> players)
        {
            if (WindowState == PlayWindowState.None)
            {
                GameBoard = board;
                Players = players;
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

        public void StartQuestion(Question currentQuestion)
        {
            if (WindowState == PlayWindowState.ShowBoard)
            {
                CurrentQuestion = currentQuestion;
                InShowContent = true;
                WindowState = PlayWindowState.ShowQuestion;
            }
        }

        public void PlayerHasPressed(Player player)
        {
            if (!InPlayerAnswering)
            {
                CurrentlyAnsweringPlayer = player;
                InPlayerAnswering = true;
            }
        }

        public void PlayerHasAnswered(bool isCorrect)
        {
            if (InPlayerAnswering)
            {
                InPlayerHasAnswered = true;
                WindowState = isCorrect ? PlayWindowState.AnswerCorrect : PlayWindowState.AnswerIncorrect;
                InPlayerAnswering = false;
                Task.Run(() =>
                {
                    // Give eventual animations some time to run
                    Thread.Sleep(1500);
                    InPlayerHasAnswered = false;
                    if (isCorrect)
                        ResetGameBoardAfterAnswer();
                    else
                        WindowState = PlayWindowState.ShowQuestion;
                });
            }
        }

        public void AbandonQuestion()
        {
            ResetGameBoardAfterAnswer();
        }

        public void NotifyWindowClosed()
        {

        }

        private void ResetGameBoardAfterAnswer()
        {
            InShowContent = false;
            InShowMediaContent = false;
            CurrentQuestion = default;
            CurrentlyAnsweringPlayer = default;
            WindowState = PlayWindowState.ShowBoard;
        }
    }
}
