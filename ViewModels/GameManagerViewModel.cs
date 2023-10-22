﻿using System;
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
        private bool _answersAllowed;
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
        private RelayCommand? _progressQuestionCommand;
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

                    PlayWindowViewModel.StartQuestion(QuestionModeManager.CurrentlySelectedQuestion);
                    AnswersAllowed = _gameAnswerModeSetting == GameComponents.GameAnswerMode.AllowImmediately;
                });
                return _startQuestionCommand;
            }
        }

        public ICommand ProgressQuestionCommand
        {
            get
            {
                _progressQuestionCommand ??= new RelayCommand(() =>
                {
                    if (QuestionModeManager.CurrentlySelectedQuestion == default)
                        return;
                    PlayWindowViewModel.ProgressQuestion(QuestionModeManager.CurrentlySelectedQuestion);
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
                    if (PlayWindowViewModel.CurrentlyAnsweringPlayer != default &&
                        PlayWindowViewModel.CurrentQuestion != default)
                    {
                        if (isCorrect)
                        {
                            PlayWindowViewModel.CurrentlyAnsweringPlayer.AddCashForQuestion(PlayWindowViewModel.CurrentQuestion);
                            SetQuestionToAnswered(PlayWindowViewModel.CurrentQuestion);
                        }
                        else
                        {
                            PlayWindowViewModel.CurrentlyAnsweringPlayer.SubtractCashForQuestion(PlayWindowViewModel.CurrentQuestion);
                        }
                    }
                    PlayWindowViewModel.PlayerHasAnswered(isCorrect);

                    // Automatically allow answers after failed attempt
                    if (!isCorrect)
                        PlayWindowViewModel.PropertyChanged += SetAllowAnswerOnStateChange;
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
                        SetQuestionToAnswered(PlayWindowViewModel.CurrentQuestion);
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
                    PlayWindowViewModel.InMediaContentPlaying = true;
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

        private void SetQuestionToAnswered(Question question)
        {
            question.IsAnswered = true;
            var matchingCategory = GameBoard?.Categories.FirstOrDefault(x => x.Id == question.CategoryId);
            if (matchingCategory != default)
                matchingCategory.CheckIfAllQuestionsAnswered();
        }

        private void MonitorInputThread(object? state)
        {
            while (!_shouldExit)
            {
                if (_eventQueue.TryDequeue(out var newKeyEvent))
                {
                    if (_eventAction != default)
                        _eventAction(newKeyEvent);

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
