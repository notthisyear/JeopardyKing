using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.Common;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.ViewModels
{
    using InputManager = Communication.InputManager;

    public class GameManagerViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private Board? _gameBoard;
        private bool _allPlayersHasMapping;
        private bool _settingBetAmountForPlayer;
        private bool _answersAllowed;
        private bool _allPlayersHaveAnsweredGambleQuestion = false;
        private string _gameAnswerMode = string.Empty;
        private string _gameAnswerModeToolTip = string.Empty;
        #endregion

        public Board? GameBoard
        {
            get => _gameBoard;
            private set => SetProperty(ref _gameBoard, value);
        }

        public bool AllPlayersHasMapping
        {
            get => _allPlayersHasMapping;
            private set => SetProperty(ref _allPlayersHasMapping, value);
        }

        public bool SettingBetAmountForPlayer
        {
            get => _settingBetAmountForPlayer;
            set => SetProperty(ref _settingBetAmountForPlayer, value);
        }

        public bool AllPlayersHaveAnsweredGambleQuestion
        {
            get => _allPlayersHaveAnsweredGambleQuestion;
            private set => SetProperty(ref _allPlayersHaveAnsweredGambleQuestion, value);
        }

        public bool AnswersAllowed
        {
            get => _answersAllowed;
            set => SetProperty(ref _answersAllowed, value);
        }

        public string GameAnswerMode
        {
            get => _gameAnswerMode;
            set
            {
                _gameAnswerModeSetting = _gameModeMap[value].mode;
                GameAnswerModeToolTip = _gameModeMap[value].tooltip;
                SetProperty(ref _gameAnswerMode, value);
            }
        }

        public string GameAnswerModeToolTip
        {
            get => _gameAnswerModeToolTip;
            set => SetProperty(ref _gameAnswerModeToolTip, value);
        }

        public QuestionModeManager QuestionModeManager { get; }

        public CategoryViewViewModel CategoryViewModel { get; }

        public ObservableCollection<Player> Players { get; }

        public PlayWindowViewModel PlayWindowViewModel { get; }

        public List<string> GameModeOptions { get; }

        public string ProgramDescription { get; init; } = "Manage";

        public const int MaxNumberOfPlayers = 6;
        #endregion

        #region Commands
        private RelayCommand? _toggleAnswerAllowedCommand;
        private RelayCommand? _loadBoardCommand;
        private RelayCommand? _addPlayerCommand;
        private RelayCommand<Player>? _assignPlayerCommand;
        private RelayCommand<Player>? _removePlayerCommand;
        private RelayCommand? _startGameCommand;
        private RelayCommand? _revealNextCategoryCommand;
        private RelayCommand? _startQuestionCommand;
        private RelayCommand? _setQuestionToUnansweredCommand;
        private RelayCommand<PlayWindowViewModel.QuestionProgressType>? _progressQuestionCommand;
        private RelayCommand<bool>? _answerQuestionCommand;
        private RelayCommand? _abandonQuestionCommand;
        private RelayCommand? _playMediaAgainCommand;

        public ICommand ToggleAnswerAllowedCommand
        {
            get
            {
                _toggleAnswerAllowedCommand ??= new RelayCommand(() =>
                {
                    if (PlayWindowViewModel.WindowState == PlayWindowState.ShowQuestion)
                        AnswersAllowed = !AnswersAllowed;
                });
                return _toggleAnswerAllowedCommand;
            }
        }

        public ICommand LoadBoardCommand
        {
            get
            {
                _loadBoardCommand ??= new RelayCommand(() =>
                {
                    if (_loadDialog.ShowDialog() == true)
                    {
                        // TODO: Make error handling nicer
                        if (!_loadDialog.FileName.TryLoadGameFromJsonFile(out var board, out var e))
                            throw e!;
                        GameBoard = board!;
                    }
                });
                return _loadBoardCommand;
            }
        }

        public ICommand AddPlayerCommand
        {
            get
            {
                _addPlayerCommand ??= new RelayCommand(() =>
                {
                    var hasLock = Monitor.TryEnter(_playersAccessLock);
                    if (!hasLock)
                        return;

                    if (Players.Count < MaxNumberOfPlayers)
                    {
                        var playerName = $"Player {Players.Count + 1}";
                        Players.Add(new(_playerIdCounter++, playerName));
                        AllPlayersHasMapping = false;
                    }
                    Monitor.Exit(_playersAccessLock);
                });
                return _addPlayerCommand;
            }
        }

        public ICommand AssignPlayerCommand
        {
            get
            {
                _assignPlayerCommand ??= new RelayCommand<Player>(p =>
                {
                    if (p == default)
                        return;

                    var hasLock = Monitor.TryEnter(_playersAccessLock);
                    if (!hasLock)
                        return;

                    _inputManager.RemovePlayerKeyMappingIfNeeded(p.Id);
                    SelectButtonPopupModal buttonSelectionDialog = new(Application.Current.MainWindow,
                        p.Name,
                        (gotEvent, deviceId, key) =>
                        {
                            if (!gotEvent)
                                return;

                            if (_inputManager.TryAddPlayerKeyMapping(p.Id, deviceId, key))
                                AllPlayersHasMapping = !Players.Where(x => !_inputManager.PlayerHasMapping(x.Id)).Any();
                        });

                    _eventAction = e =>
                    {
                        if (e.Event == InputManager.KeyEvent.KeyDown)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                buttonSelectionDialog.LastEvent = e;
                                buttonSelectionDialog.LastKeyPressed = e.Key.ToString();
                                if (_inputManager.TryGetInformationForKeyboard(e.SourceId, out var info))
                                    buttonSelectionDialog.LastPressedSource = $"{info!.DeviceDescription} [{info!.KeyboardType}] ({e.SourceId})";
                                else
                                    buttonSelectionDialog.LastPressedSource = $"Unknown source ({e.SourceId})";
                                buttonSelectionDialog.LastKeyAvailable = _inputManager.MappingAvailable(e.SourceId, e.Key);
                            });
                        }
                    };

                    _inputManager.SetPropagationMode(InputManager.PropagationMode.All);
                    _ = buttonSelectionDialog.ShowDialog();

                    _inputManager.SetPropagationMode(InputManager.PropagationMode.OnlyMappedKeys);
                    _eventAction = default;

                    Monitor.Exit(_playersAccessLock);
                });
                return _assignPlayerCommand;
            }
        }

        public ICommand RemovePlayerCommand
        {
            get
            {
                _removePlayerCommand ??= new RelayCommand<Player>(p =>
                {
                    var hasLock = Monitor.TryEnter(_playersAccessLock);
                    if (!hasLock)
                        return;

                    if (p != default && Players.Count > 0)
                    {
                        _inputManager.RemovePlayerKeyMappingIfNeeded(p.Id);
                        Players.Remove(p);
                        AllPlayersHasMapping = !Players.Where(x => !_inputManager.PlayerHasMapping(x.Id)).Any();
                    }

                    Monitor.Exit(_playersAccessLock);
                });
                return _removePlayerCommand;
            }
        }

        public ICommand StartGameCommand
        {
            get
            {
                _startGameCommand ??= new RelayCommand(() =>
                {
                    if (GameBoard != default)
                    {
                        PlayWindowViewModel.StartGame(GameBoard, new ReadOnlyObservableCollection<Player>(Players));
                        _inputManager.SetPropagationMode(InputManager.PropagationMode.OnlyMappedKeys);
                    }
                });
                return _startGameCommand;
            }
        }

        public ICommand RevealNextCategoryCommand
        {
            get
            {
                _revealNextCategoryCommand ??= new RelayCommand(() => PlayWindowViewModel.RevealNextCategory());
                return _revealNextCategoryCommand;
            }
        }

        public ICommand StartQuestionCommand
        {
            get
            {
                _startQuestionCommand ??= new RelayCommand(() =>
                {
                    if (QuestionModeManager.CurrentlySelectedQuestion == default)
                        return;

                    _isGambleQuestion = QuestionModeManager.CurrentlySelectedQuestion.IsGamble;
                    PlayWindowViewModel.StartQuestion(QuestionModeManager.CurrentlySelectedQuestion);
                    AnswersAllowed = !_isGambleQuestion && (_gameAnswerModeSetting == GameComponents.GameAnswerMode.AllowImmediately);
                });
                return _startQuestionCommand;
            }
        }

        public ICommand SetQuestionToUnansweredCommand
        {
            get
            {
                _setQuestionToUnansweredCommand ??= new RelayCommand(() =>
                {
                    if (QuestionModeManager.CurrentlySelectedQuestion == default)
                        return;
                    SetQuestionAnswerStatus(QuestionModeManager.CurrentlySelectedQuestion, false);
                });
                return _setQuestionToUnansweredCommand;
            }
        }

        public ICommand ProgressQuestionCommand
        {
            get
            {
                _progressQuestionCommand ??= new RelayCommand<PlayWindowViewModel.QuestionProgressType>(p =>
                {
                    if (QuestionModeManager.CurrentlySelectedQuestion == default)
                        return;

                    PlayWindowViewModel.ProgressQuestion(QuestionModeManager.CurrentlySelectedQuestion, p);
                });
                return _progressQuestionCommand;
            }
        }

        public ICommand AnswerQuestionCommand
        {
            get
            {
                _answerQuestionCommand ??= new RelayCommand<bool>(isCorrect =>
                {
                    if (PlayWindowViewModel.CurrentQuestion == default)
                        return;

                    var answeringPlayer = _isGambleQuestion ? PlayWindowViewModel.PlayerCurrentlyInGambleFocus : PlayWindowViewModel.CurrentlyAnsweringPlayer;
                    if (answeringPlayer == default)
                        return;

                    if (isCorrect)
                    {
                        answeringPlayer.AddCashForQuestion(PlayWindowViewModel.CurrentQuestion);
                        if (!_isGambleQuestion)
                            SetQuestionAnswerStatus(PlayWindowViewModel.CurrentQuestion, true);
                    }
                    else
                    {
                        answeringPlayer.SubtractCashForQuestion(PlayWindowViewModel.CurrentQuestion);
                    }

                    PlayWindowViewModel.PlayerHasAnswered(PlayWindowViewModel.CurrentQuestion, isCorrect);

                    // Automatically allow answers after failed attempt or progress to the next player for a gamble question
                    if (!isCorrect && !_isGambleQuestion)
                    {
                        PlayWindowViewModel.PropertyChanged += SetAllowAnswerOnStateChange;
                    }
                    else if (_isGambleQuestion)
                    {
                        PlayWindowViewModel.PropertyChanged += SetProgressAnsweringPlayerOnStateChange;
                        _lastPlayerGambleQuestionAnswerChecked = !Players.Where(x => x.HasAnsweredGambleQuestion).Any();
                        if (_lastPlayerGambleQuestionAnswerChecked)
                            SetQuestionAnswerStatus(PlayWindowViewModel.CurrentQuestion, true);

                    }
                });
                return _answerQuestionCommand;
            }
        }

        public ICommand AbandonQuestionCommand
        {
            get
            {
                _abandonQuestionCommand ??= new RelayCommand(() =>
                {
                    if (PlayWindowViewModel.CurrentQuestion != default)
                    {
                        SetQuestionAnswerStatus(PlayWindowViewModel.CurrentQuestion, true);
                        if (PlayWindowViewModel.CurrentQuestion.IsGamble)
                        {
                            lock (_playersAccessLock)
                            {
                                foreach (var p in Players)
                                    p.ResetBet();
                            }
                            AllPlayersHaveAnsweredGambleQuestion = false;
                        }
                    }
                    PlayWindowViewModel.AbandonQuestion();
                    AnswersAllowed = false;
                });
                return _abandonQuestionCommand;
            }
        }

        public ICommand PlayMediaAgainCommand
        {
            get
            {
                _playMediaAgainCommand ??= new RelayCommand(() =>
                {
                    if (PlayWindowViewModel.CurrentQuestion == default)
                        return;

                    if (PlayWindowViewModel.CurrentQuestion.Type == QuestionType.YoutubeVideo)
                    {
                        PlayWindowViewModel.CurrentQuestion.MultimediaContentLink = string.Empty;
                        PlayWindowViewModel.CurrentQuestion.RefreshYoutubeVideoUrl(true, false);
                    }
                    else
                    {
                        PlayWindowViewModel.SetMediaContentPlaybackStatus(PlayWindowViewModel.MediaPlaybackStatus.Playing);
                    }
                });
                return _playMediaAgainCommand;
            }
        }
        #endregion

        #region Private fields
        private readonly VistaOpenFileDialog _loadDialog = new()
        {
            Title = "Load game board",
            Filter = "JSON file (*.json)|*.json",
            Multiselect = false,
        };
        private readonly object _playersAccessLock = new();
        private readonly InputManager _inputManager;
        private readonly ConcurrentQueue<InputManager.KeyboardEvent> _eventQueue = new();
        private readonly Thread _inputThread;
        private readonly Dictionary<string, (GameAnswerMode mode, string tooltip)> _gameModeMap;
        private Action<InputManager.KeyboardEvent>? _eventAction;
        private int _playerIdCounter = 0;
        private bool _shouldExit = false;
        private GameAnswerMode _gameAnswerModeSetting;
        private bool _isGambleQuestion = false;
        private bool _lastPlayerGambleQuestionAnswerChecked = false;
        #endregion

        public GameManagerViewModel(PlayWindowViewModel playWindowViewModel)
        {
            PlayWindowViewModel = playWindowViewModel;
            QuestionModeManager = new();
            CategoryViewModel = new(QuestionModeManager, PlayWindowViewModel);
            Players = new();

            _inputThread = new(MonitorInputThread) { IsBackground = true };
            _inputThread.Start();
            _inputManager = new(_eventQueue);
            _ = _inputManager.TryEnumerateKeyboardDevices(out _);

            BindingOperations.EnableCollectionSynchronization(Players, _playersAccessLock);

            _gameModeMap = new();
            GameModeOptions = new();
            EnumerationUtilities.ActOnEnumMembersWithAttribute<GameAnswerMode, GameAnswerModeTypeAttribute>((m, a) =>
            {
                _gameModeMap.Add(a.DisplayText, (m, a.ToolTip));
                GameModeOptions.Add(a.DisplayText);
            });

            GameAnswerMode = _gameModeMap.First().Key;
            GameAnswerModeToolTip = _gameModeMap.First().Value.tooltip;
            _gameAnswerModeSetting = _gameModeMap.First().Value.mode;
        }

        public void NotifyWindowClosed()
        {
            _shouldExit = true;
        }

        #region Private methods
        private void SetAllowAnswerOnStateChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayWindowViewModel.WindowState) && PlayWindowViewModel.WindowState == PlayWindowState.ShowQuestion)
            {
                AnswersAllowed = true;
                PlayWindowViewModel.PropertyChanged -= SetAllowAnswerOnStateChange;
            }
        }

        private void SetProgressAnsweringPlayerOnStateChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayWindowViewModel.WindowState) && PlayWindowViewModel.WindowState == PlayWindowState.ShowQuestion)
            {
                PlayWindowViewModel.PropertyChanged -= SetProgressAnsweringPlayerOnStateChange;
                ProgressQuestionCommand.Execute(PlayWindowViewModel.QuestionProgressType.GambleCheckAnswerPhase);
                if (_lastPlayerGambleQuestionAnswerChecked)
                {
                    AllPlayersHaveAnsweredGambleQuestion = false;
                    _isGambleQuestion = false;
                    _lastPlayerGambleQuestionAnswerChecked = false;
                }
            }
        }

        private class CompareCash : IComparer<(decimal cash, int id)>
        {
            public int Compare((decimal cash, int id) x, (decimal cash, int id) y)
                => (x.cash > y.cash) ? 1 : ((x.cash < y.cash) ? -1 : 0);
        }

        private void SetQuestionAnswerStatus(Question question, bool isAnswered)
        {
            question.IsAnswered = isAnswered;
            var matchingCategory = GameBoard?.Categories.FirstOrDefault(x => x.Id == question.CategoryId);
            if (matchingCategory != default)
                matchingCategory.CheckIfAllQuestionsAnswered();

            if (GameBoard != default && isAnswered && GameBoard.AllQuestionsAnswered())
            {
                var winningPlayersId = new List<int>();
                ObservableCollection<Player> playersOrdered = new();
                lock (_playersAccessLock)
                {
                    List<(decimal cash, int id)> playersOrderedByCash = Players.Select(x => (x.Cash, x.Id)).OrderDescending(new CompareCash()).ToList();
                    for (var i = 0; i < playersOrderedByCash.Count; i++)
                    {
                        var p = Players.First(x => x.Id == playersOrderedByCash[i].id);
                        p.FinishingPlace = i + 1;
                        playersOrdered.Add(p);
                    }
                }
                PlayWindowViewModel.BoardDone(new(playersOrdered));
            }
        }

        private void MonitorInputThread(object? state)
        {
            // Note: We probably should lock on the collection on players before accessing,
            //       but let's skip that for now (it "feels" like it'd hurt performance,
            //       but we really should measure before assuming).
            while (!_shouldExit)
            {
                if (_eventQueue.TryDequeue(out var newKeyEvent))
                {
                    if (_eventAction != default)
                        _eventAction(newKeyEvent);

                    if (_isGambleQuestion && !PlayWindowViewModel.InShowPreOrPostQuestionContent)
                    {
                        var p = Players.FirstOrDefault(x => x.Id == newKeyEvent.PlayerId);
                        if (p != default)
                        {
                            p.HasAnsweredGambleQuestion = true;
                            AllPlayersHaveAnsweredGambleQuestion = !Players.Where(x => !x.HasAnsweredGambleQuestion).Any();
                        }
                        continue;
                    }

                    if (PlayWindowViewModel.WindowState == PlayWindowState.ShowQuestion && !AnswersAllowed)
                        continue;

                    var player = Players.FirstOrDefault(x => x.Id == newKeyEvent.PlayerId);
                    if (player != default)
                    {
                        var isKeyDown = newKeyEvent.Event == InputManager.KeyEvent.KeyDown;
                        player.IsPressingKey = isKeyDown;
                        if (isKeyDown && PlayWindowViewModel.WindowState == PlayWindowState.ShowQuestion)
                        {
                            player.IsPressingKey = false;
                            AnswersAllowed = false;
                            PlayWindowViewModel.PlayerHasPressed(player);
                        }
                    }
                }
                Thread.Sleep(50);
            }
        }
        #endregion
    }
}
