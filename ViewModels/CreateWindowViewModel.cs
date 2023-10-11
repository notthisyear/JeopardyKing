using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.Common;
using JeopardyKing.Common.FileUtilities;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using JeopardyKing.WpfComponents;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.ViewModels
{
    public class CreateWindowViewModel : ObservableObject
    {
        #region Public properties
        public Board GameBoard { get; }

        public QuestionModeManager ModeManager { get; }

        public EditQuestionBoxViewModel EditQuestionViewModel { get; }

        public CategoryViewEditableViewModel CategoryViewViewModel { get; }

        public static string ProgramDescription => "Jeopardy game creator";
        #endregion

        private readonly VistaSaveFileDialog _saveDialog = new()
        {
            Title = "Save current board",
            Filter = "JSON file (*.json)|*.json",
            DefaultExt = "json",
            ValidateNames = true
        };

        private string _lastLoadedGamePath = string.Empty;

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
            switch (button)
            {
                case MenuItemButton.Save:
                    if (string.IsNullOrEmpty(_lastLoadedGamePath))
                    {
                        if (_saveDialog.ShowDialog() == true)
                            _lastLoadedGamePath = _saveDialog.FileName;
                        else
                            return;
                    }

                    GameBoard.GameName = Path.GetFileNameWithoutExtension(_lastLoadedGamePath);
                    var (s, e) = GameBoard.SerializeToJsonString(convertPascalCaseToSnakeCase: true, indent: true);
                    if (e != default)
                        throw e;

                    FileTextWriter writer = new(s!, _lastLoadedGamePath);
                    if (writer.SuccessfulWrite && writer.WriteException != default)
                        throw writer.WriteException;

                    break;
                case MenuItemButton.SaveAs:
                    break;
                case MenuItemButton.Open:
                    break;
                case MenuItemButton.AddCategory:
                    GameBoard.AddNewCategory();
                    break;
                default:
                    break;
            }
        }
    }
}
