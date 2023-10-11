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
        public QuestionModeManager QuestionModeManager { get; }

        public CategoryViewViewModel CategoryViewModel { get; }

        public string ProgramDescription { get; init; } = "Manage";
        #endregion

        #region Commands
        private RelayCommand? _loadBoardCommand;

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
        #endregion

        #region Private fields
        private readonly VistaOpenFileDialog _loadDialog = new()
        {
            Title = "Load game board",
            Filter = "JSON file (*.json)|*.json",
            Multiselect = false,
        };
        #endregion

        public GameManagerViewModel()
        {
            QuestionModeManager = new();
            CategoryViewModel = new(QuestionModeManager);
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
