﻿using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.Common;
using JeopardyKing.Common.FileUtilities;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.ViewModels
{
    public class GameManagerViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private Board? _gameBoard;
        private Question? _selectedQuestion;
        private bool _buttonAssignmentOngoing;
        #endregion

        public Board? GameBoard
        {
            get => _gameBoard;
            private set => SetProperty(ref _gameBoard, value);
        }

        public Question? SelectedQuestion
        {
            get => _selectedQuestion;
            private set => SetProperty(ref _selectedQuestion, value);
        }

        public bool ButtonAssignmentOngoing
        {
            get => _buttonAssignmentOngoing;
            private set => SetProperty(ref _buttonAssignmentOngoing, value);
        }


        public QuestionModeManager QuestionModeManager { get; }

        public CategoryViewViewModel CategoryViewModel { get; }

        public ObservableCollection<Player> Players { get; }

        public string ProgramDescription { get; init; } = "Manage";

        public const int MaxNumberOfPlayers = 6;
        #endregion

        #region Commands
        private RelayCommand? _loadBoardCommand;
        private RelayCommand? _addPlayerCommand;
        private RelayCommand<Player>? _assignPlayerCommand;
        private RelayCommand<Player>? _removePlayerCommand;

        public ICommand LoadBoardCommand
        {
            get
            {
                _loadBoardCommand ??= new RelayCommand(() =>
                {
                    if (_loadDialog.ShowDialog() == true)
                    {
                        var reader = new FileTextReader(_loadDialog.FileName);
                        // TODO: Make error handling nicer
                        if (!reader.SuccessfulRead)
                            throw reader.ReadException!;

                        var (board, e) = reader.AllText.DeserializeJsonString<Board>(convertSnakeCaseToPascalCase: true);
                        // TODO: Make error handling nicer
                        if (e != default)
                            throw e;

                        GameBoard = board;
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
                    lock (_playersAccessLock)
                    {
                        if (Players.Count == MaxNumberOfPlayers)
                            return;
                        var playerName = $"Player {Players.Count + 1}";
                        Players.Add(new(playerName));
                    }
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
                    var hasLock = Monitor.TryEnter(_playersAccessLock);
                    if (!hasLock)
                        return;

                    // Note: This should be impossible as the flag as always set after the lock is acquired
                    if (ButtonAssignmentOngoing)
                    {
                        Monitor.Exit(_playersAccessLock);
                        return;
                    }

                    ButtonAssignmentOngoing = true;
                    ButtonAssignmentOngoing = false;
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
                    lock (_playersAccessLock)
                    {
                        if (p == default || Players.Count == 0)
                            return;
                        Players.Remove(p);
                    }
                });
                return _removePlayerCommand;
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
        #endregion

        public GameManagerViewModel()
        {
            QuestionModeManager = new();
            CategoryViewModel = new(QuestionModeManager);
            Players = new();
            BindingOperations.EnableCollectionSynchronization(Players, _playersAccessLock);
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
