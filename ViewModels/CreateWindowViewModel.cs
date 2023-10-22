using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.Common;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using JeopardyKing.WpfComponents;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.ViewModels
{
    public class CreateWindowViewModel : ObservableObject
    {
        #region Public properties
        private string _lastLoadedGamePath = string.Empty;

        public string LastLoadedGamePath
        {
            get => _lastLoadedGamePath;
            private set => SetProperty(ref _lastLoadedGamePath, value);
        }

        public Board GameBoard { get; }

        public QuestionModeManager ModeManager { get; }

        public EditQuestionBoxViewModel EditQuestionViewModel { get; }

        public CategoryViewEditableViewModel CategoryViewViewModel { get; }

        public static string ProgramDescription => "Jeopardy game creator";
        #endregion

        private readonly VistaSaveFileDialog _saveDialog = new()
        {
            Title = "Save current board as",
            Filter = "JSON file (*.json)|*.json",
            DefaultExt = "json",
            ValidateNames = true
        };

        private readonly VistaOpenFileDialog _loadDialog = new()
        {
            Title = "Load game board",
            Filter = "JSON file (*.json)|*.json",
            Multiselect = false,
        };
        public CreateWindowViewModel()
        {
            ModeManager = new();
            GameBoard = new(createDefault: true);
            EditQuestionViewModel = new(this);
            CategoryViewViewModel = new(ModeManager);
        }

        public void NotifyWindowClosed()
        {
        }

        internal void MenuItemPressed(MenuItemButton button)
        {
            if (button == MenuItemButton.Save || button == MenuItemButton.SaveAs)
            {
                if (string.IsNullOrEmpty(GameBoard.GameName))
                {
                    SetGameName(() => Application.Current.Dispatcher.Invoke(() => MenuItemPressed(button)));
                    return;
                }

                if (button == MenuItemButton.SaveAs || string.IsNullOrEmpty(LastLoadedGamePath))
                {
                    if (_saveDialog.ShowDialog() == true)
                        LastLoadedGamePath = _saveDialog.FileName;
                    else
                        return;
                }
            }

            // TODO: Handle exceptions in a nicer way
            switch (button)
            {
                case MenuItemButton.Save:
                case MenuItemButton.SaveAs:
                    {
                        if (!GameBoard.TrySaveGameToJsonFile(LastLoadedGamePath, out var e))
                            throw e!;
                    }
                    break;
                case MenuItemButton.Open:
                    if (_loadDialog.ShowDialog() == true)
                    {
                        if (!_loadDialog.FileName.TryLoadGameFromJsonFile(out var board, out var e))
                            throw e!;

                        GameBoard.CopyFromExisting(board!);
                        GameBoard.SetParametersForAllYoutubeQuestions(false, true);
                        LastLoadedGamePath = _loadDialog.FileName;
                    }
                    break;
                case MenuItemButton.SetName:
                    SetGameName();
                    break;
                case MenuItemButton.AddCategory:
                    GameBoard.AddNewCategory();
                    break;
                default:
                    break;
            }
        }

        private void SetGameName(Action? extraActionOnSuccess = default)
        {
            PopupWindowModal setNameWindow = new(
                "Set game name",
                string.Empty,
                (x, s) =>
                {
                    if (x == ModalWindowButton.OK && !string.IsNullOrEmpty(s))
                    {
                        GameBoard.GameName = s;
                        if (extraActionOnSuccess != default)
                            extraActionOnSuccess.Invoke();
                    }
                },
                GameBoard.GameName,
                s => (!string.IsNullOrEmpty(s), "Game name cannot be empty"));
            _ = setNameWindow.ShowDialog();
        }
    }
}
