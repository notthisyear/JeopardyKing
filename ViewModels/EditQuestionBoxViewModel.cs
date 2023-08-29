using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.Common;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.ViewModels
{
    public partial class EditQuestionBoxViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _selectedCurrency = string.Empty;
        #endregion

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (ModeManager.CurrentlySelectedQuestion != default)
                    ModeManager.CurrentlySelectedQuestion.Currency = _currencyNameMap[value];
                SetProperty(ref _selectedCurrency, value);
            }
        }

        public CreateWindowModeManager ModeManager { get; }

        public List<string> CurrencyNames { get; }
        #endregion

        #region Commands
        private RelayCommand? _deleteQuestionCommand;
        private RelayCommand? _loadMediaCommand;
        private RelayCommand? _getYoutubeLinkCommand;

        public ICommand DeleteQuestionCommand
        {
            get
            {
                _deleteQuestionCommand ??= new RelayCommand(() =>
                {
                    PopupWindowModal confirmationDialog = new(
                        Application.Current.MainWindow,
                        "Are you sure?",
                        "Delete question",
                        x =>
                        {
                            if (x == ModalWindowButton.OK)
                            {
                                if (ModeManager.CurrentlySelectedQuestion == default)
                                    return;

                                _createWindowViewModel.GameBoard.DeleteQuestion(
                                    ModeManager.CurrentlySelectedQuestion.CategoryId,
                                    ModeManager.CurrentlySelectedQuestion.Id);

                                ModeManager.DeselectQuestion();
                            }
                        },
                        $"You are about to delete a question. Are you sure?");
                    _ = confirmationDialog.ShowDialog();
                });
                return _deleteQuestionCommand;
            }
        }

        public ICommand LoadMediaCommand
        {
            get
            {
                _loadMediaCommand ??= new RelayCommand(() =>
                {
                    if (ModeManager.CurrentlySelectedQuestion == default)
                        return;

                    VistaOpenFileDialog dialog = new()
                    {
                        Title = $"Select {ModeManager.CurrentlySelectedQuestion.Type} resource",
                        Multiselect = false,
                        Filter = GetFileExtensionsForType(ModeManager.CurrentlySelectedQuestion.Type),
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        var fileName = dialog.FileName;
                    }
                });
                return _loadMediaCommand;
            }
        }

        public ICommand GetYoutubeLinkCommand
        {
            get
            {
                _getYoutubeLinkCommand ??= new RelayCommand(() =>
                {
                    if (ModeManager.CurrentlySelectedQuestion == default)
                        return;

                    PopupWindowModal confirmationDialog =
                    new(Application.Current.MainWindow,
                        "Add YouTube link",
                        string.Empty,
                        (response, link) =>
                        {
                            if (response == ModalWindowButton.OK && TryExtractVideoId(link, out var videoId))
                            {
                                ModeManager.CurrentlySelectedQuestion.IsEmbeddedMedia = false;
                                ModeManager.CurrentlySelectedQuestion.YouTubeVideoId = link;
                                ModeManager.CurrentlySelectedQuestion.MultimediaContentLink = videoId;
                            }
                        },
                        ModeManager.CurrentlySelectedQuestion.IsEmbeddedMedia ?
                            string.Empty : ModeManager.CurrentlySelectedQuestion.YouTubeVideoId,
                        ValidateYouTubeLink);

                    _ = confirmationDialog.ShowDialog();
                });
                return _getYoutubeLinkCommand;
            }
        }
        #endregion

        #region Private fields        
        private readonly Dictionary<string, CurrencyType> _currencyNameMap;
        private readonly Dictionary<CurrencyType, string> _currencyTypeMap;
        private readonly CreateWindowViewModel _createWindowViewModel;
        #endregion

        public EditQuestionBoxViewModel(CreateWindowViewModel createWindowViewModel)
        {
            _createWindowViewModel = createWindowViewModel;
            ModeManager = _createWindowViewModel.ModeManager;

            _currencyNameMap = new();
            _currencyTypeMap = new();
            CurrencyNames = new();

            foreach (var t in Enum.GetValues<CurrencyType>())
            {
                var (attr, _) = t.GetCustomAttributeFromEnum<CurrencyAttribute>();
                if (attr != default)
                {
                    var displayName = $"{attr.Name} ({attr.Code})";
                    CurrencyNames!.Add(displayName);
                    _currencyNameMap.Add(displayName, t);
                    _currencyTypeMap.Add(t, displayName);
                }
            }

            SelectedCurrency = CurrencyNames.First();
            ModeManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ModeManager.CurrentlySelectedQuestion))
                {
                    if (ModeManager.CurrentlySelectedQuestion != null)
                        SelectedCurrency = _currencyTypeMap[ModeManager.CurrentlySelectedQuestion.Currency];
                }
            };
        }

        #region Private methods
        private static string GetFileExtensionsForType(QuestionType type)
           => type switch
           {
               QuestionType.Image => "PNG images (*.png)|*.png|JPG images (*.jpg)|*.jpg|Bitmap images (*.bmp)|*.bmp",
               QuestionType.Audio => "All files (*.*)|*.*",
               QuestionType.Video => "All files (*.*)|*.*",
               _ => throw new NotSupportedException(),
           };

        private static (bool isValid, string errorMessage) ValidateYouTubeLink(string link)
        {
            if (string.IsNullOrEmpty(link))
                return (false, "YouTube link invalid - input is empty");

            var linkValid = TryExtractVideoId(link, out _);
            return (linkValid, linkValid ? string.Empty : "YouTube link invalid - could not extract video ID");
        }

        private static bool TryExtractVideoId(string link, out string videoId)
        {
            var m = ExtractVideoIdRegex().Match(link);
            videoId = m.Success ? m.Value : string.Empty;
            return m.Success;
        }

        [GeneratedRegex(@"((?<=(v|V)/)|(?<=(\\?|\\&)v=)|(?<=be/)|(?<=embed/)|(?<=shorts/))([a-zA-Z0-9-_]{5,30})")]
        private static partial Regex ExtractVideoIdRegex();
        #endregion
    }
}
