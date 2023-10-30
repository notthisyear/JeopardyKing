﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        public enum MediaPlaybackStatus
        {
            Stopped,
            Paused,
            Playing
        }

        public enum QuestionProgressType
        {
            MediaOrContent,
            GambleBettingPhase,
            GambleCheckAnswerPhase
        }

        #region Public properties

        #region Private fields
        private PlayWindowState _windowState = PlayWindowState.None;
        private Category? _categoryBeingRevealed = default;
        private Player? _playerCurrentlyInGambleFocus = default;
        private bool _categoryChanging = true;
        private bool _inShowPreOrPostQuestionContent = false;
        private bool _inShowContent = false;
        private bool _inShowMediaContent = false;
        private bool _inMediaContentPlaying = false;
        private int _currentPlayingMediaPositionSeconds = 0;
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

        public Player? PlayerCurrentlyInGambleFocus
        {
            get => _playerCurrentlyInGambleFocus;
            private set => SetProperty(ref _playerCurrentlyInGambleFocus, value);
        }

        public bool CategoryChanging
        {
            get => _categoryChanging;
            private set => SetProperty(ref _categoryChanging, value);
        }

        public bool InShowPreOrPostQuestionContent
        {
            get => _inShowPreOrPostQuestionContent;
            private set => SetProperty(ref _inShowPreOrPostQuestionContent, value);
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

        public bool InMediaContentPlaying
        {
            get => _inMediaContentPlaying;
            private set => SetProperty(ref _inMediaContentPlaying, value);
        }

        public int CurrentPlayingMediaPositionSeconds
        {
            get => _currentPlayingMediaPositionSeconds;
            set => SetProperty(ref _currentPlayingMediaPositionSeconds, value);
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
        private int _currentPlayerIdx = 0;
        private MediaPlaybackStatus _mediaPlaybackStatus = MediaPlaybackStatus.Stopped;
        #endregion

        #region Public methods
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

                Task.Run(async () =>
                {
                    // Give eventual animations some time to run
                    await Task.Delay(750);
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
                if (currentQuestion.Type == QuestionType.YoutubeVideo)
                    currentQuestion.MultimediaContentLink = string.Empty;

                CurrentQuestion = currentQuestion;
                InShowPreOrPostQuestionContent = CurrentQuestion.IsBonus || CurrentQuestion.IsGamble;
                WindowState = PlayWindowState.ShowQuestion;

                if (InShowPreOrPostQuestionContent && CurrentQuestion.IsBonus)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2500);
                        SetStateToShowQuestion(currentQuestion);
                    });
                }
                else if (InShowPreOrPostQuestionContent && CurrentQuestion.IsGamble)
                {
                    _currentPlayerIdx = 0;
                    PlayerCurrentlyInGambleFocus = default;
                    Task.Run(async () =>
                    {
                        await Task.Delay(2500);
                        IncrementPlayerCurrentlyInFocus(true);
                    });
                }
                else if (!InShowPreOrPostQuestionContent)
                {
                    SetStateToShowQuestion(currentQuestion);
                }
            }
        }

        public void ProgressQuestion(Question currentQuestion, QuestionProgressType progressType)
        {
            // This method is when either
            //  - progressing a media question 
            //  - progressing the betting phase of a gamble question
            //  - progressing the checking answer phase a gamble question

            if (WindowState != PlayWindowState.ShowQuestion)
                return;

            switch (progressType)
            {
                case QuestionProgressType.MediaOrContent:
                    if (InShowContent && currentQuestion.HasMediaLink && currentQuestion.MediaQuestionFlow == MediaQuestionFlow.TextThenMedia)
                    {
                        InShowMediaContent = true;
                        InMediaContentPlaying = true;
                        if (currentQuestion.Type == QuestionType.YoutubeVideo)
                            currentQuestion.RefreshYoutubeVideoUrl(true, false);
                    }
                    else if (InShowMediaContent && currentQuestion.HasMediaLink && currentQuestion.MediaQuestionFlow == MediaQuestionFlow.MediaThenText)
                    {
                        InShowContent = true;
                    }
                    return;

                case QuestionProgressType.GambleCheckAnswerPhase:
                    InPlayerAnswering = true;
                    InShowContent = false;
                    InShowMediaContent = false;
                    InMediaContentPlaying = false;
                    InShowPreOrPostQuestionContent = true;

                    if (CurrentQuestion != default && CurrentQuestion.Type == QuestionType.YoutubeVideo)
                        CurrentQuestion.RefreshYoutubeVideoUrl(false, false);

                    IncrementPlayerCurrentlyInFocus(false);
                    break;

                case QuestionProgressType.GambleBettingPhase:
                    InShowPreOrPostQuestionContent = true;
                    IncrementPlayerCurrentlyInFocus(true);
                    break;
            }
        }

        public void PlayerHasPressed(Player player)
        {
            if (CurrentQuestion == default)
                return;

            CurrentlyAnsweringPlayer = player;
            InPlayerAnswering = true;

            if (CurrentQuestion.Type == QuestionType.YoutubeVideo)
                CurrentQuestion.RefreshYoutubeVideoUrl(false, false);
        }

        public void PlayerHasAnswered(Question q, bool isCorrect)
        {
            if (!InPlayerAnswering)
                return;

            InPlayerHasAnswered = true;
            WindowState = isCorrect ? PlayWindowState.AnswerCorrect : PlayWindowState.AnswerIncorrect;
            PlayerCurrentlyInGambleFocus = default;
            InPlayerAnswering = false;

            Task.Run(async () =>
            {
                // Give eventual animations some time to run
                await Task.Delay(1500);
                InPlayerHasAnswered = false;
                if (q.IsGamble)
                {
                    WindowState = PlayWindowState.ShowQuestion;
                    return;
                }

                if (isCorrect)
                {
                    ResetGameBoardAfterAnswer();
                    return;
                }

                WindowState = PlayWindowState.ShowQuestion;
                var isYoutubeQuestion = q.Type == QuestionType.YoutubeVideo;
                var audioOrVideoMedia = q.Type == QuestionType.Audio || q.Type == QuestionType.Video || isYoutubeQuestion;
                if (InShowMediaContent && audioOrVideoMedia)
                {
                    if (_mediaPlaybackStatus == MediaPlaybackStatus.Paused || isYoutubeQuestion)
                    {
                        InMediaContentPlaying = true;
                        if (isYoutubeQuestion)
                            q.RefreshYoutubeVideoUrl(true, false);
                        _mediaPlaybackStatus = MediaPlaybackStatus.Playing;
                    }
                }
            });
        }

        public void AbandonQuestion()
        {
            ResetGameBoardAfterAnswer();
        }

        public void SetMediaContentPlaybackStatus(MediaPlaybackStatus newStatus)
        {
            _mediaPlaybackStatus = newStatus;
            InMediaContentPlaying = _mediaPlaybackStatus == MediaPlaybackStatus.Playing;
        }

        public void NotifyWindowClosed()
        {

        }
        #endregion

        private void SetStateToShowQuestion(Question currentQuestion)
        {
            InShowPreOrPostQuestionContent = false;
            switch (currentQuestion.Type)
            {
                case QuestionType.Text:
                    InShowContent = true;
                    break;
                case QuestionType.Image:
                case QuestionType.Audio:
                case QuestionType.Video:
                case QuestionType.YoutubeVideo:
                    InShowContent = currentQuestion.MediaQuestionFlow == MediaQuestionFlow.TextThenMedia ||
                        currentQuestion.MediaQuestionFlow == MediaQuestionFlow.MediaAndText;
                    InShowMediaContent = currentQuestion.MediaQuestionFlow == MediaQuestionFlow.MediaThenText
                        || currentQuestion.MediaQuestionFlow == MediaQuestionFlow.MediaAndText;

                    if (InShowMediaContent && (currentQuestion.Type == QuestionType.Audio || currentQuestion.Type == QuestionType.Video))
                        SetMediaContentPlaybackStatus(MediaPlaybackStatus.Playing);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void IncrementPlayerCurrentlyInFocus(bool isInBettingPhase)
        {
            if (Players == default || CurrentQuestion == default)
                return;

            if (PlayerCurrentlyInGambleFocus != default && isInBettingPhase)
                PlayerCurrentlyInGambleFocus.IsBetting = false;

            if (_currentPlayerIdx == Players.Count)
            {
                _currentPlayerIdx = 0;
                PlayerCurrentlyInGambleFocus = default;
                if (isInBettingPhase)
                    SetStateToShowQuestion(CurrentQuestion);
                else
                    ResetGameBoardAfterAnswer();
                return;
            }

            PlayerCurrentlyInGambleFocus = Players[_currentPlayerIdx++];
            if (isInBettingPhase)
                PlayerCurrentlyInGambleFocus.IsBetting = true;
        }

        private void ResetGameBoardAfterAnswer()
        {
            InShowContent = false;
            InShowMediaContent = false;
            InMediaContentPlaying = false;
            InPlayerAnswering = false;
            InShowPreOrPostQuestionContent = false;

            CurrentQuestion = default;
            CurrentlyAnsweringPlayer = default;
            WindowState = PlayWindowState.ShowBoard;
        }
    }
}
